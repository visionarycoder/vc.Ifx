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

    /// <summary>
    /// Matches uppercase CA/CS identifiers with four ASCII digits, IFX identifiers with
    /// four ASCII digits, and the existing legacy IFX001 through IFX006 identifiers.
    /// </summary>
    /// <remarks>This recognizes references, not whether a diagnostic has an emitter.</remarks>
    public const string ReferencePattern = @"\b(?:(?:CA|CS|IFX)[0-9]{4}|IFX00[1-6])\b";

    /// <summary>Gets the opt-in matcher for upstream and vc.Ifx diagnostic references.</summary>
    /// <remarks>The existing <see cref="Matcher"/> remains CA/CS-only for compatibility.</remarks>
    public static readonly Regex ReferenceMatcher = new(ReferencePattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
}
