using Microsoft.CodeAnalysis.CodeFixes;
using Ifx.Analyzers.Abstractions;
using Ifx.CodeFixes.Providers;

namespace Ifx.Tests.Roslyn.CodeFixes;

[TestClass]
public sealed class Ifx1200NoAsyncVoidMethodCodeFixProviderTests
{
    [TestMethod]
    public async Task ChangesAsyncVoidToAsyncTaskAndCompiles()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("class C { async void Run() { await System.Threading.Tasks.Task.Yield(); } }");
        var provider = new Ifx1200NoAsyncVoidMethodCodeFixProvider();
        provider.FixableDiagnosticIds.Should().Equal(DiagnosticIds.Ifx1200NoAsyncVoidMethod);
        provider.GetFixAllProvider().Should().BeNull();
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1200NoAsyncVoidMethod, "Run(");
        var action = (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Single();
        action.Title.Should().Be("Change 'async void' to 'async Task'");
        action.EquivalenceKey.Should().Be("Ifx.IFX1200.AsyncVoidToTask");
        var changed = await CodeFixHarness.ApplyAsync(action, document);
        await CodeFixHarness.AssertCompilesAsync(changed);
        (await changed.GetTextAsync()).ToString().Should().Contain("using System.Threading.Tasks;").And.Contain("async Task Run()");
        (await CodeFixHarness.ActionsAsync(provider, changed,
            await CodeFixHarness.DiagnosticAsync(changed, DiagnosticIds.Ifx1200NoAsyncVoidMethod, "Run("))).Should().BeEmpty();
    }

    [TestMethod]
    public async Task DoesNotDuplicateExistingTaskUsing()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("using System.Threading.Tasks;\nclass C { async void Run() { await Task.Yield(); } }");
        var action = (await CodeFixHarness.ActionsAsync(new Ifx1200NoAsyncVoidMethodCodeFixProvider(), document,
            await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1200NoAsyncVoidMethod, "Run("))).Single();
        var changed = await CodeFixHarness.ApplyAsync(action, document);
        await CodeFixHarness.AssertCompilesAsync(changed);
        var text = (await changed.GetTextAsync()).ToString();
        text.IndexOf("using System.Threading.Tasks;", StringComparison.Ordinal).Should().Be(text.LastIndexOf("using System.Threading.Tasks;", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("using Tasks = System.Threading.Tasks;\nclass C { async void Run() { await System.Threading.Tasks.Task.Yield(); } }")]
    [DataRow("using static System.Threading.Tasks.Task;\nclass C { async void Run() { await Yield(); } }")]
    public async Task AddsOrdinaryTaskUsingWhenAliasOrStaticUsingExists(string source)
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document(source);
        var action = (await CodeFixHarness.ActionsAsync(new Ifx1200NoAsyncVoidMethodCodeFixProvider(), document,
            await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1200NoAsyncVoidMethod, "Run("))).Single();
        var changed = await CodeFixHarness.ApplyAsync(action, document);
        await CodeFixHarness.AssertCompilesAsync(changed);
        (await changed.GetTextAsync()).ToString().Should().Contain("using System.Threading.Tasks;");
    }

    [TestMethod]
    public async Task RejectsWrongDiagnosticForeignTreeWrongNodeAndMalformedSyntax()
    {
        using var harness = new CodeFixHarness();
        CodeFixProvider provider = new Ifx1200NoAsyncVoidMethodCodeFixProvider();
        var document = harness.Document("class C { async void Run() { await System.Threading.Tasks.Task.Yield(); } }");
        var other = harness.Document("class C { async void Run() { await System.Threading.Tasks.Task.Yield(); } }");
        foreach (var diagnostic in new[]
                 {
                     await CodeFixHarness.DiagnosticAsync(document, "OTHER", "Run("),
                     await CodeFixHarness.DiagnosticAsync(other, DiagnosticIds.Ifx1200NoAsyncVoidMethod, "Run("),
                     await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1200NoAsyncVoidMethod, "class C {")
                 })
        {
            (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Should().BeEmpty();
        }

        var broken = harness.Document("class C { async void Run(");
        (await CodeFixHarness.ActionsAsync(provider, broken,
            await CodeFixHarness.DiagnosticAsync(broken, DiagnosticIds.Ifx1200NoAsyncVoidMethod, "Run("))).Should().BeEmpty();
    }

    [TestMethod]
    public async Task RegistrationAndApplyHonorCancellation()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("class C { async void Run() { await System.Threading.Tasks.Task.Yield(); } }");
        var provider = new Ifx1200NoAsyncVoidMethodCodeFixProvider();
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1200NoAsyncVoidMethod, "Run(");
        var action = (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Single();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Func<Task> register = async () => await CodeFixHarness.ActionsAsync(provider, document, diagnostic, cancellation.Token);
        Func<Task> apply = async () => await CodeFixHarness.ApplyAsync(action, document, cancellation.Token);
        await register.Should().ThrowAsync<OperationCanceledException>();
        await apply.Should().ThrowAsync<OperationCanceledException>();
    }
}
