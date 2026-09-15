namespace Ifx.Roslyn;

/// <summary>Categories for shared Ifx diagnostic descriptors.</summary>
public static class DiagnosticCategories
{
    /// <summary>Source policies that make diagnostic debt reviewable.</summary>
    public const string Maintainability = "Maintainability";

    /// <summary>Source policies that prevent reliability defects.</summary>
    public const string Reliability = "Reliability";

    /// <summary>Source policies that govern identifier naming.</summary>
    public const string Naming = "Naming";
}
