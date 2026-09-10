# Secure Storage Services

A comprehensive storage abstraction library that provides unified access to local and remote storage systems with integrated secret management for secure credential handling.

## Overview

The VisionaryCoder Framework Storage Services provide:

- **Unified Interface**: Single `IFileSystem` interface for all file operations
- **Multiple Implementations**: Local file system, FTP, and secure FTP with secret management
- **Secret Integration**: Seamless integration with Azure Key Vault and other secret providers
- **Performance**: Credential caching, connection pooling, and async operations
- **Testability**: Fully mockable interfaces for unit testing
- **Enterprise Ready**: Comprehensive logging, error handling, and monitoring

## Key Features

### 🔒 Secure Credential Management

- Integration with `ISecretProvider` for secure credential retrieval
- Support for Azure Key Vault, HashiCorp Vault, and custom secret providers
- Configurable credential caching with automatic expiration
- Secret reference format: `"secret:secret-name"`

### 🌐 Remote File System Support

- FTP and FTPS (SSL/TLS) support
- Configurable connection parameters (passive mode, keep-alive, timeouts)
- Automatic SSL certificate validation
- Connection pooling and reuse

### ⚡ Performance Optimized

- Async/await throughout for non-blocking operations
- Memory caching for credentials with configurable TTL
- Efficient buffer management for file transfers
- Connection keep-alive for repeated operations

### 🧪 Testing & Mocking

- Complete interface abstraction for easy mocking
- Comprehensive unit test examples with Moq
- Integration test patterns for validation
- Factory pattern for multiple file system configurations

## Quick Start

### 1. Basic Local File System

```csharp
services.AddLocalFileSystem();

// Usage
var fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
await fileSystem.WriteAllTextAsync("/path/to/file.txt", "Hello World!");
var content = await fileSystem.ReadAllTextAsync("/path/to/file.txt");
```

### 2. Regular FTP File System

```csharp
services.AddFtpFileSystem(options =>
{
    options.Host = "ftp.example.com";
    options.Username = "myuser";
    options.Password = "mypassword";
    options.UseSsl = true;
});
```

### 3. Secure FTP with Azure Key Vault

```csharp
// Register Key Vault secret provider
services.AddKeyVaultSecretProvider(options =>
{
    options.VaultUri = "https://mykeyvault.vault.azure.net/";
});

// Register secure FTP with secret references
services.AddSecureFtpFileSystem(options =>
{
    options.Host = "secure-ftp.example.com";
    options.Username = "myuser";
    options.Password = "secret:ftp-server-password"; // Retrieved from Key Vault
    options.UseSsl = true;
    options.CacheCredentials = true;
});
```

### 4. Multiple File Systems with Factory

```csharp
services.AddFileSystemFactory()
    .AddLocal("local")
    .AddFtp("regular-ftp", ftpOptions)
    .AddSecureFtp("secure-ftp", secureFtpOptions);

// Usage
var factory = serviceProvider.GetRequiredService<IFileSystemFactory>();
var localFs = factory.Create("local");
var secureFtp = factory.Create("secure-ftp");
```

## Configuration

### FTP Options

```csharp
public class FtpFileSystemOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 21;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = false;
    public bool UsePassive { get; set; } = true;
    public bool KeepAlive { get; set; } = false;
    public int TimeoutMilliseconds { get; set; } = 30000;
    public int BufferSize { get; set; } = 4096;
}
```

### Secure FTP Options

```csharp
public class SecureFtpFileSystemOptions : FtpFileSystemOptions
{
    public bool CacheCredentials { get; set; } = true;
    public TimeSpan CredentialCacheDuration { get; set; } = TimeSpan.FromMinutes(30);
    
    // Secret detection properties
    public bool IsUsernameSecret => Username.StartsWith("secret:", StringComparison.OrdinalIgnoreCase);
    public bool IsPasswordSecret => Password.StartsWith("secret:", StringComparison.OrdinalIgnoreCase);
}
```

### Configuration from appsettings.json

```json
{
  "SecureFtp": {
    "Host": "ftp.example.com",
    "Port": 990,
    "Username": "myuser",
    "Password": "secret:my-ftp-password",
    "UseSsl": true,
    "UsePassive": true,
    "CacheCredentials": true,
    "CredentialCacheDurationMinutes": 30,
    "TimeoutSeconds": 30
  },
  "KeyVault": {
    "VaultUri": "https://mykeyvault.vault.azure.net/"
  }
}
```

## Interface Reference

### IFileSystem

The main interface providing unified file system operations:

```csharp
public interface IFileSystem
{
    // File Operations
    bool FileExists(string path);
    Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default);
    string ReadAllText(string path);
    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);
    void WriteAllText(string path, string content);
    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);
    byte[] ReadAllBytes(string path);
    Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default);
    void WriteAllBytes(string path, byte[] bytes);
    Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);
    void DeleteFile(string path);
    Task DeleteFileAsync(string path, CancellationToken cancellationToken = default);

    // Directory Operations
    bool DirectoryExists(string path);
    DirectoryInfo CreateDirectory(string path);
    Task<DirectoryInfo> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default);
    void DeleteDirectory(string path, bool recursive = true);
    Task DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default);
    string[] GetFiles(string path, string searchPattern = "*");
    string[] GetDirectories(string path, string searchPattern = "*");
    IAsyncEnumerable<string> EnumerateFilesAsync(string path, string searchPattern = "*", CancellationToken cancellationToken = default);

    // Path Utilities
    string GetFullPath(string path);
    string? GetDirectoryName(string path);
    string GetFileName(string path);
}
```

## Secret Management Integration

### Secret Reference Format

Use the format `"secret:secret-name"` to reference secrets stored in your secret provider:

```csharp
options.Username = "myuser";                    // Direct value
options.Password = "secret:ftp-password";       // Secret reference
options.Password = "secret:prod-ftp-creds";     // Environment-specific secret
```

### Supported Secret Providers

- **Azure Key Vault**: `services.AddKeyVaultSecretProvider()`
- **HashiCorp Vault**: Custom implementation using `ISecretProvider`
- **AWS Secrets Manager**: Custom implementation using `ISecretProvider`
- **Custom Provider**: Implement `ISecretProvider` interface

### Credential Caching

```csharp
options.CacheCredentials = true;
options.CredentialCacheDuration = TimeSpan.FromMinutes(30);

// Manual cache management
if (fileSystem is SecureFtpFileSystemService secureFtp)
{
    secureFtp.ClearCredentialCache(); // Force fresh credential retrieval
}
```

## Error Handling

### Common Exceptions

- `ArgumentException`: Invalid parameters or configuration
- `InvalidOperationException`: Secret not found or configuration errors
- `UnauthorizedAccessException`: Authentication failures
- `TimeoutException`: Connection or operation timeouts
- `WebException`: FTP protocol errors

### Retry Policies

Implement retry policies using libraries like Polly:

```csharp
var retryPolicy = Policy
    .Handle<WebException>()
    .Or<TimeoutException>()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

await retryPolicy.ExecuteAsync(async () =>
{
    await fileSystem.WriteAllTextAsync("/remote/file.txt", content);
});
```

## Testing

### Unit Testing with Moq

```csharp
[Test]
public async Task ProcessFiles_ShouldHandleMultipleFiles()
{
    // Arrange
    var mockFileSystem = new Mock<IFileSystem>();
    mockFileSystem.Setup(fs => fs.GetFilesAsync("/test", "*.txt", It.IsAny<CancellationToken>()))
              .ReturnsAsync(new[] { "/test/file1.txt", "/test/file2.txt" });

    mockFileSystem.Setup(fs => fs.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync("test content");

    var processor = new FileProcessorService(mockFileSystem.Object);

    // Act
    var result = await processor.ProcessFilesAsync("/test");

    // Assert
    Assert.IsTrue(result.Success);
    mockFileSystem.Verify(fs => fs.GetFilesAsync("/test", "*.txt", It.IsAny<CancellationToken>()), Times.Once);
}
```

### Integration Testing

```csharp
[Test]
public async Task SecureFtp_ShouldConnectWithValidCredentials()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddMemoryCache();
    
    // Use test secret provider
    services.AddSingleton<ISecretProvider>(new TestSecretProvider(new Dictionary<string, string>
    {
        ["test-ftp-password"] = "actual-password"
    }));

    services.AddSecureFtpFileSystem(options =>
    {
        options.Host = "test-ftp.example.com";
        options.Username = "testuser";
        options.Password = "secret:test-ftp-password";
    });

    var serviceProvider = services.BuildServiceProvider();
    var fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    // Act & Assert
    var exists = await fileSystem.DirectoryExistsAsync("/");
    Assert.IsTrue(exists);
}
```

## Performance Considerations

### Connection Management

- Use `KeepAlive = true` for multiple operations
- Configure appropriate timeouts based on network conditions
- Enable credential caching for repeated operations

### File Transfer Optimization

- Adjust `BufferSize` based on file sizes (larger buffers for large files)
- Use async methods to avoid blocking threads
- Consider parallel operations for multiple files

### Memory Management

- Use streaming for large files instead of loading entire content
- Configure credential cache duration to balance security and performance
- Monitor memory usage in long-running applications

## Security Best Practices

### Credential Management

- ✅ Store secrets in secure secret stores (Key Vault, HashiCorp Vault)
- ✅ Use secret references instead of plain text passwords
- ✅ Configure appropriate cache durations (shorter for sensitive environments)
- ❌ Never store credentials in configuration files or code

### Network Security

- ✅ Always use SSL/TLS for remote connections (`UseSsl = true`)
- ✅ Validate SSL certificates in production
- ✅ Use strong, unique passwords for FTP accounts
- ❌ Don't use plain FTP (port 21) for sensitive data

### Operational Security

- ✅ Implement proper logging without exposing credentials
- ✅ Monitor for authentication failures and unusual activity
- ✅ Rotate credentials regularly
- ✅ Use principle of least privilege for FTP account permissions

## Migration Guide

### From Direct FTP Libraries

```csharp
// Old approach
var ftpClient = new FtpClient("ftp.example.com", "user", "password");
await ftpClient.ConnectAsync();
await ftpClient.UploadAsync(localFile, remoteFile);

// New approach
services.AddSecureFtpFileSystem(options => { /* configure */ });
var fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
var content = await File.ReadAllTextAsync(localFile);
await fileSystem.WriteAllTextAsync(remoteFile, content);
```

### From System.IO

```csharp
// Old approach
if (File.Exists(path))
{
    var content = File.ReadAllText(path);
    // process content
}

// New approach - same API, works with any file system
if (await fileSystem.FileExistsAsync(path))
{
    var content = await fileSystem.ReadAllTextAsync(path);
    // process content
}
```

## Troubleshooting

### Common Issues

#### Connection Timeouts

- Increase `TimeoutMilliseconds` value
- Check network connectivity and firewall settings
- Verify FTP server is accessible

#### Authentication Failures

- Verify secret names and values in secret store
- Check credential cache settings
- Ensure FTP user has necessary permissions

#### SSL/TLS Issues

- Verify server supports FTPS
- Check certificate validity
- Consider using implicit vs explicit SSL modes

### Debugging

Enable detailed logging:

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Trace); // Maximum detail
});
```

Monitor secret provider calls:

```csharp
// Add custom logging to track secret retrieval
services.Decorate<ISecretProvider>((provider, services) => 
    new LoggingSecretProviderDecorator(provider, services.GetService<ILogger>()));
```

## Dependencies

- `Microsoft.Extensions.DependencyInjection` - Dependency injection
- `Microsoft.Extensions.Caching.Memory` - Credential caching
- `Microsoft.Extensions.Logging` - Comprehensive logging
- `VisionaryCoder.Framework.Abstractions` - Base service classes
- `VisionaryCoder.Framework.Secrets.Abstractions` - Secret management
- `System.Net.FtpClient` (built-in .NET) - FTP protocol support

## License

This library is part of the VisionaryCoder Framework and follows the same licensing terms.
