using System.Collections.Immutable;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace vc.Ifx.CodeFixes.Common
{
    public abstract class PragmaSuppressCodeFixProviderBase : CodeFixProvider
    {
        protected abstract string DiagnosticId { get; }

        protected abstract string Title { get; }

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(this.DiagnosticId);

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (context.Diagnostics.IsEmpty)
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    this.Title,
                    cancellationToken => CaCodeFixUtilities.AddFilePragmaSuppressionAsync(context.Document, this.DiagnosticId, cancellationToken),
                    this.Title),
                context.Diagnostics[0]);

            return Task.CompletedTask;
        }
    }
}