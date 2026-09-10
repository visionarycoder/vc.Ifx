using System.Threading;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility;

/// <summary>
/// VBD102: Infrastructure project cannot depend on domain projects
/// Volatility-Based Decomposition: Infrastructure must remain stable and domain-agnostic
/// </summary>
public static class Vbd102InfrastructureCannotDependOnDomain
{
    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticIds.Vbd102InfrastructureCannotDependOnDomain,
        title: "Infrastructure project cannot depend on domain projects",
        messageFormat: "Infrastructure project '{0}' cannot depend on domain project '{1}'. Infrastructure must remain stable and domain-agnostic.",
        category: "Architecture.Volatility",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Infrastructure projects (Ifx.*) are very stable and should not depend on volatile domain projects. This ensures infrastructure remains reusable across domains.",
        helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-vbd-policy");

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

        if (ProjectAnalyzer.GetProjectType(assemblyName!) != ProjectType.Infrastructure)
            return;

        var violation = FindDomainDependency(namedType, context.CancellationToken);
        if (violation is null)
            return;

        var (source, target) = violation.Value;
        var location = source.Locations[0];
        var diagnostic = Diagnostic.Create(Rule, location, assemblyName!, target.ContainingAssembly.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static (ISymbol Source, ITypeSymbol Target)? FindDomainDependency(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        foreach (var dependency in TypeDependencies.Enumerate(type, cancellationToken))
        {
            var projectType = ProjectAnalyzer.GetProjectType(dependency.Type.ContainingAssembly?.Name ?? string.Empty);
            if (projectType is ProjectType.Service or ProjectType.Contract or ProjectType.Orm)
            {
                return dependency;
            }
        }
        return null;
    }
}
