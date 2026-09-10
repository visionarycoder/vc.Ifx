using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility
{

    public static class Vbd101VaultNoLeakage
    {
        public static readonly DiagnosticDescriptor Rule = new(
            id: DiagnosticIds.Vbd101VaultNoLeakage,
            title: "Vault internals must not leak",
            messageFormat: "Infrastructure project '{0}' leaks internals to '{1}'. Only test assemblies may be granted InternalsVisibleTo.",
            category: "Architecture.Volatility",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: "Docs/Volatility/vbd101.md",
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

            var assemblySymbol = compilation.Assembly;
            var internalsVisibleToAttributes = assemblySymbol.GetAttributes()
                .Where(attr => attr.AttributeClass?.ToDisplayString() == "System.Runtime.CompilerServices.InternalsVisibleToAttribute")
                .ToList();

            foreach (var attribute in internalsVisibleToAttributes)
            {
                if (attribute.ConstructorArguments.Length == 0)
                    continue;

                var friendAssemblyName = attribute.ConstructorArguments[0].Value as string;
                if (string.IsNullOrWhiteSpace(friendAssemblyName))
                    continue;

                if (IsTestOrBenchmarkAssembly(friendAssemblyName!))
                    continue;

                var syntaxReference = attribute.ApplicationSyntaxReference;
                var location = syntaxReference != null
                    ? syntaxReference.GetSyntax(context.CancellationToken).GetLocation()
                    : Location.None;

                var diagnostic = Diagnostic.Create(Rule, location, assemblyName);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsTestOrBenchmarkAssembly(string assemblyName)
        {
            return assemblyName.IndexOf("Test", System.StringComparison.OrdinalIgnoreCase) >= 0
                || assemblyName.IndexOf("Tests", System.StringComparison.OrdinalIgnoreCase) >= 0
                || assemblyName.EndsWith(".UnitTests", System.StringComparison.OrdinalIgnoreCase)
                || assemblyName.EndsWith(".Tests", System.StringComparison.OrdinalIgnoreCase)
                || assemblyName.IndexOf("Benchmark", System.StringComparison.OrdinalIgnoreCase) >= 0
                || assemblyName.EndsWith(".Benchmarks", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
