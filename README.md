# NuvTools.Storage

A suite of .NET libraries for abstracting and simplifying storage operations, with comprehensive support for Microsoft Azure Blob Storage.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-512BD4)](https://dotnet.microsoft.com/)

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [Target Frameworks](#target-frameworks)
- [Building from Source](#building-from-source)
- [Documentation](#documentation)
- [License](#license)

## Overview

NuvTools.Storage provides a clean, intuitive abstraction layer for file storage operations. The library follows a provider pattern, allowing you to implement custom storage solutions while maintaining a consistent API.

### Libraries

#### NuvTools.Storage (Core)

The core library defines the foundational abstractions:

- **`IFileManager`** - Interface for managing file storage operations (add, get, remove, list, SAS URI generation)
- **`IFile`** - Represents a file with metadata and content in multiple formats (URI, Stream, Base64)
- **`File`** - Concrete implementation supporting three initialization modes
- **`AccessPermissions`** - Enum for fine-grained access control (Read, Write, Delete, List, etc.)
- **Extension methods** for seamless conversion between Stream and Base64 formats

#### NuvTools.Storage.Azure

Azure Blob Storage implementation:

- **`AzureFileManager`** - Full `IFileManager` implementation for Azure Blob Storage
- **`Security`** - Helper class for generating signed tokens for Azure Storage
- Automatic conversion between Azure SDK types and NuvTools abstractions
- Support for paginated file listing
- Parallel batch file uploads
- Direct stream uploads to specific blob paths
- File-level signed URI generation with custom expiration
- Built on Azure.Storage.Blobs SDK

## Features

✅ **Provider Pattern** - Easy to implement custom storage providers
✅ **Async/Await** - Fully asynchronous API with `CancellationToken` support
✅ **Multiple Content Formats** - Work with files as URIs, Streams, or Base64 strings
✅ **Azure Integration** - Production-ready Azure Blob Storage implementation
✅ **Signed URI Generation** - Container-level and file-level signed URIs with custom expiration
✅ **Batch Operations** - Upload multiple files in parallel with `Task.WhenAll`
✅ **Direct Stream Uploads** - Upload streams directly to specific blob paths
✅ **Comprehensive Documentation** - Full XML documentation for IntelliSense
✅ **Multi-Targeting** - Supports .NET 8, 9, and 10

## Installation

Install via NuGet Package Manager:

```bash
# Core library
dotnet add package NuvTools.Storage

# Azure Blob Storage implementation
dotnet add package NuvTools.Storage.Azure
```

Or via Package Manager Console:

```powershell
Install-Package NuvTools.Storage
Install-Package NuvTools.Storage.Azure
```

## Quick Start

### Azure Blob Storage Example

```csharp
using NuvTools.Storage;
using NuvTools.Storage.Azure;

// Initialize the Azure file manager
var fileManager = new AzureFileManager(
    connectionString: "your-azure-storage-connection-string",
    repositoryName: "my-container"
);

// Upload a file from a stream
using var stream = File.OpenRead("document.pdf");
var file = new File("document.pdf", "application/pdf", stream);
var uploadedFile = await fileManager.AddFileAsync(file);

Console.WriteLine($"File uploaded: {uploadedFile.Uri}");
```

## Usage Examples

### Working with Files

```csharp
// Create a file from Base64 string
var file = new File("image.jpg", "image/jpeg", base64String);

// Create a file from stream
using var stream = new MemoryStream(bytes);
var file = new File("data.bin", "application/octet-stream", stream);

// Create a file reference with URI only (no content)
var file = new File("remote.txt", "text/plain", new Uri("https://..."));
```

### Uploading Files

```csharp
// Upload a single file to a specific directory
var uploadedFile = await fileManager.AddFileAsync(file, rootDir: "documents");

// Upload directly from a stream with explicit path
await using var stream = File.OpenRead("report.pdf");
var uploadedFile = await fileManager.AddFileAsync(
    stream,
    filePath: "reports/2026/report.pdf",
    contentType: "application/pdf"
);

// Batch upload multiple files in parallel
var files = new[] { file1, file2, file3 };
var uploadedFiles = await fileManager.AddFilesAsync(files);
```

### Retrieving Files

```csharp
// Get file metadata only (no download)
var file = await fileManager.GetFileAsync("document.pdf", download: false);
Console.WriteLine($"File URI: {file.Uri}");

// Download file content
var file = await fileManager.GetFileAsync("document.pdf", download: true);
await using var stream = file.Content;
// Process the stream...

// List all files with pagination
var files = await fileManager.GetFilesAsync(pageSize: 100);
foreach (var file in files)
{
    Console.WriteLine($"{file.Name} - {file.Type}");
}
```

### Managing Files

```csharp
// Check if a file exists
bool exists = await fileManager.FileExistsAsync("document.pdf");

// Remove a file
await fileManager.RemoveFileAsync("document.pdf");
```

### Working with Paths and Folders

A repository has no real folders: a folder is the prefix its files share. The operations below work on a path or on a prefix, and stay inside the repository.

```csharp
// List what is under a prefix, without downloading anything
await foreach (FileDescriptor file in fileManager.ListFilesAsync("invoices/2026/"))
    Console.WriteLine($"{file.Path} ({file.Size} bytes, {file.ContentType}, {file.LastModified})");

// Write a file straight into a stream — a response body, a zip entry — instead of holding it in memory
bool found = await fileManager.DownloadFileAsync("invoices/2026/january.pdf", destination);

// Copy, and move — which is also how a file is renamed
await fileManager.CopyFileAsync("invoices/2026/january.pdf", "archive/january.pdf");
await fileManager.MoveFileAsync("invoices/2026/january.pdf", "invoices/2026/01.pdf");

// Rename a folder: every file under the prefix moves, keeping the rest of its path
int moved = await fileManager.MoveFilesAsync("drafts/", "final/");

// Remove a folder
int removed = await fileManager.RemoveFilesAsync("drafts/");
```

Moving is copying and then removing the original, one file at a time. If a move stops midway the file exists at both paths, never at none, and running `MoveFilesAsync` again finishes the job. `CopyFileAsync` and `MoveFileAsync` replace what is at the target and throw `FileNotFoundException` when the source does not exist. `RemoveFilesAsync` refuses an empty prefix, so a repository is never emptied by accident.

### Generating Signed URIs

```csharp
// Generate a read-only signed URI for the entire container (valid for 24 hours)
var containerSignedUri = fileManager.GetRepositorySignedUri(AccessPermissions.Read);

// Generate a signed URI for a specific file with custom expiration
var fileSignedUri = fileManager.GetFileSignedUri(
    filePath: "documents/report.pdf",
    validFor: TimeSpan.FromHours(1),
    permissions: AccessPermissions.Read
);

// Generate signed token with custom permissions
var signedToken = Security.GetAccountSignedToken(
    accountName: "myaccount",
    accountKey: "key",
    permissions: AccessPermissions.Read | AccessPermissions.List
);
```

## Target Frameworks

- .NET 8.0
- .NET 9.0
- .NET 10.0

All libraries use C# 12 features and enable nullable reference types for improved null safety.

## Building from Source

This solution uses the modern `.slnx` (XML-based) solution format.

### Prerequisites

- .NET 10 SDK or later
- Visual Studio 2022 17.0+ or Visual Studio Code with C# extension

### Build Commands

```bash
# Clone the repository
git clone https://github.com/nuvtools/nuvtools-storage.git
cd nuvtools-storage

# Build the solution
dotnet build NuvTools.Storage.slnx

# Run tests
dotnet test NuvTools.Storage.slnx

# Create NuGet packages
dotnet pack NuvTools.Storage.slnx -c Release
```

The solution structure:
```
nuvtools-storage/
├── src/
│   ├── NuvTools.Storage/           # Core library
│   └── NuvTools.Storage.Azure/     # Azure implementation
├── tests/
│   └── NuvTools.Storage.Azure.Test/ # NUnit tests
└── NuvTools.Storage.slnx           # Solution file
```

## Releasing

Publishing to nuget.org is irreversible per version, so the workflow makes it the last and smallest
step.

**One workflow per library.** Each package has its own, so a run and an approval each concern
exactly one package. The steps themselves are shared, in the reusable `package-publish.yml`.

| Package | Workflow | Release tag |
|---|---|---|
| `NuvTools.Storage` | `storage-publish.yml` | `storage-vX.Y.Z` |
| `NuvTools.Storage.Azure` | `azure-publish.yml` | `azure-vX.Y.Z` |

1. Set the package's `<Version>` in its `.csproj` and merge it. That is the version that gets
   published; the workflow reads it and never computes one.
2. Run the library's workflow by hand (*Actions → Storage Publish → Run workflow*) with *push*
   checked.
3. The workflow builds the solution with `-warnaserror`, runs the tests, takes **that package only**
   from the build output, verifies it carries its README, licence, icon, symbols and every target
   framework, and then waits for approval on the `nuget` GitHub Environment before pushing.
4. **After the push succeeds, the workflow tags the commit** with the tag in the table above. The tag is a
   record of what is on nuget.org, never what starts a release, so it cannot exist for a version
   that did not get there. A run whose tag already exists stops before building: the version has
   been released and `<Version>` needs a bump.

**`NuvTools.Storage` goes first.** `NuvTools.Storage.Azure` references it as a project, so its package
depends on whatever `<Version>` `NuvTools.Storage` has in that commit. The workflow checks that
version is on nuget.org and refuses to push a package whose dependency is not there yet.

That check knows a version exists, not what is in it. **When `IFileManager` gains a member, bump
`NuvTools.Storage` and publish it, then bump and publish `NuvTools.Storage.Azure`** — an Azure package
published against an older `NuvTools.Storage` would be paired, by whoever installs both, with a
contract it does not implement.

**Versions published by hand are refused.** Releases made before these workflows exist on nuget.org
with no tag. Pushing one again would skip the duplicate and then tag the current commit as a
release it is not, so a version that is already on nuget.org stops the run. The exception is a
re-run of the same run, which is how a push that succeeded and a tag that failed gets finished.

**No API key is stored.** The push uses nuget.org's trusted publishing: the job presents a GitHub
OIDC token, nuget.org checks it against the policy registered for this repository and answers with a
key that lives for one hour. The repository needs one secret, `NUGET_USER` — the nuget.org profile
name the policy belongs to — and the policy itself, registered under *Trusted Publishing* on
nuget.org with the owner `nuvtools`, the repository `nuvtools-storage`, the workflow file and the
environment `nuget`.

Run a library's workflow by hand with *push* unchecked for a dry run that stops after the
verification.

## Documentation

All public APIs include comprehensive XML documentation comments. IntelliSense in Visual Studio, Visual Studio Code, and Rider will display:

- Detailed descriptions of all types, methods, and properties
- Parameter information with expected values
- Return value descriptions
- Exception documentation
- Usage examples and best practices

XML documentation files are included in the NuGet packages for seamless integration.

## License

Licensed under the [MIT License](LICENSE).

Copyright © 2026 Nuv Tools