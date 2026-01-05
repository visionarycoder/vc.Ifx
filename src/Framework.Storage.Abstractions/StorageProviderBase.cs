using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.Storage.Abstractions;

/// <summary>
/// Optional base class for storage providers with common logging patterns and error handling.
/// </summary>
/// <typeparam name="TProvider">The type of the storage provider implementation.</typeparam>
public abstract class StorageProviderBase<TProvider> : IStorageProvider
    where TProvider : class, IStorageProvider
{
    /// <summary>
    /// Gets the logger instance for this storage provider.
    /// </summary>
    protected ILogger<TProvider> Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageProviderBase{TProvider}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    protected StorageProviderBase(ILogger<TProvider> logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public abstract bool FileExists(string path);

    /// <inheritdoc />
    public abstract bool FileExists(FileInfo fileInfo);

    /// <inheritdoc />
    public abstract string ReadAllText(string path);

    /// <inheritdoc />
    public abstract Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract byte[] ReadAllBytes(string path);

    /// <inheritdoc />
    public abstract Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract void WriteAllText(string path, string content);

    /// <inheritdoc />
    public abstract Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract void WriteAllBytes(string path, byte[] bytes);

    /// <inheritdoc />
    public abstract Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract void DeleteFile(string path);

    /// <inheritdoc />
    public abstract Task DeleteFileAsync(string path, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract bool DirectoryExists(string path);

    /// <inheritdoc />
    public abstract DirectoryInfo CreateDirectory(string path);

    /// <inheritdoc />
    public abstract Task<DirectoryInfo> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract void DeleteDirectory(string path, bool recursive = true);

    /// <inheritdoc />
    public abstract Task DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract string[] GetFiles(string path, string searchPattern = "*");

    /// <inheritdoc />
    public abstract string[] GetDirectories(string path, string searchPattern = "*");

    /// <inheritdoc />
    public abstract IAsyncEnumerable<string> EnumerateFilesAsync(string path, string searchPattern = "*", CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract string GetFullPath(string path);

    /// <inheritdoc />
    public abstract string? GetDirectoryName(string path);

    /// <inheritdoc />
    public abstract string GetFileName(string path);

    /// <summary>
    /// Wraps an operation with try-catch and logging for consistent error handling.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="operationName">The name of the operation for logging.</param>
    /// <param name="path">The path being operated on.</param>
    /// <returns>The result of the operation.</returns>
    protected T ExecuteWithLogging<T>(Func<T> operation, string operationName, string path)
    {
        try
        {
            Logger.LogDebug("{OperationName} on '{Path}'", operationName, path);
            T result = operation();
            Logger.LogTrace("{OperationName} completed successfully on '{Path}'", operationName, path);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{OperationName} failed on '{Path}'", operationName, path);
            throw new StorageException($"{operationName} failed on '{path}'", ex)
            {
                Path = path,
                ProviderType = typeof(TProvider).Name
            };
        }
    }

    /// <summary>
    /// Wraps an async operation with try-catch and logging for consistent error handling.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="operationName">The name of the operation for logging.</param>
    /// <param name="path">The path being operated on.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    protected async Task<T> ExecuteWithLoggingAsync<T>(Func<Task<T>> operation, string operationName, string path)
    {
        try
        {
            Logger.LogDebug("{OperationName} async on '{Path}'", operationName, path);
            T result = await operation();
            Logger.LogTrace("{OperationName} async completed successfully on '{Path}'", operationName, path);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{OperationName} async failed on '{Path}'", operationName, path);
            throw new StorageException($"{operationName} async failed on '{path}'", ex)
            {
                Path = path,
                ProviderType = typeof(TProvider).Name
            };
        }
    }

    /// <summary>
    /// Wraps an async operation without a return value with try-catch and logging.
    /// </summary>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="operationName">The name of the operation for logging.</param>
    /// <param name="path">The path being operated on.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task ExecuteWithLoggingAsync(Func<Task> operation, string operationName, string path)
    {
        try
        {
            Logger.LogDebug("{OperationName} async on '{Path}'", operationName, path);
            await operation();
            Logger.LogTrace("{OperationName} async completed successfully on '{Path}'", operationName, path);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{OperationName} async failed on '{Path}'", operationName, path);
            throw new StorageException($"{operationName} async failed on '{path}'", ex)
            {
                Path = path,
                ProviderType = typeof(TProvider).Name
            };
        }
    }
}
