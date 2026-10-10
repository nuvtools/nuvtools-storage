# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

NuvTools.Storage is a suite of .NET libraries for abstracting storage operations with a focus on Microsoft Azure Storage. The solution consists of:

- **NuvTools.Storage**: Core library with storage abstractions (`IFileManager`, `IFile`) and base implementations
- **NuvTools.Storage.Azure**: Azure-specific implementation for Azure Blob Storage
- **NuvTools.Storage.Azure.Test**: NUnit test suite using Moq for mocking Azure SDK components

## Build and Test Commands

### Building the Solution
```bash
dotnet build NuvTools.Storage.slnx
```

### Running All Tests
```bash
dotnet test NuvTools.Storage.slnx
```

### Running Tests for Specific Project
```bash
dotnet test tests/NuvTools.Storage.Azure.Test/NuvTools.Storage.Azure.Test.csproj
```

### Running a Single Test
```bash
dotnet test tests/NuvTools.Storage.Azure.Test/NuvTools.Storage.Azure.Test.csproj --filter "FullyQualifiedName~AzureFileManagerTests.AddFileAsync_ShouldUploadAndReturnFile"
```

### Building for Package
```bash
dotnet pack NuvTools.Storage.slnx -c Release
```
Note: `GeneratePackageOnBuild` is enabled in both main projects, so packages are automatically generated during builds.

**Solution Format**: This solution uses the modern `.slnx` (XML-based) format instead of the legacy `.sln` format.

## Architecture

### Core Abstractions (NuvTools.Storage)

The library uses a provider pattern where concrete implementations extend the core interfaces:

- **IFileManager**: Main interface for storage operations (add, get, remove, list files)
  - `AddFileAsync()`: Uploads single file with optional root directory
  - `AddFilesAsync()`: Batch upload files in parallel using `Task.WhenAll`
  - `GetFileAsync()`: Retrieves file metadata or downloads content based on `download` parameter
  - `GetFilesAsync()`: Lists files with pagination support
  - `RemoveFileAsync()`: Deletes a file by ID
  - `FileExistsAsync()`: Checks file existence
  - `GetAccessRepositoryUri()`: Generates SAS URI with specified permissions
  - `ListFilesAsync(prefix)`: Streams `FileDescriptor`s (path, size, last modified, content type) under a prefix
  - `DownloadFileAsync(path, stream)`: Writes a file into a stream; `false` when it does not exist
  - `CopyFileAsync()` / `MoveFileAsync()`: By path, inside the repository; moving is also renaming
  - `MoveFilesAsync()` / `RemoveFilesAsync()`: By prefix — renaming and removing a "folder"

- **FileDescriptor**: What a listing returns — a file without its content

A repository has no folders, only prefixes. Folder conventions (a marker file for an empty folder, how many levels there are) belong to the application, not to this contract.

- **IFile**: Represents a file with three possible states:
  - URI-only (metadata without content)
  - Stream-based (with content)
  - Base64-encoded (with content as string)

- **AccessPermissions**: Flags enum for granular access control (Read, Add, Create, Update, Write, Delete, List)

### Azure Implementation (NuvTools.Storage.Azure)

**AzureFileManager** implements `IFileManager` using Azure Blob Storage:
- Wraps `BlobServiceClient` and `BlobContainerClient` from Azure SDK
- Uses lazy initialization for `BlobContainerClient` (accessed via `Repository` property)
- Constructor accepts either connection string or `BlobServiceClient` instance
- All async methods support `CancellationToken` for cancellation

**Key Design Patterns**:
1. **Extension Methods**: `AzureFileManagerExtensions.ToFileAsync()` converts `BlobClient` to `IFile`
2. **Parallel Uploads**: `AddFilesAsync()` uses `Task.WhenAll` for concurrent uploads
3. **SAS Token Generation**: `GetAccessRepositoryUri()` generates 24-hour SAS tokens via `PermissionsHelper`
4. **Pagination**: `GetFilesAsync()` uses Azure SDK's `AsPages()` for paginated listing
5. **Copy through the client**: `CopyFileAsync()` reads the source and uploads the target instead of asking the service to copy, because a service-side copy needs the source readable by URL and a private container is not. `MoveFileAsync()` deletes the source only after the copy exists
6. **Prefix operations list first**: `MoveFilesAsync()` and `RemoveFilesAsync()` collect the paths before acting, so a file already moved never comes up again in the listing; both are restartable, and `RemoveFilesAsync()` refuses an empty prefix
7. **Not found**: a 404 from the service becomes `false` (`DownloadFileAsync`) or `FileNotFoundException` (`CopyFileAsync`/`MoveFileAsync`), so callers never catch an Azure exception

### File Handling

The `File` class supports three initialization patterns:
```csharp
// URI-only (no content)
new File("name.txt", "text/plain", new Uri("https://..."))

// Stream-based (auto-converts to Base64)
new File("name.txt", "text/plain", stream)

// Base64-encoded (auto-converts to Stream)
new File("name.txt", "text/plain", "QlJVTk8=")
```

`FileHelper.GetFileName()` sanitizes filenames before upload.

## Testing Approach

Tests use **Moq** to mock Azure SDK components:
- Mock `BlobServiceClient`, `BlobContainerClient`, and `BlobClient`
- Use `BlobsModelFactory` for creating Azure response objects
- `Response.FromValue()` creates mock Azure responses
- All tests verify proper cancellation token propagation
- `AzureFileManagerPrefixTests` keeps one `Mock<BlobClient>` per path, so a test can assert which path was written and which was deleted

Example test pattern:
```csharp
_mockBlobClient
    .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));
```

## CI and Releasing

`.github/workflows/ci.yml` builds the solution in Release with `-warnaserror` and runs the tests on every push and pull request to `main`.

Each package has its own manually-run publish workflow (`storage-publish.yml`, `azure-publish.yml`), both calling the reusable `package-publish.yml`. It reads `<Version>` from the `.csproj` (never computes it), verifies the package, pushes through nuget.org trusted publishing behind the `nuget` environment, and then writes the release tag (`storage-vX.Y.Z`, `azure-vX.Y.Z`). See *Releasing* in the README.

- A new package needs its own small workflow file and a row in the README table.
- The SourceLink and symbol settings (`PublishRepositoryUrl`, `EmbedUntrackedSources`, `IncludeSymbols`, `snupkg`) must be in every packable project: the workflow fails a package without its `.snupkg`.
- `NuvTools.Storage` is published first. A new `IFileManager` member means both packages get a new `<Version>`, the contract before the implementation.

## Multi-Targeting

Both main projects target **net8**, **net9**, and **net10.0** (note the `.0` suffix for .NET 10).
Tests target only **net10.0**.

When adding new language features, ensure compatibility with C# 12 (available in .NET 8+).

## Assembly Signing

Both production projects use strong-name signing with `.snk` files. Do not modify or remove these files.

## Code Style

- **Nullable reference types** are enabled (`<Nullable>enable</Nullable>`)
- Use `ArgumentNullException.ThrowIfNull()` and `ArgumentException.ThrowIfNullOrWhiteSpace()` for validation
- All async methods should accept `CancellationToken cancellationToken = default` as the last parameter
- Use `.ConfigureAwait(false)` for all `await` calls in library code
- Prefer `await foreach` with `AsPages()` for paginated Azure operations
