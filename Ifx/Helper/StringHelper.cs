using System.Diagnostics.Contracts;

namespace vc.Helper;

public static class StringHelper
{

    /// <summary>
    /// Performs multiple operations on a string to clean it up.
    /// Uses Trim() and Replace() to remove unwanted characters.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="replacements"></param>
    /// <returns></returns>
    public static string Transmogrify(string source, params (string oldValue, string newvalue)[] replacements)
    {
        Contract.Assert(source != null, nameof(source) + " != null");
        source = source.Trim();

        Contract.Assert(replacements != null, nameof(replacements) + " != null");
        foreach (var (oldValue, newValue) in replacements)
        {
            Contract.Assert(oldValue != null, nameof(oldValue) + " != null");
            Contract.Assert(newValue != null, nameof(newValue) + " != null");
            source = source.Replace(oldValue, newValue, StringComparison.CurrentCultureIgnoreCase).Trim();
        }
        
        return source;
    }

}