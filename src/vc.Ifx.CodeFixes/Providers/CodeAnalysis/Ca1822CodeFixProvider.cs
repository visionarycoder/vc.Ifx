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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ca1822CodeFixProvider))]
    [Shared]
    public sealed class Ca1822CodeFixProvider : CodeFixProvider
    {
        private const string DiagnosticId = "CA1822";
        private const string Title = "Mark member as static";

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

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    cancellationToken => ApplyFixAsync(context.Document, root, node),
                    Title),
                diagnostic);
        }

        private static Task<Document> ApplyFixAsync(Document document, SyntaxNode root, SyntaxNode node)
        {
            var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (method != null && !method.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                var updated = method.WithModifiers(method.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword)));
                return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(method, updated)));
            }

            var property = node.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            if (property != null && !property.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                var updated = property.WithModifiers(property.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword)));
                return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(property, updated)));
            }

            var eventDecl = node.FirstAncestorOrSelf<EventDeclarationSyntax>();
            if (eventDecl != null && !eventDecl.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                var updated = eventDecl.WithModifiers(eventDecl.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword)));
                return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(eventDecl, updated)));
            }

            return CaCodeFixUtilities.AddFilePragmaSuppressionAsync(document, DiagnosticId, default);
        }
    }
}
