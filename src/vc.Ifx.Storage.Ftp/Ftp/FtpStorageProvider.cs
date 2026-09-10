using FluentFTP;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace VisionaryCoder.Framework.Storage.Ftp;

/// <summary>
/// Provides FTP-based storage operations implementation following Microsoft I/O patterns.
/// This service wraps FluentFTP operations with logging, error handling, and async support.
/// Supports both standard FTP and secure FTPS protocols.
/// </summary>
public sealed class FtpStorageProvider : ServiceBase<FtpStorageProvider>, IStorageProvider
{
    private static readonly Encoding defaultEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly RegexOptions patternOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    private readonly FtpStorageOptions options;

    public FtpStorageProvider(FtpStorageOptions options, ILogger<FtpStorageProvider> logger)
        : base(logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.options.Validate();
    }

    public bool FileExists(string path)
    {
        string normalizedPath = NormalizePath(path);
        using FtpClient client = CreateClient();
        client.Connect();
        return client.FileExists(normalizedPath);
    }

    public bool FileExists(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return FileExists(fileInfo.FullName);
    }

    public string ReadAllText(string path)
    {
        byte[] bytes = ReadAllBytes(path);
        return defaultEncoding.GetString(bytes);
    }

    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default) => Task.Run(() => ReadAllText(path), cancellationToken);

    public byte[] ReadAllBytes(string path)
    {
        string normalizedPath = NormalizePath(path);
        using FtpClient client = CreateClient();
        client.Connect();
        if (!client.DownloadBytes(out byte[]? data, normalizedPath))
        {
            throw new FileNotFoundException($"The file '{normalizedPath}' does not exist on the FTP server.", normalizedPath);
        }

        return data;
    }

    public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default) => Task.Run(() => ReadAllBytes(path), cancellationToken);

    public void WriteAllText(string path, string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        WriteAllBytes(path, defaultEncoding.GetBytes(content));
    }

    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default) => Task.Run(() => WriteAllText(path, content), cancellationToken);

    public void WriteAllBytes(string path, byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        string normalizedPath = NormalizePath(path);
        using FtpClient client = CreateClient();
        client.Connect();
        EnsureDirectory(client, normalizedPath);
        FtpStatus status = client.UploadBytes(bytes, normalizedPath, FtpRemoteExists.Overwrite, true);
        if (status == FtpStatus.Failed)
        {
            throw new IOException($"Failed to upload file '{normalizedPath}' to the FTP server.");
        }
    }

    public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default) => Task.Run(() => WriteAllBytes(path, bytes), cancellationToken);

    public void DeleteFile(string path)
    {
        string normalizedPath = NormalizePath(path);
        using FtpClient client = CreateClient();
        client.Connect();
        client.DeleteFile(normalizedPath);
    }

    public Task DeleteFileAsync(string path, CancellationToken cancellationToken = default) => Task.Run(() => DeleteFile(path), cancellationToken);

    public bool DirectoryExists(string path)
    {
        string normalizedPath = NormalizeDirectoryPath(path);
        using FtpClient client = CreateClient();
        client.Connect();
        return client.DirectoryExists(normalizedPath);
    }

    public DirectoryInfo CreateDirectory(string path)
    {
        string normalizedPath = NormalizeDirectoryPath(path);
        using FtpClient client = CreateClient();
        client.Connect();
        client.CreateDirectory(normalizedPath, true);
        return new DirectoryInfo(path);
    }

    public Task<DirectoryInfo> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        => Task.Run(() => CreateDirectory(path), cancellationToken);

    public void DeleteDirectory(string path, bool recursive = true)
    {
        string normalizedPath = NormalizeDirectoryPath(path);
        using FtpClient client = CreateClient();
        client.Connect();
        DeleteDirectoryInternal(client, normalizedPath, recursive);
    }

    public Task DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default)
        => Task.Run(() => DeleteDirectory(path, recursive), cancellationToken);

    public string[] GetFiles(string path, string searchPattern = "*")
    {
        string normalizedPath = NormalizeDirectoryPath(path);
        using FtpClient client = CreateClient();
        client.Connect();
        FtpListItem[]? items = client.GetListing(normalizedPath);
        return items
            .Where(item => item.Type == FtpObjectType.File && MatchesPattern(item.Name, searchPattern))
            .Select(item => item.FullName)
            .ToArray();
    }

    public string[] GetDirectories(string path, string searchPattern = "*")
    {
        string normalizedPath = NormalizeDirectoryPath(path);
        using FtpClient client = CreateClient();
        client.Connect();
        FtpListItem[]? items = client.GetListing(normalizedPath);
        return items
            .Where(item => item.Type == FtpObjectType.Directory && MatchesPattern(item.Name, searchPattern))
            .Select(item => item.FullName)
            .ToArray();
    }

    public async IAsyncEnumerable<string> EnumerateFilesAsync(string path, string searchPattern = "*", [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string[] files = await Task.Run(() => GetFiles(path, searchPattern), cancellationToken).ConfigureAwait(false);
        foreach (string file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return file;
        }
    }

    public string GetFullPath(string path)
    {
        string normalizedPath = NormalizePath(path);
        var serverUri = new Uri(options.ServerUri, UriKind.Absolute);
        return new Uri(serverUri, normalizedPath.TrimStart('/')).ToString();
    }

    public string? GetDirectoryName(string path)
    {
        string normalized = NormalizePath(path);
        string? directory = Path.GetDirectoryName(normalized.Replace('/', Path.DirectorySeparatorChar));
        return directory?.Replace(Path.DirectorySeparatorChar, '/');
    }

    public string GetFileName(string path)
    {
        string normalized = NormalizePath(path);
        return Path.GetFileName(normalized);
    }

    private FtpClient CreateClient()
    {
        var client = new FtpClient(options.Host)
        {
            Port = options.Port,
            Credentials = new NetworkCredential(options.Username, options.Password)
        };

        client.Config.EncryptionMode = options.UseSsl ? FtpEncryptionMode.Explicit : FtpEncryptionMode.None;
        client.Config.DataConnectionType = options.UsePassive ? FtpDataConnectionType.PASV : FtpDataConnectionType.PORT;
        client.Config.SocketKeepAlive = options.KeepAlive;
        client.Config.ConnectTimeout = options.TimeoutMilliseconds;
        client.Config.ReadTimeout = options.TimeoutMilliseconds;
        client.Config.DataConnectionConnectTimeout = options.TimeoutMilliseconds;
        client.Config.DataConnectionReadTimeout = options.TimeoutMilliseconds;
        client.Config.UploadDataType = options.UseBinary ? FtpDataType.Binary : FtpDataType.ASCII;
        client.Config.DownloadDataType = options.UseBinary ? FtpDataType.Binary : FtpDataType.ASCII;

        return client;
    }

    private void EnsureDirectory(FtpClient client, string normalizedFilePath)
    {
        string? directory = GetDirectoryFromFilePath(normalizedFilePath);
        if (string.IsNullOrEmpty(directory) || directory == "/")
        {
            return;
        }

        client.CreateDirectory(directory, true);
    }

    private void DeleteDirectoryInternal(FtpClient client, string path, bool recursive)
    {
        if (!client.DirectoryExists(path))
        {
            return;
        }

        if (!recursive)
        {
            if (client.GetListing(path).Any())
            {
                throw new IOException($"The directory '{path}' is not empty.");
            }

            client.DeleteDirectory(path);
            return;
        }

        foreach (FtpListItem? item in client.GetListing(path))
        {
            if (item.Type == FtpObjectType.File)
            {
                client.DeleteFile(item.FullName);
            }
            else if (item.Type == FtpObjectType.Directory)
            {
                DeleteDirectoryInternal(client, item.FullName, true);
            }
        }

        client.DeleteDirectory(path);
    }

    private static string? GetDirectoryFromFilePath(string normalizedFilePath)
    {
        string? directory = Path.GetDirectoryName(normalizedFilePath.Replace('/', Path.DirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(directory))
        {
            return null;
        }

        return NormalizeDirectoryString(directory);
    }

    private static string NormalizeDirectoryPath(string path)
    {
        string normalized = NormalizePath(path);
        return normalized == "/" ? normalized : normalized.TrimEnd('/');
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));

        if (Uri.TryCreate(path, UriKind.Absolute, out Uri? uri) &&
            (uri.Scheme.Equals(Uri.UriSchemeFtp, StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals("ftps", StringComparison.OrdinalIgnoreCase)))
        {
            path = uri.AbsolutePath;
        }

        string normalized = path.Replace('\\', '/');
        normalized = Regex.Replace(normalized, "/+", "/").Trim();

        if (normalized.Length == 0 || normalized == "/")
        {
            return "/";
        }

        normalized = normalized.Trim('/');
        return "/" + normalized;
    }

    private static string NormalizeDirectoryString(string directory)
    {
        string normalized = directory.Replace('\\', '/');
        normalized = Regex.Replace(normalized, "/+", "/").Trim();

        if (normalized.Length == 0 || normalized == "/")
        {
            return "/";
        }

        normalized = normalized.Trim('/');
        return "/" + normalized;
    }

    private static bool MatchesPattern(string value, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern) || pattern == "*")
        {
            return true;
        }

        string regexPattern = Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".");

        return Regex.IsMatch(value, $"^{regexPattern}$", patternOptions);
    }
}
