using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace vc.Ifx.Helper;

public static class FileHelper
{
 
    public static async Task<string> Load(FileInfo fileInfo)
    {
        Contract.Assert(fileInfo != null, nameof(fileInfo) + " != null");
        return await File.ReadAllTextAsync(fileInfo.FullName).ConfigureAwait(false);
    }

    public static async Task<string> Load(string filename)
    {
        var fileInfo = new FileInfo(filename);
        return await Load(fileInfo).ConfigureAwait(false);
    }

}