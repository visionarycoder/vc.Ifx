using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.Storage.Local;

public class LocalStorageProvider(ILogger<LocalStorageProvider> logger) : IStorageProvider
{

    private readonly ILogger<LocalStorageProvider> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public bool FileExists(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        try
        {
            fileInfo.Refresh();
            bool exists = fileInfo.Exists;
            logger.LogTrace("File existence check for FileInfo '{Path}': {Exists}", fileInfo.FullName, exists);
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking file existence for FileInfo '{Path}'", fileInfo.FullName);
            throw;
        }
    }

    public bool FileExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fileInfo = new FileInfo(path);
        try
        {
            fileInfo.Refresh(); // Ensure we have current information
            bool exists = fileInfo.Exists;
            logger.LogTrace("File existence check for '{Path}': {Exists}", fileInfo.FullName, exists);
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking file existence for '{Path}'", fileInfo.FullName);
            throw;
        }
    }

    public string ReadAllText(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try
        {
            logger.LogDebug("Reading all text from '{Path}'", path);
            string content = File.ReadAllText(path);
            logger.LogTrace("Successfully read {Length} characters from '{Path}'", content.Length, path);
            return content;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading text from '{Path}'", path);
            throw;
        }
    }

    public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try
        {
            logger.LogDebug("Reading all text async from '{Path}'", path);
            string content = await File.ReadAllTextAsync(path, cancellationToken);
            logger.LogTrace("Successfully read {Length} characters from '{Path}'", content.Length, path);
            return content;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading text async from '{Path}'", path);
            throw;
        }
    }

    public byte[] ReadAllBytes(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try
        {
            logger.LogDebug("Reading all bytes from '{Path}'", path);
            byte[] bytes = File.ReadAllBytes(path);
            logger.LogTrace("Successfully read {Length} bytes from '{Path}'", bytes.Length, path);
            return bytes;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading bytes from '{Path}'", path);
            throw;
        }
    }

    public async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try
        {
            logger.LogDebug("Reading all bytes async from '{Path}'", path);
            byte[] bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            logger.LogTrace("Successfully read {Length} bytes async from '{Path}'", bytes.Length, path);
            return bytes;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading bytes async from '{Path}'", path);
            throw;
        }
    }

    public void WriteAllText(string path, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        try
        {
            logger.LogDebug("Writing {Length} characters to '{Path}'", content.Length, path);
            File.WriteAllText(path, content);
            logger.LogTrace("Successfully wrote text to '{Path}'", path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error writing text to '{Path}'", path);
            throw;
        }
    }

    public async Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        try
        {
            logger.LogDebug("Writing {Length} characters async to '{Path}'", content.Length, path);
            await File.WriteAllTextAsync(path, content, cancellationToken);
            logger.LogTrace("Successfully wrote text async to '{Path}'", path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error writing text async to '{Path}'", path);
            throw;
        }
    }

    public void WriteAllBytes(string path, byte[] bytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(bytes);
        try
        {
            logger.LogDebug("Writing {Length} bytes to '{Path}'", bytes.Length, path);
            File.WriteAllBytes(path, bytes);
            logger.LogTrace("Successfully wrote bytes to '{Path}'", path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error writing bytes to '{Path}'", path);
            throw;
        }
    }

    public async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(bytes);
        try
        {
            logger.LogDebug("Writing {Length} bytes async to '{Path}'", bytes.Length, path);
            await File.WriteAllBytesAsync(path, bytes, cancellationToken);
            logger.LogTrace("Successfully wrote bytes async to '{Path}'", path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error writing bytes async to '{Path}'", path);
            throw;
        }
    }

    public void DeleteFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try
        {
            if (File.Exists(path))
            {
                logger.LogDebug("Deleting file '{Path}'", path);
                File.Delete(path);
                logger.LogTrace("Successfully deleted file '{Path}'", path);
            }
            else
            {
                logger.LogTrace("File '{Path}' does not exist, no deletion needed", path);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting file '{Path}'", path);
            throw;
        }
    }

    public Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        // File.Delete is not I/O bound, so we run it in a task for consistency
        return Task.Run(() => DeleteFile(path), cancellationToken);
    }

    public bool DirectoryExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try
        {
            bool exists = Directory.Exists(path);
            logger.LogTrace("Directory existence check for '{Path}': {Exists}", path, exists);
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking directory existence for '{Path}'", path);
            throw;
        }
    }

    public DirectoryInfo CreateDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try
        {
            logger.LogDebug("Creating directory '{Path}'", path);
            DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
            logger.LogTrace("Successfully created directory '{Path}'", path);
            return directoryInfo;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating directory '{Path}'", path);
            throw;
        }
    }

    public Task<DirectoryInfo> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        // Directory.CreateDirectory is not I/O bound, so we run it in a task for consistency
        return Task.Run(() => CreateDirectory(path), cancellationToken);
    }

    public void DeleteDirectory(string path, bool recursive = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try
        {
            if (Directory.Exists(path))
            {
                logger.LogDebug("Deleting directory '{Path}' (recursive: {Recursive})", path, recursive);
                Directory.Delete(path, recursive);
                logger.LogTrace("Successfully deleted directory '{Path}'", path);
            }
            else
            {
                logger.LogTrace("Directory '{Path}' does not exist, no deletion needed", path);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting directory '{Path}' (recursive: {Recursive})", path, recursive);
            throw;
        }
    }

    public Task DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default)
    {
        // Directory.Delete is not I/O bound, so we run it in a task for consistency
        return Task.Run(() => DeleteDirectory(path, recursive), cancellationToken);
    }

    public string[] GetFiles(string path, string searchPattern = "*")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        try
        {
            logger.LogDebug("Getting files from '{Path}' with pattern '{Pattern}'", path, searchPattern);
            string[] files = Directory.GetFiles(path, searchPattern);
            logger.LogTrace("Found {Count} files in '{Path}'", files.Length, path);
            return files;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting files from '{Path}' with pattern '{Pattern}'", path, searchPattern);
            throw;
        }
    }

    public string[] GetDirectories(string path, string searchPattern = "*")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        try
        {
            logger.LogDebug("Getting directories from '{Path}' with pattern '{Pattern}'", path, searchPattern);
            string[] directories = Directory.GetDirectories(path, searchPattern);
            logger.LogTrace("Found {Count} directories in '{Path}'", directories.Length, path);
            return directories;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting directories from '{Path}' with pattern '{Pattern}'", path, searchPattern);
            throw;
        }
    }

    public async IAsyncEnumerable<string> EnumerateFilesAsync(string path, string searchPattern = "*", [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);
        logger.LogDebug("Enumerating files async from '{Path}' with pattern '{Pattern}'", path, searchPattern);
        await Task.Yield(); // Make it actually async
        List<string> files;
        try
        {
            files = Directory.EnumerateFiles(path, searchPattern).ToList();
            logger.LogTrace("Completed enumerating files from '{Path}'", path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enumerating files from '{Path}' with pattern '{Pattern}'", path, searchPattern);
            throw;
        }
        foreach (string file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return file;
        }
    }

    public string GetFullPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try
        {
            string fullPath = Path.GetFullPath(path);
            logger.LogTrace("Resolved full path for '{Path}': '{FullPath}'", path, fullPath);
            return fullPath;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resolving full path for '{Path}'", path);
            throw;
        }
    }

    public string? GetDirectoryName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try
        {
            string? directoryName = Path.GetDirectoryName(path);
            logger.LogTrace("Resolved directory name for '{Path}': '{DirectoryName}'", path, directoryName ?? "<null>");
            return directoryName;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resolving directory name for '{Path}'", path);
            throw;
        }
    }

    public string GetFileName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try
        {
            string fileName = Path.GetFileName(path);
            logger.LogTrace("Resolved file name for '{Path}': '{FileName}'", path, fileName);
            return fileName;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resolving file name for '{Path}'", path);
            throw;
        }
    }
}
