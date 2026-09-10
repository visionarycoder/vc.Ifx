namespace vc.Ifx.Roslyn;

/// <summary>Property keys exchanged by diagnostic producers and remediation consumers.</summary>
public static class DiagnosticPropertyNames
{
    /// <summary>
    /// The referenced diagnostic identifier, distinct from the emitted IFX diagnostic identifier.
    /// </summary>
    public const string DiagnosticId = "DiagnosticId";
}
