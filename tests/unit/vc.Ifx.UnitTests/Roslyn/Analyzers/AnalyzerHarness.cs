using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace VisionaryCoder.Framework.Tests.Roslyn.Analyzers;

internal static class AnalyzerHarness
{
    internal static readonly ImmutableArray<MetadataReference> References = ImmutableArray.Create<MetadataReference>(
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "System.Runtime.dll")));

    internal static async Task<ImmutableArray<Diagnostic>> RunAsync(string source, DiagnosticAnalyzer analyzer,
        Dictionary<string, string>? settings = null, string path = "Policy.cs", bool concurrent = true,
        CancellationToken cancellationToken = default, Action? onOptionsRead = null,
        ReportDiagnostic severity = ReportDiagnostic.Default, bool allowCompilerErrors = false,
        string assemblyName = "PolicyTests", IEnumerable<MetadataReference>? additionalReferences = null)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14), path);
        ImmutableDictionary<string, ReportDiagnostic> severities = analyzer.SupportedDiagnostics
            .ToImmutableDictionary(rule => rule.Id, rule => severity);
        CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, new[] { tree }, References.AddRange(additionalReferences ?? []),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, specificDiagnosticOptions: severities, allowUnsafe: true));
        if (!allowCompilerErrors)
        {
            Assert.AreEqual(0, compilation.GetDiagnostics().Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error),
                string.Join(Environment.NewLine, compilation.GetDiagnostics()));
        }

        AnalyzerOptions options = new(ImmutableArray<AdditionalText>.Empty, new OptionsProvider(settings, onOptionsRead));
        CompilationWithAnalyzers driver = compilation.WithAnalyzers(ImmutableArray.Create(analyzer),
            new CompilationWithAnalyzersOptions(options, null, concurrent, false, false));
        ImmutableArray<Diagnostic> diagnostics = await driver.GetAnalyzerDiagnosticsAsync(cancellationToken);
        Assert.IsFalse(diagnostics.Any(diagnostic => diagnostic.Id == "AD0001"), string.Join(Environment.NewLine, diagnostics));
        return diagnostics.OrderBy(diagnostic => diagnostic.Location.SourceSpan.Start).ThenBy(diagnostic => diagnostic.Id, StringComparer.Ordinal).ToImmutableArray();
    }

    private sealed class OptionsProvider(Dictionary<string, string>? values, Action? onRead) : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions options = new DictionaryOptions(values ?? new());
        public override AnalyzerConfigOptions GlobalOptions => new DictionaryOptions(new());
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            onRead?.Invoke();
            return options;
        }
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => options;
    }

    private sealed class DictionaryOptions(Dictionary<string, string> values) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value) => values.TryGetValue(key, out value!);
    }
}
