using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using vc.Ifx.CodeFixes.Common;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace vc.Ifx.CodeFixes.Providers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ca2207CodeFixProvider))]
    [Shared]
    public sealed class Ca2207CodeFixProvider : CodeFixProvider
    {
        private const string DiagnosticId = "CA2207";
        private const string Title = "Add a zero-valued enum member";

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
            var enumDeclaration = node.FirstAncestorOrSelf<EnumDeclarationSyntax>();

            if (enumDeclaration == null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Suppress CA2207 for this file",
                        cancellationToken => CaCodeFixUtilities.AddFilePragmaSuppressionAsync(context.Document, DiagnosticId, cancellationToken),
                        "Suppress CA2207 for this file"),
                    diagnostic);
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    cancellationToken => ApplyFixAsync(context.Document, root, enumDeclaration),
                    Title),
                diagnostic);
        }

        private static Task<Document> ApplyFixAsync(Document document, SyntaxNode root, EnumDeclarationSyntax enumDeclaration)
        {
            if (enumDeclaration.Members.Any(HasZeroValue))
            {
                return Task.FromResult(document);
            }

            var noneMember = SyntaxFactory.EnumMemberDeclaration("None")
                .WithEqualsValue(
                    SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal(0))));

            var updated = enumDeclaration.WithMembers(enumDeclaration.Members.Insert(0, noneMember));
            return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(enumDeclaration, updated)));
        }

        private static bool HasZeroValue(EnumMemberDeclarationSyntax member)
        {
            var literal = member.EqualsValue?.Value as LiteralExpressionSyntax;
            return literal != null
                && literal.IsKind(SyntaxKind.NumericLiteralExpression)
                && literal.Token.ValueText == "0";
        }
    }
}