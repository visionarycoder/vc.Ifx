using System.Collections.Generic;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ca1051CodeFixProvider))]
    [Shared]
    public sealed class Ca1051CodeFixProvider : CodeFixProvider
    {
        private const string DiagnosticId = "CA1051";
        private const string Title = "Convert visible field to auto-property";

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
            var field = declarator?.FirstAncestorOrSelf<FieldDeclarationSyntax>();

            if (declarator == null || field == null || field.Declaration.Variables.Count != 1)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Suppress CA1051 for this file",
                        cancellationToken => CaCodeFixUtilities.AddFilePragmaSuppressionAsync(context.Document, DiagnosticId, cancellationToken),
                        "Suppress CA1051 for this file"),
                    diagnostic);
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    cancellationToken => ApplyFixAsync(context.Document, root, field, declarator),
                    Title),
                diagnostic);
        }

        private static Task<Document> ApplyFixAsync(Document document, SyntaxNode root, FieldDeclarationSyntax field, VariableDeclaratorSyntax declarator)
        {
            var allowedModifiers = new HashSet<SyntaxKind>
            {
                SyntaxKind.PublicKeyword,
                SyntaxKind.PrivateKeyword,
                SyntaxKind.ProtectedKeyword,
                SyntaxKind.InternalKeyword,
                SyntaxKind.StaticKeyword,
                SyntaxKind.NewKeyword,
                SyntaxKind.VirtualKeyword,
                SyntaxKind.OverrideKeyword,
                SyntaxKind.SealedKeyword,
                SyntaxKind.AbstractKeyword
            };

            var modifiers = string.Join(" ", field.Modifiers.Where(token => allowedModifiers.Contains(token.Kind())).Select(token => token.Text));
            if (!string.IsNullOrWhiteSpace(modifiers))
            {
                modifiers += " ";
            }

            var accessor = field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword) ? "{ get; }" : "{ get; set; }";
            var initializer = declarator.Initializer != null ? " = " + declarator.Initializer.Value + ";" : string.Empty;
            var propertyText = modifiers + field.Declaration.Type + " " + declarator.Identifier.ValueText + " " + accessor + initializer;
            var property = SyntaxFactory.ParseMemberDeclaration(propertyText);

            if (property == null)
            {
                return CaCodeFixUtilities.AddFilePragmaSuppressionAsync(document, DiagnosticId, default);
            }

            property = property.WithLeadingTrivia(field.GetLeadingTrivia()).WithTrailingTrivia(field.GetTrailingTrivia());
            return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(field, property)));
        }
    }
}