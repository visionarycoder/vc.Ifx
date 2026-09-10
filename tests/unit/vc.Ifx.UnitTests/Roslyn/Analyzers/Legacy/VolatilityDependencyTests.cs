using Microsoft.CodeAnalysis;

namespace VisionaryCoder.Framework.Tests.Roslyn.Analyzers.Legacy;

[TestClass]
public sealed class VolatilityDependencyTests
{
    private const string RemoteSource = "namespace Remote { public class Item {} public struct Value {} public interface I {} public delegate void Handler(); }";

    [TestMethod]
    [DataRow("Vbd102InfrastructureCannotDependOnDomain", "Ifx.Core", "Access.Data.Contract")]
    [DataRow("Vbd400NoAccessToAccessCalls", "Access.Local.Service", "Access.Remote.Service")]
    [DataRow("Vbd501ContractCannotDependOnService", "Access.Local.Contract", "Access.Remote.Service")]
    [DataRow("Vbd600StableDependsOnVolatile", "Access.Local.Contract", "Access.Remote.Service")]
    [DataRow("Vbd602ForbiddenDependencyDirection", "Access.Local.Service", "Manager.Remote.Service")]
    [DataRow("Vbd700ServiceCannotDependOnOtherService", "Access.Local.Service", "Access.Remote.Service")]
    [DataRow("Vbd800OrmMustBeReferencedByServiceOnly", "Client.Local.WebApi", "Access.Remote.Orm")]
    public async Task MemberDependencyShapesAreRecognized(string rule, string assembly, string target)
    {
        var reference = LegacyHarness.Reference(target, RemoteSource);
        string[] declarations =
        [
            "public class C : Remote.Item {}",
            "public class C : Remote.I {}",
            "public class C { public Remote.Item Field; }",
            "public class C { public Remote.Item Property { get; } }",
            "public class C { public int this[Remote.Item key] => 0; }",
            "public class C { public Remote.Item Run() => null; }",
            "public class C { public void Run(Remote.Item argument) {} }",
            "public class C { public void Run<T>() where T : Remote.Item {} }",
            "public class C { public event Remote.Handler Changed; }",
            "public class C { public Remote.Item[] Values; }",
            "public unsafe class C { public Remote.Value* Pointer; }",
            "public class C { public System.Tuple<Remote.Item, Remote.Item> Items; }"
        ];
        foreach (string source in declarations)
        {
            var diagnostics = await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule(rule), assemblyName: assembly, additionalReferences: [reference]);
            Assert.AreEqual(1, diagnostics.Length, $"{rule}: {source}");
            Assert.IsTrue(diagnostics[0].Location.IsInSource);
            StringAssert.Contains(diagnostics[0].GetMessage(), rule == "Vbd602ForbiddenDependencyDirection" ? "forbidden type" : target);
        }
    }

    [TestMethod]
    [DataRow("Vbd102InfrastructureCannotDependOnDomain", "Ifx.Core")]
    [DataRow("Vbd400NoAccessToAccessCalls", "Access.Local.Service")]
    [DataRow("Vbd501ContractCannotDependOnService", "Access.Local.Contract")]
    [DataRow("Vbd600StableDependsOnVolatile", "Access.Local.Contract")]
    [DataRow("Vbd602ForbiddenDependencyDirection", "Access.Local.Service")]
    [DataRow("Vbd700ServiceCannotDependOnOtherService", "Access.Local.Service")]
    [DataRow("Vbd800OrmMustBeReferencedByServiceOnly", "Client.Local.WebApi")]
    public async Task CompleteAllowedGraphDoesNotProduceViolations(string rule, string assembly)
    {
        var reference = LegacyHarness.Reference("Unclassified.Library", RemoteSource);
        const string source = """
            public unsafe class C<T> : Remote.Item, Remote.I where T : Remote.Item
            {
                public C<T> Self;
                public Remote.Item Stored;
                public Remote.Item[] Items;
                public Remote.Value* Pointer;
                public System.Tuple<Remote.Item, Remote.Item> Pair;
                public event Remote.Handler Changed;
                public Remote.Item Property { get; set; }
                public Remote.Item this[Remote.Item key] => key;
                public Remote.Item Run<U>(Remote.Item value) where U : Remote.Item => value;
                public class Nested {}
            }
            """;
        var diagnostics = await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule(rule), assemblyName: assembly, additionalReferences: [reference]);
        Assert.AreEqual(0, diagnostics.Length, rule);
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync("class C {}", LegacyHarness.Rule(rule))).Length);
    }

    [TestMethod]
    [DataRow("Client.Remote.WebApi", "Access.Remote.Orm", 1)]
    [DataRow("Access.Remote.Service", "Access.Remote.Orm.Financial", 0)]
    [DataRow("Access.Remote.Service", "Access.Remote.Orm.Administration.Financial", 0)]
    [DataRow("Access.Other.Service", "Access.Remote.Orm", 1)]
    [DataRow("Access.Other.Orm", "Access.Remote.Orm", 0)]
    [DataRow("Ifx.Core", "Access.Remote.Orm", 0)]
    [DataRow("App.Tests", "Access.Remote.Orm", 0)]
    [DataRow("Unknown", "Access.Remote.Orm", 0)]
    public async Task OrmOwnershipNormalizesCompoundSuffixes(string assembly, string target, int expected)
    {
        var reference = LegacyHarness.Reference(target, RemoteSource);
        Assert.AreEqual(expected, (await AnalyzerHarness.RunAsync("class C { Remote.Item item; }", LegacyHarness.Rule("Vbd800OrmMustBeReferencedByServiceOnly"),
            assemblyName: assembly, additionalReferences: [reference])).Length);
    }

    [TestMethod]
    [DataRow("Vbd100SingleVault", "Ifx.Core", "Access.Remote.Service", "Engine.Remote.Service", 1)]
    [DataRow("Vbd100SingleVault", "Ifx.Core", "Access.First.Service", "Access.Second.Service", 0)]
    [DataRow("Vbd100SingleVault", "Client.App", "Access.Remote.Service", "Engine.Remote.Service", 0)]
    [DataRow("Vbd202ManagerCannotDependOnAccess", "Manager.Local.Service", "Access.Remote.Service", "Engine.Remote.Service", 1)]
    [DataRow("Vbd202ManagerCannotDependOnAccess", "Manager.Local.Tests", "Access.Remote.Service", "Client.Remote.WebApi", 0)]
    [DataRow("Vbd202ManagerCannotDependOnAccess", "Engine.Local.Service", "Access.Remote.Service", "Client.Remote.WebApi", 0)]
    [DataRow("Vbd202ManagerCannotDependOnAccess", "Ifx.Core", "Access.Remote.Service", "Client.Remote.WebApi", 0)]
    [DataRow("Vbd601InvalidLayerDependency", "Access.Local.Service", "Engine.Remote.Service", "Client.Remote.WebApi", 2)]
    [DataRow("Vbd601InvalidLayerDependency", "Client.Local.WebApi", "Access.Remote.Service", "Manager.Remote.Service", 0)]
    [DataRow("Vbd601InvalidLayerDependency", "Access.Local.Tests", "Engine.Remote.Service", "Client.Remote.WebApi", 0)]
    [DataRow("Vbd601InvalidLayerDependency", "Unknown", "Engine.Remote.Service", "Client.Remote.WebApi", 0)]
    public async Task CompilationDependenciesUseProjectPolicy(string rule, string assembly, string first, string second, int expected)
    {
        var references = new[] { LegacyHarness.Reference(first, "public class First {}"), LegacyHarness.Reference(second, "public class Second {}") };
        var diagnostics = await AnalyzerHarness.RunAsync("class C {}", LegacyHarness.Rule(rule), assemblyName: assembly, additionalReferences: references);
        Assert.AreEqual(expected, diagnostics.Length, rule);
        Assert.IsTrue(diagnostics.All(diagnostic => diagnostic.Location == Location.None));
        if (rule == "Vbd100SingleVault" && expected == 1)
        {
            StringAssert.Contains(diagnostics[0].GetMessage(), "Access: Access.Remote.Service, Engine: Engine.Remote.Service");
            var reversed = await AnalyzerHarness.RunAsync("class C {}", LegacyHarness.Rule(rule), assemblyName: assembly, additionalReferences: references.Reverse());
            Assert.AreEqual(diagnostics[0].GetMessage(), reversed[0].GetMessage());
        }
    }

    [TestMethod]
    [DataRow("Client.Consumer", 1)]
    [DataRow("Consumer.UnitTests", 0)]
    [DataRow("Consumer.Benchmarks", 0)]
    [DataRow("", 0)]
    public async Task VaultFriendAssembliesRequireTestIntent(string friend, int expected)
    {
        string source = $"[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"{friend}\")] class C {{}}";
        var diagnostics = await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd101VaultNoLeakage"), assemblyName: "Ifx.Core");
        Assert.AreEqual(expected, diagnostics.Length);
        if (expected == 1) StringAssert.Contains(diagnostics[0].GetMessage(), friend);
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd101VaultNoLeakage"), assemblyName: "Client.App")).Length);
    }
}
