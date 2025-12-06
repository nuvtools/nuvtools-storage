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

- **`IFileManager`** - Interface for managing file storage operations (add, get, remove, list)
- **`IFile`** - Represents a file with metadata and content in multiple formats (URI, Stream, Base64)
- **`File`** - Concrete implementation supporting three initialization modes
- **`AccessPermissions`** - Enum for fine-grained access control (Read, Write, Delete, List, etc.)
- **Extension methods** for seamless conversion between Stream and Base64 formats

#### NuvTools.Storage.Azure

Azure Blob Storage implementation:

- **`AzureFileManager`** - Full `IFileManager` implementation for Azure Blob Storage
- **`Security`** - Helper class for generating SAS (Shared Access Signature) tokens
- Automatic conversion between Azure SDK types and NuvTools abstractions
- Support for paginated file listing
- Parallel batch file uploads
- Built on Azure.Storage.Blobs SDK

## Features

✅ **Provider Pattern** - Easy to implement custom storage providers
✅ **Async/Await** - Fully asynchronous API with `CancellationToken` support
✅ **Multiple Content Formats** - Work with files as URIs, Streams, or Base64 strings
✅ **Azure Integration** - Production-ready Azure Blob Storage implementation
✅ **SAS Token Generation** - Built-in support for Azure Shared Access Signatures
✅ **Batch Operations** - Upload multiple files in parallel with `Task.WhenAll`
✅ **Comprehensive Documentation** - Full XML documentation for IntelliSense
✅ **Strong-Named Assemblies** - Signed for use in strong-named projects
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

### Generating SAS Tokens

```csharp
// Generate a read-only SAS URI valid for 24 hours
var sasUri = fileManager.GetAccessRepositoryUri(AccessPermissions.Read);

// Generate SAS token with custom permissions
var sasToken = Security.GetAccessAccountToken(
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

Copyright © 2025 Nuv Tools