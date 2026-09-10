using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility
{

    /// <summary>
    /// VBD202: Manager layer should not directly depend on Access layer
    /// Volatility-Based Decomposition: Manager should orchestrate through Engine layer
    /// </summary>
    public static class Vbd202ManagerCannotDependOnAccess
    {
        public static readonly DiagnosticDescriptor Rule = new(id: DiagnosticIds.Vbd202ManagerCannotDependOnAccess, title: "Manager layer should not depend directly on Access layer", messageFormat: "Manager project '{0}' has forbidden Access dependency on '{1}'. Managers should orchestrate through Engines instead of depending on Access directly.", category: "Architecture.Layers", defaultSeverity: DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "Manager projects should coordinate orchestration and delegate business/data operations. Direct Access dependencies create tight coupling and bypass the Engine layer.", helpLinkUri: "Docs/Volatility/vbd202.md", customTags: WellKnownDiagnosticTags.CompilationEnd);

        /// <summary>
        /// Registers the manager-access dependency analysis logic with Roslyn.
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

            if (string.IsNullOrEmpty(assemblyName))
                return;

            var currentLayer = ProjectAnalyzer.GetLayer(assemblyName!);
            if (currentLayer is Layer.Unknown or Layer.Infrastructure)
                return;

            var currentProjectType = ProjectAnalyzer.GetProjectType(assemblyName!);
            if (currentProjectType == ProjectType.Test)
                return;

            foreach (var reference in compilation.References)
            {
                if (reference is not PortableExecutableReference portableRef)
                    continue;

                if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol referencedAssembly)
                    continue;

                var referencedName = referencedAssembly.Name;
                if (string.IsNullOrEmpty(referencedName))
                    continue;

                if (!referencedName.StartsWith("Access.", System.StringComparison.Ordinal) && !referencedName.StartsWith("Engine.", System.StringComparison.Ordinal) && !referencedName.StartsWith("Manager.", System.StringComparison.Ordinal) && !referencedName.StartsWith("Client.", System.StringComparison.Ordinal))
                    continue;

                var referencedLayer = ProjectAnalyzer.GetLayer(referencedName);
                if (referencedLayer == currentLayer || referencedLayer == Layer.Infrastructure)
                    continue;

                if (currentLayer == Layer.Manager && referencedLayer == Layer.Access)
                {
                    var diagnostic = Diagnostic.Create(Rule, Location.None, assemblyName!, referencedName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
