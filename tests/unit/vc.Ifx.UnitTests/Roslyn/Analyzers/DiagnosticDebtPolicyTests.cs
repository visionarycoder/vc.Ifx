using Microsoft.CodeAnalysis;
using vc.Ifx.Analyzers.Rules.Diagnostics;
using vc.Ifx.Roslyn;

namespace VisionaryCoder.Framework.Tests.Roslyn.Analyzers;

[TestClass]
public sealed class DiagnosticDebtPolicyTests
{
    [TestMethod]
    [DataRow("// CA1822", 1)]
    [DataRow("/* CS0168 */", 1)]
    [DataRow("/// <summary>IFX1000</summary>", 1)]
    [DataRow("/** <summary>IFX005</summary> */", 1)]
    [DataRow("// CA1822 CS0168 IFX1100 IFX006 CA1822", 5)]
    [DataRow("// XCA1822 CA18222 ca1822 IFX007 VBD100 CA１２３４", 0)]
    [DataRow("// CA1822 Justification:", 1)]
    [DataRow("/* CA1822 Justification: */", 1)]
    [DataRow("/// <summary>CA1822 Justification:</summary>", 1)]
    [DataRow("// CA1822 Tracked-by: TODO - issue", 1)]
    [DataRow("// CA1822 Tracked-by: tbd", 1)]
    [DataRow("// CA1822 Fix-by: fixme", 1)]
    [DataRow("// CA1822 Tracked-by: Justification:", 1)]
    [DataRow("// CA1822 NotJustification: reason", 1)]
    [DataRow("// CA1822 Tracked-by: issue-123", 0)]
    [DataRow("// CA1822 jUsTiFiCaTiOn: required API", 0)]
    [DataRow("// CA1822 Fix-by: 2026-10-01", 0)]
    [DataRow("// CA1822 Intentional: reflection activation", 0)]
    [DataRow("// CA1822 Tracked-by: TODO Justification: required API", 0)]
    [DataRow("// CA1822 Tracked-by:\n// reason on unrelated comment", 1)]
    public async Task CommentPolicyHonorsScopeAndMeaningfulValues(string comment, int expected)
    {
        string source = comment + "\nclass C { string Text => \"CA1822\"; }";
        var diagnostics = await AnalyzerHarness.RunAsync(source, new DiagnosticDebtAnalyzer());
        Assert.AreEqual(expected, diagnostics.Length);
        foreach (Diagnostic diagnostic in diagnostics)
        {
            Assert.AreEqual("IFX1000", diagnostic.Id);
            Assert.AreEqual(DiagnosticSeverity.Info, diagnostic.Severity);
            string referenced = diagnostic.Properties[DiagnosticPropertyNames.DiagnosticId]!;
            Assert.AreEqual(referenced, source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
            Assert.AreEqual($"Diagnostic '{referenced}' is referenced without an explicit disposition", diagnostic.GetMessage());
        }
    }

    [TestMethod]
    [DataRow("#pragma warning disable CA1822", "CA1822")]
    [DataRow("#pragma warning disable 168", "CS0168")]
    [DataRow("#pragma warning disable IFX1100", "IFX1100")]
    [DataRow("#pragma warning disable CA1822 // CS0168", "CA1822")]
    [DataRow("#pragma warning disable CA1822 // Tracked-by: issue-42", "CA1822")]
    [DataRow("#pragma warning disable CA1822 // Justification: TODO", "CA1822")]
    [DataRow("#pragma warning disable CA1822 /* Justification: required signature */", "CA1822")]
    public async Task PragmaReportsOnlyDisabledCode(string pragma, string referenced)
    {
        var diagnostics = await AnalyzerHarness.RunAsync(pragma + "\nclass C { }", new DiagnosticDebtAnalyzer());
        Assert.AreEqual(1, diagnostics.Length);
        Assert.AreEqual("IFX1001", diagnostics[0].Id);
        Assert.AreEqual(DiagnosticSeverity.Warning, diagnostics[0].Severity);
        Assert.AreEqual(referenced, diagnostics[0].Properties[DiagnosticPropertyNames.DiagnosticId]);
        Assert.AreEqual($"Suppression for diagnostic '{referenced}' requires a justification", diagnostics[0].GetMessage());
    }

    [TestMethod]
    [DataRow("#pragma warning disable CA1822 // Justification: required signature")]
    [DataRow("#pragma warning restore CA1822 // CS0168")]
    [DataRow("#pragma warning disable")]
    [DataRow("#pragma warning disable VBD100")]
    [DataRow("#pragma warning disable 0, 10000")]
    [DataRow("#if false\n// CA1822\n#pragma warning disable CS0168\n#endif")]
    public async Task PragmaScopeExcludesJustifiedAndUnsupportedForms(string pragma)
    {
        var diagnostics = await AnalyzerHarness.RunAsync(pragma + "\nclass C { }", new DiagnosticDebtAnalyzer());
        Assert.AreEqual(0, diagnostics.Length);
    }

    [TestMethod]
    public async Task PragmaReportsMultipleCodesAtExactTokenLocations()
    {
        const string source = "#pragma warning disable CA1822, CS0168, 219\nclass C {}";
        var diagnostics = await AnalyzerHarness.RunAsync(source, new DiagnosticDebtAnalyzer());
        CollectionAssert.AreEqual(new[] { "CA1822", "CS0168", "CS0219" }, diagnostics.Select(d => d.Properties["DiagnosticId"]).ToArray());
        CollectionAssert.AreEqual(new[] { "CA1822", "CS0168", "219" }, diagnostics.Select(d => source.Substring(d.Location.SourceSpan.Start, d.Location.SourceSpan.Length)).ToArray());
    }

    [TestMethod]
    [DataRow("SuppressMessage", "\"CA1822:Mark members as static\"", "", 1)]
    [DataRow("UnconditionalSuppressMessage", "\"IFX1100\"", "", 1)]
    [DataRow("SuppressMessage", "\"CS0168\"", ", Justification = \" \"", 1)]
    [DataRow("SuppressMessage", "\"CA1822\"", ", Justification = \"TODO: explain\"", 1)]
    [DataRow("SuppressMessage", "\"CA1822\"", ", Justification = \"required by API\"", 0)]
    [DataRow("SuppressMessage", "\"CA1822\"", ", Scope = \"member\", Justification = null", 1)]
    [DataRow("SuppressMessage", "\"VBD100\"", "", 0)]
    [DataRow("SuppressMessage", "\"prefix CA1822\"", "", 0)]
    [DataRow("SuppressMessage", "null", "", 0)]
    public async Task SuppressionAttributesRequireRealConstantJustification(string name, string checkId, string named, int expected)
    {
        string source = $"using System.Diagnostics.CodeAnalysis; [{name}(\"Category\", {checkId}{named})] class C {{}}";
        var diagnostics = await AnalyzerHarness.RunAsync(source, new DiagnosticDebtAnalyzer());
        Assert.AreEqual(expected, diagnostics.Length);
        if (expected == 1)
        {
            Assert.AreEqual("IFX1001", diagnostics[0].Id);
            Assert.AreEqual(checkId, source.Substring(diagnostics[0].Location.SourceSpan.Start, diagnostics[0].Location.SourceSpan.Length));
        }
    }

    [TestMethod]
    public async Task NamedConstructorArgumentsAndConstantExpressionsAreResolved()
    {
        const string source = """
            using System.Diagnostics.CodeAnalysis;
            [SuppressMessage(checkId: "CA" + "1822", category: "Performance")]
            class C { }
            """;
        var diagnostics = await AnalyzerHarness.RunAsync(source, new DiagnosticDebtAnalyzer());
        Assert.AreEqual(1, diagnostics.Length);
        Assert.AreEqual("CA1822", diagnostics[0].Properties["DiagnosticId"]);
    }

    [TestMethod]
    public async Task LookalikeAndUnresolvedAttributesAreIgnored()
    {
        const string source = """
            [SuppressMessage("Category", "CA1822")]
            [Missing]
            class C { }
            class SuppressMessageAttribute : System.Attribute
            {
                public SuppressMessageAttribute(string category, string checkId) { }
            }
            """;
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, new DiagnosticDebtAnalyzer(), allowCompilerErrors: true)).Length);
    }

    [TestMethod]
    [DataRow("false", "false", 0)]
    [DataRow("false", "true", 2)]
    [DataRow("true", "false", 1)]
    [DataRow("invalid", "invalid", 3)]
    [DataRow("TRUE", "TRUE", 3)]
    public async Task PerFileOptionsControlCommentsAndBothSuppressionForms(string comments, string suppressions, int expected)
    {
        const string source = """
            // CA1822
            #pragma warning disable CS0168
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Category", "IFX1100")]
            class C { }
            """;
        var settings = new Dictionary<string, string>
        {
            ["dotnet_code_quality.IFX1000.analyze_comments"] = comments,
            ["dotnet_code_quality.IFX1001.analyze_suppressions"] = suppressions
        };
        Assert.AreEqual(expected, (await AnalyzerHarness.RunAsync(source, new DiagnosticDebtAnalyzer(), settings)).Length);
    }

    [TestMethod]
    [DataRow("Policy.g.cs", "")]
    [DataRow("Policy.generated.cs", "")]
    [DataRow("Policy.cs", "// <auto-generated/>\n")]
    public async Task GeneratedFilesAreExcluded(string path, string header)
    {
        string source = header + "// CA1822\n#pragma warning disable CS0168\n[System.Diagnostics.CodeAnalysis.SuppressMessage(\"Category\", \"CA1822\")] class C {}";
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, new DiagnosticDebtAnalyzer(), path: path)).Length);
    }
}
