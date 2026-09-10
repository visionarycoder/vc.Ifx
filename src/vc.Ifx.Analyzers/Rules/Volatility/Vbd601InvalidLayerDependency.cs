using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility;

/// <summary>
/// VBD601: Invalid layer dependency detected
/// Volatility-Based Decomposition: Dependencies must flow in the correct layer direction
/// </summary>
public static class Vbd601InvalidLayerDependency
{

    public static readonly DiagnosticDescriptor Rule = new(id: DiagnosticIds.Vbd601InvalidLayerDependency, title: "Invalid layer dependency detected", messageFormat: "{0} layer project '{1}' cannot depend on {2} layer project '{3}'. Dependencies should flow: Client -> Manager -> Engine -> Access.", category: "Architecture.Layers", defaultSeverity: DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "Layer dependencies must follow the architectural flow: Access (data) -> Engine (logic) -> Manager (orchestration) -> Client (presentation). This ensures proper separation of concerns.", helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-vbd-policy", customTags: WellKnownDiagnosticTags.CompilationEnd);

    /// <summary>
    /// Registers the layer dependency analysis logic with Roslyn.
    /// </summary>
    public static void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var compilation = context.Compilation;
        var assemblyName = context.Compilation.Assembly.Name;
        var currentLayer = ProjectAnalyzer.GetLayer(assemblyName!);
        if (currentLayer is Layer.Unknown or Layer.Infrastructure) return;
        var currentProjectType = ProjectAnalyzer.GetProjectType(assemblyName!);
        if (currentProjectType == ProjectType.Test) return;
        foreach (var referencedAssembly in compilation.SourceModule.ReferencedAssemblySymbols.OrderBy(symbol => symbol.Name, System.StringComparer.Ordinal))
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var referencedName = referencedAssembly.Name;
            if (!referencedName.StartsWith("Access.", System.StringComparison.Ordinal) && !referencedName.StartsWith("Engine.", System.StringComparison.Ordinal) && !referencedName.StartsWith("Manager.", System.StringComparison.Ordinal) && !referencedName.StartsWith("Client.", System.StringComparison.Ordinal)) continue;
            var referencedLayer = ProjectAnalyzer.GetLayer(referencedName);
            if (referencedLayer == currentLayer) continue;
            if (!ProjectAnalyzer.IsLayerDependencyAllowed(currentLayer, referencedLayer))
            {
                var diagnostic = Diagnostic.Create(Rule, Location.None, currentLayer, assemblyName!, referencedLayer, referencedName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
