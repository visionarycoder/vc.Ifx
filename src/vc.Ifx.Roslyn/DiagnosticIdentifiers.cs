namespace vc.Ifx.Roslyn;

/// <summary>
/// Diagnostic identifiers produced by vc.Ifx Roslyn components.
/// </summary>
public static class DiagnosticIdentifiers
{
    /// <summary>
    /// A source comment references a compiler or code-analysis diagnostic.
    /// </summary>
    public const string DiagnosticReferenceRequiresDisposition = "IFX1000";

    /// <summary>
    /// A compiler or code-analysis diagnostic suppression requires an explicit justification.
    /// </summary>
    public const string DiagnosticSuppressionRequiresJustification = "IFX1001";
}
