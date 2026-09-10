using System.Globalization;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vc.Ifx.Roslyn.Reporting;

namespace VisionaryCoder.Framework.Tests.Reporting;

[TestClass]
public sealed class ReportEngineTests
{
    private const string Package = "vc.Ifx.Roslyn.Reporting";
    private const string Source = "class Example { public int Add(int left, int right) => left + right; }";
    private const string Sarif = """{"version":"2.1.0","runs":[{"tool":{"driver":{"name":"compiler"}},"results":[]}]}""";

    private static string Coverage(int lineHits = 1, int branchHits = 1) =>
        JsonSerializer.Serialize(new Dictionary<string, object> { [Package + ".dll"] = new
        {
            file = new { type = new { method = new { Lines = new Dictionary<string, int> { ["1"] = lineHits }, Branches = new[] { new { Hits = branchHits } } } } }
        } });

    private static ReportRequest Request(string? coverage = "default", string? sarif = Sarif, SourceEvidence[]? sources = null) =>
        new("revision", "scoped", [Package], coverage == "default" ? Coverage() : coverage,
            [new(Package, sarif, sources ?? [new("src/Example.cs", Source, [])])]);

    [TestMethod]
    public void ExactCountsAndDeterministicRenderingAreRoundTrippable()
    {
        FrameworkReport report = ReportEngine.Create(Request());
        Assert.IsTrue(report.Passed);
        Assert.AreEqual(1, report.SchemaVersion);
        Assert.AreEqual("ifx-method-source-v1", report.MetricPolicy);
        Assert.AreEqual(100m, report.Packages[0].Coverage!.LinePercent);
        Assert.AreEqual(100m, report.Packages[0].Coverage!.BranchPercent);
        Assert.AreEqual(2, report.Packages[0].Methods[0].ParameterCount);
        Assert.AreEqual(1, report.Packages[0].Methods[0].CyclomaticComplexity);
        Assert.AreEqual(0, report.Packages[0].Methods[0].NestingDepth);
        Assert.AreEqual(1, report.Packages[0].Methods[0].MethodLength);
        string json = ReportEngine.ToJson(report);
        using JsonDocument document = JsonDocument.Parse(json);
        Assert.AreEqual("scoped", document.RootElement.GetProperty("scope").GetString());
        Assert.AreEqual(64, report.CoverageSha256!.Length);
        Assert.AreEqual(64, report.Packages[0].SarifSha256!.Length);
        Assert.AreEqual(64, report.Packages[0].Sources[0].Sha256.Length);
        Assert.AreEqual(json, ReportEngine.ToJson(ReportEngine.Create(Request())));
        string markdown = ReportEngine.ToMarkdown(report);
        Assert.IsTrue(markdown.Contains("1/1", StringComparison.Ordinal));
        Assert.IsFalse(markdown.Contains('\r'));
        Assert.IsTrue(markdown.Contains("Passed", StringComparison.Ordinal));
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            Assert.AreEqual(markdown, ReportEngine.ToMarkdown(report));
            Assert.AreEqual(json, ReportEngine.ToJson(report));
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [TestMethod]
    public void MissingAndPartialEvidenceNeverBecomesZeroOrGreen()
    {
        ReportRequest missing = new("revision", "full", [Package, "absent"], null, [new(Package, null, null)]);
        FrameworkReport report = ReportEngine.Create(missing);
        Assert.IsFalse(report.Passed);
        Assert.AreEqual(6, report.Issues.Length);
        Assert.IsNull(report.Packages[0].Coverage);
        Assert.IsNull(report.CoverageSha256);
        Assert.IsTrue(ReportEngine.ToJson(report).Contains("\"coverage\": null", StringComparison.Ordinal));
        Assert.IsTrue(ReportEngine.ToMarkdown(report).Contains("Unknown", StringComparison.Ordinal));
        Assert.IsFalse(ReportEngine.Create(Request("{}")).Passed);
        Assert.IsFalse(ReportEngine.Create(Request() with { Packages = [new(Package, Sarif, [])] }).Passed);
        Assert.IsFalse(ReportEngine.Create(Request(Coverage(0))).Passed);
        Assert.IsFalse(ReportEngine.Create(Request(Coverage(branchHits: 0))).Passed);
        Assert.AreEqual("BelowThreshold", ReportEngine.Create(Request(Coverage(0, 0))).Packages[0].Coverage!.Status);
    }

    [TestMethod]
    public void BranchlessAndEmptyCoverageAreExplicit()
    {
        string branchless = Coverage().Replace("[{\"Hits\":1}]", "[]", StringComparison.Ordinal);
        FrameworkReport report = ReportEngine.Create(Request(branchless));
        Assert.IsTrue(report.Passed);
        Assert.IsNull(report.Packages[0].Coverage!.BranchPercent);
        Assert.AreEqual(0L, report.Packages[0].Coverage!.BranchesTotal);
        string empty = branchless.Replace("{\"1\":1}", "{}", StringComparison.Ordinal);
        CoverageMeasure measure = ReportEngine.Create(Request(empty)).Packages[0].Coverage!;
        Assert.AreEqual("NoExecutableLines", measure.Status);
        Assert.IsNull(measure.LinePercent);
    }

    [TestMethod]
    public void RoundedPercentageCannotPass()
    {
        Dictionary<string, int> lines = Enumerable.Range(1, 10001).ToDictionary(value => value.ToString(CultureInfo.InvariantCulture), value => 1);
        lines["1"] = 0;
        string json = JsonSerializer.Serialize(new Dictionary<string, object> { [Package + ".dll"] = new { file = new { type = new { method = new { Lines = lines, Branches = Array.Empty<object>() } } } } });
        FrameworkReport report = ReportEngine.Create(Request(json));
        Assert.IsFalse(report.Passed);
        Assert.AreEqual(10000L, report.Packages[0].Coverage!.LinesCovered);
        Assert.AreEqual(10001L, report.Packages[0].Coverage!.LinesTotal);
    }

    [TestMethod]
    public void MalformedContractsFailClosed()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ReportEngine.Create(null!));
        Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request() with { Revision = " " }));
        Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request() with { Scope = "preview" }));
        Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request() with { ExpectedPackages = [] }));
        Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request() with { ExpectedPackages = [""] }));
        Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request() with { ExpectedPackages = [Package, Package] }));
        Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request() with { Packages = [new("other", Sarif, [])] }));
        Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request() with { Packages = [new(Package, Sarif, []), new(Package, Sarif, [])] }));
        Assert.Throws<JsonException>(() => ReportEngine.Create(Request("{")));
        Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request(Coverage(-1))));
        Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request(Coverage(branchHits: -1))));
        Assert.ThrowsExactly<FormatException>(() => ReportEngine.Create(Request(Coverage().Replace("\"1\":1", "\"1\":0.5", StringComparison.Ordinal))));
        Assert.ThrowsExactly<InvalidOperationException>(() => ReportEngine.Create(Request("[]")));
        string duplicate = Coverage()[..^1] + "," + Coverage().Replace(Package + ".dll", "folder/" + Package + ".dll", StringComparison.Ordinal)[1..];
        Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request(duplicate)));
        Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request(sarif: Sarif.Replace("2.1.0", "1.0.0", StringComparison.Ordinal))));
        Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request(sarif: """{"version":"2.1.0","runs":[]} """)));
        Assert.ThrowsExactly<KeyNotFoundException>(() => ReportEngine.Create(Request(sarif: "{}")));
        Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request(sarif: Sarif.Replace("\"results\":[]", "\"results\":[],\"invocations\":[{\"executionSuccessful\":false}]", StringComparison.Ordinal))));
        Assert.IsTrue(ReportEngine.Create(Request(sarif: Sarif.Replace("\"results\":[]", "\"results\":[],\"invocations\":[{\"executionSuccessful\":true}]", StringComparison.Ordinal))).Passed);
        Assert.ThrowsExactly<OperationCanceledException>(() => ReportEngine.Create(Request(), new CancellationToken(true)));
    }

    [TestMethod]
    public void SarifResultsPreserveSeveritySuppressionsAndMetricFacts()
    {
        object[] results =
        [
            new { ruleId = "IFX1100", level = "warning", message = new { text = "A|<B> & `C`\r\n" }, properties = new { customProperties = new { MetricName = "ControlFlowNestingDepth", MetricVersion = "ifx-control-nesting-v1", MeasuredValue = "5", Threshold = "4" } } },
            new { ruleId = "error", level = "error", message = new { text = "suppressed" }, suppressions = new[] { new { status = "accepted" } } },
            new { ruleId = "note", level = "note", message = new { text = "note" }, properties = new { MetricName = "M" }, suppressions = new[] { new { kind = "inSource" } } },
            new { ruleId = "none", level = "none", message = new { text = "none" }, properties = new { other = 1 }, suppressions = Array.Empty<object>() }
        ];
        string Log(object[] entries) => JsonSerializer.Serialize(new { version = "2.1.0", runs = new[] { new { tool = new { driver = new { name = "compiler" } }, results = entries } } });
        FrameworkReport report = ReportEngine.Create(Request(sarif: Log(results)));
        Assert.IsTrue(report.Passed);
        DiagnosticFact metric = report.Packages[0].Diagnostics.Single(diagnostic => diagnostic.RuleId == "IFX1100");
        Assert.AreEqual("ControlFlowNestingDepth", metric.MetricName);
        Assert.AreEqual("ifx-control-nesting-v1", metric.MetricVersion);
        Assert.AreEqual("5", metric.MeasuredValue);
        Assert.AreEqual("4", metric.Threshold);
        Assert.IsTrue(ReportEngine.ToMarkdown(report).Contains("&#124;&lt;B&gt; &amp; &#96;C&#96;", StringComparison.Ordinal));
        FrameworkReport reversed = ReportEngine.Create(Request(sarif: Log(results.Reverse().ToArray())));
        CollectionAssert.AreEqual(report.Packages[0].Diagnostics, reversed.Packages[0].Diagnostics);
        foreach (object entry in new object[]
        {
            new { ruleId = "x", level = "error", message = new { text = "x" }, suppressions = new[] { new { status = "rejected" } } },
            new { ruleId = "x", level = "future", message = new { text = "x" } },
            new { ruleId = "x", message = new { text = "x" } }
        }) Assert.IsFalse(ReportEngine.Create(Request(sarif: Log([entry]))).Passed);
    }

    [TestMethod]
    public void SourceMetricsCountDocumentedDecisionsAndExcludeNestedFunctions()
    {
        const string code = """
            class C {
              int M(int x, bool a, bool b, string? s) {
                if (a && b || a) x++;
                for (;;) break;
                foreach (var i in new int[0]) x++;
                foreach (var (i, j) in new (int,int)[0]) x++;
                while (a) break;
                do {} while (b);
                try { x++; } catch { x--; }
                switch(x) { case 1: break; case int z when z > 3: break; default: break; }
                var value = x switch { 1 => 2, _ => 3 };
                var text = s ?? "";
                int Local() { if (a) return 1; return 0; }
                System.Func<int> f = () => { if (a) return 1; return 0; };
                return a ? x : value;
              }
              int Deep() { if (true) while (true) for (;;) try { switch(1) { default: return 0; } } finally {} return 1; }
              int Plain() { return 1; }
              abstract class A { public abstract void Unknown(int x); }
            }
            """;
        FrameworkReport report = ReportEngine.Create(Request(sources: [new("src\\C.cs", code, [])]));
        Assert.IsTrue(report.Passed);
        MethodMetric metric = report.Packages[0].Methods.Single(method => method.Name == "M");
        Assert.AreEqual(15, metric.CyclomaticComplexity);
        Assert.AreEqual(1, metric.NestingDepth);
        Assert.AreEqual(15, metric.MethodLength);
        Assert.AreEqual(4, metric.ParameterCount);
        Assert.AreEqual("Review", report.Packages[0].Methods.Single(method => method.Name == "Deep").Status);
        Assert.AreEqual(5, report.Packages[0].Methods.Single(method => method.Name == "Deep").NestingDepth);
        Assert.AreEqual("Unknown", report.Packages[0].Methods.Single(method => method.Name == "Unknown").Status);
        Assert.IsTrue(ReportEngine.ToMarkdown(report).Contains("Unknown", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GeneratedFilesAndMembersAreExplicitButRecordsAndAsyncRemain()
    {
        SourceEvidence[] sources =
        [
            new("src/obj/C.cs", Source, []), new("src/bin/C.cs", Source, []),
            new("src/C.g.cs", Source, []), new("src/C.generated.cs", Source, []), new("src/C.designer.cs", Source, []),
            new("src/Header.cs", "// <auto-generated/>\n" + Source, []),
            new("src/Normal.cs", """
                using System.CodeDom.Compiler;
                using Alias = System.CodeDom.Compiler.GeneratedCodeAttribute;
                [GeneratedCode("tool", "1")] class Generated { public void M() {} }
                record R {
                  [Alias("tool", "1")] public void Generated() {}
                  [Unknown] public async System.Threading.Tasks.Task Work() { await System.Threading.Tasks.Task.Yield(); }
                  public void Plain() {}
                }
                """, [])
        ];
        FrameworkReport report = ReportEngine.Create(Request(sources: sources));
        Assert.IsTrue(report.Passed);
        Assert.AreEqual(8, report.Packages[0].Sources.Count(source => source.Exclusion is not null));
        CollectionAssert.AreEqual(new[] { "Work", "Plain" }, report.Packages[0].Methods.Select(method => method.Name).ToArray());
        Assert.IsTrue(ReportEngine.ToMarkdown(report).Contains("Excluded:", StringComparison.Ordinal));
        Assert.AreEqual(ReportEngine.ToJson(report), ReportEngine.ToJson(ReportEngine.Create(Request(sources: sources.Reverse().ToArray()))));
    }

    [TestMethod]
    public void SourceInventoryValidatesPathsSyntaxAndPreprocessorSymbols()
    {
        foreach (string path in new[] { "/absolute.cs", "C:/absolute.cs", "src/../escape.cs" })
            Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request(sources: [new(path, Source, [])])));
        Assert.ThrowsExactly<ArgumentException>(() => ReportEngine.Create(Request(sources: [new("src/C.cs", Source, []), new("src\\C.cs", Source, [])])));
        Assert.IsFalse(ReportEngine.Create(Request(sources: [new("src/C.cs", "class {", [])])).Passed);
        string conditional = "#if ENABLED\n" + Source + "\n#endif";
        Assert.AreEqual(1, ReportEngine.Create(Request(sources: [new("src/C.cs", conditional, ["ENABLED"])])).Packages[0].Methods.Length);
        Assert.AreEqual(0, ReportEngine.Create(Request(sources: [new("src/C.cs", conditional, [])])).Packages[0].Methods.Length);
    }
}
