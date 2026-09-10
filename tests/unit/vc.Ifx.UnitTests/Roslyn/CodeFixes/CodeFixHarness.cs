using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using vc.Ifx.Analyzers.Rules.Diagnostics;
using vc.Ifx.Roslyn;

namespace VisionaryCoder.Framework.Tests.Roslyn.CodeFixes;

internal sealed class CodeFixHarness : IDisposable
{
    private static readonly ImmutableArray<MetadataReference> References = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator).Distinct(StringComparer.OrdinalIgnoreCase)
        .Select(path => MetadataReference.CreateFromFile(path)).ToImmutableArray<MetadataReference>();
    private readonly AdhocWorkspace workspace = new();

    internal Document Document(string source, string name = "Fixture.cs", ProjectId? projectId = null)
    {
        if (projectId == null)
        {
            var project = workspace.AddProject(ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Fixture", "Fixture", LanguageNames.CSharp,
                parseOptions: new CSharpParseOptions(LanguageVersion.CSharp14),
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable),
                metadataReferences: References));
            projectId = project.Id;
        }
        return workspace.AddDocument(projectId, name, SourceText.From(source, Encoding.UTF8));
    }

    internal static async Task<ImmutableArray<Diagnostic>> DebtAsync(Document document) => await (await document.Project.GetCompilationAsync())!
        .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new DiagnosticDebtAnalyzer())).GetAnalyzerDiagnosticsAsync();

    internal static async Task<Diagnostic> DiagnosticAsync(Document document, string id, string text, string? referencedId = null)
    {
        var source = await document.GetTextAsync();
        var position = source.ToString().IndexOf(text, StringComparison.Ordinal);
        position.Should().BeGreaterThanOrEqualTo(0);
        var descriptor = new DiagnosticDescriptor(id, id, "Fixture diagnostic", "Fixture", DiagnosticSeverity.Warning, true);
        var properties = referencedId == null ? ImmutableDictionary<string, string?>.Empty
            : ImmutableDictionary<string, string?>.Empty.Add(DiagnosticPropertyNames.DiagnosticId, referencedId);
        return Diagnostic.Create(descriptor, Location.Create((await document.GetSyntaxTreeAsync())!, new TextSpan(position, text.Length)), properties);
    }

    internal static async Task<IReadOnlyList<CodeAction>> ActionsAsync(CodeFixProvider provider, Document document, Diagnostic diagnostic, CancellationToken token = default)
    {
        var actions = new List<CodeAction>();
        await provider.RegisterCodeFixesAsync(new CodeFixContext(document, diagnostic, (action, diagnostics) => actions.Add(action), token));
        return actions;
    }

    internal static async Task<Document> ApplyAsync(CodeAction action, Document document, CancellationToken token = default)
    {
        var operations = await action.GetOperationsAsync(token);
        return operations.OfType<ApplyChangesOperation>().Single().ChangedSolution.GetDocument(document.Id)!;
    }

    internal static async Task AssertCompilesAsync(Document document)
    {
        var diagnostics = (await document.Project.GetCompilationAsync())!.GetDiagnostics();
        diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty(string.Join("\n", diagnostics));
    }

    internal static async Task<Solution> FixAllAsync(CodeFixProvider provider, Document document, string equivalenceKey,
        FixAllScope scope = FixAllScope.Document, CancellationToken token = default)
    {
        var context = new FixAllContext(document, provider, scope, equivalenceKey, provider.FixableDiagnosticIds, new DebtDiagnosticProvider(), token);
        var action = await provider.GetFixAllProvider()!.GetFixAsync(context);
        action.Should().NotBeNull();
        return (await action!.GetOperationsAsync(token)).OfType<ApplyChangesOperation>().Single().ChangedSolution;
    }

    internal sealed class DebtDiagnosticProvider : FixAllContext.DiagnosticProvider
    {
        public override async Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
        {
            var tree = await document.GetSyntaxTreeAsync(cancellationToken);
            return (await DebtAsync(document)).Where(diagnostic => diagnostic.Location.SourceTree == tree);
        }
        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken) =>
            Task.FromResult(Enumerable.Empty<Diagnostic>());
        public override async Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken) =>
            await (await project.GetCompilationAsync(cancellationToken))!.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new DiagnosticDebtAnalyzer())).GetAnalyzerDiagnosticsAsync(cancellationToken);
    }

    public void Dispose() => workspace.Dispose();
}
