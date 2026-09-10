using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace VisionaryCoder.Framework.Storage.Azure.Blob;

/// <summary>Block blob storage with SDK-owned retries and caller-owned streams.</summary>
public sealed class AzureBlobStorageProvider : ServiceBase<AzureBlobStorageProvider>, IStorageProvider, IObjectStorageProvider
{
    private readonly AzureBlobStorageOptions options;
    private readonly BlobContainerClient containerClient;

    /// <summary>Constructs SDK clients without network I/O. Optional container creation is deferred to writes.</summary>
    public AzureBlobStorageProvider(AzureBlobStorageOptions options, ILogger<AzureBlobStorageProvider> logger)
        : this(options, logger, CreateContainerClient(options)) { }

    /// <summary>Uses an externally configured, reusable SDK client. Endpoint credentials and retries belong to that client.</summary>
    public AzureBlobStorageProvider(AzureBlobStorageOptions options, ILogger<AzureBlobStorageProvider> logger, BlobContainerClient containerClient)
        : base(logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        options.ValidateBehavior();
        this.containerClient = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
        if (containerClient.Name != options.ContainerName) throw new ArgumentException("The injected client must target ContainerName.", nameof(containerClient));
    }

    /// <inheritdoc />
    public StorageCapabilities Capabilities => StorageCapabilities.Read | StorageCapabilities.Write | StorageCapabilities.Delete | StorageCapabilities.Metadata | StorageCapabilities.List | StorageCapabilities.CreateOnly;

    /// <inheritdoc />
    public async Task<Stream> OpenReadAsync(StorageObjectRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string name = ObjectName(request.Path);
        Response<BlobDownloadStreamingResult> response = await ExecuteAsync(() => containerClient.GetBlobClient(name).DownloadStreamingAsync(null, cancellationToken), cancellationToken).ConfigureAwait(false);
        Stream content = response.Value.Content;
        if (cancellationToken.IsCancellationRequested)
        {
            content.Dispose();
            throw new OperationCanceledException(cancellationToken);
        }
        return content;
    }

    /// <inheritdoc />
    public async Task<StorageObjectMetadata> WriteAsync(StorageWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string name = ObjectName(request.Path);
        cancellationToken.ThrowIfCancellationRequested();
        if (options.CreateContainerIfNotExists)
            await ExecuteAsync(() => containerClient.CreateIfNotExistsAsync(options.ContainerPublicAccess, null, null, cancellationToken), cancellationToken).ConfigureAwait(false);
        var upload = new BlobUploadOptions
        {
            AccessTier = options.DefaultAccessTier,
            Conditions = request.Overwrite ? null : new BlobRequestConditions { IfNoneMatch = ETag.All },
            TransferOptions = new StorageTransferOptions { InitialTransferSize = options.BufferSize, MaximumTransferSize = options.BufferSize, MaximumConcurrency = 1 }
        };
        Response<BlobContentInfo> response = await ExecuteAsync(() => containerClient.GetBlobClient(name).UploadAsync(request.Content, upload, cancellationToken), cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return new StorageObjectMetadata(name, lastModified: response.Value.LastModified, version: response.Value.ETag.ToString());
    }

    /// <inheritdoc />
    public async Task<StorageObjectMetadata?> GetMetadataAsync(StorageObjectRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string name = ObjectName(request.Path);
        try
        {
            Response<BlobProperties> response = await ExecuteAsync(() => containerClient.GetBlobClient(name).GetPropertiesAsync(null, cancellationToken), cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            BlobProperties value = response.Value;
            return new StorageObjectMetadata(name, value.ContentLength, value.LastModified, value.ContentType, value.ETag.ToString());
        }
        catch (FileNotFoundException) { return null; }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(StorageObjectRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string name = ObjectName(request.Path);
        try
        {
            await ExecuteAsync(() => containerClient.GetBlobClient(name).DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, cancellationToken), cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (FileNotFoundException) { }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StorageObjectMetadata> ListAsync(StorageListRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidatePrefix(request.Prefix);
        await foreach (BlobItem item in ReadItemsAsync(request.Prefix, cancellationToken).ConfigureAwait(false))
        {
            BlobItemProperties value = item.Properties;
            yield return new StorageObjectMetadata(item.Name, value.ContentLength, value.LastModified, value.ContentType, value.ETag?.ToString());
        }
    }

    public bool FileExists(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return FileExists(fileInfo.FullName);
    }
    public bool FileExists(string path) => GetMetadataAsync(new(LegacyName(path))).GetAwaiter().GetResult() is not null;
    public string ReadAllText(string path) => ReadAllTextAsync(path).GetAwaiter().GetResult();
    public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
        => Encoding.UTF8.GetString(await ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false));
    public byte[] ReadAllBytes(string path) => ReadAllBytesAsync(path).GetAwaiter().GetResult();
    public async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        using Stream content = await OpenReadAsync(new(LegacyName(path)), cancellationToken).ConfigureAwait(false);
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, options.BufferSize, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return buffer.ToArray();
    }
    public void WriteAllText(string path, string content) => WriteAllTextAsync(path, content).GetAwaiter().GetResult();
    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        return WriteAllBytesAsync(path, Encoding.UTF8.GetBytes(content), cancellationToken);
    }
    public void WriteAllBytes(string path, byte[] bytes) => WriteAllBytesAsync(path, bytes).GetAwaiter().GetResult();
    public async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        using var content = new MemoryStream(bytes, writable: false);
        await WriteAsync(new(LegacyName(path), content), cancellationToken).ConfigureAwait(false);
    }
    public void DeleteFile(string path) => DeleteFileAsync(path).GetAwaiter().GetResult();
    public Task DeleteFileAsync(string path, CancellationToken cancellationToken = default) => DeleteAsync(new(LegacyName(path)), cancellationToken);
    public bool DirectoryExists(string path) => DirectoryExistsAsync(DirectoryPrefix(path)).GetAwaiter().GetResult();
    private async Task<bool> DirectoryExistsAsync(string prefix)
    {
        await foreach (BlobItem item in ReadItemsAsync(prefix, default).ConfigureAwait(false)) return true;
        return false;
    }
    public DirectoryInfo CreateDirectory(string path) => CreateDirectoryAsync(path).GetAwaiter().GetResult();
    public async Task<DirectoryInfo> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        string prefix = DirectoryPrefix(path);
        var result = new DirectoryInfo(path);
        await WriteAllBytesAsync(prefix + ".directory", [], cancellationToken).ConfigureAwait(false);
        return result;
    }
    public void DeleteDirectory(string path, bool recursive = true) => DeleteDirectoryAsync(path, recursive).GetAwaiter().GetResult();
    public async Task DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default)
    {
        string prefix = DirectoryPrefix(path);
        var names = new List<string>();
        await foreach (BlobItem item in ReadItemsAsync(prefix, cancellationToken).ConfigureAwait(false)) names.Add(item.Name);
        if (!recursive && names.Any(name => name != prefix + ".directory")) throw new IOException("The virtual directory is not empty.");
        foreach (string name in names) await DeleteAsync(new(name), cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }
    public string[] GetFiles(string path, string searchPattern = "*") => NamesAsync(path, searchPattern, false).GetAwaiter().GetResult();
    public string[] GetDirectories(string path, string searchPattern = "*") => NamesAsync(path, searchPattern, true).GetAwaiter().GetResult();
    private async Task<string[]> NamesAsync(string path, string pattern, bool directories)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);
        string prefix = DirectoryPrefix(path);
        var names = new HashSet<string>(StringComparer.Ordinal);
        await foreach (BlobItem item in ReadItemsAsync(prefix, default).ConfigureAwait(false))
        {
            if (directories)
            {
                int separator = item.Name.IndexOf('/', prefix.Length);
                if (separator >= 0)
                {
                    string directory = item.Name[..separator];
                    if (MatchesPattern(GetFileName(directory), pattern)) names.Add(directory);
                }
            }
            else if (!IsMarker(item.Name) && MatchesPattern(GetFileName(item.Name), pattern)) names.Add(item.Name);
        }
        return names.ToArray();
    }
    public async IAsyncEnumerable<string> EnumerateFilesAsync(string path, string searchPattern = "*", [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        await foreach (BlobItem item in ReadItemsAsync(DirectoryPrefix(path), cancellationToken).ConfigureAwait(false))
            if (!IsMarker(item.Name) && MatchesPattern(GetFileName(item.Name), searchPattern)) yield return item.Name;
    }
    public string GetFullPath(string path)
    {
        string name = LegacyName(path);
        // Do not leak SAS credentials through a descriptive legacy path result.
        return containerClient.GetBlobClient(name).Uri.GetLeftPart(UriPartial.Path);
    }
    public string? GetDirectoryName(string path)
    {
        string name = LegacyName(path);
        int separator = name.LastIndexOf('/');
        return separator < 0 ? null : name[..separator];
    }
    public string GetFileName(string path)
    {
        string name = LegacyName(path);
        return name[(name.LastIndexOf('/') + 1)..];
    }

    internal static BlobContainerClient CreateContainerClient(AzureBlobStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        BlobClientOptions clientOptions = CreateClientOptions(options);
        BlobServiceClient service = options.UseManagedIdentity
            ? new BlobServiceClient(new Uri(options.StorageAccountUri!), new DefaultAzureCredential(), clientOptions)
            : new BlobServiceClient(options.ConnectionString, clientOptions);
        return service.GetBlobContainerClient(options.ContainerName);
    }

    internal static BlobClientOptions CreateClientOptions(AzureBlobStorageOptions options)
    {
        var result = new BlobClientOptions();
        result.Retry.Mode = RetryMode.Exponential;
        result.Retry.MaxRetries = options.MaxRetries;
        result.Retry.Delay = TimeSpan.FromMilliseconds(options.RetryDelayMilliseconds);
        result.Retry.MaxDelay = TimeSpan.FromMilliseconds(options.MaxRetryDelayMilliseconds);
        result.Retry.NetworkTimeout = TimeSpan.FromMilliseconds(options.TimeoutMilliseconds);
        return result;
    }

    private async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken token)
    {
        ThrowIfDisposed();
        token.ThrowIfCancellationRequested();
        try { return await action().ConfigureAwait(false); }
        catch (RequestFailedException exception) when (exception.Status == 404 && exception.ErrorCode is "BlobNotFound" or "ContainerNotFound")
        { throw new FileNotFoundException("The blob or container does not exist.", exception); }
        catch (RequestFailedException exception) when (exception.Status is 401 or 403)
        { throw new UnauthorizedAccessException("Azure Blob access was denied.", exception); }
        catch (RequestFailedException exception) { throw new IOException("Azure Blob operation failed.", exception); }
        catch (AuthenticationFailedException exception) { throw new UnauthorizedAccessException("Azure credential authentication failed.", exception); }
        catch (AggregateException exception) { throw new IOException("Azure Blob transfer failed.", exception); }
    }

    private async IAsyncEnumerable<BlobItem> ReadItemsAsync(string prefix, [EnumeratorCancellation] CancellationToken token)
    {
        AsyncPageable<BlobItem> pageable = await ExecuteAsync(() => Task.FromResult(containerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, token)), token).ConfigureAwait(false);
        await using IAsyncEnumerator<BlobItem> iterator = pageable.GetAsyncEnumerator(token);
        while (await MoveNextAsync(iterator, token).ConfigureAwait(false))
        {
            if (iterator.Current.Name.StartsWith(prefix, StringComparison.Ordinal)) yield return iterator.Current;
        }
    }

    private async Task<bool> MoveNextAsync(IAsyncEnumerator<BlobItem> iterator, CancellationToken token)
    {
        try
        {
            bool found = await ExecuteAsync(() => iterator.MoveNextAsync().AsTask(), token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            return found;
        }
        catch (FileNotFoundException) { return false; }
    }

    private string ObjectName(string name)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ValidatePrefix(name);
        if (name.Split('/').Any(segment => segment is "." or "..")) throw new ArgumentException("Dot path segments are not supported.", nameof(name));
        return name;
    }
    private void ValidatePrefix(string prefix)
    {
        ThrowIfDisposed();
        if (prefix.Length > 1024 || prefix.StartsWith('/') || prefix.Contains('\\') || prefix.Any(char.IsControl))
            throw new ArgumentException("Use a container-relative blob name/prefix up to 1024 characters without backslashes or controls.", nameof(prefix));
    }
    private string LegacyName(string path)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string name = path.Replace('\\', '/').TrimStart('/');
        while (name.Contains("//", StringComparison.Ordinal)) name = name.Replace("//", "/", StringComparison.Ordinal);
        return ObjectName(name);
    }
    private string DirectoryPrefix(string path) => LegacyName(path).TrimEnd('/') + "/";
    private static bool IsMarker(string name) => name.EndsWith("/.directory", StringComparison.Ordinal);
    private static bool MatchesPattern(string value, string pattern) => Regex.IsMatch(value,
        "\\A" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "\\z",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
}
