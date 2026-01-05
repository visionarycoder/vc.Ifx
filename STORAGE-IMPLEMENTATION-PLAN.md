# Storage Implementation Plan
**Date:** January 4, 2026  
**Version:** 1.0.0  
**Status:** Awaiting Approval

## Executive Summary

This plan reorganizes Framework.Storage to align with Microsoft best practices for file-based storage implementations. The structure separates concerns by storage type (Azure Blob, Local File System, FTP/SFTP) while maintaining the existing `IStorageProvider` interface contract.

---

## Current State Analysis

### Existing Structure
```
Framework.Storage/
├── Azure/
│   └── Blob/
│       ├── AzureBlobStorageProvider.cs    ✅ Implemented
│       └── AzureBlobStorageOptions.cs     ✅ Implemented
├── Framework.Storage.csproj               ✅ Has FluentFTP package reference
└── README.md                              ⚠️  Minimal content
```

### Dependencies Already Configured
- ✅ `Azure.Storage.Blobs` - Azure Blob Storage SDK
- ✅ `Azure.Identity` - Managed Identity support
- ✅ `FluentFTP` - FTP/SFTP client library
- ✅ `AspNetCore.HealthChecks.Azure.Storage.Blobs` - Health checks

### Interface Contract
`IStorageProvider` defines 20+ methods covering:
- **File Operations:** FileExists, ReadAllText/Bytes, WriteAllText/Bytes, DeleteFile
- **Directory Operations:** DirectoryExists, CreateDirectory, DeleteDirectory, GetFiles, GetDirectories
- **Async Support:** All operations have async equivalents
- **Path Utilities:** GetFullPath, GetDirectoryName, GetFileName

---

## Proposed Structure

### Target Organization (Microsoft Best Practices)

```
Framework.Storage/
├── Abstractions/                          📦 NEW
│   ├── IStorageProvider.cs               🔄 MOVE from root
│   ├── StorageProviderBase.cs            📦 NEW (optional base class)
│   └── StorageException.cs               📦 NEW (custom exception)
│
├── Azure/                                 ✅ EXISTS
│   └── Blob/
│       ├── AzureBlobStorageProvider.cs   ✅ EXISTS (no changes)
│       ├── AzureBlobStorageOptions.cs    ✅ EXISTS (no changes)
│       ├── AzureBlobHealthCheck.cs       📦 NEW (health check implementation)
│       └── AzureBlobServiceCollectionExtensions.cs  📦 NEW (DI registration)
│
├── Local/                                 📦 NEW SECTION
│   ├── LocalFileStorageProvider.cs       📦 NEW
│   ├── LocalFileStorageOptions.cs        📦 NEW
│   ├── LocalFileHealthCheck.cs           📦 NEW
│   └── LocalFileServiceCollectionExtensions.cs  📦 NEW
│
├── Ftp/                                   📦 NEW SECTION
│   ├── FtpStorageProvider.cs             📦 NEW
│   ├── FtpStorageOptions.cs              📦 NEW
│   ├── FtpHealthCheck.cs                 📦 NEW
│   └── FtpServiceCollectionExtensions.cs 📦 NEW
│
├── Sftp/                                  📦 NEW SECTION (Optional - Phase 2)
│   ├── SftpStorageProvider.cs            📦 NEW
│   ├── SftpStorageOptions.cs             📦 NEW
│   ├── SftpHealthCheck.cs                📦 NEW
│   └── SftpServiceCollectionExtensions.cs  📦 NEW
│
├── Framework.Storage.csproj              🔄 UPDATE (add SSH.NET for SFTP)
└── README.md                              🔄 UPDATE (comprehensive documentation)
```

---

## Implementation Phases

### Phase 1: Abstractions & Refactoring (2-3 hours)
**Goal:** Establish clean separation of concerns and shared infrastructure

#### Task 1.1: Create Abstractions Layer
- [ ] **Create** `Abstractions/IStorageProvider.cs` (move from current location)
- [ ] **Create** `Abstractions/StorageProviderBase.cs` with common logging patterns
- [ ] **Create** `Abstractions/StorageException.cs` for consistent error handling
- [ ] **Update** all existing provider namespaces

**Deliverable:** Clean contract separation, reusable base class

#### Task 1.2: Enhance Azure Blob Implementation
- [ ] **Create** `Azure/Blob/AzureBlobHealthCheck.cs` (implements `IHealthCheck`)
- [ ] **Create** `Azure/Blob/AzureBlobServiceCollectionExtensions.cs`
  - `AddAzureBlobStorage(this IServiceCollection, Action<AzureBlobStorageOptions>)`
  - `AddAzureBlobStorageHealthCheck(this IHealthCheckBuilder)`
- [ ] **Update** `AzureBlobStorageProvider.cs` to inherit from `StorageProviderBase`

**Deliverable:** Production-ready Azure Blob implementation with health checks

---

### Phase 2: Local File System Implementation (3-4 hours)
**Goal:** Implement local/file storage following Microsoft System.IO patterns

#### Task 2.1: Core Local Implementation
- [ ] **Create** `Local/LocalFileStorageProvider.cs`
  - Implement `IStorageProvider` using `System.IO.File` and `System.IO.Directory`
  - Handle Windows/Linux path separators correctly
  - Implement proper error handling for file locks, permissions
  - Support both absolute and relative paths
  - Use `FileStream` for efficient large file operations

- [ ] **Create** `Local/LocalFileStorageOptions.cs`
  ```csharp
  public sealed class LocalFileStorageOptions
  {
      public required string RootDirectory { get; init; }
      public bool CreateRootIfNotExists { get; init; } = true;
      public bool RestrictToRootDirectory { get; init; } = true; // Security: prevent path traversal
      public int BufferSize { get; init; } = 81920; // 80KB buffer for large files
      public FileOptions FileOptions { get; init; } = FileOptions.Asynchronous;
  }
  ```

#### Task 2.2: Local File Extensions & Health
- [ ] **Create** `Local/LocalFileHealthCheck.cs`
  - Check if root directory exists and is accessible
  - Verify read/write permissions
  - Optional: Check disk space

- [ ] **Create** `Local/LocalFileServiceCollectionExtensions.cs`
  - `AddLocalFileStorage(this IServiceCollection, Action<LocalFileStorageOptions>)`
  - `AddLocalFileStorageHealthCheck(this IHealthCheckBuilder)`

**Deliverable:** Full-featured local file storage with security constraints

---

### Phase 3: FTP Implementation (4-5 hours)
**Goal:** Implement FTP/FTPS storage using FluentFTP library

#### Task 3.1: Core FTP Implementation
- [ ] **Create** `Ftp/FtpStorageProvider.cs`
  - Implement `IStorageProvider` using `FluentFTP.AsyncFtpClient`
  - Support FTP, FTPS (FTP over TLS), FTPE (explicit TLS)
  - Implement connection pooling for performance
  - Handle disconnect/reconnect scenarios
  - Support passive and active modes
  - Implement directory caching for `DirectoryExists` optimization

- [ ] **Create** `Ftp/FtpStorageOptions.cs`
  ```csharp
  public sealed class FtpStorageOptions
  {
      public required string Host { get; init; }
      public int Port { get; init; } = 21;
      public required string UserName { get; init; }
      public required string Password { get; init; } // Consider SecureString
      
      public FtpEncryptionMode EncryptionMode { get; init; } = FtpEncryptionMode.Auto;
      public FtpDataConnectionType DataConnectionType { get; init; } = FtpDataConnectionType.AutoPassive;
      
      public string? RemoteRootDirectory { get; init; } = "/";
      public bool ValidateAnyCertificate { get; init; } = false; // Security warning
      public int ConnectionTimeout { get; init; } = 15000; // 15 seconds
      public int ReadTimeout { get; init; } = 15000;
      public int WriteTimeout { get; init; } = 15000;
      
      public bool EnableConnectionPooling { get; init; } = true;
      public int MaxPoolSize { get; init; } = 10;
  }
  ```

#### Task 3.2: FTP Extensions & Health
- [ ] **Create** `Ftp/FtpHealthCheck.cs`
  - Connect to FTP server
  - Verify credentials
  - Check remote directory accessibility
  - Test read/write permissions (create/delete temp file)

- [ ] **Create** `Ftp/FtpServiceCollectionExtensions.cs`
  - `AddFtpStorage(this IServiceCollection, Action<FtpStorageOptions>)`
  - `AddFtpStorageHealthCheck(this IHealthCheckBuilder)`

**Deliverable:** Production-ready FTP storage with connection pooling

---

### Phase 4: SFTP Implementation (Optional - Phase 2, 4-5 hours)
**Goal:** Implement SFTP (SSH File Transfer Protocol) using SSH.NET

#### Task 4.1: Add SFTP Package
- [ ] **Update** `Framework.Storage.csproj`
  ```xml
  <PackageReference Include="SSH.NET" />
  ```

#### Task 4.2: Core SFTP Implementation
- [ ] **Create** `Sftp/SftpStorageProvider.cs`
  - Implement `IStorageProvider` using `Renci.SshNet.SftpClient`
  - Support SSH key-based authentication
  - Support password authentication
  - Implement connection keep-alive
  - Handle SSH connection timeouts
  - Implement proper disposal of SSH connections

- [ ] **Create** `Sftp/SftpStorageOptions.cs`
  ```csharp
  public sealed class SftpStorageOptions
  {
      public required string Host { get; init; }
      public int Port { get; init; } = 22;
      public required string UserName { get; init; }
      
      // Authentication options (choose one)
      public string? Password { get; init; }
      public string? PrivateKeyPath { get; init; }
      public string? PrivateKeyPassphrase { get; init; }
      
      public string? RemoteRootDirectory { get; init; } = "/";
      public int ConnectionTimeout { get; init; } = 30000; // 30 seconds
      public int OperationTimeout { get; init; } = 30000;
      public int BufferSize { get; init; } = 32768; // 32KB for SFTP
      public bool KeepAliveEnabled { get; init; } = true;
      public TimeSpan KeepAliveInterval { get; init; } = TimeSpan.FromSeconds(30);
  }
  ```

#### Task 4.3: SFTP Extensions & Health
- [ ] **Create** `Sftp/SftpHealthCheck.cs`
- [ ] **Create** `Sftp/SftpServiceCollectionExtensions.cs`

**Deliverable:** Secure SFTP storage with SSH key support

---

### Phase 5: Testing & Documentation (4-5 hours)
**Goal:** Ensure 100% test coverage and comprehensive documentation

#### Task 5.1: Unit Tests
- [ ] **Create** `tests/Framework.Storage.Tests/Abstractions/StorageProviderBaseTests.cs`
- [ ] **Create** `tests/Framework.Storage.Tests/Local/LocalFileStorageProviderTests.cs`
  - Test Windows and Linux path handling
  - Test path traversal prevention
  - Test file locking scenarios
  - Test permission errors
- [ ] **Create** `tests/Framework.Storage.Tests/Ftp/FtpStorageProviderTests.cs`
  - Use mock FTP server for testing
  - Test connection failures and retries
  - Test passive/active mode switching
- [ ] **Create** `tests/Framework.Storage.Tests/Sftp/SftpStorageProviderTests.cs` (if implemented)

#### Task 5.2: Integration Tests
- [ ] **Create** integration tests for each provider
- [ ] Test against real Azure Blob Storage (dev account)
- [ ] Test against FTP server (Docker container)
- [ ] Test local file system on Windows and Linux

#### Task 5.3: Documentation
- [ ] **Update** `README.md` with comprehensive guide:
  - Overview of each storage provider
  - Configuration examples for each provider
  - Security best practices
  - Performance considerations
  - Migration guide from old structure
  - Troubleshooting section

**Deliverable:** 100% test coverage, production-ready documentation

---

## Microsoft Best Practices Alignment

### Naming Conventions ✅
- **Classes:** `PascalCase` (e.g., `LocalFileStorageProvider`)
- **Interfaces:** `IPascalCase` (e.g., `IStorageProvider`)
- **Folders:** Match namespace hierarchy (`Local/`, `Ftp/`, `Sftp/`)
- **Files:** One class per file, file name matches class name

### Security Best Practices ✅
- **Secret Management:** Options classes support configuration providers (Azure Key Vault)
- **Path Traversal Prevention:** Local provider restricts to root directory
- **TLS/SSL:** FTP supports FTPS, SFTP is SSH-based
- **Certificate Validation:** FTP/SFTP validate server certificates by default

### Performance Patterns ✅
- **Async/Await:** All I/O operations are async
- **Connection Pooling:** FTP implements connection pooling
- **Buffering:** Configurable buffer sizes for large files
- **Streaming:** Use `FileStream` for efficient large file operations

### Architecture Patterns ✅
- **Abstraction Layer:** `IStorageProvider` separates contract from implementation
- **Options Pattern:** All providers use typed options classes
- **Dependency Injection:** Extension methods for clean DI registration
- **Health Checks:** All providers implement `IHealthCheck`

### VBD Alignment ✅
- **Accessor Pattern:** All providers are Accessors (data access layer)
- **Component Isolation:** Each storage type in separate folder
- **Contract-Based:** All implementations adhere to `IStorageProvider` contract
- **No Cross-Dependencies:** Storage providers don't depend on each other

---

## File Estimates

### Code Files to Create
| Phase | Files | Estimated Lines | Time |
|-------|-------|----------------|------|
| Phase 1 | 5 files | ~600 lines | 2-3 hours |
| Phase 2 | 4 files | ~800 lines | 3-4 hours |
| Phase 3 | 4 files | ~1000 lines | 4-5 hours |
| Phase 4 | 4 files | ~900 lines | 4-5 hours |
| Phase 5 | 10+ test files | ~2000 lines | 4-5 hours |

### Total Effort
- **Minimum (Phases 1-3 + Tests):** 13-17 hours
- **Full Implementation (All Phases):** 18-23 hours

---

## Configuration Examples

### Azure Blob Storage
```csharp
services.AddAzureBlobStorage(options =>
{
    options.ConnectionString = configuration["AzureStorage:ConnectionString"];
    options.ContainerName = "my-container";
    options.CreateContainerIfNotExists = true;
});
```

### Local File Storage
```csharp
services.AddLocalFileStorage(options =>
{
    options.RootDirectory = Path.Combine(environment.ContentRootPath, "storage");
    options.CreateRootIfNotExists = true;
    options.RestrictToRootDirectory = true;
});
```

### FTP Storage
```csharp
services.AddFtpStorage(options =>
{
    options.Host = configuration["Ftp:Host"];
    options.Port = 21;
    options.UserName = configuration["Ftp:UserName"];
    options.Password = configuration["Ftp:Password"];
    options.EncryptionMode = FtpEncryptionMode.Explicit;
    options.EnableConnectionPooling = true;
});
```

### SFTP Storage
```csharp
services.AddSftpStorage(options =>
{
    options.Host = configuration["Sftp:Host"];
    options.Port = 22;
    options.UserName = configuration["Sftp:UserName"];
    options.PrivateKeyPath = "/path/to/private/key";
    options.PrivateKeyPassphrase = configuration["Sftp:KeyPassphrase"];
});
```

---

## Risks & Mitigations

### Risk 1: Breaking Changes to Existing Azure Blob Implementation
**Probability:** Low  
**Impact:** Medium  
**Mitigation:** 
- Keep existing `Azure/Blob/` structure intact
- Only add new files, don't modify existing providers
- Maintain backward compatibility with current `AzureBlobStorageProvider`

### Risk 2: FTP Connection Instability
**Probability:** Medium  
**Impact:** Medium  
**Mitigation:**
- Implement robust retry logic with exponential backoff
- Use connection pooling to avoid connection churn
- Implement circuit breaker pattern for failed connections
- Add comprehensive logging for connection diagnostics

### Risk 3: Path Traversal Security Vulnerabilities
**Probability:** Low  
**Impact:** High  
**Mitigation:**
- Use `Path.GetFullPath()` to resolve relative paths
- Validate all paths are within configured root directory
- Block path sequences like `..`, `~`, absolute paths
- Add security unit tests for path traversal attempts

### Risk 4: Performance Degradation with Remote Storage
**Probability:** Medium  
**Impact:** Medium  
**Mitigation:**
- Implement caching for directory listings (FTP/SFTP)
- Use connection pooling (FTP)
- Configure appropriate timeouts
- Add performance metrics and monitoring
- Document performance characteristics per provider

---

## Success Criteria

### Functional Requirements ✅
- [ ] All storage providers implement `IStorageProvider` interface
- [ ] Azure Blob, Local File, FTP providers fully functional
- [ ] SFTP provider optional but recommended
- [ ] Health checks implemented for all providers
- [ ] DI extension methods for all providers

### Quality Requirements ✅
- [ ] 100% unit test coverage for all providers
- [ ] Integration tests for each provider
- [ ] Zero breaking changes to existing code
- [ ] Comprehensive XML documentation
- [ ] README.md with examples and troubleshooting

### Performance Requirements ✅
- [ ] Local file operations: < 50ms for small files (< 1MB)
- [ ] Azure Blob operations: < 200ms for small files
- [ ] FTP operations: < 500ms for small files
- [ ] Connection pooling reduces FTP overhead by 80%

### Security Requirements ✅
- [ ] No hardcoded credentials in source code
- [ ] All options support Azure Key Vault integration
- [ ] TLS/SSL enabled by default for remote storage
- [ ] Path traversal prevention tested and documented
- [ ] Certificate validation enforced (configurable)

---

## Approval & Sign-Off

This plan requires approval before implementation begins.

**Prepared By:** GitHub Copilot  
**Date:** January 4, 2026  
**Status:** 🟡 Awaiting User Approval

---

## Next Steps After Approval

1. **Create Feature Branch:** `feature/storage-provider-separation`
2. **Create TODO List:** Import this plan into `manage_todo_list` tool
3. **Begin Phase 1:** Abstractions & Refactoring
4. **Incremental Commits:** Commit after each phase completion
5. **Pull Request:** Create PR with comprehensive description and testing evidence

---

## Questions for User

Before proceeding, please confirm:

1. **Scope:** Should I implement all phases (1-5) or prioritize specific providers?
2. **SFTP:** Is SFTP (SSH) required, or can it be deferred to Phase 2?
3. **Tests:** Should I migrate existing tests from `Framework.Tests` to `Framework.Storage.Tests`?
4. **Breaking Changes:** Is it acceptable to move `IStorageProvider` to `Abstractions/` folder?
5. **Timeline:** Is the 18-23 hour estimate acceptable for full implementation?

---

**END OF PLAN**
