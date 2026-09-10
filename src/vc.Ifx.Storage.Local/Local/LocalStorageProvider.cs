using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace VisionaryCoder.Framework.Storage.Local;

/// <summary>Provides native file operations and rooted object storage.</summary>
/// <remarks>
/// The legacy file API retains native paths. Object operations use a captured root and
/// portable relative keys. Lexical validation does not prevent symbolic-link or junction escapes.
/// </remarks>
public class LocalStorageProvider : IStorageProvider, IObjectStorageProvider
{
    private readonly ILogger<LocalStorageProvider> logger;
    private readonly string rootPath;

    /// <summary>Initializes the provider, capturing the current directory as the object root.</summary>
    /// <param name="logger">The operation logger.</param>
    public LocalStorageProvider(ILogger<LocalStorageProvider> logger) : this(new LocalStorageOptions(), logger) { }

    /// <summary>Initializes the provider with a snapshot of the configured object root.</summary>
    /// <param name="options">The root configuration; does not alter legacy path operations.</param>
    /// <param name="logger">The operation logger.</param>
    public LocalStorageProvider(LocalStorageOptions options, ILogger<LocalStorageProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        options.Validate();
        rootPath = Path.GetFullPath(options.RootPath);
    }

    /// <inheritdoc/>
    public StorageCapabilities Capabilities => StorageCapabilities.Read | StorageCapabilities.Write |
        StorageCapabilities.Delete | StorageCapabilities.Metadata | StorageCapabilities.List | StorageCapabilities.CreateOnly;

    /// <inheritdoc/>
    public Task<Stream> OpenReadAsync(StorageObjectRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        string path = ResolveObjectPath(request.Path);
        try
        {
            return Task.FromResult<Stream>(new FileStream(path, FileMode.Open, FileAccess.Read,
                FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan));
        }
        catch (DirectoryNotFoundException exception)
        {
            throw new FileNotFoundException("The storage object does not exist.", request.Path, exception);
        }
    }

    /// <inheritdoc/>
    public async Task<StorageObjectMetadata> WriteAsync(StorageWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        string path = ResolveObjectPath(request.Path);
        if (!request.Content.CanRead)
        {
            throw new ArgumentException("The content stream must remain readable until the write completes.", nameof(request));
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using (var destination = new FileStream(path, request.Overwrite ? FileMode.Create : FileMode.CreateNew,
            FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
        {
            await request.Content.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
            await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return ReadMetadata(path, request.Path);
    }

    /// <inheritdoc/>
    public Task<StorageObjectMetadata?> GetMetadataAsync(StorageObjectRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        string path = ResolveObjectPath(request.Path);
        try
        {
            return Task.FromResult<StorageObjectMetadata?>(ReadMetadata(path, request.Path));
        }
        catch (FileNotFoundException)
        {
            // FileInfo.Length also reports missing parents and directory entries as missing files.
            return Task.FromResult<StorageObjectMetadata?>(null);
        }
    }

    /// <inheritdoc/>
    public Task DeleteAsync(StorageObjectRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        DeleteFile(ResolveObjectPath(request.Path));
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<StorageObjectMetadata> ListAsync(StorageListRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        await Task.CompletedTask.ConfigureAwait(false);
        IEnumerable<string> paths;
        try
        {
            if ((File.GetAttributes(rootPath) & FileAttributes.Directory) == 0)
            {
                throw new IOException("The object storage root is not a directory.");
            }

            paths = Directory.EnumerateFiles(rootPath, "*", new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = false,
                AttributesToSkip = FileAttributes.ReparsePoint
            });
        }
        catch (FileNotFoundException)
        {
            yield break;
        }
        catch (DirectoryNotFoundException)
        {
            yield break;
        }

        using IEnumerator<string> files = paths.GetEnumerator();
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!files.MoveNext())
            {
                yield break;
            }

            string objectPath = Path.GetRelativePath(rootPath, files.Current).Replace(Path.DirectorySeparatorChar, '/');
            if (objectPath.StartsWith(request.Prefix, StringComparison.Ordinal))
            {
                yield return ReadMetadata(files.Current, objectPath);
            }
        }
    }

    /// <inheritdoc/>
    public bool FileExists(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        fileInfo.Refresh();
        return fileInfo.Exists;
    }

    /// <inheritdoc/>
    public bool FileExists(string path) => File.Exists(ValidatePath(path));

    /// <inheritdoc/>
    public string ReadAllText(string path) => File.ReadAllText(ValidatePath(path));

    /// <inheritdoc/>
    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default) =>
        File.ReadAllTextAsync(ValidatePath(path), cancellationToken);

    /// <inheritdoc/>
    public byte[] ReadAllBytes(string path) => File.ReadAllBytes(ValidatePath(path));

    /// <inheritdoc/>
    public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default) =>
        File.ReadAllBytesAsync(ValidatePath(path), cancellationToken);

    /// <inheritdoc/>
    public void WriteAllText(string path, string content)
    {
        ValidatePath(path);
        ArgumentNullException.ThrowIfNull(content);
        File.WriteAllText(path, content);
    }

    /// <inheritdoc/>
    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        ValidatePath(path);
        ArgumentNullException.ThrowIfNull(content);
        return File.WriteAllTextAsync(path, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteAllBytes(string path, byte[] bytes)
    {
        ValidatePath(path);
        ArgumentNullException.ThrowIfNull(bytes);
        File.WriteAllBytes(path, bytes);
    }

    /// <inheritdoc/>
    public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        ValidatePath(path);
        ArgumentNullException.ThrowIfNull(bytes);
        return File.WriteAllBytesAsync(path, bytes, cancellationToken);
    }

    /// <inheritdoc/>
    public void DeleteFile(string path)
    {
        ValidatePath(path);
        try
        {
            File.Delete(path);
        }
        catch (DirectoryNotFoundException) { }
    }

    /// <inheritdoc/>
    public Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DeleteFile(path);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public bool DirectoryExists(string path) => Directory.Exists(ValidatePath(path));

    /// <inheritdoc/>
    public DirectoryInfo CreateDirectory(string path) => Directory.CreateDirectory(ValidatePath(path));

    /// <inheritdoc/>
    public Task<DirectoryInfo> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateDirectory(path));
    }

    /// <inheritdoc/>
    public void DeleteDirectory(string path, bool recursive = true)
    {
        ValidatePath(path);
        try
        {
            Directory.Delete(path, recursive);
        }
        catch (DirectoryNotFoundException) { }
    }

    /// <inheritdoc/>
    public Task DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DeleteDirectory(path, recursive);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public string[] GetFiles(string path, string searchPattern = "*")
    {
        ValidatePath(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        return Directory.GetFiles(path, searchPattern);
    }

    /// <inheritdoc/>
    public string[] GetDirectories(string path, string searchPattern = "*")
    {
        ValidatePath(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        return Directory.GetDirectories(path, searchPattern);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> EnumerateFilesAsync(string path, string searchPattern = "*",
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ValidatePath(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        cancellationToken.ThrowIfCancellationRequested();
        await Task.CompletedTask.ConfigureAwait(false);
        using IEnumerator<string> files = Directory.EnumerateFiles(path, searchPattern).GetEnumerator();
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!files.MoveNext())
            {
                yield break;
            }

            yield return files.Current;
        }
    }

    /// <inheritdoc/>
    public string GetFullPath(string path) => Path.GetFullPath(ValidatePath(path));

    /// <inheritdoc/>
    public string? GetDirectoryName(string path) => Path.GetDirectoryName(ValidatePath(path));

    /// <inheritdoc/>
    public string GetFileName(string path) => Path.GetFileName(ValidatePath(path));

    private string ValidatePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (path.Contains('\0'))
        {
            throw new ArgumentException("Paths must not contain null characters.", nameof(path));
        }

        logger.LogDebug("Local storage path: {Path}", path);
        return path;
    }

    private string ResolveObjectPath(string path)
    {
        ValidatePath(path);
        foreach (string segment in path.Split('/'))
        {
            if (segment.Length == 0 || segment is "." or ".." || segment.EndsWith('.') || segment.EndsWith(' ') ||
                segment.IndexOfAny(['\\', ':', '<', '>', '"', '|', '?', '*']) >= 0 || segment.Any(char.IsControl))
            {
                throw new ArgumentException("Object paths require relative slash-separated names without traversal or special path characters.", nameof(path));
            }
        }

        if (File.Exists(rootPath))
        {
            throw new IOException("The object storage root is not a directory.");
        }

        return Path.Combine(rootPath, path.Replace('/', Path.DirectorySeparatorChar));
    }

    private static StorageObjectMetadata ReadMetadata(string path, string objectPath)
    {
        var file = new FileInfo(path);
        return new StorageObjectMetadata(objectPath, file.Length, new DateTimeOffset(file.LastWriteTimeUtc));
    }
}
