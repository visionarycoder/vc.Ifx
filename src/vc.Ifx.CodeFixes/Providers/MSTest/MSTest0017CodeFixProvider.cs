using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace vc.Ifx.CodeFixes.Providers
{

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MSTest0017CodeFixProvider))]
    [Shared]
    public sealed class MSTest0017CodeFixProvider : CodeFixProvider
    {
        private const string DiagnosticId = "MSTEST0017";
        private const string Title = "Swap expected/actual assertion arguments";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId);

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null)
            {
                return;
            }

            var diagnostic = context.Diagnostics.First();
            var targetNode = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var invocation = targetNode.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation is null)
            {
                return;
            }

            if (!CanFix(invocation))
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    cancellationToken => SwapArgumentsAsync(context.Document, root, invocation),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static bool CanFix(InvocationExpressionSyntax invocation)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null)
            {
                return false;
            }

            var assertIdentifier = memberAccess.Expression as IdentifierNameSyntax;
            if (assertIdentifier == null || assertIdentifier.Identifier.Text != "Assert")
            {
                return false;
            }

            var methodName = memberAccess.Name.Identifier.Text;
            if (methodName != "AreEqual" && methodName != "AreNotEqual" && methodName != "AreSame" && methodName != "AreNotSame")
            {
                return false;
            }

            return invocation.ArgumentList.Arguments.Count >= 2;
        }

        private static Task<Document> SwapArgumentsAsync(Document document, SyntaxNode root, InvocationExpressionSyntax invocation)
        {
            var arguments = invocation.ArgumentList.Arguments;
            var firstArgument = arguments[0];
            var secondArgument = arguments[1];

            var swappedFirst = secondArgument.WithTriviaFrom(firstArgument);
            var swappedSecond = firstArgument.WithTriviaFrom(secondArgument);

            var updatedArguments = arguments;
            updatedArguments = updatedArguments.Replace(updatedArguments[0], swappedFirst);
            updatedArguments = updatedArguments.Replace(updatedArguments[1], swappedSecond);

            var updatedInvocation = invocation.WithArgumentList(invocation.ArgumentList.WithArguments(updatedArguments));
            var updatedRoot = root.ReplaceNode(invocation, updatedInvocation);

            return Task.FromResult(document.WithSyntaxRoot(updatedRoot));
        }
    }
}
