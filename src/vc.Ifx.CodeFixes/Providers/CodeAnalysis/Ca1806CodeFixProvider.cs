using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using vc.Ifx.CodeFixes.Common;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace vc.Ifx.CodeFixes.Providers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ca1806CodeFixProvider))]
    [Shared]
    public sealed class Ca1806CodeFixProvider : CodeFixProvider
    {
        private const string DiagnosticId = "CA1806";
        private const string Title = "Assign ignored result to discard";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId);

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null || context.Diagnostics.IsEmpty)
            {
                return;
            }

            var diagnostic = context.Diagnostics[0];
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            var statement = invocation?.FirstAncestorOrSelf<ExpressionStatementSyntax>();

            if (invocation == null || statement == null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Suppress CA1806 for this file",
                        cancellationToken => CaCodeFixUtilities.AddFilePragmaSuppressionAsync(context.Document, DiagnosticId, cancellationToken),
                        "Suppress CA1806 for this file"),
                    diagnostic);
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    cancellationToken => ApplyFixAsync(context.Document, root, statement, invocation),
                    Title),
                diagnostic);
        }

        private static Task<Document> ApplyFixAsync(Document document, SyntaxNode root, ExpressionStatementSyntax statement, InvocationExpressionSyntax invocation)
        {
            var replacement = SyntaxFactory.ParseStatement("_ = " + invocation + ";").WithTriviaFrom(statement);
            var updatedRoot = root.ReplaceNode(statement, replacement);
            return Task.FromResult(document.WithSyntaxRoot(updatedRoot));
        }
    }
}
