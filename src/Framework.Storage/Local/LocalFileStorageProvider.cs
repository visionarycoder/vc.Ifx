using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Storage.Abstractions;
using IoPath = System.IO.Path;

namespace VisionaryCoder.Framework.Storage.Local;

/// <summary>
/// Provides local file system-based storage operations implementation following Microsoft I/O patterns.
/// This service wraps System.IO operations with logging, error handling, security, and async support.
/// </summary>
public sealed class LocalFileStorageProvider : StorageProviderBase<LocalFileStorageProvider>
{
    private readonly LocalFileStorageOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalFileStorageProvider"/> class.
    /// </summary>
    /// <param name="options">The configuration options for local file storage.</param>
    /// <param name="logger">The logger instance.</param>
    public LocalFileStorageProvider(LocalFileStorageOptions options, ILogger<LocalFileStorageProvider> logger)
        : base(logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.options.Validate();

        // Create root directory if configured
        if (options.CreateRootIfNotExists && !Directory.Exists(options.RootDirectory))
        {
            Directory.CreateDirectory(options.RootDirectory);
            Logger.LogInformation("Created root directory '{RootDirectory}'", options.RootDirectory);
        }
    }

    /// <inheritdoc />
    public override bool FileExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = ResolvePath(path);
        return File.Exists(fullPath);
    }

    /// <inheritdoc />
    public override bool FileExists(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return fileInfo.Exists;
    }

    /// <inheritdoc />
    public override string ReadAllText(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = ResolvePath(path);
        return ExecuteWithLogging(() => File.ReadAllText(fullPath), nameof(ReadAllText), path);
    }

    /// <inheritdoc />
    public override async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = ResolvePath(path);
        return await ExecuteWithLoggingAsync(
            async () => await File.ReadAllTextAsync(fullPath, cancellationToken).ConfigureAwait(false),
            nameof(ReadAllTextAsync),
            path).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override byte[] ReadAllBytes(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = ResolvePath(path);
        return ExecuteWithLogging(() => File.ReadAllBytes(fullPath), nameof(ReadAllBytes), path);
    }

    /// <inheritdoc />
    public override async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = ResolvePath(path);
        return await ExecuteWithLoggingAsync(
            async () => await File.ReadAllBytesAsync(fullPath, cancellationToken).ConfigureAwait(false),
            nameof(ReadAllBytesAsync),
            path).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override void WriteAllText(string path, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        string fullPath = ResolvePath(path);
        EnsureDirectoryExists(fullPath);
        ExecuteWithLogging(() => { File.WriteAllText(fullPath, content); return true; }, nameof(WriteAllText), path);
    }

    /// <inheritdoc />
    public override async Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        string fullPath = ResolvePath(path);
        EnsureDirectoryExists(fullPath);
        await ExecuteWithLoggingAsync(
            async () => await File.WriteAllTextAsync(fullPath, content, cancellationToken).ConfigureAwait(false),
            nameof(WriteAllTextAsync),
            path).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override void WriteAllBytes(string path, byte[] bytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(bytes);
        string fullPath = ResolvePath(path);
        EnsureDirectoryExists(fullPath);
        ExecuteWithLogging(() => { File.WriteAllBytes(fullPath, bytes); return true; }, nameof(WriteAllBytes), path);
    }

    /// <inheritdoc />
    public override async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(bytes);
        string fullPath = ResolvePath(path);
        EnsureDirectoryExists(fullPath);
        await ExecuteWithLoggingAsync(
            async () => await File.WriteAllBytesAsync(fullPath, bytes, cancellationToken).ConfigureAwait(false),
            nameof(WriteAllBytesAsync),
            path).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override void DeleteFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = ResolvePath(path);
        if (File.Exists(fullPath))
        {
            ExecuteWithLogging(() => { File.Delete(fullPath); return true; }, nameof(DeleteFile), path);
        }
    }

    /// <inheritdoc />
    public override async Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = ResolvePath(path);
        if (File.Exists(fullPath))
        {
            await ExecuteWithLoggingAsync(
                () => Task.Run(() => File.Delete(fullPath), cancellationToken),
                nameof(DeleteFileAsync),
                path).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public override bool DirectoryExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = ResolvePath(path);
        return Directory.Exists(fullPath);
    }

    /// <inheritdoc />
    public override DirectoryInfo CreateDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = ResolvePath(path);
        return ExecuteWithLogging(() => Directory.CreateDirectory(fullPath), nameof(CreateDirectory), path);
    }

    /// <inheritdoc />
    public override async Task<DirectoryInfo> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = ResolvePath(path);
        return await ExecuteWithLoggingAsync(
            () => Task.Run(() => Directory.CreateDirectory(fullPath), cancellationToken),
            nameof(CreateDirectoryAsync),
            path).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override void DeleteDirectory(string path, bool recursive = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = ResolvePath(path);
        if (Directory.Exists(fullPath))
        {
            ExecuteWithLogging(() => { Directory.Delete(fullPath, recursive); return true; }, nameof(DeleteDirectory), path);
        }
    }

    /// <inheritdoc />
    public override async Task DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = ResolvePath(path);
        if (Directory.Exists(fullPath))
        {
            await ExecuteWithLoggingAsync(
                () => Task.Run(() => Directory.Delete(fullPath, recursive), cancellationToken),
                nameof(DeleteDirectoryAsync),
                path).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public override string[] GetFiles(string path, string searchPattern = "*")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        string fullPath = ResolvePath(path);
        return ExecuteWithLogging(() => Directory.GetFiles(fullPath, searchPattern), nameof(GetFiles), path);
    }

    /// <inheritdoc />
    public override string[] GetDirectories(string path, string searchPattern = "*")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        string fullPath = ResolvePath(path);
        return ExecuteWithLogging(() => Directory.GetDirectories(fullPath, searchPattern), nameof(GetDirectories), path);
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<string> EnumerateFilesAsync(
        string path,
        string searchPattern = "*",
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        string fullPath = ResolvePath(path);

        Logger.LogDebug("Enumerating files async from '{Path}' with pattern '{Pattern}'", path, searchPattern);

        IEnumerable<string> files = Directory.EnumerateFiles(fullPath, searchPattern);
        foreach (string file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return file;
        }
    }

    /// <inheritdoc />
    public override string GetFullPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return IoPath.GetFullPath(ResolvePath(path));
    }

    /// <inheritdoc />
    public override string? GetDirectoryName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return IoPath.GetDirectoryName(ResolvePath(path));
    }

    /// <inheritdoc />
    public override string GetFileName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return IoPath.GetFileName(path);
    }

    /// <summary>
    /// Resolves a relative path to an absolute path within the root directory.
    /// Implements path traversal protection when RestrictToRootDirectory is enabled.
    /// </summary>
    /// <param name="path">The relative or absolute path to resolve.</param>
    /// <returns>The resolved absolute path.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when path attempts to escape the root directory.</exception>
    private string ResolvePath(string path)
    {
        // Convert to absolute path
        string fullPath = IoPath.IsPathRooted(path)
            ? path
            : IoPath.Combine(options.RootDirectory, path);

        // Normalize the path
        fullPath = IoPath.GetFullPath(fullPath);

        // Security: Check for path traversal if restriction is enabled
        if (options.RestrictToRootDirectory)
        {
            string normalizedRoot = IoPath.GetFullPath(options.RootDirectory);
            if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException(
                    $"Access denied: Path '{path}' resolves to '{fullPath}' which is outside the root directory '{normalizedRoot}'");
            }
        }

        return fullPath;
    }

    /// <summary>
    /// Ensures the directory for a file path exists, creating it if necessary.
    /// </summary>
    /// <param name="filePath">The full file path.</param>
    private void EnsureDirectoryExists(string filePath)
    {
        string? directory = IoPath.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Logger.LogTrace("Created directory '{Directory}'", directory);
        }
    }
}
