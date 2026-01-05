using System.Runtime.CompilerServices;
using System.Text;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Storage.Abstractions;
using IoPath = System.IO.Path;

namespace VisionaryCoder.Framework.Storage.Azure.Blob;

/// <summary>
/// Provides Azure Blob Storage-based storage operations implementation following Microsoft I/O patterns.
/// This service wraps Azure Blob Storage operations with logging, error handling, and async support.
/// Supports both connection string and managed identity authentication.
/// </summary>
public sealed partial class AzureBlobStorageProvider : StorageProviderBase<AzureBlobStorageProvider>, IStorageProvider
{

    private static readonly Encoding defaultEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly AzureBlobStorageOptions options;
    private readonly BlobServiceClient? blobServiceClient;
    private readonly BlobContainerClient containerClient;

    /// <summary>
    /// Initializes a new instance of <see cref="AzureBlobStorageProvider"/> with automatic client creation.
    /// </summary>
    /// <param name="options">Azure Blob Storage configuration options.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    public AzureBlobStorageProvider(AzureBlobStorageOptions options, ILogger<AzureBlobStorageProvider> logger)
        : base(logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.options.Validate();

        try
        {
            // Create blob service client based on authentication method
            if (options.UseManagedIdentity)
            {
                blobServiceClient = new BlobServiceClient(new Uri(options.StorageAccountUri!), new DefaultAzureCredential());
            }
            else
            {
                blobServiceClient = new BlobServiceClient(options.ConnectionString);
            }

            containerClient = blobServiceClient.GetBlobContainerClient(options.ContainerName);

            // Create container if it doesn't exist and option is enabled
            if (options.CreateContainerIfNotExists)
            {
                containerClient.CreateIfNotExists(options.ContainerPublicAccess);
            }
        }
        catch (Exception ex)
        {
            LogInitializationError(Logger, ex);
            throw;
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="AzureBlobStorageProvider"/> with an injected container client.
    /// This constructor enables dependency injection and unit testing with mocked clients.
    /// </summary>
    /// <param name="options">Azure Blob Storage configuration options.</param>
    /// <param name="containerClient">Pre-configured BlobContainerClient instance.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    public AzureBlobStorageProvider(AzureBlobStorageOptions options, BlobContainerClient containerClient, ILogger<AzureBlobStorageProvider> logger)
        : base(logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.containerClient = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
        this.options.Validate();
    }

    public override bool FileExists(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return FileExists(fileInfo.FullName);
    }

    public override bool FileExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string blobName = NormalizeBlobName(path);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);
            Response<bool>? response = blobClient.Exists();

            LogBlobExistenceCheck(Logger, blobName, response.Value);
            return response.Value;
        }
        catch (Exception ex)
        {
            LogErrorCheckingBlobExistence(Logger, path, ex);
            throw;
        }
    }

    public override string ReadAllText(string path)
    {
        byte[] bytes = ReadAllBytes(path);
        return defaultEncoding.GetString(bytes);
    }

    public override async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        byte[] bytes = await ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return defaultEncoding.GetString(bytes);
    }

    public override byte[] ReadAllBytes(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string blobName = NormalizeBlobName(path);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            LogReadingAllBytes(Logger, blobName);

            if (!blobClient.Exists())
            {
                throw new FileNotFoundException($"The blob '{blobName}' does not exist in container '{options.ContainerName}'.", path);
            }

            using var memoryStream = new MemoryStream();
            blobClient.DownloadTo(memoryStream);
            byte[] bytes = memoryStream.ToArray();

            LogSuccessfullyReadBytes(Logger, bytes.Length, blobName);
            return bytes;
        }
        catch (Exception ex)
        {
            LogErrorReadingBytes(Logger, path, ex);
            throw;
        }
    }

    public override async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string blobName = NormalizeBlobName(path);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            LogReadingAllBytesAsync(Logger, blobName);

            Response<bool>? existsResponse = await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
            if (!existsResponse.Value)
            {
                throw new FileNotFoundException($"The blob '{blobName}' does not exist in container '{options.ContainerName}'.", path);
            }

            using var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            byte[] bytes = memoryStream.ToArray();

            LogSuccessfullyReadBytesAsync(Logger, bytes.Length, blobName);
            return bytes;
        }
        catch (Exception ex)
        {
            LogErrorReadingBytesAsync(Logger, path, ex);
            throw;
        }
    }

    public override void WriteAllText(string path, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        WriteAllBytes(path, defaultEncoding.GetBytes(content));
    }

    public override async Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        await WriteAllBytesAsync(path, defaultEncoding.GetBytes(content), cancellationToken).ConfigureAwait(false);
    }

    public override void WriteAllBytes(string path, byte[] bytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(bytes);

        try
        {
            string blobName = NormalizeBlobName(path);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            LogWritingBytes(Logger, bytes.Length, blobName);

            using var memoryStream = new MemoryStream(bytes);
            var uploadOptions = new BlobUploadOptions
            {
                AccessTier = options.DefaultAccessTier
            };

            blobClient.Upload(memoryStream, uploadOptions);
            LogSuccessfullyWroteBytes(Logger, blobName);
        }
        catch (Exception ex)
        {
            LogErrorWritingBytes(Logger, path, ex);
            throw;
        }
    }

    public override async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(bytes);

        try
        {
            string blobName = NormalizeBlobName(path);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            LogWritingBytesAsync(Logger, bytes.Length, blobName);

            using var memoryStream = new MemoryStream(bytes);
            var uploadOptions = new BlobUploadOptions
            {
                AccessTier = options.DefaultAccessTier
            };

            await blobClient.UploadAsync(memoryStream, uploadOptions, cancellationToken).ConfigureAwait(false);
            LogSuccessfullyWroteBytesAsync(Logger, blobName);
        }
        catch (Exception ex)
        {
            LogErrorWritingBytesAsync(Logger, path, ex);
            throw;
        }
    }

    public override void DeleteFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string blobName = NormalizeBlobName(path);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            LogDeletingBlob(Logger, blobName);
            blobClient.DeleteIfExists();
            LogSuccessfullyDeletedBlob(Logger, blobName);
        }
        catch (Exception ex)
        {
            LogErrorDeletingBlob(Logger, path, ex);
            throw;
        }
    }

    public override async Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string blobName = NormalizeBlobName(path);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            LogDeletingBlobAsync(Logger, blobName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            LogSuccessfullyDeletedBlobAsync(Logger, blobName);
        }
        catch (Exception ex)
        {
            LogErrorDeletingBlobAsync(Logger, path, ex);
            throw;
        }
    }

    public override bool DirectoryExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string prefix = NormalizeDirectoryPrefix(path);
            LogCheckingDirectoryExistence(Logger, prefix);

            // In blob storage, directories are virtual - check if any blobs start with the prefix
            IEnumerable<BlobItem> blobs = containerClient.GetBlobs(prefix: prefix).Take(1);
            bool exists = blobs.Any();

            LogDirectoryExistenceCheck(Logger, path, exists);
            return exists;
        }
        catch (Exception ex)
        {
            LogErrorCheckingDirectoryExistence(Logger, path, ex);
            throw;
        }
    }

    public override DirectoryInfo CreateDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            LogCreatingDirectory(Logger, path);

            // In blob storage, directories are virtual and created implicitly when blobs are added
            // We'll create a placeholder blob to represent the directory
            string directoryMarkerPath = IoPath.Combine(path, ".directory");
            string blobName = NormalizeBlobName(directoryMarkerPath);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            using var emptyStream = new MemoryStream([]);
            blobClient.Upload(emptyStream, overwrite: true);

            LogSuccessfullyCreatedDirectory(Logger, path);
            return new DirectoryInfo(path);
        }
        catch (Exception ex)
        {
            LogErrorCreatingDirectory(Logger, path, ex);
            throw;
        }
    }

    public override async Task<DirectoryInfo> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            LogCreatingDirectoryAsync(Logger, path);

            // Create directory marker blob
            string directoryMarkerPath = IoPath.Combine(path, ".directory");
            string blobName = NormalizeBlobName(directoryMarkerPath);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            using var emptyStream = new MemoryStream([]);
            await blobClient.UploadAsync(emptyStream, overwrite: true, cancellationToken: cancellationToken).ConfigureAwait(false);

            LogSuccessfullyCreatedDirectoryAsync(Logger, path);
            return new DirectoryInfo(path);
        }
        catch (Exception ex)
        {
            LogErrorCreatingDirectoryAsync(Logger, path, ex);
            throw;
        }
    }

    public override void DeleteDirectory(string path, bool recursive = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string prefix = NormalizeDirectoryPrefix(path);
            LogDeletingDirectory(Logger, path, recursive);

            var blobs = containerClient.GetBlobs(prefix: prefix).ToList();

            if (!recursive && blobs.Count > 1)
            {
                // Check if there are any blobs other than the directory marker
                var nonMarkerBlobs = blobs.Where(b => !b.Name.EndsWith("/.directory")).ToList();
                if (nonMarkerBlobs.Any())
                {
                    throw new IOException($"The directory '{path}' is not empty.");
                }
            }

            foreach (BlobItem? blob in blobs)
            {
                BlobClient? blobClient = containerClient.GetBlobClient(blob.Name);
                blobClient.DeleteIfExists();
            }

            LogSuccessfullyDeletedDirectory(Logger, path);
        }
        catch (Exception ex)
        {
            LogErrorDeletingDirectory(Logger, path, recursive, ex);
            throw;
        }
    }

    public override async Task DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string prefix = NormalizeDirectoryPrefix(path);
            LogDeletingDirectoryAsync(Logger, path, recursive);

            var blobs = new List<BlobItem>();
            await foreach (BlobItem? blob in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                blobs.Add(blob);
            }

            if (!recursive && blobs.Count > 1)
            {
                var nonMarkerBlobs = blobs.Where(b => !b.Name.EndsWith("/.directory")).ToList();
                if (nonMarkerBlobs.Any())
                {
                    throw new IOException($"The directory '{path}' is not empty.");
                }
            }

            foreach (BlobItem blob in blobs)
            {
                BlobClient? blobClient = containerClient.GetBlobClient(blob.Name);
                await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            LogSuccessfullyDeletedDirectoryAsync(Logger, path);
        }
        catch (Exception ex)
        {
            LogErrorDeletingDirectoryAsync(Logger, path, recursive, ex);
            throw;
        }
    }

    public override string[] GetFiles(string path, string searchPattern = "*")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        try
        {
            string prefix = NormalizeDirectoryPrefix(path);
            LogGettingFiles(Logger, path, searchPattern);

            string[] blobs = containerClient.GetBlobs(prefix: prefix)
                .Where(b => !b.Name.EndsWith("/.directory"))
                .Where(b => MatchesPattern(IoPath.GetFileName(b.Name), searchPattern))
                .Select(b => b.Name)
                .ToArray();

            LogFoundFiles(Logger, blobs.Length, path);
            return blobs;
        }
        catch (Exception ex)
        {
            LogErrorGettingFiles(Logger, path, searchPattern, ex);
            throw;
        }
    }

    public override string[] GetDirectories(string path, string searchPattern = "*")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        try
        {
            string prefix = NormalizeDirectoryPrefix(path);
            LogGettingDirectories(Logger, path, searchPattern);

            string[] directories = containerClient.GetBlobsByHierarchy(prefix: prefix, delimiter: "/")
                .Where(item => item.IsPrefix)
                .Select(item => item.Prefix.TrimEnd('/'))
                .Where(dir => MatchesPattern(IoPath.GetFileName(dir), searchPattern))
                .ToArray();

            LogFoundDirectories(Logger, directories.Length, path);
            return directories;
        }
        catch (Exception ex)
        {
            LogErrorGettingDirectories(Logger, path, searchPattern, ex);
            throw;
        }
    }

    public override async IAsyncEnumerable<string> EnumerateFilesAsync(string path, string searchPattern = "*",
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        LogEnumeratingFilesAsync(Logger, path, searchPattern);

        string prefix = NormalizeDirectoryPrefix(path);

        await foreach (BlobItem? blob in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!blob.Name.EndsWith("/.directory") && MatchesPattern(IoPath.GetFileName(blob.Name), searchPattern))
            {
                yield return blob.Name;
            }
        }
    }

    public override string GetFullPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string blobName = NormalizeBlobName(path);
            string fullUri = containerClient.GetBlobClient(blobName).Uri.ToString();
            LogResolvedFullPath(Logger, path, fullUri);
            return fullUri;
        }
        catch (Exception ex)
        {
            LogErrorResolvingFullPath(Logger, path, ex);
            throw;
        }
    }

    public override string? GetDirectoryName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string normalized = NormalizeBlobName(path);
            string? directory = IoPath.GetDirectoryName(normalized);
            LogResolvedDirectoryName(Logger, path, directory ?? "<null>");
            return directory?.Replace('\\', '/');
        }
        catch (Exception ex)
        {
            LogErrorResolvingDirectoryName(Logger, path, ex);
            throw;
        }
    }

    public override string GetFileName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string normalized = NormalizeBlobName(path);
            string fileName = IoPath.GetFileName(normalized);
            LogResolvedFileName(Logger, path, fileName);
            return fileName;
        }
        catch (Exception ex)
        {
            LogErrorResolvingFileName(Logger, path, ex);
            throw;
        }
    }

    private static string NormalizeBlobName(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));

        // Replace backslashes with forward slashes and remove leading slashes
        string normalized = path.Replace('\\', '/').TrimStart('/');

        // Remove duplicate slashes
        while (normalized.Contains("//"))
        {
            normalized = normalized.Replace("//", "/");
        }

        return normalized;
    }

    private static string NormalizeDirectoryPrefix(string path)
    {
        string normalized = NormalizeBlobName(path);
        return normalized.EndsWith('/') ? normalized : normalized + "/";
    }

    private static bool MatchesPattern(string value, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern) || pattern == "*")
        {
            return true;
        }

        // Simple pattern matching for * and ? wildcards
        string regexPattern = "^" + pattern
            .Replace(".", "\\.")
            .Replace("*", ".*")
            .Replace("?", ".") + "$";

        return System.Text.RegularExpressions.Regex.IsMatch(value, regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    #region High-Performance Logging Methods

    [LoggerMessage(LogLevel.Error, "Failed to initialize Azure Blob Storage client")]
    private static partial void LogInitializationError(ILogger logger, Exception ex);

    [LoggerMessage(LogLevel.Trace, "Blob existence check for '{blobName}': {exists}")]
    private static partial void LogBlobExistenceCheck(ILogger logger, string blobName, bool exists);

    [LoggerMessage(LogLevel.Error, "Error checking blob existence for '{path}'")]
    private static partial void LogErrorCheckingBlobExistence(ILogger logger, string path, Exception ex);

    [LoggerMessage(LogLevel.Debug, "Reading all bytes from blob '{blobName}'")]
    private static partial void LogReadingAllBytes(ILogger logger, string blobName);

    [LoggerMessage(LogLevel.Trace, "Successfully read {length} bytes from blob '{blobName}'")]
    private static partial void LogSuccessfullyReadBytes(ILogger logger, int length, string blobName);

    [LoggerMessage(LogLevel.Error, "Error reading bytes from blob '{path}'")]
    private static partial void LogErrorReadingBytes(ILogger logger, string path, Exception ex);

    [LoggerMessage(LogLevel.Debug, "Reading all bytes async from blob '{blobName}'")]
    private static partial void LogReadingAllBytesAsync(ILogger logger, string blobName);

    [LoggerMessage(LogLevel.Trace, "Successfully read {length} bytes async from blob '{blobName}'")]
    private static partial void LogSuccessfullyReadBytesAsync(ILogger logger, int length, string blobName);

    [LoggerMessage(LogLevel.Error, "Error reading bytes async from blob '{path}'")]
    private static partial void LogErrorReadingBytesAsync(ILogger logger, string path, Exception ex);

    [LoggerMessage(LogLevel.Debug, "Writing {length} bytes to blob '{blobName}'")]
    private static partial void LogWritingBytes(ILogger logger, int length, string blobName);

    [LoggerMessage(LogLevel.Trace, "Successfully wrote bytes to blob '{blobName}'")]
    private static partial void LogSuccessfullyWroteBytes(ILogger logger, string blobName);

    [LoggerMessage(LogLevel.Error, "Error writing bytes to blob '{path}'")]
    private static partial void LogErrorWritingBytes(ILogger logger, string path, Exception ex);

    [LoggerMessage(LogLevel.Debug, "Writing {length} bytes async to blob '{blobName}'")]
    private static partial void LogWritingBytesAsync(ILogger logger, int length, string blobName);

    [LoggerMessage(LogLevel.Trace, "Successfully wrote bytes async to blob '{blobName}'")]
    private static partial void LogSuccessfullyWroteBytesAsync(ILogger logger, string blobName);

    [LoggerMessage(LogLevel.Error, "Error writing bytes async to blob '{path}'")]
    private static partial void LogErrorWritingBytesAsync(ILogger logger, string path, Exception ex);

    [LoggerMessage(LogLevel.Debug, "Deleting blob '{blobName}'")]
    private static partial void LogDeletingBlob(ILogger logger, string blobName);

    [LoggerMessage(LogLevel.Trace, "Successfully deleted blob '{blobName}'")]
    private static partial void LogSuccessfullyDeletedBlob(ILogger logger, string blobName);

    [LoggerMessage(LogLevel.Error, "Error deleting blob '{path}'")]
    private static partial void LogErrorDeletingBlob(ILogger logger, string path, Exception ex);

    [LoggerMessage(LogLevel.Debug, "Deleting blob async '{blobName}'")]
    private static partial void LogDeletingBlobAsync(ILogger logger, string blobName);

    [LoggerMessage(LogLevel.Trace, "Successfully deleted blob async '{blobName}'")]
    private static partial void LogSuccessfullyDeletedBlobAsync(ILogger logger, string blobName);

    [LoggerMessage(LogLevel.Error, "Error deleting blob async '{path}'")]
    private static partial void LogErrorDeletingBlobAsync(ILogger logger, string path, Exception ex);

    [LoggerMessage(LogLevel.Trace, "Checking directory existence for prefix '{prefix}'")]
    private static partial void LogCheckingDirectoryExistence(ILogger logger, string prefix);

    [LoggerMessage(LogLevel.Trace, "Directory existence check for '{path}': {exists}")]
    private static partial void LogDirectoryExistenceCheck(ILogger logger, string path, bool exists);

    [LoggerMessage(LogLevel.Error, "Error checking directory existence for '{path}'")]
    private static partial void LogErrorCheckingDirectoryExistence(ILogger logger, string path, Exception ex);

    [LoggerMessage(LogLevel.Debug, "Creating directory '{path}'")]
    private static partial void LogCreatingDirectory(ILogger logger, string path);

    [LoggerMessage(LogLevel.Trace, "Successfully created directory '{path}'")]
    private static partial void LogSuccessfullyCreatedDirectory(ILogger logger, string path);

    [LoggerMessage(LogLevel.Error, "Error creating directory '{path}'")]
    private static partial void LogErrorCreatingDirectory(ILogger logger, string path, Exception ex);

    [LoggerMessage(LogLevel.Debug, "Creating directory async '{path}'")]
    private static partial void LogCreatingDirectoryAsync(ILogger logger, string path);

    [LoggerMessage(LogLevel.Trace, "Successfully created directory async '{path}'")]
    private static partial void LogSuccessfullyCreatedDirectoryAsync(ILogger logger, string path);

    [LoggerMessage(LogLevel.Error, "Error creating directory async '{path}'")]
    private static partial void LogErrorCreatingDirectoryAsync(ILogger logger, string path, Exception ex);

    [LoggerMessage(LogLevel.Debug, "Deleting directory '{path}' (recursive: {recursive})")]
    private static partial void LogDeletingDirectory(ILogger logger, string path, bool recursive);

    [LoggerMessage(LogLevel.Trace, "Successfully deleted directory '{path}'")]
    private static partial void LogSuccessfullyDeletedDirectory(ILogger logger, string path);

    [LoggerMessage(LogLevel.Error, "Error deleting directory '{path}' (recursive: {recursive})")]
    private static partial void LogErrorDeletingDirectory(ILogger logger, string path, bool recursive, Exception ex);

    [LoggerMessage(LogLevel.Debug, "Deleting directory async '{path}' (recursive: {recursive})")]
    private static partial void LogDeletingDirectoryAsync(ILogger logger, string path, bool recursive);

    [LoggerMessage(LogLevel.Trace, "Successfully deleted directory async '{path}'")]
    private static partial void LogSuccessfullyDeletedDirectoryAsync(ILogger logger, string path);

    [LoggerMessage(LogLevel.Error, "Error deleting directory async '{path}' (recursive: {recursive})")]
    private static partial void LogErrorDeletingDirectoryAsync(ILogger logger, string path, bool recursive, Exception ex);

    [LoggerMessage(LogLevel.Debug, "Getting files from '{path}' with pattern '{searchPattern}'")]
    private static partial void LogGettingFiles(ILogger logger, string path, string searchPattern);

    [LoggerMessage(LogLevel.Trace, "Found {count} files in '{path}'")]
    private static partial void LogFoundFiles(ILogger logger, int count, string path);

    [LoggerMessage(LogLevel.Error, "Error getting files from '{path}' with pattern '{searchPattern}'")]
    private static partial void LogErrorGettingFiles(ILogger logger, string path, string searchPattern, Exception ex);

    [LoggerMessage(LogLevel.Debug, "Getting directories from '{path}' with pattern '{searchPattern}'")]
    private static partial void LogGettingDirectories(ILogger logger, string path, string searchPattern);

    [LoggerMessage(LogLevel.Trace, "Found {count} directories in '{path}'")]
    private static partial void LogFoundDirectories(ILogger logger, int count, string path);

    [LoggerMessage(LogLevel.Error, "Error getting directories from '{path}' with pattern '{searchPattern}'")]
    private static partial void LogErrorGettingDirectories(ILogger logger, string path, string searchPattern, Exception ex);

    [LoggerMessage(LogLevel.Debug, "Enumerating files async from '{path}' with pattern '{searchPattern}'")]
    private static partial void LogEnumeratingFilesAsync(ILogger logger, string path, string searchPattern);

    [LoggerMessage(LogLevel.Trace, "Resolved full path for '{path}': '{fullPath}'")]
    private static partial void LogResolvedFullPath(ILogger logger, string path, string fullPath);

    [LoggerMessage(LogLevel.Error, "Error resolving full path for '{path}'")]
    private static partial void LogErrorResolvingFullPath(ILogger logger, string path, Exception ex);

    [LoggerMessage(LogLevel.Trace, "Resolved directory name for '{path}': '{directoryName}'")]
    private static partial void LogResolvedDirectoryName(ILogger logger, string path, string directoryName);

    [LoggerMessage(LogLevel.Error, "Error resolving directory name for '{path}'")]
    private static partial void LogErrorResolvingDirectoryName(ILogger logger, string path, Exception ex);

    [LoggerMessage(LogLevel.Trace, "Resolved file name for '{path}': '{fileName}'")]
    private static partial void LogResolvedFileName(ILogger logger, string path, string fileName);

    [LoggerMessage(LogLevel.Error, "Error resolving file name for '{path}'")]
    private static partial void LogErrorResolvingFileName(ILogger logger, string path, Exception ex);

    #endregion
}
