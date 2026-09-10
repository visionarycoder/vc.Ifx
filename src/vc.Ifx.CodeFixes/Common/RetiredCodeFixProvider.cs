using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;

namespace vc.Ifx.CodeFixes.Common;

/// <summary>Preserves construction of withdrawn providers without advertising unsafe edits.</summary>
public abstract class RetiredCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray<string>.Empty;
    /// <inheritdoc />
    public override FixAllProvider? GetFixAllProvider() => null;
    /// <inheritdoc />
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
