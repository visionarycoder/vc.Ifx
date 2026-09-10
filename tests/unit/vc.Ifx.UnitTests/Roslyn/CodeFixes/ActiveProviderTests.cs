using vc.Ifx.CodeFixes.Providers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace VisionaryCoder.Framework.Tests.Roslyn.CodeFixes;

[TestClass]
public sealed class ActiveProviderTests
{
    [TestMethod]
    public async Task NullGuardPreservesBodyAndCompiles()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("public class C { public int M(string value) { return value.Length; } }");
        var provider = new Ca1062CodeFixProvider();
        provider.FixableDiagnosticIds.Should().Equal("CA1062");
        provider.GetFixAllProvider().Should().BeNull();
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, "CA1062", "value.Length");
        var action = (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Single();
        var changed = await CodeFixHarness.ApplyAsync(action, document);
        await CodeFixHarness.AssertCompilesAsync(changed);
        (await changed.GetTextAsync()).ToString().Should().Contain("ThrowIfNull(value)").And.Contain("return value.Length;");
        (await CodeFixHarness.ActionsAsync(provider, changed, await CodeFixHarness.DiagnosticAsync(changed, "CA1062", "value.Length"))).Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("public int M(string? value) { return value!.Length; }")]
    [DataRow("public int M(string first, string second) { return first.Length; }")]
    [DataRow("public int M(string value) => value.Length;")]
    [DataRow("public int M(ref string value) { return value.Length; }")]
    [DataRow("public int M(int value) { return value; }")]
    [DataRow("private int M(string value) { return value.Length; }")]
    [DataRow("public int M(string value = \"\") { return value.Length; }")]
    [DataRow("public int M(Unknown value) { return 0; }")]
    [DataRow("public System.Collections.Generic.IEnumerable<int> M(string value) { yield return value.Length; }")]
    [DataRow("public int M(string value) {\n#if true\nreturn value.Length;\n#endif\n}")]
    [DataRow("public int M(string value) { return value.Length; ")]
    [DataRow("public async System.Threading.Tasks.Task<int> M(string value) { await System.Threading.Tasks.Task.Yield(); return value.Length; }")]
    [DataRow("public int M(string value) { if (value == null) return 0; return value.Length; }")]
    public async Task NullGuardDeclinesAmbiguousOrBehaviorSensitiveMethods(string member)
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("public class C { " + member + " }");
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, "CA1062", "M");
        (await CodeFixHarness.ActionsAsync(new Ca1062CodeFixProvider(), document, diagnostic)).Should().BeEmpty();
    }

    [TestMethod]
    public async Task AttributeOrderPreservesAuthorizationAndTrivia()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Authorization;\n// route\n[Route(\"/api\")]\n// auth\n[Authorize(Roles = \"Admin\")]\npublic class C { }");
        var provider = new Ifx005ControllerAttributeOrderCodeFixProvider();
        provider.FixableDiagnosticIds.Should().Equal("IFX005");
        provider.GetFixAllProvider().Should().BeNull();
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, "IFX005", "C {");
        var action = (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Single();
        var changed = await CodeFixHarness.ApplyAsync(action, document);
        await CodeFixHarness.AssertCompilesAsync(changed);
        var text = (await changed.GetTextAsync()).ToString();
        text.Should().Contain("// auth\n[Authorize(Roles = \"Admin\")]").And.Contain("// route\n[Route(\"/api\")]");
        text.IndexOf("[Authorize", StringComparison.Ordinal).Should().BeLessThan(text.IndexOf("[Route", StringComparison.Ordinal));
        (await CodeFixHarness.ActionsAsync(provider, changed, await CodeFixHarness.DiagnosticAsync(changed, "IFX005", "C {"))).Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("[Route(\"/api\"), Authorize]")]
    [DataRow("[Authorize][Route(\"/api\")]")]
    [DataRow("[Route(\"/api\")][Unknown]")]
    [DataRow("[Route(\"/api\")]\n#if true\n[Authorize]\n#endif\n")]
    [DataRow("[method: Route(\"/api\")][Authorize]")]
    [DataRow("/// <summary>Controller</summary>\n[Route(\"/api\")][Authorize]")]
    [DataRow("[Route(\"/api\")][Produces(\"application/json\")]")]
    public async Task AttributeOrderDeclinesUnsupportedShapes(string attributes)
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Authorization; " + attributes + "public class C { }");
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, "IFX005", "C {");
        (await CodeFixHarness.ActionsAsync(new Ifx005ControllerAttributeOrderCodeFixProvider(), document, diagnostic)).Should().BeEmpty();
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task ActiveProvidersRejectWrongDiagnosticForeignTreeAndWrongNode(bool nullGuard)
    {
        using var harness = new CodeFixHarness();
        CodeFixProvider provider = nullGuard ? new Ca1062CodeFixProvider() : new Ifx005ControllerAttributeOrderCodeFixProvider();
        var id = nullGuard ? "CA1062" : "IFX005";
        var document = harness.Document("class C { }");
        var other = harness.Document("class C { }");
        foreach (var diagnostic in new[] {
            await CodeFixHarness.DiagnosticAsync(document, "OTHER", "C"),
            await CodeFixHarness.DiagnosticAsync(other, id, "C"),
            await CodeFixHarness.DiagnosticAsync(document, id, "class C { }") })
            (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Should().BeEmpty();
        var broken = harness.Document("class C {");
        (await CodeFixHarness.ActionsAsync(provider, broken, await CodeFixHarness.DiagnosticAsync(broken, id, "C"))).Should().BeEmpty();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Func<Task> register = async () => await CodeFixHarness.ActionsAsync(provider, document, await CodeFixHarness.DiagnosticAsync(document, id, "C"), cancellation.Token);
        await register.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task ActiveActionsHonorCancellation(bool nullGuard)
    {
        using var harness = new CodeFixHarness();
        var source = nullGuard ? "public class C { public int M(string value) { return value.Length; } }"
            : "[Microsoft.AspNetCore.Mvc.Route(\"/api\")][Microsoft.AspNetCore.Authorization.Authorize] public class C { }";
        var document = harness.Document(source);
        CodeFixProvider provider = nullGuard ? new Ca1062CodeFixProvider() : new Ifx005ControllerAttributeOrderCodeFixProvider();
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, nullGuard ? "CA1062" : "IFX005", nullGuard ? "M" : "C {");
        var action = (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Single();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Func<Task> apply = async () => await CodeFixHarness.ApplyAsync(action, document, cancellation.Token);
        await apply.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task AttributeOrderRejectsSourceLookalikes()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("class RouteAttribute : System.Attribute { } class AuthorizeAttribute : System.Attribute { } [Route][Authorize] public class C { }");
        (await CodeFixHarness.ActionsAsync(new Ifx005ControllerAttributeOrderCodeFixProvider(), document,
            await CodeFixHarness.DiagnosticAsync(document, "IFX005", "C {"))).Should().BeEmpty();
    }

    [TestMethod]
    public async Task GuardRetainsOtherCallsAndEscapedIdentifier()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("public class C { public int M(string @event) { System.Console.WriteLine(@event); return @event.Length; } }");
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, "CA1062", "M");
        var action = (await CodeFixHarness.ActionsAsync(new Ca1062CodeFixProvider(), document, diagnostic)).Single();
        var changed = await CodeFixHarness.ApplyAsync(action, document);
        await CodeFixHarness.AssertCompilesAsync(changed);
        (await changed.GetTextAsync()).ToString().Should().Contain("ThrowIfNull(@event)").And.Contain("System.Console.WriteLine(@event)");
    }
}
