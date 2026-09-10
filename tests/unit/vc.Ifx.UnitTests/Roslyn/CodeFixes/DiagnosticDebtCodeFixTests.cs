using Microsoft.CodeAnalysis.CodeFixes;
using vc.Ifx.CodeFixes.Providers.Diagnostics;

namespace VisionaryCoder.Framework.Tests.Roslyn.CodeFixes;

[TestClass]
public sealed class DiagnosticDebtCodeFixTests
{
    [TestMethod]
    [DataRow("// CA1825\npublic class C { }", "Tracked-by:")]
    [DataRow("/* CA1825 */ public class C { }", "Tracked-by:")]
    [DataRow("/*\n CA1825\n */ public class C { }", "Tracked-by:")]
    [DataRow("#pragma warning disable CA1825\npublic class C { }", "Justification:")]
    [DataRow("#pragma warning disable 168 // review\r\npublic class C { }", "Justification:")]
    [DataRow("[System.Diagnostics.CodeAnalysis.SuppressMessage(\"Usage\", \"CA1825\")] public class C { }", "Justification =")]
    [DataRow("[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage(\"Usage\", \"CA1825:Title\", Justification=\"\")] public class C { }", "Justification=")]
    [DataRow("[System.Diagnostics.CodeAnalysis.SuppressMessage(checkId: \"CA1825\", category: \"Usage\")] public class C { }", "Justification =")]
    [DataRow("[System.Diagnostics.CodeAnalysis.SuppressMessage(\"Usage\", \"CA1825\", Justification=null)] public class C { }", "Justification=")]
    [DataRow("[System.Diagnostics.CodeAnalysis.SuppressMessage(\"Usage\", \"CA1825\", Justification=\"  \" )] public class C { }", "Justification=")]
    public async Task ScaffoldPreservesCompilationAndDoesNotClaimDebtIsResolved(string source, string marker)
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document(source);
        await CodeFixHarness.AssertCompilesAsync(document);
        var diagnostic = (await CodeFixHarness.DebtAsync(document)).Single();
        var provider = new DiagnosticDebtCodeFixProvider();
        var actions = await CodeFixHarness.ActionsAsync(provider, document, diagnostic);
        actions.Should().ContainSingle();
        var changed = await CodeFixHarness.ApplyAsync(actions[0], document);
        await CodeFixHarness.AssertCompilesAsync(changed);
        (await changed.GetTextAsync()).ToString().Should().Contain(marker).And.Contain("TODO");
        (await changed.GetTextAsync()).Encoding.Should().Be((await document.GetTextAsync()).Encoding);
        var outstanding = await CodeFixHarness.DebtAsync(changed);
        outstanding.Should().ContainSingle(d => d.Id == diagnostic.Id);
        (await CodeFixHarness.ActionsAsync(provider, changed, outstanding.Single())).Should().BeEmpty();
    }

    [TestMethod]
    public async Task FixAllDeduplicatesSeveralIdsOnOneCommentOrPragma()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("// CA1825 CS0168\n#pragma warning disable CA1825, CS0168\npublic class C { }\n");
        var provider = new DiagnosticDebtCodeFixProvider();
        var diagnostic = (await CodeFixHarness.DebtAsync(document)).First(d => d.Id == "IFX1000");
        var action = (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Single();
        var solution = await CodeFixHarness.FixAllAsync(provider, document, action.EquivalenceKey!);
        var changed = solution.GetDocument(document.Id)!;
        (await changed.GetTextAsync()).ToString().Split("Tracked-by:").Should().HaveCount(2);
        await CodeFixHarness.AssertCompilesAsync(changed);
        diagnostic = (await CodeFixHarness.DebtAsync(changed)).First(d => d.Id == "IFX1001");
        action = (await CodeFixHarness.ActionsAsync(provider, changed, diagnostic)).Single();
        solution = await CodeFixHarness.FixAllAsync(provider, changed, action.EquivalenceKey!);
        (await solution.GetDocument(document.Id)!.GetTextAsync()).ToString().Split("Justification:").Should().HaveCount(2);
    }

    [TestMethod]
    public async Task RegistrationAndApplyHonorCancellation()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("// CA1825\npublic class C { }");
        var diagnostic = (await CodeFixHarness.DebtAsync(document)).Single();
        var provider = new DiagnosticDebtCodeFixProvider();
        var action = (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Single();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Func<Task> register = async () => await CodeFixHarness.ActionsAsync(provider, document, diagnostic, cancellation.Token);
        Func<Task> apply = async () => await CodeFixHarness.ApplyAsync(action, document, cancellation.Token);
        await register.Should().ThrowAsync<OperationCanceledException>();
        await apply.Should().ThrowAsync<OperationCanceledException>();
    }
}
