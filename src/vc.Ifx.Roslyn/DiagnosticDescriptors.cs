using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace vc.Ifx.Roslyn;

/// <summary>Immutable descriptors for shared vc.Ifx-owned diagnostic policies.</summary>
public static class DiagnosticDescriptors
{
    private const string CatalogUrl = "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md";

    /// <summary>A diagnostic reference in a comment needs an explicit disposition.</summary>
    public static readonly DiagnosticDescriptor DiagnosticReferenceRequiresDisposition = new(
        DiagnosticIdentifiers.DiagnosticReferenceRequiresDisposition,
        "Diagnostic reference requires disposition",
        "Diagnostic '{0}' is referenced without an explicit disposition",
        DiagnosticCategories.Maintainability,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "References to CA#### and CS#### diagnostics should state whether the issue will be fixed, is intentionally suppressed, or is tracked elsewhere.",
        helpLinkUri: CatalogUrl + "#ifx1000");

    /// <summary>A diagnostic suppression needs an explicit justification.</summary>
    public static readonly DiagnosticDescriptor DiagnosticSuppressionRequiresJustification = new(
        DiagnosticIdentifiers.DiagnosticSuppressionRequiresJustification,
        "Diagnostic suppression requires justification",
        "Suppression for diagnostic '{0}' requires a justification",
        DiagnosticCategories.Maintainability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Suppressions for CA#### and CS#### diagnostics should include an explicit justification so diagnostic debt can be reviewed.",
        helpLinkUri: CatalogUrl + "#ifx1001");

    /// <summary>Gets all shared descriptors in ordinal diagnostic-ID order.</summary>
    public static ImmutableArray<DiagnosticDescriptor> All { get; } =
        ImmutableArray.Create(DiagnosticReferenceRequiresDisposition, DiagnosticSuppressionRequiresJustification);
}
