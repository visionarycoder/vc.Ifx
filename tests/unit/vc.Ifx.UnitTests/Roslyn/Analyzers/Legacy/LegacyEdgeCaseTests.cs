using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Rules.Framework;

namespace VisionaryCoder.Framework.Tests.Roslyn.Analyzers.Legacy;

[TestClass]
public sealed class LegacyEdgeCaseTests
{
    [TestMethod]
    public async Task MissingFrameworkReferencesDoNotCrashFriendAnalysis()
    {
        var compilation = CSharpCompilation.Create("Ifx.Local", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = await compilation.WithAnalyzers(ImmutableArray.Create(LegacyHarness.Rule("Vbd101VaultNoLeakage"))).GetAnalyzerDiagnosticsAsync();
        Assert.AreEqual(0, diagnostics.Length);
    }

    [TestMethod]
    public void UnresolvedAttributeDataIsNotReflectionDiscovery()
    {
        var predicate = typeof(Ifx004UnusedType).GetMethod("IsReflectionDiscoveryAttribute", BindingFlags.NonPublic | BindingFlags.Static)!.CreateDelegate<Func<AttributeData, bool>>();
        Assert.IsFalse(predicate(new UnresolvedAttribute()));
    }

    private sealed class UnresolvedAttribute : AttributeData
    {
        protected override INamedTypeSymbol? CommonAttributeClass => null;
        protected override IMethodSymbol? CommonAttributeConstructor => null;
        protected override SyntaxReference? CommonApplicationSyntaxReference => null;
        protected override ImmutableArray<TypedConstant> CommonConstructorArguments => [];
        protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments => [];
    }

    [TestMethod]
    public async Task PublicAndImplicitMemberUsesAreNotPrivateCandidates()
    {
        const string source = "record R; public class C { public int Field; public int Prop {get;set;} public void Work() {} void Run() { Field = Prop; Work(); System.Action a = Work; new R().ToString(); } }";
        var diagnostics = await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx003UnusedPrivateMember"));
        Assert.AreEqual(1, diagnostics.Length);
        StringAssert.Contains(diagnostics[0].GetMessage(), "Run");
    }

    [TestMethod]
    public async Task AnnotatedNonProxyInterfaceIsIgnored()
    {
        const string source = "namespace Ifx.Proxy { class ProxyContractAttribute : System.Attribute {} } [System.Obsolete] interface I { void Run(); }";
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx001Signature"))).Length);
    }

    [TestMethod]
    public async Task NonRefactorAttributeDoesNotMatchByShape()
    {
        const string source = "namespace Wsdot.Idl.Ifx.Attributes { class RefactorAttribute : System.Attribute {} } [System.Obsolete] class C {}";
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx002Refactor"))).Length);
    }

    [TestMethod]
    [DataRow("Vbd100SingleVault", "Ifx.Local", "Access.Remote.Service", "Manager.Remote.Service", 1)]
    [DataRow("Vbd100SingleVault", "Ifx.Local", "Access.Remote.Service", "Access.Other.Service", 0)]
    [DataRow("Vbd202ManagerCannotDependOnAccess", "Manager.Local.Service", "Access.Remote.Service", "Manager.Remote.Service", 1)]
    [DataRow("Vbd601InvalidLayerDependency", "Access.Local.Service", "Manager.Remote.Service", "Access.Other.Service", 1)]
    public async Task CompilationReferencesParticipateInDependencyPolicy(string rule, string assembly, string first, string second, int expected)
    {
        MetadataReference[] references = new[] { first, second }.Select(name =>
            CSharpCompilation.Create(name, references: AnalyzerHarness.References,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)).ToMetadataReference()).ToArray();
        var diagnostics = await AnalyzerHarness.RunAsync("class C {}", LegacyHarness.Rule(rule), assemblyName: assembly, additionalReferences: references);
        Assert.AreEqual(expected, diagnostics.Length);
        Assert.IsTrue(diagnostics.All(diagnostic => diagnostic.Location == Location.None));
    }

    [TestMethod]
    [DataRow("public int Value => 1;", 0)]
    [DataRow("public int Value { get => 1; init {} }", 0)]
    [DataRow("public int Value { get => 1; set {} }", 1)]
    public async Task ExplicitPropertiesDoNotDependOnBackingFieldOrder(string member, int expected)
    {
        foreach (var (rule, assembly) in new[] { ("Vbd300EngineStateless", "Engine.Local.Service"), ("Vbd500ContractImmutableRule", "Access.Local.Contract") })
            Assert.AreEqual(expected, (await AnalyzerHarness.RunAsync("class C { " + member + " }", LegacyHarness.Rule(rule), assemblyName: assembly)).Length);
    }

    [TestMethod]
    public async Task EngineManagerReferencesIncludeExplicitPropertiesAndMixedInterfaces()
    {
        var reference = LegacyHarness.Reference("Manager.Other.Service", "namespace Remote { public class Item {} public interface I {} }");
        const string source = "interface ISafe {} interface IEmpty {} class A : ISafe, Remote.I {} class B { Remote.Item Property => null; } class C { System.Tuple<int,string> safe; }";
        Assert.AreEqual(2, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd301EngineNoManagerDependency"), assemblyName: "Engine.Local.Service", additionalReferences: [reference])).Length);
    }

    [TestMethod]
    [DataRow("[assembly: System.Runtime.CompilerServices.InternalsVisibleTo] namespace System.Runtime.CompilerServices { class InternalsVisibleToAttribute : System.Attribute {} }")]
    [DataRow("[assembly: Missing]")]
    public async Task InvalidFriendAttributesDoNotCrash(string source)
    {
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Vbd101VaultNoLeakage"), assemblyName: "Ifx.Local", allowCompilerErrors: true)).Length);
    }

    [TestMethod]
    public async Task InvalidAttributesDoNotCrashFrameworkRules()
    {
        foreach (string rule in new[] { "Ifx002Refactor", "Ifx004UnusedType" })
        {
            var diagnostics = await AnalyzerHarness.RunAsync("[Missing] class C {}", LegacyHarness.Rule(rule), allowCompilerErrors: true);
            Assert.AreEqual(rule == "Ifx004UnusedType" ? 1 : 0, diagnostics.Length);
        }
    }

    [TestMethod]
    public async Task UnannotatedInterfaceIsNotAProxyContract()
    {
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync("interface I { void Run(); }", LegacyHarness.Rule("Ifx001Signature"))).Length);
    }

    [TestMethod]
    public async Task DynamicIsNotAnExactTaskReturnType()
    {
        const string source = "namespace Ifx.Proxy { class ProxyContractAttribute : System.Attribute {} } [Ifx.Proxy.ProxyContract] interface I { dynamic Run(); }";
        Assert.AreEqual(1, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx001Signature"), additionalReferences: [MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly.Location)])).Length);
    }

    [TestMethod]
    [DataRow("namespace Outer { namespace Inner { class A {} class B {} } }", 1, false)]
    [DataRow("namespace Outer; namespace Inner { class A {} class B {} }", 1, true)]
    [DataRow("class A { class B { class C {} } }", 0, false)]
    public async Task NestedNamespacesAreNotNestedTypes(string source, int expected, bool invalidSource)
    {
        Assert.AreEqual(expected, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx006SingleClassPerFile"), allowCompilerErrors: invalidSource)).Length);
    }

    [TestMethod]
    public void LegacyNestingDoesNotCountNestedFunctionBodies()
    {
        var method = CSharpSyntaxTree.ParseText("class C { void Run() { void Local() { if (true) {} } System.Action a = () => { if (true) {} }; System.Action b = delegate { if (true) {} }; } }")
            .GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
        Assert.AreEqual(1, CodeQualityMetrics.ComputeNestingDepth(method));
    }

    [TestMethod]
    [DataRow("HttpGet")]
    [DataRow("HttpPost")]
    [DataRow("HttpPut")]
    [DataRow("HttpDelete")]
    [DataRow("HttpPatch")]
    [DataRow("HttpHead")]
    [DataRow("HttpOptions")]
    public void RecognizesEveryLegacyHttpVerb(string name)
    {
        var method = CSharpSyntaxTree.ParseText("class C { [" + name + "] void Run() {} }").GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
        Assert.IsTrue(ControllerSecurityHelpers.IsControllerMethod(method));
    }

    [TestMethod]
    [DataRow("LogDebug")]
    [DataRow("LogInformation")]
    [DataRow("LogWarning")]
    [DataRow("LogError")]
    [DataRow("LogCritical")]
    [DataRow("Log")]
    [DataRow("SetTag")]
    [DataRow("SetStatus")]
    [DataRow("AddEvent")]
    public void RecognizesEveryLegacyLoggingSink(string name)
    {
        Assert.IsTrue(ControllerSecurityHelpers.IsLoggingOrActivityMethod((InvocationExpressionSyntax)SyntaxFactory.ParseExpression("logger." + name + "(value)")));
    }

    [TestMethod]
    public async Task ImplementedInterfacesAndAnonymousTypesAreHandled()
    {
        const string source = "interface I {} public class C : I { object Run() => new { Value = 1 }; }";
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx004UnusedType"))).Length);
    }

    [TestMethod]
    [DataRow("[] class C : Microsoft.AspNetCore.Mvc.ControllerBase {}")]
    [DataRow("record C : Microsoft.AspNetCore.Mvc.ControllerBase;")]
    [DataRow("[Authorize(\"policy\")] class C : Microsoft.AspNetCore.Mvc.ControllerBase {}")]
    [DataRow("[Authorize(Roles=\"admin\")] class C : Microsoft.AspNetCore.Mvc.ControllerBase {}")]
    public async Task IncompleteOrUnorderedControllerAttributesAreSafe(string declaration)
    {
        string source = "namespace Microsoft.AspNetCore.Mvc { public class ControllerBase {} } class AuthorizeAttribute : System.Attribute { public AuthorizeAttribute() {} public AuthorizeAttribute(string policy) {} public string Roles {get;set;} } " + declaration;
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx005ControllerAttributeOrder"), allowCompilerErrors: true)).Length);
    }
}
