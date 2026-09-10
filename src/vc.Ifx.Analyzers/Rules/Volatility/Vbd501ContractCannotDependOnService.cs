using System.Threading;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility;

/// <summary>
/// VBD501: Contract project cannot depend on Service project
/// Volatility-Based Decomposition: Stable contracts should not depend on volatile implementations
/// </summary>
public static class Vbd501ContractCannotDependOnService
{
    public static readonly DiagnosticDescriptor Rule = new(id: DiagnosticIds.Vbd501ContractCannotDependOnService, title: "Contract project cannot depend on Service project", messageFormat: "Contract project '{0}' cannot depend on Service project '{1}'. Contracts are stable and should not depend on volatile implementations.", category: "Architecture.Volatility", defaultSeverity: DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Contract projects define stable interfaces and should not depend on volatile service implementations. This violates the Stable Dependencies Principle.", helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-vbd-policy");

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

        if (ProjectAnalyzer.GetProjectType(assemblyName!) != ProjectType.Contract)
            return;

        var violation = FindServiceDependency(namedType, assemblyName!, context.CancellationToken);
        if (violation == null)
            return;

        var (source, referencedAssembly) = violation.Value;
        var location = source.Locations[0];

        var diagnostic = Diagnostic.Create(Rule, location, assemblyName!, referencedAssembly);
        context.ReportDiagnostic(diagnostic);
    }

    private static (ISymbol Source, string TargetAssembly)? FindServiceDependency(INamedTypeSymbol type, string currentAssembly, CancellationToken cancellationToken)
    {
        foreach (var (source, referencedType) in TypeDependencies.Enumerate(type, cancellationToken))
        {
            var referencedAssembly = referencedType.ContainingAssembly?.Name;
            if (string.IsNullOrWhiteSpace(referencedAssembly))
                continue;

            if (referencedAssembly!.Equals(currentAssembly, System.StringComparison.OrdinalIgnoreCase))
                continue;

            if (ProjectAnalyzer.GetProjectType(referencedAssembly) == ProjectType.Service)
                return (source, referencedAssembly);
        }

        return null;
    }

}
