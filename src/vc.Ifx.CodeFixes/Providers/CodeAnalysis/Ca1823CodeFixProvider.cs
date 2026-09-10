using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using vc.Ifx.CodeFixes.Common;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace vc.Ifx.CodeFixes.Providers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ca1823CodeFixProvider))]
    [Shared]
    public sealed class Ca1823CodeFixProvider : CodeFixProvider
    {
        private const string DiagnosticId = "CA1823";
        private const string Title = "Remove unused private field";

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
            var declarator = node.FirstAncestorOrSelf<VariableDeclaratorSyntax>();

            if (declarator == null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Suppress CA1823 for this file",
                        cancellationToken => CaCodeFixUtilities.AddFilePragmaSuppressionAsync(context.Document, DiagnosticId, cancellationToken),
                        "Suppress CA1823 for this file"),
                    diagnostic);
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    cancellationToken => ApplyFixAsync(context.Document, root, declarator),
                    Title),
                diagnostic);
        }

        private static Task<Document> ApplyFixAsync(Document document, SyntaxNode root, VariableDeclaratorSyntax declarator)
        {
            var field = declarator.FirstAncestorOrSelf<FieldDeclarationSyntax>();
            if (field == null)
            {
                return Task.FromResult(document);
            }

            if (field.Declaration.Variables.Count == 1)
            {
                var updatedRoot = root.RemoveNode(field, SyntaxRemoveOptions.KeepExteriorTrivia) ?? root;
                return Task.FromResult(document.WithSyntaxRoot(updatedRoot));
            }

            var updatedField = field.WithDeclaration(field.Declaration.WithVariables(field.Declaration.Variables.Remove(declarator)));
            return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(field, updatedField)));
        }
    }
}
