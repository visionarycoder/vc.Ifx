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
    public static string Load(FileInfo fileInfo)
    {
        return fileInfo.Exists ? File.ReadAllText(fileInfo.FullName) : string.Empty;
    }

    /// <summary>
    /// Loads the content of the file at the specified path.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>The content of the file as a string, or an empty string if the file does not exist.</returns>
    public static string Load(string path)
    {
        var fileInfo = new FileInfo(path);
        return fileInfo.Exists ? Load(fileInfo) : string.Empty;
    }

    /// <summary>
    /// Asynchronously loads the content of the specified file.
    /// </summary>
    /// <param name="fileInfo">The file information.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the content of the file as a string, or an empty string if the file does not exist.</returns>
    public static async Task<string> LoadAsync(FileInfo fileInfo, CancellationToken cancellationToken = default)
    {
        return fileInfo.Exists ? await File.ReadAllTextAsync(fileInfo.FullName, cancellationToken) : string.Empty;
    }

    /// <summary>
    /// Asynchronously loads the content of the file at the specified path.
    /// </summary>
    /// <param name="filename">The path to the file.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the content of the file as a string, or an empty string if the file does not exist.</returns>
    public static async Task<string> LoadAsync(string filename, CancellationToken cancellationToken = default)
    {
        var fileInfo = new FileInfo(filename);
        return fileInfo.Exists ? await LoadAsync(fileInfo, cancellationToken) : string.Empty;
    }
}
