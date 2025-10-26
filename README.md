# Nuv Tools Storage Libraries

A suite of .NET libraries for abstracting and simplifying storage operations, with a focus on extensibility and support for Microsoft Azure Storage.

## Library Overview

### NuvTools.Storage

- **Description:**  
  Core library providing abstractions and helpers for general storage manipulation.

- **Key Features:**  
  - Interfaces for file and storage management (`IFileManager`, `IFile`)
  - Base classes for implementing custom storage solutions
  - `AccessPermissions` enum for fine-grained access control
  - Utilities for reading, writing, creating, updating, deleting, and listing resources

### NuvTools.Storage.Azure

- **Description:**  
  Extension library with helpers for Microsoft Azure Storage services.

- **Key Features:**  
  - Azure-specific file and container management
  - Extensions for working with Azure Blob, File, and other storage types
  - Utilities for uploading, downloading, and managing files in Azure
  - Support for access policies and metadata

## Getting Started

1. **Install via NuGet:**
   - `NuvTools.Storage`
   - `NuvTools.Storage.Azure`

2. **Reference the desired library in your project and use the provided APIs for storage operations.

## License

Licensed under the MIT License. See [LICENSE](LICENSE) for details.