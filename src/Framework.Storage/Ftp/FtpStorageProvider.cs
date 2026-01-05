using System.Runtime.CompilerServices;
using System.Text;
using FluentFTP;
using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Storage.Abstractions;

namespace VisionaryCoder.Framework.Storage.Ftp;

/// <summary>
/// Provides FTP/FTPS-based storage operations implementation.
/// Supports both FTP and FTPS (FTP over SSL/TLS) protocols with connection pooling.
/// </summary>
public sealed class FtpStorageProvider : StorageProviderBase<FtpStorageProvider>, IDisposable
{
    private static readonly Encoding DefaultEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly FtpStorageOptions options;
    private readonly IAsyncFtpClient client;

    /// <summary>
    /// Initializes a new instance of <see cref="FtpStorageProvider"/> with automatic client creation.
    /// </summary>
    /// <param name="options">FTP storage configuration options.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    public FtpStorageProvider(FtpStorageOptions options, ILogger<FtpStorageProvider> logger)
        : base(logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.options.Validate();

        client = new AsyncFtpClient(options.Host, options.Username, options.Password, options.Port)
        {
            Config = new FtpConfig
            {
                EncryptionMode = options.EncryptionMode,
                DataConnectionType = options.DataConnectionType,
                ValidateAnyCertificate = !options.ValidateCertificate,
                ConnectTimeout = options.ConnectTimeout * 1000,
                DataConnectionConnectTimeout = options.DataConnectionTimeout * 1000,
                ReadTimeout = options.ReadTimeout * 1000,
                RetryAttempts = options.RetryAttempts
            }
        };

        Logger.LogInformation("FTP storage provider initialized for host '{Host}:{Port}' with encryption mode '{EncryptionMode}'", options.Host, options.Port, options.EncryptionMode);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FtpStorageProvider"/> with an injected FTP client.
    /// This constructor enables dependency injection and unit testing with mocked clients.
    /// </summary>
    /// <param name="options">FTP storage configuration options.</param>
    /// <param name="client">Pre-configured IAsyncFtpClient instance.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    public FtpStorageProvider(FtpStorageOptions options, IAsyncFtpClient client, ILogger<FtpStorageProvider> logger)
        : base(logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.client = client ?? throw new ArgumentNullException(nameof(client));
        this.options.Validate();

        Logger.LogInformation("FTP storage provider initialized with injected client");
    }

    public override bool FileExists(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return FileExists(fileInfo.FullName);
    }

    public override bool FileExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return ExecuteWithLogging(() =>
        {
            string ftpPath = ResolvePath(path);
            EnsureConnected();
            return client.FileExists(ftpPath).GetAwaiter().GetResult();
        }, "FileExists", path);
    }

    public override string ReadAllText(string path)
    {
        byte[] bytes = ReadAllBytes(path);
        return DefaultEncoding.GetString(bytes);
    }

    public override async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        byte[] bytes = await ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return DefaultEncoding.GetString(bytes);
    }

    public override byte[] ReadAllBytes(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return ExecuteWithLogging(() =>
        {
            string ftpPath = ResolvePath(path);
            EnsureConnected();

            if (!client.FileExists(ftpPath).GetAwaiter().GetResult())
            {
                throw new FileNotFoundException($"The file '{ftpPath}' does not exist on FTP server.", path);
            }

            using var memoryStream = new MemoryStream();
            client.DownloadStream(memoryStream, ftpPath).GetAwaiter().GetResult();
            return memoryStream.ToArray();
        }, "ReadAllBytes", path);
    }

    public override async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return await ExecuteWithLoggingAsync(async () =>
        {
            string ftpPath = ResolvePath(path);
            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

            if (!await client.FileExists(ftpPath, cancellationToken).ConfigureAwait(false))
            {
                throw new FileNotFoundException($"The file '{ftpPath}' does not exist on FTP server.", path);
            }

            using var memoryStream = new MemoryStream();
            await client.DownloadStream(memoryStream, ftpPath, 0, null, cancellationToken).ConfigureAwait(false);
            return memoryStream.ToArray();
        }, "ReadAllBytesAsync", path).ConfigureAwait(false);
    }

    public override void WriteAllText(string path, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        WriteAllBytes(path, DefaultEncoding.GetBytes(content));
    }

    public override async Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        await WriteAllBytesAsync(path, DefaultEncoding.GetBytes(content), cancellationToken).ConfigureAwait(false);
    }

    public override void WriteAllBytes(string path, byte[] bytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(bytes);

        ExecuteWithLogging(() =>
        {
            string ftpPath = ResolvePath(path);
            EnsureConnected();

            // Ensure parent directory exists
            string? directory = GetDirectoryName(ftpPath);
            if (!string.IsNullOrEmpty(directory))
            {
                client.CreateDirectory(directory).GetAwaiter().GetResult();
            }

            using var memoryStream = new MemoryStream(bytes);
            client.UploadStream(memoryStream, ftpPath, FtpRemoteExists.Overwrite).GetAwaiter().GetResult();
            return true;
        }, "WriteAllBytes", path);
    }

    public override async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(bytes);

        await ExecuteWithLoggingAsync(async () =>
        {
            string ftpPath = ResolvePath(path);
            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

            // Ensure parent directory exists
            string? directory = GetDirectoryName(ftpPath);
            if (!string.IsNullOrEmpty(directory))
            {
                await client.CreateDirectory(directory, cancellationToken).ConfigureAwait(false);
            }

            using var memoryStream = new MemoryStream(bytes);
            await client.UploadStream(memoryStream, ftpPath, FtpRemoteExists.Overwrite, false, null, cancellationToken).ConfigureAwait(false);
        }, "WriteAllBytesAsync", path).ConfigureAwait(false);
    }

    public override void DeleteFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        ExecuteWithLogging(() =>
        {
            string ftpPath = ResolvePath(path);
            EnsureConnected();
            client.DeleteFile(ftpPath);
            return true;
        }, "DeleteFile", path);
    }

    public override async Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        await ExecuteWithLoggingAsync(async () =>
        {
            string ftpPath = ResolvePath(path);
            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
            await client.DeleteFile(ftpPath, cancellationToken).ConfigureAwait(false);
        }, "DeleteFileAsync", path).ConfigureAwait(false);
    }

    public override bool DirectoryExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return ExecuteWithLogging(() =>
        {
            string ftpPath = ResolvePath(path);
            EnsureConnected();
            return client.DirectoryExists(ftpPath).GetAwaiter().GetResult();
        }, "DirectoryExists", path);
    }

    public override DirectoryInfo CreateDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return ExecuteWithLogging(() =>
        {
            string ftpPath = ResolvePath(path);
            EnsureConnected();
            client.CreateDirectory(ftpPath, true);
            return new DirectoryInfo(ftpPath);
        }, "CreateDirectory", path);
    }

    public override async Task<DirectoryInfo> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return await ExecuteWithLoggingAsync(async () =>
        {
            string ftpPath = ResolvePath(path);
            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
            await client.CreateDirectory(ftpPath, cancellationToken).ConfigureAwait(false);
            return new DirectoryInfo(ftpPath);
        }, "CreateDirectoryAsync", path).ConfigureAwait(false);
    }

    public override void DeleteDirectory(string path, bool recursive = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        ExecuteWithLogging(() =>
        {
            string ftpPath = ResolvePath(path);
            EnsureConnected();

            if (!recursive)
            {
                // Check if directory is empty
                FtpListItem[] items = client.GetListing(ftpPath).GetAwaiter().GetResult();
                if (items.Length > 0)
                {
                    throw new IOException($"The directory '{path}' is not empty.");
                }
            }

            client.DeleteDirectory(ftpPath).GetAwaiter().GetResult();
            return true;
        }, "DeleteDirectory", path);
    }

    public override async Task DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        await ExecuteWithLoggingAsync(async () =>
        {
            string ftpPath = ResolvePath(path);
            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

            if (!recursive)
            {
                // Check if directory is empty
                FtpListItem[] items = await client.GetListing(ftpPath, cancellationToken).ConfigureAwait(false);
                if (items.Length > 0)
                {
                    throw new IOException($"The directory '{path}' is not empty.");
                }
            }

            await client.DeleteDirectory(ftpPath, cancellationToken).ConfigureAwait(false);
        }, "DeleteDirectoryAsync", path).ConfigureAwait(false);
    }

    public override string[] GetFiles(string path, string searchPattern = "*")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        return ExecuteWithLogging(() =>
        {
            string ftpPath = ResolvePath(path);
            EnsureConnected();

            FtpListItem[] items = client.GetListing(ftpPath).GetAwaiter().GetResult();
            return items
                .Where(item => item.Type == FtpObjectType.File)
                .Where(item => MatchesPattern(item.Name, searchPattern))
                .Select(item => item.FullName)
                .ToArray();
        }, "GetFiles", path);
    }

    public override string[] GetDirectories(string path, string searchPattern = "*")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        return ExecuteWithLogging(() =>
        {
            string ftpPath = ResolvePath(path);
            EnsureConnected();

            FtpListItem[] items = client.GetListing(ftpPath).GetAwaiter().GetResult();
            return items
                .Where(item => item.Type == FtpObjectType.Directory)
                .Where(item => MatchesPattern(item.Name, searchPattern))
                .Select(item => item.FullName)
                .ToArray();
        }, "GetDirectories", path);
    }

    public override async IAsyncEnumerable<string> EnumerateFilesAsync(string path, string searchPattern = "*",
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        Logger.LogDebug("Enumerating files async from '{Path}' with pattern '{Pattern}'", path, searchPattern);

        string ftpPath = ResolvePath(path);
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

        FtpListItem[] items = await client.GetListing(ftpPath, cancellationToken).ConfigureAwait(false);

        foreach (FtpListItem item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (item.Type == FtpObjectType.File && MatchesPattern(item.Name, searchPattern))
            {
                yield return item.FullName;
            }
        }
    }

    public override string GetFullPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return ResolvePath(path);
    }

    public override string? GetDirectoryName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string normalized = path.Replace('\\', '/').TrimEnd('/');
        int lastSlashIndex = normalized.LastIndexOf('/');

        if (lastSlashIndex <= 0)
        {
            return "/";
        }

        return normalized[..lastSlashIndex];
    }

    public override string GetFileName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string normalized = path.Replace('\\', '/').TrimEnd('/');
        int lastSlashIndex = normalized.LastIndexOf('/');

        return lastSlashIndex >= 0 ? normalized[(lastSlashIndex + 1)..] : normalized;
    }

    private string ResolvePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
        }

        // Normalize path separators
        string normalized = path.Replace('\\', '/');

        // Remove leading slash if present (will be combined with root)
        normalized = normalized.TrimStart('/');

        // Combine with root directory
        string fullPath = options.RootDirectory == "/" 
            ? "/" + normalized 
            : options.RootDirectory + "/" + normalized;

        // Normalize the combined path
        fullPath = NormalizePath(fullPath);

        // Security check: ensure path doesn't escape root directory
        if (options.RestrictToRootDirectory)
        {
            if (!fullPath.StartsWith(options.RootDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException(
                    $"Access denied. Path '{path}' resolves to '{fullPath}' which is outside the root directory '{options.RootDirectory}'.");
            }
        }

        return fullPath;
    }

    private static string NormalizePath(string path)
    {
        // Split path and process each segment
        string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var normalizedSegments = new List<string>();

        foreach (string segment in segments)
        {
            if (segment == ".")
            {
                continue; // Skip current directory references
            }

            if (segment == "..")
            {
                // Go up one level if possible
                if (normalizedSegments.Count > 0)
                {
                    normalizedSegments.RemoveAt(normalizedSegments.Count - 1);
                }
            }
            else
            {
                normalizedSegments.Add(segment);
            }
        }

        return "/" + string.Join("/", normalizedSegments);
    }

    private static bool MatchesPattern(string value, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern) || pattern == "*")
        {
            return true;
        }

        // Simple pattern matching for * and ? wildcards
        string regexPattern = "^" + pattern.Replace(".", "\\.").Replace("*", ".*").Replace("?", ".") + "$";

        return System.Text.RegularExpressions.Regex.IsMatch(value, regexPattern, 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private void EnsureConnected()
    {
        if (!client.IsConnected)
        {
            Logger.LogDebug("Connecting to FTP server '{Host}:{Port}'", options.Host, options.Port);
            client.Connect();
            Logger.LogTrace("Connected to FTP server '{Host}:{Port}'", options.Host, options.Port);
        }
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
    {
        if (!client.IsConnected)
        {
            Logger.LogDebug("Connecting async to FTP server '{Host}:{Port}'", options.Host, options.Port);
            await client.Connect(cancellationToken).ConfigureAwait(false);
            Logger.LogTrace("Connected async to FTP server '{Host}:{Port}'", options.Host, options.Port);
        }
    }

    public void Dispose()
    {
        try
        {
            if (client.IsConnected)
            {
                Logger.LogDebug("Disconnecting from FTP server '{Host}:{Port}'", options.Host, options.Port);
                client.Disconnect();
            }
            client.Dispose();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while disposing FTP client");
        }
    }
}
