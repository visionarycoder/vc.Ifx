// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable DerivedClasses
#pragma warning disable ClassMethodMissingInterface

namespace vc.Ifx.Helpers;

public static class FileHelper
{

    public static string Load(FileInfo fileInfo)
    {
        return fileInfo.Exists ? File.ReadAllText(fileInfo.FullName) : string.Empty;
    }

    public static string Load(string path)
    {
        var fileInfo = new FileInfo(path);
        return fileInfo.Exists ? Load(fileInfo) : string.Empty;
    }

    public static async Task<string> LoadAsync(FileInfo fileInfo, CancellationToken cancellationToken = default)
    {
        return fileInfo.Exists ? await File.ReadAllTextAsync(fileInfo.FullName, cancellationToken) : string.Empty;
    }

    public static async Task<string> LoadAsync(string filename, CancellationToken cancellationToken = default)
    {
        var fileInfo = new FileInfo(filename);
        return fileInfo.Exists ? await LoadAsync(fileInfo, cancellationToken) : string.Empty;
    }

}