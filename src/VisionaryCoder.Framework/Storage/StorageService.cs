// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;
using IoPath = System.IO.Path;

namespace VisionaryCoder.Framework.Storage;

/// <summary>
/// Provides storage operations for files and data.
/// </summary>
public class StorageService(ILogger<StorageService> logger) : ServiceBase<StorageService>(logger)
{

    private readonly ILogger<StorageService> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // File operations
    public bool FileExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return File.Exists(path);
    }

    public bool FileExists(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return fileInfo.Exists;
    }

    public string ReadAllText(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return File.ReadAllText(path);
    }

    public async Task<string> ReadAllTextAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return await File.ReadAllTextAsync(path);
    }

    public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    public byte[] ReadAllBytes(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return File.ReadAllBytes(path);
    }

    public async Task<byte[]> ReadAllBytesAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return await File.ReadAllBytesAsync(path);
    }

    public void WriteAllText(string path, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        File.WriteAllText(path, content);
    }

    public async Task WriteAllTextAsync(string path, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        await File.WriteAllTextAsync(path, content);
    }

    public void WriteAllBytes(string path, byte[] bytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(bytes);
        File.WriteAllBytes(path, bytes);
    }

    public async Task WriteAllBytesAsync(string path, byte[] bytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(bytes);
        await File.WriteAllBytesAsync(path, bytes);
    }

    public void DeleteFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        File.Delete(path);
    }

    public Task DeleteFileAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        File.Delete(path);
        return Task.CompletedTask;
    }

    // Directory operations
    public bool DirectoryExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Directory.Exists(path);
    }

    public DirectoryInfo CreateDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Directory.CreateDirectory(path);
    }

    public Task<DirectoryInfo> CreateDirectoryAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Task.FromResult(Directory.CreateDirectory(path));
    }

    public void DeleteDirectory(string path, bool recursive = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive);
        }
    }

    public Task DeleteDirectoryAsync(string path, bool recursive = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive);
        }
        return Task.CompletedTask;
    }

    public string[] GetFiles(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Directory.GetFiles(path);
    }

    public string[] GetFiles(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        return Directory.GetFiles(path, searchPattern);
    }

    public string[] GetDirectories(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Directory.GetDirectories(path);
    }

    public string[] GetDirectories(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        return Directory.GetDirectories(path, searchPattern);
    }

    public async IAsyncEnumerable<string> EnumerateFilesAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        await Task.Yield();
        foreach (string file in Directory.EnumerateFiles(path))
        {
            yield return file;
        }
    }

    public async IAsyncEnumerable<string> EnumerateFilesAsync(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        await Task.Yield();
        foreach (string file in Directory.EnumerateFiles(path, searchPattern))
        {
            yield return file;
        }
    }

    public async IAsyncEnumerable<string> EnumerateFilesAsync(string path, string searchPattern, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        await Task.Yield();
        foreach (string file in Directory.EnumerateFiles(path, searchPattern))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return file;
        }
    }

    // Path operations
    public string GetFullPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return IoPath.GetFullPath(path);
    }

    public string? GetDirectoryName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return IoPath.GetDirectoryName(path);
    }

    public string? GetFileName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return IoPath.GetFileName(path);
    }
}
