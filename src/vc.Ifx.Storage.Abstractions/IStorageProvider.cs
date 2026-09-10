namespace VisionaryCoder.Framework.Storage;

/// <summary>
/// Defines a comprehensive contract for storage operations following Microsoft I/O patterns.
/// This interface consolidates both file and directory operations for improved testability
/// and follows the accessor pattern for VBD (Volatility-Based Decomposition) architecture.
/// </summary>
/// <remarks>
/// This interface is designed to be easily mockable for unit testing and provides
/// both synchronous and asynchronous operations for maximum flexibility.
/// Based on Microsoft's System.IO.Abstractions patterns.
/// </remarks>
public interface IStorageProvider
{
    // ==========================================
    // File Operations
    // ==========================================

    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <returns>true if the file exists; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    bool FileExists(string path);

    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="fileInfo">The FileInfo object representing the file to check.</param>
    /// <returns>true if the file exists; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when fileInfo is null.</exception>
    bool FileExists(FileInfo fileInfo);

    /// <summary>
    /// Reads all text from a file synchronously.
    /// </summary>
    /// <param name="path">The file path to read from.</param>
    /// <returns>The file contents as a string.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    string ReadAllText(string path);

    /// <summary>
    /// Reads all text from a file asynchronously.
    /// </summary>
    /// <param name="path">The file path to read from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the file contents.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all bytes from a file synchronously.
    /// </summary>
    /// <param name="path">The file path to read from.</param>
    /// <returns>The file contents as a byte array.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    byte[] ReadAllBytes(string path);

    /// <summary>
    /// Reads all bytes from a file asynchronously.
    /// </summary>
    /// <param name="path">The file path to read from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the file contents as a byte array.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes text to a file synchronously, creating the file if it doesn't exist.
    /// </summary>
    /// <param name="path">The file path to write to.</param>
    /// <param name="content">The content to write.</param>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    void WriteAllText(string path, string content);

    /// <summary>
    /// Writes text to a file asynchronously, creating the file if it doesn't exist.
    /// </summary>
    /// <param name="path">The file path to write to.</param>
    /// <param name="content">The content to write.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes bytes to a file synchronously, creating the file if it doesn't exist.
    /// </summary>
    /// <param name="path">The file path to write to.</param>
    /// <param name="bytes">The bytes to write.</param>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when bytes is null.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    void WriteAllBytes(string path, byte[] bytes);

    /// <summary>
    /// Writes bytes to a file asynchronously, creating the file if it doesn't exist.
    /// </summary>
    /// <param name="path">The file path to write to.</param>
    /// <param name="bytes">The bytes to write.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when bytes is null.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified file if it exists.
    /// </summary>
    /// <param name="path">The file path to delete.</param>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    void DeleteFile(string path);

    /// <summary>
    /// Deletes the specified file asynchronously if it exists.
    /// </summary>
    /// <param name="path">The file path to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    Task DeleteFileAsync(string path, CancellationToken cancellationToken = default);

    // ==========================================
    // Directory Operations
    // ==========================================

    /// <summary>
    /// Determines whether the specified directory exists.
    /// </summary>
    /// <param name="path">The directory path to check.</param>
    /// <returns>true if the directory exists; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    bool DirectoryExists(string path);

    /// <summary>
    /// Creates a directory at the specified path, including any necessary parent directories.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    /// <returns>A DirectoryInfo object representing the created directory.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    DirectoryInfo CreateDirectory(string path);

    /// <summary>
    /// Creates a directory at the specified path asynchronously, including any necessary parent directories.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the created DirectoryInfo.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    Task<DirectoryInfo> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified directory and optionally all its contents.
    /// </summary>
    /// <param name="path">The directory path to delete.</param>
    /// <param name="recursive">true to delete the directory and all its contents; otherwise, false.</param>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist and recursive is false.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    void DeleteDirectory(string path, bool recursive = true);

    /// <summary>
    /// Deletes the specified directory and optionally all its contents asynchronously.
    /// </summary>
    /// <param name="path">The directory path to delete.</param>
    /// <param name="recursive">true to delete the directory and all its contents; otherwise, false.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist and recursive is false.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    Task DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the names of files in the specified directory.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <param name="searchPattern">The search pattern to match file names against.</param>
    /// <returns>An array of file names in the directory.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist.</exception>
    string[] GetFiles(string path, string searchPattern = "*");

    /// <summary>
    /// Gets the names of directories in the specified directory.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <param name="searchPattern">The search pattern to match directory names against.</param>
    /// <returns>An array of directory names in the directory.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist.</exception>
    string[] GetDirectories(string path, string searchPattern = "*");

    /// <summary>
    /// Enumerates files in the specified directory asynchronously.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <param name="searchPattern">The search pattern to match file names against.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of file names in the directory.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist.</exception>
    IAsyncEnumerable<string> EnumerateFilesAsync(string path, string searchPattern = "*", CancellationToken cancellationToken = default);

    // ==========================================
    // Path Operations
    // ==========================================

    /// <summary>
    /// Gets the full path for the specified relative path.
    /// </summary>
    /// <param name="path">The relative or absolute path.</param>
    /// <returns>The full path.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    string GetFullPath(string path);

    /// <summary>
    /// Gets the directory name from the specified path.
    /// </summary>
    /// <param name="path">The file or directory path.</param>
    /// <returns>The directory name, or null if path is a root directory.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    string? GetDirectoryName(string path);

    /// <summary>
    /// Gets the file name from the specified path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The file name including extension.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or whitespace.</exception>
    string GetFileName(string path);
}