using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility
{

    /// <summary>
    /// VBD601: Invalid layer dependency detected
    /// Volatility-Based Decomposition: Dependencies must flow in the correct layer direction
    /// </summary>
    public static class Vbd601InvalidLayerDependency
    {

        public static readonly DiagnosticDescriptor Rule = new(id: DiagnosticIds.Vbd601InvalidLayerDependency, title: "Invalid layer dependency detected", messageFormat: "{0} layer project '{1}' cannot depend on {2} layer project '{3}'. Dependencies should flow: Client -> Manager -> Engine -> Access.", category: "Architecture.Layers", defaultSeverity: DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "Layer dependencies must follow the architectural flow: Access (data) -> Engine (logic) -> Manager (orchestration) -> Client (presentation). This ensures proper separation of concerns.", helpLinkUri: "Docs/Volatility/vbd601.md", customTags: WellKnownDiagnosticTags.CompilationEnd);

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
            var compilation = context.Compilation;
            var assemblyName = compilation.AssemblyName;
            if (string.IsNullOrEmpty(assemblyName)) return;
            var currentLayer = ProjectAnalyzer.GetLayer(assemblyName!);
            if (currentLayer is Layer.Unknown or Layer.Infrastructure) return;
            var currentProjectType = ProjectAnalyzer.GetProjectType(assemblyName!);
            if (currentProjectType == ProjectType.Test) return;
            foreach (var reference in compilation.References)
            {
                if (reference is not PortableExecutableReference portableRef) continue;
                if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol referencedAssembly) continue;
                var referencedName = referencedAssembly.Name;
                if (string.IsNullOrEmpty(referencedName)) continue;
                if (!referencedName.StartsWith("Access.", System.StringComparison.Ordinal) && !referencedName.StartsWith("Engine.", System.StringComparison.Ordinal) && !referencedName.StartsWith("Manager.", System.StringComparison.Ordinal) && !referencedName.StartsWith("Client.", System.StringComparison.Ordinal)) continue;
                var referencedLayer = ProjectAnalyzer.GetLayer(referencedName);
                if (referencedLayer == currentLayer || referencedLayer == Layer.Infrastructure) continue;
                if (!ProjectAnalyzer.IsLayerDependencyAllowed(currentLayer, referencedLayer))
                {
                    var diagnostic = Diagnostic.Create(Rule, Location.None, currentLayer, assemblyName!, referencedLayer, referencedName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
