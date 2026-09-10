using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using vc.Ifx.Analyzers.Helpers;

namespace VisionaryCoder.Framework.Tests.Roslyn.Analyzers.Legacy;

[TestClass]
public sealed class LegacySecurityAndMetricsTests
{
    [TestMethod]
    [DataRow("\"select 1\"", 0)]
    [DataRow("$\"select 1\"", 0)]
    [DataRow("$\"select {input}\"", 1)]
    [DataRow("input + \" trailing\"", 1)]
    [DataRow("\"prefix\" + input", 1)]
    [DataRow("\"prefix\" + \"suffix\"", 0)]
    [DataRow("input", 1)]
    [DataRow("null", 1)]
    [DataRow("input ?? \"select 1\"", 1)]
    public async Task RawSqlRuleDoesNotMissLeadingOrInterpolatedInput(string argument, int expected)
    {
        string source = $"class C {{ void Run(string input) {{ FromSqlRaw({argument}); Other(); }} void FromSqlRaw(string sql) {{}} void Other() {{}} }}";
        var diagnostics = await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Sec001SqlInjection"));
        Assert.AreEqual(expected, diagnostics.Length);
        if (expected != 0) StringAssert.Contains(diagnostics[0].GetMessage(), "FromSqlRaw");
    }

    [TestMethod]
    public async Task EmptyRawSqlArgumentsAreNotInvented()
    {
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync("class C { void Run() { FromSqlRaw(); } void FromSqlRaw() {} }", LegacyHarness.Rule("Sec001SqlInjection"))).Length);
    }

    [TestMethod]
    [DataRow("LogInformation(input);", 1)]
    [DataRow("LogInformation(\"value:\" + input);", 1)]
    [DataRow("LogInformation(input + \"suffix\");", 1)]
    [DataRow("LogInformation(\"literal\");", 0)]
    [DataRow("Other(input);", 0)]
    [DataRow("string alias = null; alias = input; LogInformation(alias);", 1)]
    public async Task ControllerLoggingTracksDirectAndAssignedInput(string body, int expected)
    {
        string source = """
            class FromQueryAttribute : System.Attribute { }
            class ApiControllerAttribute : System.Attribute { }
            [ApiController] class C
            {
                void Run([FromQuery] string input) { BODY }
                void LogInformation(string message) { }
                void Other(string value) { }
            }
            """.Replace("BODY", body, StringComparison.Ordinal);
        var diagnostics = await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Sec002ControllerLoggingInjection"));
        Assert.AreEqual(expected, diagnostics.Length);
        if (expected != 0) Assert.AreEqual("SEC002", diagnostics[0].Id);
    }

    [TestMethod]
    public async Task ControllerExpressionAndBodylessMethodsAreHandled()
    {
        const string source = "class ApiControllerAttribute : System.Attribute {} [ApiController] abstract class C { public abstract void Run(); void Other() => Run(); }";
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Sec002ControllerLoggingInjection"))).Length);
    }

    [TestMethod]
    [DataRow("void Run() {}", 0, 0, 0, 1, 1)]
    [DataRow("int Run(int x) => x;", 1, 1, 0, 1, 0)]
    [DataRow("abstract void Run();", 0, 0, 0, 1, 0)]
    [DataRow("void Run() { int x = 0, y = 1; if (true) { int z = 2; } }", 2, 0, 3, 2, 2)]
    [DataRow("void Run() { if (true) {} if (true) {} }", 2, 0, 0, 3, 2)]
    [DataRow("void Run() { if (true) { while (true) { break; } } }", 1, 0, 0, 3, 3)]
    public void MetricHelpersHaveStableShapeSemantics(string source, int statements, int parameters, int locals, int cyclomatic, int nesting)
    {
        var method = (MethodDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration(source)!;
        Assert.AreEqual(statements, CodeQualityMetrics.CountStatements(method));
        Assert.AreEqual(parameters, CodeQualityMetrics.CountParameters(method));
        Assert.AreEqual(locals, CodeQualityMetrics.CountLocalVariables(method));
        Assert.AreEqual(cyclomatic, CodeQualityMetrics.ComputeCyclomaticComplexity(method));
        Assert.AreEqual(nesting, CodeQualityMetrics.ComputeNestingDepth(method));
    }

    [TestMethod]
    public void LegacyCyclomaticMetricCountsItsDocumentedDecisionKinds()
    {
        var method = (MethodDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("void Run() { for (;;) {} foreach (var x in xs) {} while (true) {} do {} while (false); switch (x) { case 1: break; default: break; } var y = a ? b : c; var z = a && b || c; }")!;
        Assert.AreEqual(9, CodeQualityMetrics.ComputeCyclomaticComplexity(method));
    }

    [TestMethod]
    [DataRow("Cq100MethodTooLong", 80, 81)]
    [DataRow("Cq101TooManyParameters", 5, 6)]
    [DataRow("Cq102TooDeepNesting", 4, 5)]
    [DataRow("Cq103TooManyLocalVariables", 10, 11)]
    [DataRow("Cq104TooHighCyclomaticComplexity", 10, 11)]
    public async Task LegacyQualityRulesHaveInclusiveThresholds(string rule, int boundary, int violation)
    {
        foreach (int value in new[] { boundary, violation })
        {
            string parameters = rule == "Cq101TooManyParameters" ? string.Join(",", Enumerable.Range(0, value).Select(index => $"int p{index}")) : "";
            string body = rule switch
            {
                "Cq100MethodTooLong" => string.Concat(Enumerable.Repeat("System.GC.KeepAlive(null);", value)),
                "Cq103TooManyLocalVariables" => string.Join("", Enumerable.Range(0, value).Select(index => $"int v{index}=0;")),
                "Cq102TooDeepNesting" => new string('{', value - 1) + new string('}', value - 1),
                "Cq104TooHighCyclomaticComplexity" => string.Concat(Enumerable.Repeat("if (true) {}", value - 1)),
                _ => ""
            };
            var diagnostics = await AnalyzerHarness.RunAsync($"class C {{ void Run({parameters}) {{ {body} }} }}", LegacyHarness.Rule(rule));
            Assert.AreEqual(value == violation ? 1 : 0, diagnostics.Length, rule);
        }
    }
}
