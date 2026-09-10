using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text;

namespace VisionaryCoder.Framework.Storage.Azure.Blob;

/// <summary>
/// Provides Azure Blob Storage-based storage operations implementation following Microsoft I/O patterns.
/// This service wraps Azure Blob Storage operations with logging, error handling, and async support.
/// Supports both connection string and managed identity authentication.
/// </summary>
public sealed class AzureBlobStorageProvider : ServiceBase<AzureBlobStorageProvider>, IStorageProvider
{

    private static readonly Encoding defaultEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly AzureBlobStorageOptions options;
    private readonly BlobServiceClient blobServiceClient;
    private readonly BlobContainerClient containerClient;

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
            Logger.LogError(ex, "Failed to initialize Azure Blob Storage client");
            throw;
        }
    }

    public bool FileExists(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return FileExists(fileInfo.FullName);
    }

    public bool FileExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string blobName = NormalizeBlobName(path);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);
            Response<bool>? response = blobClient.Exists();

            Logger.LogTrace("Blob existence check for '{BlobName}': {Exists}", blobName, response.Value);
            return response.Value;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking blob existence for '{Path}'", path);
            throw;
        }
    }

    public string ReadAllText(string path)
    {
        byte[] bytes = ReadAllBytes(path);
        return defaultEncoding.GetString(bytes);
    }

    public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        byte[] bytes = await ReadAllBytesAsync(path, cancellationToken);
        return defaultEncoding.GetString(bytes);
    }

    public byte[] ReadAllBytes(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string blobName = NormalizeBlobName(path);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            Logger.LogDebug("Reading all bytes from blob '{BlobName}'", blobName);

            if (!blobClient.Exists())
            {
                throw new FileNotFoundException($"The blob '{blobName}' does not exist in container '{options.ContainerName}'.", path);
            }

            using var memoryStream = new MemoryStream();
            blobClient.DownloadTo(memoryStream);
            byte[] bytes = memoryStream.ToArray();

            Logger.LogTrace("Successfully read {Length} bytes from blob '{BlobName}'", bytes.Length, blobName);
            return bytes;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error reading bytes from blob '{Path}'", path);
            throw;
        }
    }

    public async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string blobName = NormalizeBlobName(path);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            Logger.LogDebug("Reading all bytes async from blob '{BlobName}'", blobName);

            Response<bool>? existsResponse = await blobClient.ExistsAsync(cancellationToken);
            if (!existsResponse.Value)
            {
                throw new FileNotFoundException($"The blob '{blobName}' does not exist in container '{options.ContainerName}'.", path);
            }

            using var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream, cancellationToken);
            byte[] bytes = memoryStream.ToArray();

            Logger.LogTrace("Successfully read {Length} bytes async from blob '{BlobName}'", bytes.Length, blobName);
            return bytes;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error reading bytes async from blob '{Path}'", path);
            throw;
        }
    }

    public void WriteAllText(string path, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        WriteAllBytes(path, defaultEncoding.GetBytes(content));
    }

    public async Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        await WriteAllBytesAsync(path, defaultEncoding.GetBytes(content), cancellationToken);
    }

    public void WriteAllBytes(string path, byte[] bytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(bytes);

        try
        {
            string blobName = NormalizeBlobName(path);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            Logger.LogDebug("Writing {Length} bytes to blob '{BlobName}'", bytes.Length, blobName);

            using var memoryStream = new MemoryStream(bytes);
            var uploadOptions = new BlobUploadOptions
            {
                AccessTier = options.DefaultAccessTier
            };

            blobClient.Upload(memoryStream, uploadOptions);
            Logger.LogTrace("Successfully wrote bytes to blob '{BlobName}'", blobName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error writing bytes to blob '{Path}'", path);
            throw;
        }
    }

    public async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(bytes);

        try
        {
            string blobName = NormalizeBlobName(path);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            Logger.LogDebug("Writing {Length} bytes async to blob '{BlobName}'", bytes.Length, blobName);

            using var memoryStream = new MemoryStream(bytes);
            var uploadOptions = new BlobUploadOptions
            {
                AccessTier = options.DefaultAccessTier
            };

            await blobClient.UploadAsync(memoryStream, uploadOptions, cancellationToken);
            Logger.LogTrace("Successfully wrote bytes async to blob '{BlobName}'", blobName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error writing bytes async to blob '{Path}'", path);
            throw;
        }
    }

    public void DeleteFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string blobName = NormalizeBlobName(path);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            Logger.LogDebug("Deleting blob '{BlobName}'", blobName);
            blobClient.DeleteIfExists();
            Logger.LogTrace("Successfully deleted blob '{BlobName}'", blobName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting blob '{Path}'", path);
            throw;
        }
    }

    public async Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string blobName = NormalizeBlobName(path);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            Logger.LogDebug("Deleting blob async '{BlobName}'", blobName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            Logger.LogTrace("Successfully deleted blob async '{BlobName}'", blobName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting blob async '{Path}'", path);
            throw;
        }
    }

    public bool DirectoryExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string prefix = NormalizeDirectoryPrefix(path);
            Logger.LogTrace("Checking directory existence for prefix '{Prefix}'", prefix);

            // In blob storage, directories are virtual - check if any blobs start with the prefix
            IEnumerable<BlobItem> blobs = containerClient.GetBlobs(prefix: prefix).Take(1);
            bool exists = blobs.Any();

            Logger.LogTrace("Directory existence check for '{Path}': {Exists}", path, exists);
            return exists;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking directory existence for '{Path}'", path);
            throw;
        }
    }

    public DirectoryInfo CreateDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            Logger.LogDebug("Creating directory '{Path}'", path);

            // In blob storage, directories are virtual and created implicitly when blobs are added
            // We'll create a placeholder blob to represent the directory
            string directoryMarkerPath = Path.Combine(path, ".directory");
            string blobName = NormalizeBlobName(directoryMarkerPath);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            using var emptyStream = new MemoryStream([]);
            blobClient.Upload(emptyStream, overwrite: true);

            Logger.LogTrace("Successfully created directory '{Path}'", path);
            return new DirectoryInfo(path);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating directory '{Path}'", path);
            throw;
        }
    }

    public async Task<DirectoryInfo> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            Logger.LogDebug("Creating directory async '{Path}'", path);

            // Create directory marker blob
            string directoryMarkerPath = Path.Combine(path, ".directory");
            string blobName = NormalizeBlobName(directoryMarkerPath);
            BlobClient? blobClient = containerClient.GetBlobClient(blobName);

            using var emptyStream = new MemoryStream([]);
            await blobClient.UploadAsync(emptyStream, overwrite: true, cancellationToken: cancellationToken);

            Logger.LogTrace("Successfully created directory async '{Path}'", path);
            return new DirectoryInfo(path);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating directory async '{Path}'", path);
            throw;
        }
    }

    public void DeleteDirectory(string path, bool recursive = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string prefix = NormalizeDirectoryPrefix(path);
            Logger.LogDebug("Deleting directory '{Path}' (recursive: {Recursive})", path, recursive);

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

            Logger.LogTrace("Successfully deleted directory '{Path}'", path);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting directory '{Path}' (recursive: {Recursive})", path, recursive);
            throw;
        }
    }

    public async Task DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string prefix = NormalizeDirectoryPrefix(path);
            Logger.LogDebug("Deleting directory async '{Path}' (recursive: {Recursive})", path, recursive);

            var blobs = new List<BlobItem>();
            await foreach (BlobItem? blob in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
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
                await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            }

            Logger.LogTrace("Successfully deleted directory async '{Path}'", path);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting directory async '{Path}' (recursive: {Recursive})", path, recursive);
            throw;
        }
    }

    public string[] GetFiles(string path, string searchPattern = "*")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        try
        {
            string prefix = NormalizeDirectoryPrefix(path);
            Logger.LogDebug("Getting files from '{Path}' with pattern '{Pattern}'", path, searchPattern);

            string[] blobs = containerClient.GetBlobs(prefix: prefix)
                .Where(b => !b.Name.EndsWith("/.directory"))
                .Where(b => MatchesPattern(Path.GetFileName(b.Name), searchPattern))
                .Select(b => b.Name)
                .ToArray();

            Logger.LogTrace("Found {Count} files in '{Path}'", blobs.Length, path);
            return blobs;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting files from '{Path}' with pattern '{Pattern}'", path, searchPattern);
            throw;
        }
    }

    public string[] GetDirectories(string path, string searchPattern = "*")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        try
        {
            string prefix = NormalizeDirectoryPrefix(path);
            Logger.LogDebug("Getting directories from '{Path}' with pattern '{Pattern}'", path, searchPattern);

            string[] directories = containerClient.GetBlobsByHierarchy(prefix: prefix, delimiter: "/")
                .Where(item => item.IsPrefix)
                .Select(item => item.Prefix.TrimEnd('/'))
                .Where(dir => MatchesPattern(Path.GetFileName(dir), searchPattern))
                .ToArray();

            Logger.LogTrace("Found {Count} directories in '{Path}'", directories.Length, path);
            return directories;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting directories from '{Path}' with pattern '{Pattern}'", path, searchPattern);
            throw;
        }
    }

    public async IAsyncEnumerable<string> EnumerateFilesAsync(string path, string searchPattern = "*",
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        Logger.LogDebug("Enumerating files async from '{Path}' with pattern '{Pattern}'", path, searchPattern);

        string prefix = NormalizeDirectoryPrefix(path);

        await foreach (BlobItem? blob in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!blob.Name.EndsWith("/.directory") && MatchesPattern(Path.GetFileName(blob.Name), searchPattern))
            {
                yield return blob.Name;
            }
        }
    }

    public string GetFullPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string blobName = NormalizeBlobName(path);
            string fullUri = containerClient.GetBlobClient(blobName).Uri.ToString();
            Logger.LogTrace("Resolved full path for '{Path}': '{FullPath}'", path, fullUri);
            return fullUri;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error resolving full path for '{Path}'", path);
            throw;
        }
    }

    public string? GetDirectoryName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string normalized = NormalizeBlobName(path);
            string? directory = Path.GetDirectoryName(normalized);
            Logger.LogTrace("Resolved directory name for '{Path}': '{DirectoryName}'", path, directory ?? "<null>");
            return directory?.Replace('\\', '/');
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error resolving directory name for '{Path}'", path);
            throw;
        }
    }

    public string GetFileName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            string normalized = NormalizeBlobName(path);
            string fileName = Path.GetFileName(normalized);
            Logger.LogTrace("Resolved file name for '{Path}': '{FileName}'", path, fileName);
            return fileName;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error resolving file name for '{Path}'", path);
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
}
