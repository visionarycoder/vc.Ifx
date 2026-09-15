using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Ifx.Storage;

public sealed class StorageService
{
    public StorageService(ILogger<StorageService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
    }

    public bool FileExists(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return fileInfo.Exists;
    }

    public bool FileExists(string path) => File.Exists(ValidatePath(path));

    public string ReadAllText(string path) => File.ReadAllText(ValidatePath(path));

    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
        => File.ReadAllTextAsync(ValidatePath(path), cancellationToken);

    public byte[] ReadAllBytes(string path) => File.ReadAllBytes(ValidatePath(path));

    public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
        => File.ReadAllBytesAsync(ValidatePath(path), cancellationToken);

    public void WriteAllText(string path, string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        string fullPath = ValidatePath(path);
        EnsureParentDirectory(fullPath);
        File.WriteAllText(fullPath, content);
    }

    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        string fullPath = ValidatePath(path);
        EnsureParentDirectory(fullPath);
        return File.WriteAllTextAsync(fullPath, content, cancellationToken);
    }

    public void WriteAllBytes(string path, byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        string fullPath = ValidatePath(path);
        EnsureParentDirectory(fullPath);
        File.WriteAllBytes(fullPath, bytes);
    }

    public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        string fullPath = ValidatePath(path);
        EnsureParentDirectory(fullPath);
        return File.WriteAllBytesAsync(fullPath, bytes, cancellationToken);
    }

    public void DeleteFile(string path)
    {
        string fullPath = ValidatePath(path);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    public Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DeleteFile(path);
        return Task.CompletedTask;
    }

    public bool DirectoryExists(string path) => Directory.Exists(ValidatePath(path));

    public DirectoryInfo CreateDirectory(string path) => Directory.CreateDirectory(ValidatePath(path));

    public Task<DirectoryInfo> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateDirectory(path));
    }

    public void DeleteDirectory(string path, bool recursive = false)
    {
        string fullPath = ValidatePath(path);
        if (Directory.Exists(fullPath))
            Directory.Delete(fullPath, recursive);
    }

    public Task DeleteDirectoryAsync(string path, bool recursive = false, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DeleteDirectory(path, recursive);
        return Task.CompletedTask;
    }

    public string[] GetFiles(string path, string pattern = "*") => Directory.GetFiles(ValidatePath(path), ValidatePattern(pattern));

    public string[] GetDirectories(string path, string pattern = "*") => Directory.GetDirectories(ValidatePath(path), ValidatePattern(pattern));

    public async IAsyncEnumerable<string> EnumerateFilesAsync(string path, string pattern = "*", [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (string file in Directory.EnumerateFiles(ValidatePath(path), ValidatePattern(pattern)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return file;
            await Task.Yield();
        }
    }

    public string GetFullPath(string path) => Path.GetFullPath(ValidatePath(path));

    public string? GetDirectoryName(string path) => Path.GetDirectoryName(ValidatePath(path));

    public string? GetFileName(string path) => Path.GetFileName(ValidatePath(path));

    private static string ValidatePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return path;
    }

    private static string ValidatePattern(string pattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);
        return pattern;
    }

    private static void EnsureParentDirectory(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
    }
}
