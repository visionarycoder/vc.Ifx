using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility
{

    public static class Vbd100SingleVault
    {
        public static readonly DiagnosticDescriptor Rule = new(
            id: DiagnosticIds.Vbd100VaultSingleVolatility,
            title: "Vault must encapsulate a single volatility",
            messageFormat: "Vault project '{0}' references multiple volatility layers: {1}. Infrastructure projects should encapsulate a single volatility to avoid cross-layer coupling.",
            category: "Architecture.Volatility",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: "Docs/Volatility/vbd100.md",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

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
            if (string.IsNullOrWhiteSpace(assemblyName))
                return;

            if (ProjectAnalyzer.GetProjectType(assemblyName!) != ProjectType.Infrastructure)
                return;

            var referencedLayers = new Dictionary<Layer, HashSet<string>>();

            foreach (var reference in compilation.References)
            {
                if (reference is not PortableExecutableReference portableReference)
                    continue;

                if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol referencedAssembly)
                    continue;

                var referencedName = referencedAssembly.Name;
                if (string.IsNullOrWhiteSpace(referencedName))
                    continue;

                var layer = ProjectAnalyzer.GetLayer(referencedName!);
                if (!IsVolatilityLayer(layer))
                    continue;

                if (!referencedLayers.TryGetValue(layer, out var names))
                {
                    names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    referencedLayers[layer] = names;
                }
                names.Add(referencedName!);
            }

            if (referencedLayers.Count <= 1)
                return;

            var details = string.Join(", ", referencedLayers
                .OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}: {string.Join("/", pair.Value.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))}"));

            var diagnostic = Diagnostic.Create(Rule, Location.None, assemblyName!, details);
            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsVolatilityLayer(Layer layer) => layer is Layer.Access or Layer.Engine or Layer.Manager or Layer.Client;

    }
}
