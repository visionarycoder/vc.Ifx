using System.Threading;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility;

/// <summary>
/// VBD600: Stable component cannot depend on more volatile component
/// Volatility-Based Decomposition: Core principle - dependencies flow from volatile to stable
/// </summary>
public static class Vbd600StableDependsOnVolatile
{
    public static readonly DiagnosticDescriptor Rule = new(id: DiagnosticIds.Vbd600StableDependsOnVolatile, title: "Stable component cannot depend on more volatile component", messageFormat: "{0} project '{1}' cannot depend on {2} project '{3}'. Dependencies must flow from volatile to stable components.", category: "Architecture.Volatility", defaultSeverity: DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "Dependencies should flow from volatile (frequently changing) to stable (rarely changing) components. This is the fundamental principle of Volatility-Based Decomposition.", helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-vbd-policy");

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

        var currentProjectType = ProjectAnalyzer.GetProjectType(assemblyName!);
        if (currentProjectType is ProjectType.Unknown or ProjectType.Test)
            return;

        foreach (var (source, referencedType) in TypeDependencies.Enumerate(namedType, context.CancellationToken))
        {
            var referencedAssembly = referencedType.ContainingAssembly?.Name;
            if (string.IsNullOrWhiteSpace(referencedAssembly))
                continue;

            var referencedProjectType = ProjectAnalyzer.GetProjectType(referencedAssembly!);
            if (referencedProjectType == ProjectType.Unknown)
                continue;

            if (ProjectAnalyzer.IsDependencyAllowed(currentProjectType, referencedProjectType))
                continue;

            var location = source.Locations[0];
            var diagnostic = Diagnostic.Create(Rule, location, ProjectAnalyzer.GetProjectTypeName(currentProjectType),
                assemblyName!,
                ProjectAnalyzer.GetProjectTypeName(referencedProjectType),
                referencedAssembly!);
            context.ReportDiagnostic(diagnostic);
            return;
        }
    }

}
