using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using vc.Ifx.CodeFixes.Providers.Diagnostics;

namespace VisionaryCoder.Framework.Tests.Roslyn.CodeFixes;

[TestClass]
public sealed class DebtSafetyTests
{
    [TestMethod]
    [DataRow("// CA1825\npublic class C { }", "CA1825", "OTHER", "CA1825")]
    [DataRow("// CA1825\npublic class C { }", "CA1825", "IFX1000", null)]
    [DataRow("// CA1825\npublic class C { }", "CA1825", "IFX1000", "")]
    [DataRow("// CA1825\npublic class C { }", "CA1825", "IFX1000", "garbage")]
    [DataRow("// CA1825\npublic class C { }", "CA1825", "IFX1000", "CS0168")]
    [DataRow("public class CA1825 { }", "CA1825", "IFX1000", "CA1825")]
    [DataRow("/* CA1825", "CA1825", "IFX1000", "CA1825")]
    [DataRow("[System.Diagnostics.CodeAnalysis.SuppressMessage(\"Usage\", \"CA1825\",)] public class C { }", "\"CA1825\"", "IFX1001", "CA1825")]
    [DataRow("/// <summary>CA1825</summary>\npublic class C { }", "CA1825", "IFX1000", "CA1825")]
    [DataRow("// CA1825 Tracked-by: TODO\npublic class C { }", "CA1825", "IFX1000", "CA1825")]
    [DataRow("#pragma warning restore CA1825\npublic class C { }", "CA1825", "IFX1001", "CA1825")]
    [DataRow("#pragma warning disable CA1825\npublic class C { }", "CA1825", "IFX1001", "CS0168")]
    [DataRow("#pragma warning disable CA1825 // Justification: TODO\npublic class C { }", "CA1825", "IFX1001", "CA1825")]
    [DataRow("#if false\n#pragma warning disable CA1825\n#endif\npublic class C { }", "CA1825", "IFX1001", "CA1825")]
    [DataRow("#pragma warning disable CA1825\npublic class C { }", "CA", "IFX1001", "CA1825")]
    [DataRow("public class CA1825 { }", "CA1825", "IFX1001", "CA1825")]
    [DataRow("[Unknown(\"CA1825\")] public class C { }", "\"CA1825\"", "IFX1001", "CA1825")]
    [DataRow("[System.Obsolete(\"CA1825\")] public class C { }", "\"CA1825\"", "IFX1001", "CA1825")]
    [DataRow("[System.Diagnostics.CodeAnalysis.SuppressMessage(\"x\", \"CA1825\")] public class C { }", "\"CA1825\"", "IFX1001", "CS0168")]
    [DataRow("[System.Diagnostics.CodeAnalysis.SuppressMessage(\"x\", \"CA1825\", Justification=\"Reviewed\")] public class C { }", "\"CA1825\"", "IFX1001", "CA1825")]
    [DataRow("[System.Diagnostics.CodeAnalysis.SuppressMessage(\"x\", \"CA1825\", Justification=Missing)] public class C { }", "\"CA1825\"", "IFX1001", "CA1825")]
    [DataRow("[System.Diagnostics.CodeAnalysis.SuppressMessage(\"CA1825\", \"CS0168\")] public class C { }", "\"CA1825\"", "IFX1001", "CA1825")]
    public async Task DeclinesUnsupportedOrStaleDiagnostics(string source, string location, string id, string? target)
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document(source);
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, id, location, target);
        (await CodeFixHarness.ActionsAsync(new DiagnosticDebtCodeFixProvider(), document, diagnostic)).Should().BeEmpty();
    }

    [TestMethod]
    public async Task DeclinesForeignTree()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("// CA1825\nclass C { }");
        var other = harness.Document("// CA1825\nclass C { }");
        var diagnostic = (await CodeFixHarness.DebtAsync(other)).Single();
        (await CodeFixHarness.ActionsAsync(new DiagnosticDebtCodeFixProvider(), document, diagnostic)).Should().BeEmpty();
    }

    [TestMethod]
    [DataRow(FixAllScope.Project, 2)]
    [DataRow(FixAllScope.Solution, 3)]
    public async Task FixAllHonorsScopeAndPreservesOtherDocuments(FixAllScope scope, int count)
    {
        using var harness = new CodeFixHarness();
        var first = harness.Document("// CA1825\nclass A { }", "A.cs");
        var second = harness.Document("// CA1825\nclass B { }", "B.cs", first.Project.Id);
        var third = harness.Document("// CA1825\nclass C { }", "C.cs");
        var clean = harness.Document("class D { }", "D.cs", third.Project.Id);
        first = clean.Project.Solution.GetDocument(first.Id)!;
        var provider = new DiagnosticDebtCodeFixProvider();
        var solution = await CodeFixHarness.FixAllAsync(provider, first, "IFX1000", scope);
        var texts = new List<string>();
        foreach (var document in solution.Projects.SelectMany(project => project.Documents))
            texts.Add((await document.GetTextAsync()).ToString());
        texts.Count(text => text.Contains("Tracked-by:", StringComparison.Ordinal)).Should().Be(count);
        (await solution.GetDocument(clean.Id)!.GetTextAsync()).ToString().Should().Be("class D { }");
        provider.GetFixAllProvider().GetSupportedFixAllScopes().Should().BeEquivalentTo(new[] { FixAllScope.Document, FixAllScope.Project, FixAllScope.Solution });
    }

    [TestMethod]
    [DataRow("unknown", FixAllScope.Document)]
    [DataRow("IFX1000", FixAllScope.Custom)]
    [DataRow("IFX1000", FixAllScope.Document)]
    public async Task FixAllDeclinesUnknownKeyUnsupportedScopeAndNoChanges(string key, FixAllScope scope)
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("class C { }");
        var provider = new DiagnosticDebtCodeFixProvider();
        var context = new FixAllContext(document, provider, scope, key, provider.FixableDiagnosticIds, new CodeFixHarness.DebtDiagnosticProvider(), default);
        (await provider.GetFixAllProvider().GetFixAsync(context)).Should().BeNull();
    }

    [TestMethod]
    public async Task FixAllHonorsCancellationDuringRegistrationAndApplication()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("// CA1825\nclass C { }");
        var provider = new DiagnosticDebtCodeFixProvider();
        using var cancellation = new CancellationTokenSource();
        var context = new FixAllContext(document, provider, FixAllScope.Document, "IFX1000", provider.FixableDiagnosticIds, new CodeFixHarness.DebtDiagnosticProvider(), cancellation.Token);
        var action = (await provider.GetFixAllProvider().GetFixAsync(context))!;
        cancellation.Cancel();
        Func<Task> registration = async () => await provider.GetFixAllProvider().GetFixAsync(context);
        Func<Task> application = async () => await action.GetOperationsAsync(cancellation.Token);
        await registration.Should().ThrowAsync<OperationCanceledException>();
        await application.Should().ThrowAsync<OperationCanceledException>();
    }
}
