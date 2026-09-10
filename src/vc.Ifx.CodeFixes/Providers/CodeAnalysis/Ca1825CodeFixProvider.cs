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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ca1825CodeFixProvider))]
    [Shared]
    public sealed class Ca1825CodeFixProvider : CodeFixProvider
    {
        private const string DiagnosticId = "CA1825";
        private const string Title = "Use Array.Empty<T>()";

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
            var arrayCreation = node.FirstAncestorOrSelf<ArrayCreationExpressionSyntax>();

            if (arrayCreation == null || !IsZeroLengthArray(arrayCreation))
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Suppress CA1825 for this file",
                        cancellationToken => CaCodeFixUtilities.AddFilePragmaSuppressionAsync(context.Document, DiagnosticId, cancellationToken),
                        "Suppress CA1825 for this file"),
                    diagnostic);
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    cancellationToken => ApplyFixAsync(context.Document, root, arrayCreation),
                    Title),
                diagnostic);
        }

        private static bool IsZeroLengthArray(ArrayCreationExpressionSyntax arrayCreation)
        {
            var rank = arrayCreation.Type.RankSpecifiers.FirstOrDefault();
            var size = rank?.Sizes.FirstOrDefault() as LiteralExpressionSyntax;
            return size != null
                && size.IsKind(SyntaxKind.NumericLiteralExpression)
                && size.Token.ValueText == "0";
        }

        private static Task<Document> ApplyFixAsync(Document document, SyntaxNode root, ArrayCreationExpressionSyntax arrayCreation)
        {
            var elementType = arrayCreation.Type.ElementType.ToString();
            var replacement = SyntaxFactory.ParseExpression("System.Array.Empty<" + elementType + ">()").WithTriviaFrom(arrayCreation);
            var updatedRoot = root.ReplaceNode(arrayCreation, replacement);
            return Task.FromResult(document.WithSyntaxRoot(updatedRoot));
        }
    }
}
