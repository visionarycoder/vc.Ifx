namespace VisionaryCoder.Framework.Tests.Roslyn.Analyzers.Legacy;

[TestClass]
public sealed class ManagerPolicyTests
{
    private const string Domain = """
        using System.Linq;
        class Data
        {
            public decimal Amount;
            public decimal? Optional;
            public int Count;
            public string Text;
            public Data[] Children;
            public const decimal Fixed = 1;
        }
        namespace Ifx.Values { static class Clock { public static decimal Value => 1; } }
        """;

    [TestMethod]
    [DataRow("decimal Run(Data a, Data b) => a.Amount + b.Amount;", 1)]
    [DataRow("decimal? Run(Data a, Data b) => a.Optional + b.Optional;", 1)]
    [DataRow("void Run(Data a) { a.Amount += 1m; }", 1)]
    [DataRow("int Run(int a, int b) => a + b;", 0)]
    [DataRow("void Run(int a) { a += 1; }", 0)]
    [DataRow("string Run(string a) => a + \"suffix\";", 0)]
    [DataRow("bool Run(Data a, Data b) => a.Amount > b.Amount;", 1)]
    [DataRow("bool Run(Data a) => a.Amount > 0;", 0)]
    [DataRow("bool Run(Data a) => 0 < a.Amount;", 0)]
    [DataRow("bool Run(Data a) => a.Amount == Data.Fixed;", 0)]
    [DataRow("bool Run(Data a) => a.Text.Length > 0;", 0)]
    [DataRow("bool Run(string a, string b) => a.Length > b.Length;", 0)]
    [DataRow("decimal Run() => Ifx.Values.Clock.Value + 1;", 0)]
    [DataRow("decimal Run(Data a) => Ifx.Values.Clock.Value + a.Amount;", 1)]
    [DataRow("decimal Run(decimal a) => a + 1;", 1)]
    [DataRow("decimal Run(Data[] items) => items.Sum(item => item.Amount);", 1)]
    [DataRow("int Run(Data[] items) => items.Sum(item => item.Count);", 0)]
    [DataRow("decimal Run(decimal[] items) => items.Sum();", 0)]
    [DataRow("object Run(Data[] items) => items.Select(item => item.Amount > 0 ? item.Amount : 0);", 1)]
    [DataRow("object Run(Data[] items) => items.Select(item => item.Amount);", 0)]
    [DataRow("object Run(int[] items) => items.Select(item => item > 0 ? item : 0);", 0)]
    [DataRow("void Run() { System.GC.KeepAlive(null); }", 0)]
    [DataRow("void Run() {}", 0)]
    [DataRow("public abstract void Run();", 0)]
    public async Task DistinguishesDomainComputationFromOrchestration(string member, int expected)
    {
        string source = Domain + "abstract class C { " + member + " }";
        Assert.AreEqual(expected, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd200ManagerNoBusinessLogic"), assemblyName: "Manager.Local.Service")).Length, member);
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd200ManagerNoBusinessLogic"), assemblyName: "Engine.Local.Service")).Length);
    }

    [TestMethod]
    [DataRow("return missing.Sum(a => a.Amount);", 0)]
    [DataRow("return missing.Select(a => a.Amount > 0 ? a.Amount : 0);", 0)]
    [DataRow("return missing.Sum(1);", 0)]
    [DataRow("return Unknown();", 0)]
    [DataRow("return (factory())();", 0)]
    [DataRow("return a.Amount + missing;", 1)]
    [DataRow("return missing + a.Amount;", 1)]
    [DataRow("a.Amount += missing; return null;", 1)]
    [DataRow("missing += a.Amount; return null;", 1)]
    public async Task IncompleteCodeDoesNotCrashManagerAnalysis(string body, int expected)
    {
        string source = Domain + "class C { object Run(Data a) { " + body + " } }";
        var diagnostics = await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd200ManagerNoBusinessLogic"), assemblyName: "Manager.Local.Service", allowCompilerErrors: true);
        // Unresolved selector parameters do not acquire a domain identity.
        Assert.AreEqual(expected, diagnostics.Length);
    }

    [TestMethod]
    public async Task InfrastructureAssemblyCalculationsAreAllowed()
    {
        var reference = LegacyHarness.Reference("Ifx.Values", "namespace Infra { public static class Values { public static decimal Amount => 1; } }");
        var diagnostics = await AnalyzerHarness.RunAsync("class C { decimal Run() => Infra.Values.Amount + 1; }", LegacyHarness.Rule("Vbd200ManagerNoBusinessLogic"), assemblyName: "Manager.Local.Service", additionalReferences: [reference]);
        Assert.AreEqual(0, diagnostics.Length);
    }

    [TestMethod]
    [DataRow("decimal Sum() => 0; decimal Run() => Sum();", 0)]
    [DataRow("object Sum(System.Func<Data,decimal> selector) => null; object Run() => Sum(item => item.Amount);", 1)]
    [DataRow("object Sum(System.Func<Data,int> selector) => null; object Run() => Sum(item => { return item.Count; });", 0)]
    [DataRow("decimal[] Values; decimal Run() => Values.Sum();", 1)]
    public async Task AggregationSelectorAndReceiverPolicies(string members, int expected)
    {
        Assert.AreEqual(expected, (await AnalyzerHarness.RunAsync(Domain + "class C { " + members + " }", LegacyHarness.Rule("Vbd200ManagerNoBusinessLogic"), assemblyName: "Manager.Local.Service")).Length);
    }

    [TestMethod]
    [DataRow("Manager.Remote.Service", "void Run() { Remote.Manager.Work(); }", 1)]
    [DataRow("Manager.Remote.Service", "void Run() => Remote.Manager.Work();", 1)]
    [DataRow("Engine.Remote.Service", "void Run() => Remote.Manager.Work();", 0)]
    [DataRow("Manager.Remote.Service", "void Run() {}", 0)]
    [DataRow("Manager.Remote.Service", "public abstract void Run();", 0)]
    [DataRow("Manager.Remote.Service", "void Run() { Run(); Local.Work(); }", 0)]
    public async Task ManagersCallOnlyAllowedManagerBoundaries(string target, string method, int expected)
    {
        var reference = LegacyHarness.Reference(target, "namespace Remote { public static class Manager { public static void Work() {} } }");
        string source = "abstract class C { " + method + " } static class Local { public static void Work() {} }";
        Assert.AreEqual(expected, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd201ManagerNoManagerCalls"), assemblyName: "Manager.Local.Service", additionalReferences: [reference])).Length);
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd201ManagerNoManagerCalls"), additionalReferences: [reference])).Length);
    }

    [TestMethod]
    public async Task MissingManagerCallSymbolsDoNotThrow()
    {
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync("class C { void Run() { Missing(); } }", LegacyHarness.Rule("Vbd201ManagerNoManagerCalls"), assemblyName: "Manager.Local.Service", allowCompilerErrors: true)).Length);
    }
}
