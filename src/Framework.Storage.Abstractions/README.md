# VisionaryCoder.Framework.Storage.Abstractions

Storage abstractions and contracts for VisionaryCoder Framework.

## Overview

This package provides the core abstractions for file-based storage operations following Microsoft I/O patterns. It defines technology-agnostic interfaces that can be implemented by various storage providers (local file system, Azure Blob, FTP, SFTP, etc.).

## Key Interfaces

- **IStorageProvider** - Comprehensive contract for file and directory operations
- **StorageProviderBase** - Optional base class with common logging patterns
- **StorageException** - Custom exception for storage-specific errors

## Architecture

This package follows the Volatility-Based Decomposition (VBD) Accessor pattern, providing stable contracts that isolate higher layers from storage implementation details.

## Usage

Reference this package when:
- Creating new storage provider implementations
- Writing code that depends on storage abstractions
- Building Manager or Engine components that need storage access

## Dependencies

- Framework.Abstractions (base framework abstractions)

## Related Packages

- VisionaryCoder.Framework.Storage - Concrete storage implementations

## License

MIT
