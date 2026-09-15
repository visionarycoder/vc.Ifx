using Ifx.Analyzers.Abstractions;
using Ifx.CodeFixes.Providers;

namespace Ifx.Tests.Roslyn.CodeFixes;

[TestClass]
public sealed class Ifx1400NoUnderscorePrefixedIdentifierAdditionalTests
{
    [TestMethod]
    public async Task RenamesParameterInsideLocalFunctionScope()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("""
            class C
            {
                void Run()
                {
                    int Local(int _value)
                    {
                        return _value + 1;
                    }

                    Local(1);
                }
            }
            """);

        var action = (await CodeFixHarness.ActionsAsync(new Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider(), document,
            await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, "_value"))).Single();
        var changed = await CodeFixHarness.ApplyAsync(action, document);

        await CodeFixHarness.AssertCompilesAsync(changed);
        (await changed.GetTextAsync()).ToString().Should().Contain("int Local(int value)").And.Contain("return value + 1;");
    }

    [TestMethod]
    public async Task RenamesForAndUsingLocals()
    {
        using var harness = new CodeFixHarness();
        var provider = new Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider();

        var forDocument = harness.Document("class C { int Run() { for (int _index = 0; _index < 1; _index++) { } return 0; } }");
        var forChanged = await CodeFixHarness.ApplyAsync((await CodeFixHarness.ActionsAsync(provider, forDocument,
            await CodeFixHarness.DiagnosticAsync(forDocument, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, "_index"))).Single(), forDocument);
        await CodeFixHarness.AssertCompilesAsync(forChanged);
        (await forChanged.GetTextAsync()).ToString().Should().Contain("for (int index = 0; index < 1; index++)");

        var usingDocument = harness.Document("class Disposable : System.IDisposable { public void Dispose() { } } class C { void Run() { using var _resource = new Disposable(); _resource.Dispose(); } }");
        var usingChanged = await CodeFixHarness.ApplyAsync((await CodeFixHarness.ActionsAsync(provider, usingDocument,
            await CodeFixHarness.DiagnosticAsync(usingDocument, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, "_resource"))).Single(), usingDocument);
        await CodeFixHarness.AssertCompilesAsync(usingChanged);
        (await usingChanged.GetTextAsync()).ToString().Should().Contain("using var resource = new Disposable();").And.Contain("resource.Dispose();");
    }

    [TestMethod]
    public async Task RenamesFixedPointerLocal()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("""
            unsafe class C
            {
                void Run()
                {
                    int[] buffer = new int[1];
                    fixed (int* _pointer = buffer)
                    {
                        *_pointer = 1;
                    }
                }
            }
            """);

        var action = (await CodeFixHarness.ActionsAsync(new Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider(), document,
            await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, "_pointer"))).Single();
        var changed = await CodeFixHarness.ApplyAsync(action, document);

        await CodeFixHarness.AssertCompilesAsync(changed);
        (await changed.GetTextAsync()).ToString().Should().Contain("fixed (int* pointer = buffer)").And.Contain("*pointer = 1;");
    }

    [TestMethod]
    [DataRow("class C { void Run() { var _ = 0; } }", "_")]
    [DataRow("class C { private event System.Action _handler; }", "_handler")]
    [DataRow("class C { int Run(int _value) { return _value; } }", "return _value;")]
    [DataRow("class C { void Run() { int value = 0; int _value = value; } }", "_value")]
    public async Task DeclinesAccessorlessOrEmptyReplacementTargets(string source, string marker)
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document(source);

        (await CodeFixHarness.ActionsAsync(new Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider(), document,
            await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, marker))).Should().BeEmpty();
    }
}
