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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ca1801CodeFixProvider))]
    [Shared]
    public sealed class Ca1801CodeFixProvider : CodeFixProvider
    {
        private const string DiagnosticId = "CA1801";
        private const string Title = "Use parameter via discard assignment";

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
            var parameter = node.FirstAncestorOrSelf<ParameterSyntax>();

            if (parameter == null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Suppress CA1801 for this file",
                        cancellationToken => CaCodeFixUtilities.AddFilePragmaSuppressionAsync(context.Document, DiagnosticId, cancellationToken),
                        "Suppress CA1801 for this file"),
                    diagnostic);
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    cancellationToken => ApplyFixAsync(context.Document, root, parameter),
                    Title),
                diagnostic);
        }

        private static Task<Document> ApplyFixAsync(Document document, SyntaxNode root, ParameterSyntax parameter)
        {
            var parameterName = parameter.Identifier.ValueText;
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                return Task.FromResult(document);
            }

            var method = parameter.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (method?.Body != null)
            {
                var assignment = SyntaxFactory.ParseStatement("_ = " + parameterName + ";");
                var updatedBody = method.Body.WithStatements(method.Body.Statements.Insert(0, assignment));
                return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(method, method.WithBody(updatedBody))));
            }

            var constructor = parameter.FirstAncestorOrSelf<ConstructorDeclarationSyntax>();
            if (constructor?.Body != null)
            {
                var assignment = SyntaxFactory.ParseStatement("_ = " + parameterName + ";");
                var updatedBody = constructor.Body.WithStatements(constructor.Body.Statements.Insert(0, assignment));
                return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(constructor, constructor.WithBody(updatedBody))));
            }

            return CaCodeFixUtilities.AddFilePragmaSuppressionAsync(document, DiagnosticId, default);
        }
    }
}
