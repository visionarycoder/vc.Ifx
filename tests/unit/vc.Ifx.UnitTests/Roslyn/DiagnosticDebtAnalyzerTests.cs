using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using vc.Ifx.Analyzers.Rules.Diagnostics;
using vc.Ifx.CodeFixes.Providers.Diagnostics;
using vc.Ifx.Roslyn;

namespace VisionaryCoder.Framework.Tests.Roslyn;

[TestClass]
public sealed class DiagnosticDebtAnalyzerTests
{
    [TestMethod]
    public async Task Analyzer_reports_comment_diagnostic_reference_without_disposition()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeSourceAsync(
            """
            public sealed class Widget
            {
                // CA1822 should be handled later.
                public int Value() => 42;
            }
            """);

        diagnostics.Should().ContainSingle(diagnostic =>
            diagnostic.Id == DiagnosticIdentifiers.DiagnosticReferenceRequiresDisposition &&
            diagnostic.Properties["DiagnosticId"] == "CA1822");
    }

    [TestMethod]
    public async Task Analyzer_ignores_comment_diagnostic_reference_with_disposition()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeSourceAsync(
            """
            public sealed class Widget
            {
                // CA1822 Tracked-by: IFX-123
                public int Value() => 42;
            }
            """);

        diagnostics.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Analyzer_reports_pragma_suppression_without_justification()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeSourceAsync(
            """
            #pragma warning disable CS0168

            public sealed class Widget
            {
                public void Run()
                {
                    int unused;
                }
            }
            """);

        diagnostics.Should().ContainSingle(diagnostic =>
            diagnostic.Id == DiagnosticIdentifiers.DiagnosticSuppressionRequiresJustification &&
            diagnostic.Properties["DiagnosticId"] == "CS0168");
    }

    [TestMethod]
    public async Task Code_fix_adds_justification_to_pragma_suppression()
    {
        Document document = CreateDocument(
            """
            #pragma warning disable CS0168

            public sealed class Widget
            {
            }
            """);

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeDocumentAsync(document);
        List<CodeAction> actions = new();
        CodeFixContext context = new(
            document,
            diagnostics.Single(diagnostic => diagnostic.Id == DiagnosticIdentifiers.DiagnosticSuppressionRequiresJustification),
            (action, _) => actions.Add(action),
            CancellationToken.None);

        DiagnosticDebtCodeFixProvider provider = new();
        await provider.RegisterCodeFixesAsync(context);

        actions.Should().ContainSingle();
        ImmutableArray<CodeActionOperation> operations = await actions[0].GetOperationsAsync(CancellationToken.None);
        ApplyChangesOperation applyChanges = operations.OfType<ApplyChangesOperation>().Single();
        Document updatedDocument = applyChanges.ChangedSolution.GetDocument(document.Id)!;
        SourceText updatedText = await updatedDocument.GetTextAsync();

        updatedText.ToString().Should().Contain("Justification: TODO - explain why this diagnostic is intentionally suppressed.");
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeSourceAsync(string source)
    {
        Document document = CreateDocument(source);
        return await AnalyzeDocumentAsync(document);
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeDocumentAsync(Document document)
    {
        Compilation compilation = (await document.Project.GetCompilationAsync())!;
        CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(new DiagnosticDebtAnalyzer()));

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    private static Document CreateDocument(string source)
    {
        AdhocWorkspace workspace = new();
        Project project = workspace
            .AddProject("AnalyzerTests", LanguageNames.CSharp)
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.Preview))
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .WithMetadataReferences(GetMetadataReferences());

        return workspace.AddDocument(project.Id, "AnalyzerTests.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));
    }
}
