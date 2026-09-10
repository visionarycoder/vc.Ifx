using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using vc.Ifx.Roslyn;

namespace vc.Ifx.CodeFixes.Providers.Diagnostics;

/// <summary>Adds editable debt scaffolds without claiming that TODO text resolves a diagnostic.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DiagnosticDebtCodeFixProvider))]
[Shared]
public sealed class DiagnosticDebtCodeFixProvider : CodeFixProvider
{
    private const string Disposition = "Tracked-by: TODO - record the issue, owner, or removal plan.";
    private const string Justification = "TODO - explain why this diagnostic is intentionally suppressed.";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("IFX1000", "IFX1001");

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => new DebtFixAllProvider();

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        foreach (var diagnostic in context.Diagnostics)
        {
            var changes = await ChangesAsync(context.Document, new[] { diagnostic }, context.CancellationToken).ConfigureAwait(false);
            if (changes.Length == 0)
                continue;
            context.RegisterCodeFix(CodeAction.Create("Add editable diagnostic debt scaffold",
                token => ApplyAsync(context.Document, changes, token), diagnostic.Id), diagnostic);
        }
    }

    private static async Task<Document> ApplyAsync(Document document, ImmutableArray<TextChange> changes, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return document.WithText((await document.GetTextAsync(token).ConfigureAwait(false)).WithChanges(changes));
    }

    private static async Task<ImmutableArray<TextChange>> ChangesAsync(Document document, IEnumerable<Diagnostic> diagnostics, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);
        if (root is not CompilationUnitSyntax || root.ContainsDiagnostics)
            return ImmutableArray<TextChange>.Empty;
        var text = await document.GetTextAsync(token).ConfigureAwait(false);
        var changes = new Dictionary<TextSpan, TextChange>();
        foreach (var diagnostic in diagnostics)
        {
            token.ThrowIfCancellationRequested();
            if (diagnostic.Location.SourceTree != root.SyntaxTree ||
                !diagnostic.Properties.TryGetValue(DiagnosticPropertyNames.DiagnosticId, out var id) ||
                id == null || DiagnosticIdPattern.ReferenceMatcher.Match(id).Value != id || id.Length == 0)
                continue;
            var span = diagnostic.Location.SourceSpan;
            TextChange? change = null;
            if (diagnostic.Id == "IFX1000")
            {
                var trivia = root.FindTrivia(span.Start);
                if ((trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)) &&
                    trivia.Span.Contains(span) && text.ToString(span) == id &&
                    !new[] { "Tracked-by:", "Justification:", "Fix-by:", "Intentional:" }.Any(marker => trivia.ToString().IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    var end = trivia.Span.End - (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ? 2 : 0);
                    change = new TextChange(new TextSpan(end, 0), " " + Disposition + " ");
                }
            }
            else if (diagnostic.Id == "IFX1001")
            {
                var directive = root.DescendantTrivia(descendIntoTrivia: true).Select(trivia => trivia.GetStructure())
                    .OfType<PragmaWarningDirectiveTriviaSyntax>().FirstOrDefault(node => node.Span.Contains(span));
                if (directive != null)
                {
                    if (directive.IsActive && directive.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword) &&
                        directive.ErrorCodes.Any(code => code.Span == span && NormalizePragmaId(code.ToString()) == id) &&
                        directive.ToString().IndexOf("Justification:", StringComparison.OrdinalIgnoreCase) < 0)
                        change = new TextChange(new TextSpan(text.Lines.GetLineFromPosition(span.Start).End, 0), " // Justification: " + Justification);
                }
                else
                {
                    var attribute = root.FindNode(span).FirstAncestorOrSelf<AttributeSyntax>();
                    if (attribute != null)
                    {
                        // The C# compilation unit above guarantees semantic-model support.
                        var model = (await document.GetSemanticModelAsync(token).ConfigureAwait(false))!;
                        var type = (model.GetSymbolInfo(attribute, token).Symbol as IMethodSymbol)?.ContainingType;
                        if (type != null && !type.Locations.Any(location => location.IsInSource) &&
                            type.ToDisplayString() is "System.Diagnostics.CodeAnalysis.SuppressMessageAttribute" or "System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessageAttribute" &&
                            attribute.ArgumentList!.Arguments.Where((item, index) => item.NameColon != null
                                ? item.NameColon.Name.Identifier.ValueText == "checkId" : index == 1 && item.NameEquals == null).Any(item => item.Expression.Span == span &&
                                model.GetConstantValue(item.Expression, token).Value is string checkId && checkId.Split(':')[0].Trim() == id))
                        {
                            var argument = attribute.ArgumentList!.Arguments.FirstOrDefault(item => item.NameEquals?.Name.Identifier.ValueText == "Justification");
                            var literal = SymbolDisplay.FormatLiteral(Justification, quote: true);
                            if (argument == null)
                            {
                                change = new TextChange(new TextSpan(attribute.ArgumentList.CloseParenToken.SpanStart, 0), ", Justification = " + literal);
                            }
                            else
                            {
                                var constant = model.GetConstantValue(argument.Expression, token);
                                if (constant.HasValue && string.IsNullOrWhiteSpace(constant.Value as string))
                                    change = new TextChange(argument.Expression.Span, literal);
                            }
                        }
                    }
                }
            }
            if (change.HasValue)
                changes[change.Value.Span] = change.Value;
        }
        return changes.Values.OrderBy(change => change.Span.Start).ToImmutableArray();
    }

    private static string NormalizePragmaId(string value) => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number)
        ? "CS" + number.ToString("D4", CultureInfo.InvariantCulture) : value;

    private sealed class DebtFixAllProvider : FixAllProvider
    {
        public override IEnumerable<FixAllScope> GetSupportedFixAllScopes() =>
            new[] { FixAllScope.Document, FixAllScope.Project, FixAllScope.Solution };

        public override async Task<CodeAction?> GetFixAsync(FixAllContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            if (context.CodeActionEquivalenceKey is not ("IFX1000" or "IFX1001"))
                return null;
            var documents = context.Scope switch
            {
                FixAllScope.Document => new[] { context.Document! },
                FixAllScope.Project => context.Project.Documents,
                FixAllScope.Solution => context.Solution.Projects.SelectMany(project => project.Documents),
                _ => Enumerable.Empty<Document>()
            };
            var solution = context.Solution;
            foreach (var document in documents)
            {
                var diagnostics = await context.GetDocumentDiagnosticsAsync(document).ConfigureAwait(false);
                var changes = await ChangesAsync(document, diagnostics.Where(diagnostic => diagnostic.Id == context.CodeActionEquivalenceKey), context.CancellationToken).ConfigureAwait(false);
                if (changes.Length > 0)
                    solution = solution.WithDocumentText(document.Id, (await document.GetTextAsync(context.CancellationToken).ConfigureAwait(false)).WithChanges(changes));
            }
            if (solution == context.Solution)
                return null;
            return CodeAction.Create("Add editable diagnostic debt scaffolds", token =>
            {
                token.ThrowIfCancellationRequested();
                return Task.FromResult(solution);
            }, context.CodeActionEquivalenceKey);
        }
    }
}
