using vc.Ifx.Analyzers.Rules.Framework;

namespace VisionaryCoder.Framework.Tests.Roslyn.Analyzers.Legacy;

[TestClass]
public sealed class StateAndSignatureTests
{
    [TestMethod]
    [DataRow("public int Value;", 1)]
    [DataRow("public readonly int Value;", 0)]
    [DataRow("public const int Value = 0;", 0)]
    [DataRow("public static int Value;", 0)]
    [DataRow("public int Value { get; set; }", 1)]
    [DataRow("public static int Value { get; set; }", 0)]
    [DataRow("public int Value { get; }", 0)]
    [DataRow("public int Value { get; init; }", 0)]
    [DataRow("public event System.Action Changed;", 0)]
    public async Task EngineAndContractMutabilityPolicies(string member, int expected)
    {
        string source = "class C { " + member + " }";
        Assert.AreEqual(expected, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd300EngineStateless"), assemblyName: "Engine.Local.Service")).Length);
        Assert.AreEqual(expected, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd500ContractImmutableRule"), assemblyName: "Access.Local.Contract")).Length);
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd300EngineStateless"))).Length);
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd500ContractImmutableRule"))).Length);
    }

    [TestMethod]
    public async Task NonStateTypeKindsAreIgnored()
    {
        const string source = "interface I { int Value {get;set;} } enum E { First } delegate void D();";
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd300EngineStateless"), assemblyName: "Engine.Local.Service")).Length);
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd500ContractImmutableRule"), assemblyName: "Access.Local.Contract")).Length);
    }

    [TestMethod]
    [DataRow("System.Threading.Tasks.Task Run(VisionaryCoder.Framework.ServiceRequest request, System.Threading.CancellationToken token);", 0)]
    [DataRow("System.Threading.Tasks.Task<int> Run(VisionaryCoder.Framework.ServiceRequest request, System.Threading.CancellationToken token);", 0)]
    [DataRow("System.Threading.Tasks.TaskFactory Run(VisionaryCoder.Framework.ServiceRequest request, System.Threading.CancellationToken token);", 1)]
    [DataRow("void Run(VisionaryCoder.Framework.ServiceRequest request, System.Threading.CancellationToken token);", 1)]
    [DataRow("System.Threading.Tasks.Task Run(Other.ServiceRequest request, System.Threading.CancellationToken token);", 1)]
    [DataRow("System.Threading.Tasks.Task Run(VisionaryCoder.Framework.ServiceRequest request, int token);", 1)]
    [DataRow("System.Threading.Tasks.Task Run();", 1)]
    [DataRow("int Value { get; }", 0)]
    public async Task ProxySignatureUsesExactTypeIdentities(string member, int expected)
    {
        string source = """
            namespace Ifx.Proxy { public class ProxyContractAttribute : System.Attribute {} }
            namespace VisionaryCoder.Framework { public class ServiceRequest {} }
            namespace Other { public class ServiceRequest {} }
            [Ifx.Proxy.ProxyContract] interface I { MEMBER }
            class Ordinary { public void Run() {} }
            """.Replace("MEMBER", member, StringComparison.Ordinal);
        var diagnostics = await AnalyzerHarness.RunAsync(source, new Ifx001Signature());
        Assert.AreEqual(expected, diagnostics.Length);
        if (expected == 1)
        {
            Assert.AreEqual("IFX001", diagnostics[0].Id);
            Assert.AreEqual("Run", source.Substring(diagnostics[0].Location.SourceSpan.Start, diagnostics[0].Location.SourceSpan.Length));
        }
    }

    [TestMethod]
    [DataRow("public class C : Remote.Item {}")]
    [DataRow("public class C : Remote.I {}")]
    [DataRow("public class C { Remote.Item field; }")]
    [DataRow("public class C { Remote.Item Prop {get;} }")]
    [DataRow("public class C { Remote.Item[] field; }")]
    [DataRow("public class C { System.Tuple<int, Remote.Item> field; }")]
    public async Task EnginesCannotReferenceManagerTypes(string source)
    {
        var reference = LegacyHarness.Reference("Manager.Other.Service", "namespace Remote { public class Item {} public interface I {} }");
        Assert.AreEqual(1, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd301EngineNoManagerDependency"), assemblyName: "Engine.Local.Service", additionalReferences: [reference])).Length);
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd301EngineNoManagerDependency"), additionalReferences: [reference])).Length);
    }

    [TestMethod]
    public async Task EngineManagerRuleAlsoRecognizesNamespacePolicy()
    {
        const string source = "namespace Manager.Remote { public class Item {} } class C { Manager.Remote.Item field; System.Tuple<int,string> safe; }";
        Assert.AreEqual(1, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd301EngineNoManagerDependency"), assemblyName: "Engine.Local.Service")).Length);
    }
}
