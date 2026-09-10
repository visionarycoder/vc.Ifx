using FluentFTP;
using FluentFTP.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;

namespace VisionaryCoder.Framework.Storage.Ftp;

/// <summary>FTP access with operation-owned clients and no provider-level retries.</summary>
/// <remarks>Object transfers are buffered in memory. The root is lexical scope, not a server sandbox.</remarks>
public sealed class FtpStorageProvider : ServiceBase<FtpStorageProvider>, IStorageProvider, IObjectStorageProvider
{
    private readonly FtpStorageOptions options;
    private readonly Func<IAsyncFtpClient> clientFactory;
    private readonly string root;

    /// <summary>Creates a network-free provider; connections open only during operations.</summary>
    public FtpStorageProvider(FtpStorageOptions options, ILogger<FtpStorageProvider> logger)
        : this(options, logger, () => CreateClient(options)) { }

    /// <summary>Injects FluentFTP's existing interface. Each result must be fresh, disconnected and exclusively owned.</summary>
    public FtpStorageProvider(FtpStorageOptions options, ILogger<FtpStorageProvider> logger, Func<IAsyncFtpClient> clientFactory)
        : base(logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        options.Validate();
        this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        root = options.RootPath.TrimEnd('/') + "/";
    }

    /// <inheritdoc />
    public StorageCapabilities Capabilities => StorageCapabilities.Read | StorageCapabilities.Write | StorageCapabilities.Delete | StorageCapabilities.Metadata | StorageCapabilities.List;

    /// <inheritdoc />
    public Task<Stream> OpenReadAsync(StorageObjectRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ReadAsync(ObjectPath(request.Path), true, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<StorageObjectMetadata> WriteAsync(StorageWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = ObjectPath(request.Path);
        cancellationToken.ThrowIfCancellationRequested();
        if (!request.Overwrite) throw new NotSupportedException("FTP cannot atomically create only at a specified key.");
        await UploadAsync(path, request.Content, true, cancellationToken).ConfigureAwait(false);
        return new StorageObjectMetadata(request.Path);
    }

    /// <inheritdoc />
    public Task<StorageObjectMetadata?> GetMetadataAsync(StorageObjectRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = ObjectPath(request.Path);
        return ExecuteAsync(async client =>
        {
            FtpListItem? item = await FindAsync(client, path, cancellationToken).ConfigureAwait(false);
            return item is null ? null : Metadata(request.Path, item);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteAsync(StorageObjectRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return DeletePathAsync(ObjectPath(request.Path), cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StorageObjectMetadata> ListAsync(StorageListRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfDisposed();
        if (request.Prefix.Length > 0) ObjectPath(request.Prefix + "x");
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            FtpListItem[] items = await ListingAsync(pending.Pop(), cancellationToken).ConfigureAwait(false);
            foreach (FtpListItem item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string key = item.FullName[root.Length..];
                if (item.Type == FtpObjectType.Directory) pending.Push(item.FullName);
                else if (item.Type == FtpObjectType.File && key.StartsWith(request.Prefix, StringComparison.Ordinal))
                    yield return Metadata(key, item);
            }
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    public bool FileExists(string path)
    {
        string normalized = LegacyPath(path);
        return ExecuteAsync(async client => await FindAsync(client, normalized, default).ConfigureAwait(false) is not null, default).GetAwaiter().GetResult();
    }
    public bool FileExists(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return FileExists(fileInfo.FullName);
    }
    public string ReadAllText(string path) => ReadAllTextAsync(path).GetAwaiter().GetResult();
    public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
        => Encoding.UTF8.GetString(await ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false));
    public byte[] ReadAllBytes(string path) => ReadAllBytesAsync(path).GetAwaiter().GetResult();
    public async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        using Stream stream = await ReadAsync(LegacyPath(path), false, cancellationToken).ConfigureAwait(false);
        return ((MemoryStream)stream).ToArray();
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
        using var stream = new MemoryStream(bytes, writable: false);
        await UploadAsync(LegacyPath(path), stream, false, cancellationToken).ConfigureAwait(false);
    }
    public void DeleteFile(string path) => DeleteFileAsync(path).GetAwaiter().GetResult();
    public Task DeleteFileAsync(string path, CancellationToken cancellationToken = default) => DeletePathAsync(LegacyPath(path), cancellationToken);
    public bool DirectoryExists(string path)
    {
        string normalized = LegacyPath(path);
        return ExecuteAsync(client => client.DirectoryExists(normalized, default), default).GetAwaiter().GetResult();
    }
    public DirectoryInfo CreateDirectory(string path) => CreateDirectoryAsync(path).GetAwaiter().GetResult();
    public async Task<DirectoryInfo> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        string normalized = LegacyPath(path);
        var result = new DirectoryInfo(normalized);
        await ExecuteAsync(client => client.CreateDirectory(normalized, true, cancellationToken), cancellationToken).ConfigureAwait(false);
        return result;
    }
    public void DeleteDirectory(string path, bool recursive = true) => DeleteDirectoryAsync(path, recursive).GetAwaiter().GetResult();
    public Task DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default)
    {
        string normalized = LegacyPath(path);
        if (normalized == "/") throw new ArgumentException("Deleting the server root is not allowed.", nameof(path));
        return ExecuteAsync(async client =>
        {
            if (!recursive)
            {
                // RMD, unlike recursive SDK deletion, cannot remove racing new children.
                FtpReply reply = await client.Execute("RMD " + normalized, cancellationToken).ConfigureAwait(false);
                EnsureSuccess(reply);
            }
            else await client.DeleteDirectory(normalized, cancellationToken).ConfigureAwait(false);
            return true;
        }, cancellationToken);
    }
    public string[] GetFiles(string path, string searchPattern = "*") => NamesAsync(path, searchPattern, FtpObjectType.File, default).GetAwaiter().GetResult();
    public string[] GetDirectories(string path, string searchPattern = "*") => NamesAsync(path, searchPattern, FtpObjectType.Directory, default).GetAwaiter().GetResult();
    public async IAsyncEnumerable<string> EnumerateFilesAsync(string path, string searchPattern = "*", [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (string name in await NamesAsync(path, searchPattern, FtpObjectType.File, cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return name;
        }
        cancellationToken.ThrowIfCancellationRequested();
    }
    public string GetFullPath(string path) => new UriBuilder(options.ServerUri) { Path = LegacyPath(path) }.Uri.AbsoluteUri;
    public string? GetDirectoryName(string path)
    {
        string normalized = LegacyPath(path);
        if (normalized == "/") return null;
        int separator = normalized.LastIndexOf('/');
        return separator == 0 ? "/" : normalized[..separator];
    }
    public string GetFileName(string path)
    {
        string normalized = LegacyPath(path);
        return normalized[(normalized.LastIndexOf('/') + 1)..];
    }

    internal static IAsyncFtpClient CreateClient(FtpStorageOptions options)
    {
        var client = new AsyncFtpClient(options.Host, new NetworkCredential(options.Username, options.Password), options.Port);
        client.Config.EncryptionMode = options.UseSsl ? FtpEncryptionMode.Explicit : FtpEncryptionMode.None;
        client.Config.DataConnectionType = options.UsePassive ? FtpDataConnectionType.PASV : FtpDataConnectionType.PORT;
        client.Config.SocketKeepAlive = options.KeepAlive;
        client.Config.ConnectTimeout = options.TimeoutMilliseconds;
        client.Config.ReadTimeout = options.TimeoutMilliseconds;
        client.Config.DataConnectionConnectTimeout = options.TimeoutMilliseconds;
        client.Config.DataConnectionReadTimeout = options.TimeoutMilliseconds;
        client.Config.TransferChunkSize = options.BufferSize;
        client.Config.RetryAttempts = 1;
        client.Config.TimeConversion = FtpDate.ServerTime;
        client.Config.UploadDataType = options.UseBinary ? FtpDataType.Binary : FtpDataType.ASCII;
        client.Config.DownloadDataType = options.UseBinary ? FtpDataType.Binary : FtpDataType.ASCII;
        return client;
    }

    private async Task<T> ExecuteAsync<T>(Func<IAsyncFtpClient, Task<T>> action, CancellationToken token)
    {
        ThrowIfDisposed();
        token.ThrowIfCancellationRequested();
        try
        {
            using IAsyncFtpClient client = clientFactory() ?? throw new InvalidOperationException("The FTP client factory returned null.");
            await client.Connect(token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            T result = await action(client).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            return result;
        }
        catch (FtpAuthenticationException exception) { throw new UnauthorizedAccessException("FTP authentication failed.", exception); }
        catch (AuthenticationException exception) { throw new UnauthorizedAccessException("FTP TLS authentication failed.", exception); }
        catch (FtpCommandException exception) when (exception.CompletionCode is "530" or "532") { throw new UnauthorizedAccessException("FTP authentication is required.", exception); }
        catch (FtpException exception) { throw new IOException("FTP operation failed.", exception); }
        catch (SocketException exception) { throw new IOException("FTP connection failed.", exception); }
        catch (TimeoutException exception) { throw new IOException("FTP operation timed out.", exception); }
    }

    private Task<Stream> ReadAsync(string path, bool binary, CancellationToken token) => ExecuteAsync<Stream>(async client =>
    {
        if (await FindAsync(client, path, token).ConfigureAwait(false) is null) throw new FileNotFoundException("FTP file was absent from its parent listing.", path);
        if (binary) client.Config.DownloadDataType = FtpDataType.Binary;
        var buffer = new MemoryStream();
        try
        {
            if (!await client.DownloadStream(buffer, path, 0, null, token, 0).ConfigureAwait(false)) throw new IOException("FTP download failed.");
            token.ThrowIfCancellationRequested();
            buffer.Position = 0;
            return buffer;
        }
        catch { buffer.Dispose(); throw; }
    }, token);

    private async Task UploadAsync(string path, Stream content, bool binary, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, options.BufferSize, token).ConfigureAwait(false);
        buffer.Position = 0;
        await ExecuteAsync(async client =>
        {
            if (binary) client.Config.UploadDataType = FtpDataType.Binary;
            if (await client.UploadStream(buffer, path, FtpRemoteExists.Overwrite, true, null, token).ConfigureAwait(false) != FtpStatus.Success)
                throw new IOException("FTP upload did not complete successfully.");
            return true;
        }, token).ConfigureAwait(false);
    }

    private Task DeletePathAsync(string path, CancellationToken token) => ExecuteAsync(async client =>
    {
        if (await FindAsync(client, path, token).ConfigureAwait(false) is not null) await client.DeleteFile(path, token).ConfigureAwait(false);
        return true;
    }, token);

    private static async Task<FtpListItem?> FindAsync(IAsyncFtpClient client, string path, CancellationToken token)
    {
        string parent = path[..(path.LastIndexOf('/') + 1)];
        return (await ReadListingAsync(client, parent, token).ConfigureAwait(false)).FirstOrDefault(item => item.Type == FtpObjectType.File && item.FullName == path);
    }

    private Task<FtpListItem[]> ListingAsync(string path, CancellationToken token) => ExecuteAsync(client => ReadListingAsync(client, path, token), token);

    private static async Task<FtpListItem[]> ReadListingAsync(IAsyncFtpClient client, string path, CancellationToken token)
    {
        // STAT avoids data-channel listing errors that FluentFTP intentionally suppresses.
        FtpListItem[] items = await client.GetListing(path, FtpListOption.UseStat, token).ConfigureAwait(false);
        EnsureSuccess(client.LastReply);
        if (client.LastReply.Code is not ("212" or "213")) throw new IOException("FTP server did not return a path status listing.");
        string prefix = path.TrimEnd('/') + "/";
        foreach (FtpListItem item in items)
        {
            if (!item.FullName.StartsWith(prefix, StringComparison.Ordinal)) throw new IOException("FTP listing escaped its requested directory.");
            string name = item.FullName[prefix.Length..];
            if (name.Contains('/') || name is "" or "." or ".." || name.Contains('\\') || name.Contains(':') || name.Any(char.IsControl)) throw new IOException("FTP listing returned an invalid child path.");
        }
        return items;
    }

    private static void EnsureSuccess(FtpReply reply)
    {
        if (!reply.Success) throw new FtpCommandException(reply);
    }

    private async Task<string[]> NamesAsync(string path, string pattern, FtpObjectType type, CancellationToken token)
    {
        FtpListItem[] items = await ListingAsync(LegacyPath(path), token).ConfigureAwait(false);
        return items.Where(item => item.Type == type && MatchesPattern(item.Name, pattern)).Select(item => item.FullName).ToArray();
    }

    private static bool MatchesPattern(string value, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern) || pattern == "*") return true;
        return Regex.IsMatch(value, "\\A" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "\\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
    }

    private static StorageObjectMetadata Metadata(string key, FtpListItem item) => new(key,
        item.Size > 0 ? item.Size : null,
        item.Modified.Kind == DateTimeKind.Utc ? new DateTimeOffset(item.Modified) : null);

    private string ObjectPath(string key)
    {
        ThrowIfDisposed();
        FtpStorageOptions.ValidateText(key, nameof(key));
        if (key.StartsWith('/') || key.Contains('\\') || key.Contains(':')) throw new ArgumentException("Object keys must be slash-relative FTP paths.", nameof(key));
        FtpStorageOptions.ValidateSegments(key);
        return root + key;
    }

    private string LegacyPath(string path)
    {
        ThrowIfDisposed();
        FtpStorageOptions.ValidateText(path, nameof(path));
        string normalized = path.Replace('\\', '/').Trim('/');
        if (normalized.Contains(':')) throw new ArgumentException("Use server paths, not local drive paths or URIs.", nameof(path));
        FtpStorageOptions.ValidateSegments(normalized);
        return "/" + normalized;
    }
}
