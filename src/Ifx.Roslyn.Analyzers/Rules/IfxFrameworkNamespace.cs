using System;
using Microsoft.CodeAnalysis;

namespace Ifx.Analyzers.Rules;

/// <summary>
/// Shared namespace exclusion used by every Ifx analyzer so the framework's own
/// interceptor implementations do not trip diagnostics that exist to steer
/// consumer code toward those same interceptors.
/// </summary>
internal static class IfxFrameworkNamespace
{
    private const string RootNamespace = "vc.App.VisionaryCoder.Ifx";

    /// <summary>
    /// Determines whether <paramref name="symbol"/> is declared inside the Ifx framework
    /// namespace tree and should be excluded from consumer-facing diagnostics.
    /// </summary>
    public static bool Contains(ISymbol? symbol)
    {
        if (symbol is null)
        {
            return false;
        }

        var containingNamespace = symbol.ContainingNamespace.ToDisplayString();
        if (string.Equals(containingNamespace, RootNamespace, StringComparison.Ordinal))
        {
            return true;
        }

        return containingNamespace.StartsWith(RootNamespace + ".", StringComparison.Ordinal);
    }
}