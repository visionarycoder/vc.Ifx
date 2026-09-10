using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using vc.Ifx.Analyzers.Rules.CodeQuality;
using vc.Ifx.Analyzers.Rules.Diagnostics;

namespace VisionaryCoder.Framework.Tests.Roslyn.Analyzers;

[TestClass]
public sealed class AnalyzerExecutionTests
{
    private const string Source = "// CA1822\n#pragma warning disable CS0168\nclass C { void Run() { if(true) { if(true) { if(true) { if(true) { if(true) {} } } } } } }";

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task SharedAnalyzerInstanceIsDeterministicAcrossConcurrentCompilations(bool nesting)
    {
        DiagnosticAnalyzer analyzer = CreateAnalyzer(nesting);
        var sequential = await AnalyzerHarness.RunAsync(Source, analyzer, concurrent: false);
        string[] expected = sequential.Select(Describe).ToArray();
        var concurrent = await Task.WhenAll(Enumerable.Range(0, 8).Select(index => AnalyzerHarness.RunAsync(Source, analyzer)));
        foreach (var run in concurrent) CollectionAssert.AreEqual(expected, run.Select(Describe).ToArray());
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task HonorsPreCanceledAnalysis(bool nesting)
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => AnalyzerHarness.RunAsync(Source, CreateAnalyzer(nesting), cancellationToken: cancellation.Token));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ObservesCancellationAfterAnalysisStarts(bool nesting)
    {
        using CancellationTokenSource cancellation = new();
        await Assert.ThrowsAsync<OperationCanceledException>(() => AnalyzerHarness.RunAsync(Source, CreateAnalyzer(nesting),
            cancellationToken: cancellation.Token, onOptionsRead: cancellation.Cancel));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task StandardSeverityConfigurationCanSuppressOrPromote(bool nesting)
    {
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(Source, CreateAnalyzer(nesting), severity: ReportDiagnostic.Suppress)).Length);
        var diagnostics = await AnalyzerHarness.RunAsync(Source, CreateAnalyzer(nesting), severity: ReportDiagnostic.Error);
        Assert.IsTrue(diagnostics.Length > 0);
        Assert.IsTrue(diagnostics.All(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
    }

    private static DiagnosticAnalyzer CreateAnalyzer(bool nesting) => nesting ? new MethodNestingAnalyzer() : new DiagnosticDebtAnalyzer();
    private static string Describe(Diagnostic diagnostic) => $"{diagnostic.Id}:{diagnostic.Severity}:{diagnostic.Location.SourceSpan}:{diagnostic.GetMessage()}:" +
        string.Join(";", diagnostic.Properties.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => $"{pair.Key}={pair.Value}"));
}
