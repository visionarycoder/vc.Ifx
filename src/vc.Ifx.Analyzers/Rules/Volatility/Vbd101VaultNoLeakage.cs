using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility;

public static class Vbd101VaultNoLeakage
{
    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticIds.Vbd101VaultNoLeakage,
        title: "Vault internals must not leak",
        messageFormat: "Infrastructure project '{0}' leaks internals to '{1}'. Only test assemblies may be granted InternalsVisibleTo.",
        category: "Architecture.Volatility",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-vbd-policy",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

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

        if (ProjectAnalyzer.GetProjectType(assemblyName!) != ProjectType.Infrastructure)
            return;

        var assemblySymbol = compilation.Assembly;
        var friendAttribute = compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.InternalsVisibleToAttribute");
        if (friendAttribute is null)
            return;
        var internalsVisibleToAttributes = assemblySymbol.GetAttributes()
            .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, friendAttribute))
            .ToList();

        foreach (var attribute in internalsVisibleToAttributes)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            if (attribute.ConstructorArguments.Length == 0)
                continue;

            var friendAssemblyName = attribute.ConstructorArguments[0].Value as string;
            if (string.IsNullOrWhiteSpace(friendAssemblyName))
                continue;

            if (IsTestOrBenchmarkAssembly(friendAssemblyName!))
                continue;

            var location = attribute.ApplicationSyntaxReference!.GetSyntax(context.CancellationToken).GetLocation();

            var diagnostic = Diagnostic.Create(Rule, location, assemblyName, friendAssemblyName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsTestOrBenchmarkAssembly(string assemblyName)
    {
        return assemblyName.IndexOf("Test", System.StringComparison.OrdinalIgnoreCase) >= 0
            || assemblyName.IndexOf("Benchmark", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
