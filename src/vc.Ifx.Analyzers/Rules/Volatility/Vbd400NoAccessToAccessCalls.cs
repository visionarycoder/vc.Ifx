using System.Threading;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility;

/// <summary>
/// VBD400: Access projects must not depend on other Access projects.
/// This prevents access-layer implementations from taking dependencies on other access implementations.
/// </summary>
public static class Vbd400NoAccessToAccessCalls
{
    public static readonly DiagnosticDescriptor Rule = new(DiagnosticIds.Vbd400NoAccessToAccessCalls, "Access projects cannot depend on other Access projects", "Access project '{0}' depends on Access project '{1}'", "Architecture", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-vbd-policy");

    public static void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var namedType = (INamedTypeSymbol)context.Symbol;

        var assemblyName = context.Compilation.Assembly.Name;

        if (ProjectAnalyzer.GetLayer(assemblyName!) != Layer.Access)
            return;

        var violation = FindAccessDependency(namedType, assemblyName!, context.CancellationToken);
        if (violation == null)
            return;

        var (member, referencedAssembly) = violation.Value;
        var location = member.Locations[0];
        var diagnostic = Diagnostic.Create(Rule, location, assemblyName!, referencedAssembly);
        context.ReportDiagnostic(diagnostic);
    }

    private static (ISymbol Source, string TargetAssembly)? FindAccessDependency(INamedTypeSymbol type, string currentAssembly, CancellationToken cancellationToken)
    {
        foreach (var (source, referencedType) in TypeDependencies.Enumerate(type, cancellationToken))
        {
            var targetAssembly = referencedType.ContainingAssembly?.Name;
            if (string.IsNullOrWhiteSpace(targetAssembly))
                continue;

            if (targetAssembly!.Equals(currentAssembly, System.StringComparison.OrdinalIgnoreCase))
                continue;

            if (ProjectAnalyzer.GetLayer(targetAssembly) == Layer.Access && ProjectAnalyzer.GetProjectType(targetAssembly) == ProjectType.Service)
                return (source, targetAssembly);
        }

        return null;
    }

}
