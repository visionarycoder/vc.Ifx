using Ifx.Analyzers.Abstractions;
using Ifx.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ifx.Analyzers.Rules;

public static class Vbd602ForbiddenDependencyDirection
{
    public static readonly DiagnosticDescriptor Rule = new(DiagnosticIds.Vbd602ForbiddenDependencyDirection, "Forbidden dependency direction", "Type '{0}' depends on forbidden type '{1}'", "Architecture", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: "https://github.com/visionarycoder/Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-vbd-policy");

    public static void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeDependencies, SymbolKind.NamedType);
    }

    private static void AnalyzeDependencies(SymbolAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var namedType = (INamedTypeSymbol)context.Symbol;

        var assemblyName = context.Compilation.Assembly.Name;

        var currentLayer = ProjectAnalyzer.GetLayer(assemblyName!);

        // Only enforce for recognized layered projects
        if (currentLayer is Layer.Unknown or Layer.Infrastructure)
            return;

        foreach (var dependency in TypeDependencies.Enumerate(namedType, context.CancellationToken))
        {
            var targetLayer = ProjectAnalyzer.GetLayer(dependency.Type.ContainingAssembly?.Name ?? string.Empty);
            if (targetLayer == Layer.Unknown || ProjectAnalyzer.IsLayerDependencyAllowed(currentLayer, targetLayer))
            {
                continue;
            }
            var location = dependency.Source.Locations[0];
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, namedType.Name,
                dependency.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }
    }
}
