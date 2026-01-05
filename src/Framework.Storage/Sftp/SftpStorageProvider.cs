// Copyright (c) VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using VisionaryCoder.Framework.Storage.Abstractions;

namespace VisionaryCoder.Framework.Storage.Sftp;

/// <summary>
/// SFTP storage provider implementation using SSH.NET library.
/// </summary>
public sealed class SftpStorageProvider : StorageProviderBase<SftpStorageProvider>, IDisposable
{
    private readonly SftpStorageOptions options;
    private ISftpClient? client;
    private readonly bool clientInjected;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of <see cref="SftpStorageProvider"/> with automatic client creation.
    /// </summary>
    /// <param name="options">SFTP storage configuration options.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    public SftpStorageProvider(SftpStorageOptions options, ILogger<SftpStorageProvider> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        this.options = options;
        clientInjected = false;

        Logger.LogInformation("SFTP Storage Provider initialized for {Host}:{Port}", options.Host, options.Port);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SftpStorageProvider"/> with an injected SFTP client.
    /// This constructor enables dependency injection and unit testing with mocked clients.
    /// </summary>
    /// <param name="options">SFTP storage configuration options.</param>
    /// <param name="client">Pre-configured ISftpClient instance.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    public SftpStorageProvider(SftpStorageOptions options, ISftpClient client, ILogger<SftpStorageProvider> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(client);
        options.Validate();
        this.options = options;
        this.client = client;
        clientInjected = true;

        Logger.LogInformation("SFTP Storage Provider initialized with injected client");
    }

    private ISftpClient Client
    {
        get
        {
            if (client == null)
            {
                client = CreateClient();
            }

            return client;
        }
    }

    private ISftpClient CreateClient()
    {
        ConnectionInfo connectionInfo;

        if (!string.IsNullOrWhiteSpace(options.Password))
        {
            // Password-based authentication
            connectionInfo = new ConnectionInfo(
                options.Host,
                options.Port,
                options.Username,
                new PasswordAuthenticationMethod(options.Username, options.Password));
        }
        else if (!string.IsNullOrWhiteSpace(options.PrivateKeyPath))
        {
            // Key-based authentication
            PrivateKeyFile keyFile = string.IsNullOrWhiteSpace(options.PrivateKeyPassphrase)
                ? new PrivateKeyFile(options.PrivateKeyPath)
                : new PrivateKeyFile(options.PrivateKeyPath, options.PrivateKeyPassphrase);

            connectionInfo = new ConnectionInfo(
                options.Host,
                options.Port,
                options.Username,
                new PrivateKeyAuthenticationMethod(options.Username, keyFile));
        }
        else
        {
            throw new InvalidOperationException("No valid authentication method configured.");
        }

        connectionInfo.Timeout = TimeSpan.FromSeconds(options.ConnectTimeout);

        var sftpClient = new SftpClient(connectionInfo)
        {
            OperationTimeout = TimeSpan.FromSeconds(options.OperationTimeout),
            BufferSize = (uint)options.BufferSize
        };

        if (options.KeepAliveInterval > 0)
        {
            sftpClient.KeepAliveInterval = TimeSpan.FromSeconds(options.KeepAliveInterval);
        }

        return sftpClient;
    }

    private void EnsureConnected()
    {
        if (!Client.IsConnected)
        {
            Client.Connect();
        }
    }

    private async Task EnsureConnectedAsync()
    {
        await Task.Run(() => EnsureConnected()).ConfigureAwait(false);
    }

    private string ResolvePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // Convert Windows paths to Unix paths
        string unixPath = path.Replace('\\', '/');

        // Combine with root directory
        string fullPath = unixPath.StartsWith('/')
            ? unixPath
            : $"{options.RootDirectory}/{unixPath}";

        // Normalize the path
        fullPath = NormalizePath(fullPath);

        // Security check: prevent path traversal
        if (options.RestrictToRootDirectory)
        {
            string normalizedRoot = NormalizePath(options.RootDirectory);
            if (!fullPath.StartsWith(normalizedRoot, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException($"Access to path '{path}' is denied. Path escapes root directory.");
            }
        }

        return fullPath;
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        // Split path and process segments
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var stack = new Stack<string>();

        foreach (string segment in segments)
        {
            if (segment == "..")
            {
                if (stack.Count > 0)
                {
                    _ = stack.Pop();
                }
            }
            else if (segment != ".")
            {
                stack.Push(segment);
            }
        }

        return "/" + string.Join("/", stack.Reverse());
    }

    public override bool FileExists(string path)
    {
        return ExecuteWithLogging(() =>
        {
            string sftpPath = ResolvePath(path);
            EnsureConnected();
            return Client.Exists(sftpPath) && Client.GetAttributes(sftpPath).IsRegularFile;
        }, "FileExists", path);
    }

    public override bool FileExists(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return FileExists(fileInfo.FullName);
    }

    public override string ReadAllText(string path)
    {
        return ExecuteWithLogging(() =>
        {
            byte[] bytes = ReadAllBytes(path);
            return Encoding.UTF8.GetString(bytes);
        }, "ReadAllText", path);
    }

    public override async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithLoggingAsync(async () =>
        {
            byte[] bytes = await ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
            return Encoding.UTF8.GetString(bytes);
        }, "ReadAllTextAsync", path).ConfigureAwait(false);
    }

    public override byte[] ReadAllBytes(string path)
    {
        return ExecuteWithLogging(() =>
        {
            string sftpPath = ResolvePath(path);
            EnsureConnected();

            if (!Client.Exists(sftpPath))
            {
                throw new FileNotFoundException($"The file '{sftpPath}' does not exist on SFTP server.", path);
            }

            using var memoryStream = new MemoryStream();
            Client.DownloadFile(sftpPath, memoryStream);
            return memoryStream.ToArray();
        }, "ReadAllBytes", path);
    }

    public override async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithLoggingAsync(async () =>
        {
            string sftpPath = ResolvePath(path);
            await EnsureConnectedAsync().ConfigureAwait(false);

            bool exists = await Task.Run(() => Client.Exists(sftpPath), cancellationToken).ConfigureAwait(false);
            if (!exists)
            {
                throw new FileNotFoundException($"The file '{sftpPath}' does not exist on SFTP server.", path);
            }

            using var memoryStream = new MemoryStream();
            await Task.Run(() => Client.DownloadFile(sftpPath, memoryStream), cancellationToken).ConfigureAwait(false);
            return memoryStream.ToArray();
        }, "ReadAllBytesAsync", path).ConfigureAwait(false);
    }

    public override void WriteAllText(string path, string contents)
    {
        ExecuteWithLogging(() =>
        {
            byte[] bytes = Encoding.UTF8.GetBytes(contents);
            WriteAllBytes(path, bytes);
            return true;
        }, "WriteAllText", path);
    }

    public override async Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
    {
        await ExecuteWithLoggingAsync(async () =>
        {
            byte[] bytes = Encoding.UTF8.GetBytes(contents);
            await WriteAllBytesAsync(path, bytes, cancellationToken).ConfigureAwait(false);
        }, "WriteAllTextAsync", path).ConfigureAwait(false);
    }

    public override void WriteAllBytes(string path, byte[] bytes)
    {
        ExecuteWithLogging(() =>
        {
            string sftpPath = ResolvePath(path);
            EnsureConnected();

            // Ensure parent directory exists
            string? directory = Path.GetDirectoryName(sftpPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(directory) && directory != "/")
            {
                CreateDirectoryRecursive(directory);
            }

            using var memoryStream = new MemoryStream(bytes);
            Client.UploadFile(memoryStream, sftpPath, true);
            return true;
        }, "WriteAllBytes", path);
    }

    public override async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        await ExecuteWithLoggingAsync(async () =>
        {
            string sftpPath = ResolvePath(path);
            await EnsureConnectedAsync().ConfigureAwait(false);

            // Ensure parent directory exists
            string? directory = Path.GetDirectoryName(sftpPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(directory) && directory != "/")
            {
                await CreateDirectoryRecursiveAsync(directory).ConfigureAwait(false);
            }

            using var memoryStream = new MemoryStream(bytes);
            await Task.Run(() => Client.UploadFile(memoryStream, sftpPath, true), cancellationToken).ConfigureAwait(false);
        }, "WriteAllBytesAsync", path).ConfigureAwait(false);
    }

    public override void DeleteFile(string path)
    {
        ExecuteWithLogging(() =>
        {
            string sftpPath = ResolvePath(path);
            EnsureConnected();

            if (!Client.Exists(sftpPath))
            {
                throw new FileNotFoundException($"The file '{sftpPath}' does not exist.", path);
            }

            Client.DeleteFile(sftpPath);
            return true;
        }, "DeleteFile", path);
    }

    public override async Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        await ExecuteWithLoggingAsync(async () =>
        {
            string sftpPath = ResolvePath(path);
            await EnsureConnectedAsync().ConfigureAwait(false);

            bool exists = await Task.Run(() => Client.Exists(sftpPath), cancellationToken).ConfigureAwait(false);
            if (!exists)
            {
                throw new FileNotFoundException($"The file '{sftpPath}' does not exist.", path);
            }

            await Task.Run(() => Client.DeleteFile(sftpPath), cancellationToken).ConfigureAwait(false);
        }, "DeleteFileAsync", path).ConfigureAwait(false);
    }

    public override bool DirectoryExists(string path)
    {
        return ExecuteWithLogging(() =>
        {
            string sftpPath = ResolvePath(path);
            EnsureConnected();
            return Client.Exists(sftpPath) && Client.GetAttributes(sftpPath).IsDirectory;
        }, "DirectoryExists", path);
    }

    public override DirectoryInfo CreateDirectory(string path)
    {
        return ExecuteWithLogging(() =>
        {
            string sftpPath = ResolvePath(path);
            EnsureConnected();
            CreateDirectoryRecursive(sftpPath);
            return new DirectoryInfo(path);
        }, "CreateDirectory", path);
    }

    public override async Task<DirectoryInfo> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithLoggingAsync(async () =>
        {
            string sftpPath = ResolvePath(path);
            await EnsureConnectedAsync().ConfigureAwait(false);
            await CreateDirectoryRecursiveAsync(sftpPath).ConfigureAwait(false);
            return new DirectoryInfo(path);
        }, "CreateDirectoryAsync", path).ConfigureAwait(false);
    }

    private void CreateDirectoryRecursive(string path)
    {
        if (Client.Exists(path))
        {
            return;
        }

        string? parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        if (!string.IsNullOrEmpty(parent) && parent != "/")
        {
            CreateDirectoryRecursive(parent);
        }

        Client.CreateDirectory(path);
    }

    private async Task CreateDirectoryRecursiveAsync(string path)
    {
        bool exists = await Task.Run(() => Client.Exists(path)).ConfigureAwait(false);
        if (exists)
        {
            return;
        }

        string? parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        if (!string.IsNullOrEmpty(parent) && parent != "/")
        {
            await CreateDirectoryRecursiveAsync(parent).ConfigureAwait(false);
        }

        await Task.Run(() => Client.CreateDirectory(path)).ConfigureAwait(false);
    }

    public override void DeleteDirectory(string path, bool recursive = false)
    {
        ExecuteWithLogging(() =>
        {
            string sftpPath = ResolvePath(path);
            EnsureConnected();

            if (!Client.Exists(sftpPath))
            {
                throw new DirectoryNotFoundException($"The directory '{path}' does not exist.");
            }

            if (!recursive)
            {
                // Check if directory is empty
                var files = Client.ListDirectory(sftpPath)
                    .Where(f => f.Name != "." && f.Name != "..")
                    .ToArray();

                if (files.Length > 0)
                {
                    throw new IOException($"The directory '{path}' is not empty.");
                }
            }
            else
            {
                DeleteDirectoryRecursive(sftpPath);
                return true;
            }

            Client.DeleteDirectory(sftpPath);
            return true;
        }, "DeleteDirectory", path);
    }

    public override async Task DeleteDirectoryAsync(string path, bool recursive = false, CancellationToken cancellationToken = default)
    {
        await ExecuteWithLoggingAsync(async () =>
        {
            string sftpPath = ResolvePath(path);
            await EnsureConnectedAsync().ConfigureAwait(false);

            bool exists = await Task.Run(() => Client.Exists(sftpPath), cancellationToken).ConfigureAwait(false);
            if (!exists)
            {
                throw new DirectoryNotFoundException($"The directory '{path}' does not exist.");
            }

            if (!recursive)
            {
                // Check if directory is empty
                var files = await Task.Run(() =>
                    Client.ListDirectory(sftpPath)
                        .Where(f => f.Name != "." && f.Name != "..")
                        .ToArray(), cancellationToken).ConfigureAwait(false);

                if (files.Length > 0)
                {
                    throw new IOException($"The directory '{path}' is not empty.");
                }
            }
            else
            {
                await DeleteDirectoryRecursiveAsync(sftpPath).ConfigureAwait(false);
                return;
            }

            await Task.Run(() => Client.DeleteDirectory(sftpPath), cancellationToken).ConfigureAwait(false);
        }, "DeleteDirectoryAsync", path).ConfigureAwait(false);
    }

    private void DeleteDirectoryRecursive(string path)
    {
        foreach (ISftpFile file in Client.ListDirectory(path))
        {
            if (file.Name == "." || file.Name == "..")
            {
                continue;
            }

            if (file.IsDirectory)
            {
                DeleteDirectoryRecursive(file.FullName);
            }
            else
            {
                Client.DeleteFile(file.FullName);
            }
        }

        Client.DeleteDirectory(path);
    }

    private async Task DeleteDirectoryRecursiveAsync(string path)
    {
        var files = await Task.Run(() => Client.ListDirectory(path).ToList()).ConfigureAwait(false);

        foreach (ISftpFile file in files)
        {
            if (file.Name == "." || file.Name == "..")
            {
                continue;
            }

            if (file.IsDirectory)
            {
                await DeleteDirectoryRecursiveAsync(file.FullName).ConfigureAwait(false);
            }
            else
            {
                await Task.Run(() => Client.DeleteFile(file.FullName)).ConfigureAwait(false);
            }
        }

        await Task.Run(() => Client.DeleteDirectory(path)).ConfigureAwait(false);
    }

    public override string[] GetFiles(string path, string searchPattern = "*")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        return ExecuteWithLogging(() =>
        {
            string sftpPath = ResolvePath(path);
            EnsureConnected();

            return Client.ListDirectory(sftpPath)
                .Where(f => f.IsRegularFile && MatchesPattern(f.Name, searchPattern))
                .Select(f => f.FullName)
                .ToArray();
        }, "GetFiles", path);
    }

    public override string[] GetDirectories(string path, string searchPattern = "*")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        return ExecuteWithLogging(() =>
        {
            string sftpPath = ResolvePath(path);
            EnsureConnected();

            return Client.ListDirectory(sftpPath)
                .Where(f => f.IsDirectory && f.Name != "." && f.Name != ".." && MatchesPattern(f.Name, searchPattern))
                .Select(f => f.FullName)
                .ToArray();
        }, "GetDirectories", path);
    }

    public override async IAsyncEnumerable<string> EnumerateFilesAsync(
        string path,
        string searchPattern = "*",
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Enumerating files in {Path} with pattern {SearchPattern}", path, searchPattern);

        string sftpPath = ResolvePath(path);
        await EnsureConnectedAsync().ConfigureAwait(false);

        var files = await Task.Run(() =>
            Client.ListDirectory(sftpPath)
                .Where(f => f.IsRegularFile && MatchesPattern(f.Name, searchPattern))
                .Select(f => f.FullName)
                .ToList(), cancellationToken).ConfigureAwait(false);

        foreach (string file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return file;
        }
    }

    private static bool MatchesPattern(string name, string pattern)
    {
        if (pattern == "*")
        {
            return true;
        }

        // Convert wildcard pattern to regex
        string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return Regex.IsMatch(name, regexPattern, RegexOptions.IgnoreCase);
    }

    public override string GetFullPath(string path)
    {
        return ExecuteWithLogging(() =>
        {
            return ResolvePath(path);
        }, "GetFullPath", path);
    }

    public override string? GetDirectoryName(string path)
    {
        return ExecuteWithLogging(() =>
        {
            return Path.GetDirectoryName(path)?.Replace('\\', '/');
        }, "GetDirectoryName", path);
    }

    public override string GetFileName(string path)
    {
        return ExecuteWithLogging(() =>
        {
            return Path.GetFileName(path);
        }, "GetFileName", path);
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        try
        {
            // Only disconnect and dispose if we created the client (not injected)
            if (!clientInjected && client != null)
            {
                if (client.IsConnected)
                {
                    Logger.LogDebug("Disconnecting from SFTP server {Host}:{Port}", options.Host, options.Port);
                    client.Disconnect();
                }

                client.Dispose();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while disposing SFTP connection");
        }

        isDisposed = true;
    }
}
