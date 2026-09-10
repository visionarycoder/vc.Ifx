using System.Threading;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility;

/// <summary>
/// VBD700: Service project should not depend on other Service projects
/// Volatility-Based Decomposition: Services should depend on stable contracts, not volatile implementations
/// </summary>
public static class Vbd700ServiceCannotDependOnOtherService
{
    public static readonly DiagnosticDescriptor Rule = new(id: DiagnosticIds.Vbd700ServiceCannotDependOnOtherService, title: "Service project should not depend on other Service projects", messageFormat: "Service project '{0}' should not depend on Service project '{1}'. Services should depend on Contracts, not other Services.", category: "Architecture.Volatility", defaultSeverity: DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "Service projects should depend on Contract interfaces, not on other Service implementations. This improves testability and reduces coupling.", helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-vbd-policy", customTags: WellKnownDiagnosticTags.CompilationEnd);

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

        if (ProjectAnalyzer.GetProjectType(assemblyName!) != ProjectType.Service)
            return;

        var violation = FindServiceDependency(namedType, assemblyName!, context.CancellationToken);
        if (violation == null)
            return;

        var (member, referencedAssembly) = violation.Value;
        var location = member.Locations[0];
        var diagnostic = Diagnostic.Create(Rule, location, assemblyName!, referencedAssembly);
        context.ReportDiagnostic(diagnostic);
    }

    private static (ISymbol Source, string TargetAssembly)? FindServiceDependency(INamedTypeSymbol type, string currentAssembly, CancellationToken cancellationToken)
    {
        foreach (var (source, referencedType) in TypeDependencies.Enumerate(type, cancellationToken))
        {
            var targetAssembly = referencedType.ContainingAssembly?.Name;
            if (string.IsNullOrWhiteSpace(targetAssembly))
                continue;

            if (targetAssembly!.Equals(currentAssembly, System.StringComparison.OrdinalIgnoreCase))
                continue;

            if (ProjectAnalyzer.GetProjectType(targetAssembly) == ProjectType.Service)
                return (source, targetAssembly);
        }

        return null;
    }

}
