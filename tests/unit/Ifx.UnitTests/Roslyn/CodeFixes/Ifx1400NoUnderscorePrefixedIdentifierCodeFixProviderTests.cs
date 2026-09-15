using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Ifx.Analyzers.Abstractions;
using Ifx.CodeFixes.Providers;

namespace Ifx.Tests.Roslyn.CodeFixes;

[TestClass]
public sealed class Ifx1400NoUnderscorePrefixedIdentifierCodeFixProviderTests
{
    [TestMethod]
    public async Task RenamesFieldAcrossSolutionAndCompiles()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("""
            partial class C
            {
                private int _state;

                public int Get()
                {
                    return _state;
                }
            }
            """);
        var second = harness.Document("""
            partial class C
            {
                public void Set(int next)
                {
                    _state = next;
                }
            }
            """, name: "Fixture2.cs", projectId: document.Project.Id);
        document = second.Project.Solution.GetDocument(document.Id)!;
        second = document.Project.Solution.GetDocument(second.Id)!;
        var provider = new Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider();
        provider.FixableDiagnosticIds.Should().Equal(DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier);
        provider.GetFixAllProvider().Should().BeNull();
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, "_state");
        var action = (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Single();

        action.Title.Should().Be("Remove underscore prefix from '_state'");
        action.EquivalenceKey.Should().Be("Ifx.IFX1400.RemoveUnderscorePrefix");

        var changedSolution = await ApplySolutionAsync(action);
        var changed = changedSolution.GetDocument(document.Id)!;
        var changedSecond = changedSolution.GetDocument(second.Id)!;
        await AssertCompilesAsync(changedSolution, document.Project.Id);
        (await changed.GetTextAsync()).ToString().Should().Contain("private int state;").And.Contain("return state;");
        (await changedSecond.GetTextAsync()).ToString().Should().Contain("state = next;");
    }

    [TestMethod]
    public async Task RenamesParameterAndUpdatesMethodBody()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("class C { int Run(int _value) { return _value + 1; } }");
        var provider = new Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider();
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, "_value");
        var action = (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Single();
        var changed = await CodeFixHarness.ApplyAsync(action, document);

        await CodeFixHarness.AssertCompilesAsync(changed);
        (await changed.GetTextAsync()).ToString().Should().Contain("int Run(int value)").And.Contain("return value + 1;");
    }

    [TestMethod]
    public async Task RenamesLocalVariableInAccessorAndUpdatesUsages()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("class C { int Value { get { int _result = 1; return _result; } } }");
        var provider = new Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider();
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, "_result");
        var action = (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Single();
        var changed = await CodeFixHarness.ApplyAsync(action, document);

        await CodeFixHarness.AssertCompilesAsync(changed);
        (await changed.GetTextAsync()).ToString().Should().Contain("int result = 1;").And.Contain("return result;");
    }

    [TestMethod]
    public async Task RenamesLambdaParameterAndUpdatesBody()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("class C { System.Func<int, int> Increment = (_value) => _value + 1; }");
        var provider = new Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider();
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, "_value");
        var action = (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Single();
        var changed = await CodeFixHarness.ApplyAsync(action, document);

        await CodeFixHarness.AssertCompilesAsync(changed);
        (await changed.GetTextAsync()).ToString().Should().Contain("(value) => value + 1");
    }

    [TestMethod]
    public async Task RejectsWrongDiagnosticForeignTreeWrongNodeAndMalformedSyntax()
    {
        using var harness = new CodeFixHarness();
        CodeFixProvider provider = new Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider();
        var document = harness.Document("class C { int Run(int _value) { return _value; } }");
        var other = harness.Document("class C { int Run(int _value) { return _value; } }");

        foreach (var diagnostic in new[]
                 {
                     await CodeFixHarness.DiagnosticAsync(document, "OTHER", "_value"),
                     await CodeFixHarness.DiagnosticAsync(other, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, "_value"),
                     await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, "class C {")
                 })
        {
            (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Should().BeEmpty();
        }

        var broken = harness.Document("class C { int Run(int _value)");
        (await CodeFixHarness.ActionsAsync(provider, broken,
            await CodeFixHarness.DiagnosticAsync(broken, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, "_value"))).Should().BeEmpty();
    }

    [TestMethod]
    public async Task DeclinesRenameWhenResultIsEmptyOrCollides()
    {
        using var harness = new CodeFixHarness();
        var empty = harness.Document("class C { void Run() { int __ = 0; System.Console.WriteLine(__); } }");
        (await CodeFixHarness.ActionsAsync(new Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider(), empty,
            await CodeFixHarness.DiagnosticAsync(empty, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, "__"))).Should().BeEmpty();

        var collision = harness.Document("class C { int Run(int _value, int value) { return _value + value; } }");
        (await CodeFixHarness.ActionsAsync(new Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider(), collision,
            await CodeFixHarness.DiagnosticAsync(collision, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, "_value"))).Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("delegate int Converter(int _value);", "_value")]
    [DataRow("class C { public int _value; }", "_value")]
    [DataRow("class C { private int _value; private int value; }", "_value")]
    [DataRow("class C { int Run(int _value) { int value() => 0; return _value + value(); } }", "_value")]
    [DataRow("class C { int Run(int _value) { foreach (var value in new[] { 1 }) { return value; } return _value; } }", "_value")]
    [DataRow("class C { int Run(int _value) { try { } catch (System.Exception value) { return value.Message.Length; } return _value; } }", "_value")]
    [DataRow("class C { int Run(int _value) { var pair = (1, 2); var (value, other) = pair; return _value + other; } }", "_value")]
    public async Task DeclinesUnsupportedOrConflictingShapes(string source, string marker)
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document(source);
        (await CodeFixHarness.ActionsAsync(new Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider(), document,
            await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, marker))).Should().BeEmpty();
    }

    [TestMethod]
    public async Task RegistrationAndApplyHonorCancellation()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("class C { int Run(int _value) { return _value; } }");
        var provider = new Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider();
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier, "_value");
        var action = (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Single();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Func<Task> register = async () => await CodeFixHarness.ActionsAsync(provider, document, diagnostic, cancellation.Token);
        Func<Task> apply = async () => await CodeFixHarness.ApplyAsync(action, document, cancellation.Token);
        await register.Should().ThrowAsync<OperationCanceledException>();
        await apply.Should().ThrowAsync<OperationCanceledException>();
    }

    private static async Task<Solution> ApplySolutionAsync(CodeAction action, CancellationToken cancellationToken = default)
    {
        var operations = await action.GetOperationsAsync(cancellationToken);
        return operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
    }

    private static async Task AssertCompilesAsync(Solution solution, ProjectId projectId)
    {
        var diagnostics = (await solution.GetProject(projectId)!.GetCompilationAsync())!.GetDiagnostics();
        diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty(string.Join("\n", diagnostics));
    }
}
