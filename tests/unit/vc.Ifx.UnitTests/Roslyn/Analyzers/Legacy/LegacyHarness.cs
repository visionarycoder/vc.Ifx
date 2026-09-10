using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using vc.Ifx.Analyzers.Rules.Diagnostics;

namespace VisionaryCoder.Framework.Tests.Roslyn.Analyzers.Legacy;

internal static class LegacyHarness
{
    internal static DiagnosticAnalyzer Rule(string typeName)
    {
        Type type = typeof(DiagnosticDebtAnalyzer).Assembly.GetTypes().Single(type => type.Name == typeName);
        if (typeof(DiagnosticAnalyzer).IsAssignableFrom(type)) return (DiagnosticAnalyzer)Activator.CreateInstance(type)!;
        var descriptor = (DiagnosticDescriptor)type.GetField("Rule", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
        var initialize = type.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static)!.CreateDelegate<Action<AnalysisContext>>();
        return new RuleAdapter(descriptor, initialize);
    }

    internal static PortableExecutableReference Reference(string assemblyName, string source)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(assemblyName,
            new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14)) }, AnalyzerHarness.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        using MemoryStream stream = new();
        var result = compilation.Emit(stream);
        Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    private sealed class RuleAdapter(DiagnosticDescriptor descriptor, Action<AnalysisContext> initialize) : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);
        public override void Initialize(AnalysisContext context) => initialize(context);
    }
}
