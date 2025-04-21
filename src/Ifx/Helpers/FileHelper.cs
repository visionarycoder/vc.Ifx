// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable ClassMethodMissingInterface
#pragma warning disable DerivedClasses
namespace vc.Ifx.Helpers;

/// <summary>
/// Provides helper methods for file operations.
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Loads the content of the specified file.
    /// </summary>
    /// <param name="fileInfo">The file information.</param>
    /// <returns>The content of the file as a string, or an empty string if the file does not exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileInfo"/> is null.</exception>
    public static string Load(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return fileInfo.Exists ? File.ReadAllText(fileInfo.FullName) : string.Empty;
    }

    /// <summary>
    /// Loads the content of the file at the specified path.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>The content of the file as a string, or an empty string if the file does not exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> is null.</exception>
    public static string Load(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        var fileInfo = new FileInfo(path);
        return fileInfo.Exists ? Load(fileInfo) : string.Empty;
    }

    /// <summary>
    /// Asynchronously loads the content of the specified file.
    /// </summary>
    /// <param name="fileInfo">The file information.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the content of the file as a string, or an empty string if the file does not exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileInfo"/> is null.</exception>
    public static async Task<string> LoadAsync(FileInfo fileInfo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return fileInfo.Exists ? await File.ReadAllTextAsync(fileInfo.FullName, cancellationToken) : string.Empty;
    }

    /// <summary>
    /// Asynchronously loads the content of the file at the specified path.
    /// </summary>
    /// <param name="filename">The path to the file.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the content of the file as a string, or an empty string if the file does not exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="filename"/> is null.</exception>
    public static async Task<string> LoadAsync(string filename, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filename);
        var fileInfo = new FileInfo(filename);
        return fileInfo.Exists ? await LoadAsync(fileInfo, cancellationToken) : string.Empty;
    }

    /// <summary>
    /// Saves the specified content to the file at the given path.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="content">The content to save.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> or <paramref name="content"/> is null.</exception>
    public static void Save(string path, string content)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(content);
        File.WriteAllText(path, content);
    }

    /// <summary>
    /// Asynchronously saves the specified content to the file at the given path.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="content">The content to save.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> or <paramref name="content"/> is null.</exception>
    public static async Task SaveAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(content);
        await File.WriteAllTextAsync(path, content, cancellationToken);
    }

    /// <summary>
    /// Deletes the specified file.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> is null.</exception>
    public static void Delete(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Checks if the specified file is empty.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns><c>true</c> if the file is empty; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> is null.</exception>
    public static bool IsEmpty(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        var fileInfo = new FileInfo(path);
        return fileInfo is { Exists: true, Length: 0 };
    }
}
