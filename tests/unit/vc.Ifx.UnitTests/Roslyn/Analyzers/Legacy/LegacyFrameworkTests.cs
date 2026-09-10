namespace VisionaryCoder.Framework.Tests.Roslyn.Analyzers.Legacy;

[TestClass]
public sealed class LegacyFrameworkTests
{
    [TestMethod]
    [DataRow("", "", "No reason provided", "Unknown")]
    [DataRow("string reason", "null", "No reason provided", "Unknown")]
    [DataRow("string reason, string owner", "\"replace\", null", "replace", "Unknown")]
    [DataRow("string reason, string owner", "\"replace\", \"team\"", "replace", "team")]
    [DataRow("string reason, string owner, int kind", "\"replace\", \"team\", 1", "replace", "team")]
    [DataRow("string reason, string owner, Kind kind", "\"replace\", \"team\", Wsdot.Idl.Ifx.Attributes.Kind.First", "replace", "team")]
    public async Task RefactorMarkersPreserveReasonAndOwner(string parameters, string arguments, string reason, string owner)
    {
        string source = "namespace Wsdot.Idl.Ifx.Attributes { public enum Kind { First, Second } public class RefactorAttribute : System.Attribute { public RefactorAttribute(" + parameters + ") {} } } " +
            "[Wsdot.Idl.Ifx.Attributes.Refactor(" + arguments + ")] class C {}";
        var diagnostics = await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx002Refactor"));
        Assert.AreEqual(1, diagnostics.Length);
        Assert.AreEqual($"C - Submitted by {owner}: {reason}", diagnostics[0].GetMessage());
    }

    [TestMethod]
    [DataRow("class A {}", 0)]
    [DataRow("class A { class Nested {} }", 0)]
    [DataRow("class A {} class B {}", 1)]
    [DataRow("namespace N { class A {} struct S {} class B {} }", 1)]
    [DataRow("namespace N; class A {} struct S {} class B {}", 1)]
    [DataRow("struct S {}", 0)]
    public async Task ClassPerFilePolicyIgnoresNestedClassesAndOtherTypeKinds(string source, int expected)
    {
        var diagnostics = await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx006SingleClassPerFile"));
        Assert.AreEqual(expected, diagnostics.Length);
        if (expected == 1) StringAssert.Contains(diagnostics[0].GetMessage(), "2 top-level classes");
    }

    [TestMethod]
    public async Task PrivateMemberUsageIncludesGenericCallsAndMethodGroups()
    {
        const string source = """
            public class C
            {
                private int unused;
                private int used;
                private int Value { get; set; }
                private int this[int index] => index;
                private void Work<T>() { }
                private void Group() { }
                private void Dead() { }
                private extern void Native();
                public void Run() { used = Value; Work<int>(); System.Action group = Group; }
            }
            """;
        var diagnostics = await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx003UnusedPrivateMember"));
        CollectionAssert.AreEquivalent(new[] { "unused", "Dead" }, diagnostics.Select(d => source.Substring(d.Location.SourceSpan.Start, d.Location.SourceSpan.Length)).ToArray());
    }

    [TestMethod]
    public async Task TypeUsageIncludesGenericInstantiationAndNestedContainers()
    {
        const string source = """
            class Unused {}
            class Constructed<T> {}
            class Container { public class Nested {} }
            abstract class Abstract {}
            static class Static {}
            class MarkerAttribute : System.Attribute {}
            interface I {}
            public class Visible
            {
                object Make() => new Constructed<int>();
                object Nest() => new Container.Nested();
            }
            """;
        var diagnostics = await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx004UnusedType"));
        Assert.AreEqual(1, diagnostics.Length);
        StringAssert.Contains(diagnostics[0].GetMessage(), "Unused");
    }

    [TestMethod]
    public async Task PrivateNestedTypesAreCandidatesButProtectedTypesAreNot()
    {
        const string source = "public class C { private class Dead {} protected class Extensible {} private class Used {} object Run() => new Used(); }";
        var diagnostics = await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx004UnusedType"));
        Assert.AreEqual(1, diagnostics.Length);
        StringAssert.Contains(diagnostics[0].GetMessage(), "Dead");
    }

    [TestMethod]
    public async Task TypeReferencesCoverSignatureAndOperationForms()
    {
        const string source = """
            class A { public static void Work() {} }
            class B {}
            class D : A {}
            public class C
            {
                private A Field;
                private B Prop {get;set;}
                private A Identity(A argument) => argument;
                private T Generic<T>() => default;
                private object Run()
                {
                    A.Work(); Generic<B>(); Generic<int[]>();
                    var a = Field; var b = Prop;
                    object result = (B)null;
                    return typeof(D);
                }
            }
            """;
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx004UnusedType"))).Length);
    }

    [TestMethod]
    [DataRow("System.Text.Json.Serialization")]
    [DataRow("Newtonsoft.Json")]
    [DataRow("System.Runtime.Serialization")]
    [DataRow("System.ComponentModel.DataAnnotations")]
    [DataRow("Microsoft.EntityFrameworkCore")]
    [DataRow("Microsoft.VisualStudio.TestTools.UnitTesting")]
    public async Task ReflectionDiscoveryMarkersPreventFalseUnusedReports(string ns)
    {
        string name = ns.Contains("UnitTesting", StringComparison.Ordinal) ? "TestClass" : "Discover";
        string source = $"namespace {ns} {{ class {name}Attribute : System.Attribute {{}} }} [{ns}.{name}] class C {{}}";
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx004UnusedType"))).Length);
    }

    [TestMethod]
    [DataRow("", 0)]
    [DataRow("[Authorize, ApiVersion, Route, ApiController]", 0)]
    [DataRow("[Route, Authorize]", 1)]
    [DataRow("[Authorize(Roles=\"Admin\"), RequiredScope]", 1)]
    [DataRow("[Authorize, RequiredScope]", 0)]
    [DataRow("[System.Obsolete, RouteAttribute, AuthorizeAttribute]", 1)]
    public async Task ControllerAttributeOrderUsesLegacyPolicy(string attributes, int expected)
    {
        string source = """
            namespace Microsoft.AspNetCore.Mvc { public class ControllerBase {} }
            class AuthorizeAttribute : System.Attribute { public string Roles {get;set;} }
            class ApiVersionAttribute : System.Attribute {}
            class RouteAttribute : System.Attribute {}
            class ApiControllerAttribute : System.Attribute {}
            class RequiredScopeAttribute : System.Attribute {}
            ATTRIBUTES class C : Microsoft.AspNetCore.Mvc.ControllerBase {}
            """.Replace("ATTRIBUTES", attributes, StringComparison.Ordinal);
        var diagnostics = await AnalyzerHarness.RunAsync(source, LegacyHarness.Rule("Ifx005ControllerAttributeOrder"));
        Assert.AreEqual(expected, diagnostics.Length);
        if (expected == 1) Assert.AreEqual("IFX005", diagnostics[0].Id);
    }
}
