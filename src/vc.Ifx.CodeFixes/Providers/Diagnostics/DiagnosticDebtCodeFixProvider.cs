using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using vc.Ifx.Roslyn;

namespace vc.Ifx.CodeFixes.Providers.Diagnostics;

/// <summary>
/// Provides documentation fixes for diagnostic debt reported by <see cref="Analyzers.DiagnosticDebtAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DiagnosticDebtCodeFixProvider))]
[Shared]
public sealed class DiagnosticDebtCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(
            DiagnosticIdentifiers.DiagnosticReferenceRequiresDisposition,
            DiagnosticIdentifiers.DiagnosticSuppressionRequiresJustification);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            string diagnosticId = diagnostic.Properties.TryGetValue("DiagnosticId", out string? value) && value is not null
                ? value
                : "diagnostic";

            string title = diagnostic.Id == DiagnosticIdentifiers.DiagnosticSuppressionRequiresJustification
                ? $"Add justification for {diagnosticId}"
                : $"Add disposition for {diagnosticId}";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    cancellationToken => AddDispositionAsync(context.Document, diagnostic, cancellationToken),
                    equivalenceKey: title),
                diagnostic);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    private static async Task<Document> AddDispositionAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        SourceText sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        TextLine line = sourceText.Lines.GetLineFromPosition(diagnostic.Location.SourceSpan.Start);

        string addition = diagnostic.Id == DiagnosticIdentifiers.DiagnosticSuppressionRequiresJustification
            ? " // Justification: TODO - explain why this diagnostic is intentionally suppressed."
            : " Tracked-by: TODO - record the issue, owner, or removal plan.";

        TextChange change = new(new TextSpan(line.End, 0), addition);
        SourceText updatedText = sourceText.WithChanges(change);
        return document.WithText(updatedText);
    }
}
