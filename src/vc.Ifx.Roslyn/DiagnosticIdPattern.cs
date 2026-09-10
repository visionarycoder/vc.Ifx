using System.Text.RegularExpressions;

namespace vc.Ifx.Roslyn;

/// <summary>
/// Recognizes compiler and code-analysis diagnostic identifiers.
/// </summary>
public static class DiagnosticIdPattern
{
    /// <summary>
    /// Matches C# compiler diagnostics and .NET code-analysis diagnostics.
    /// </summary>
    public const string Pattern = @"\b(?:CA|CS)\d{4}\b";

    /// <summary>
    /// Gets a compiled matcher for diagnostic identifiers.
    /// </summary>
    public static readonly Regex Matcher = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
}
