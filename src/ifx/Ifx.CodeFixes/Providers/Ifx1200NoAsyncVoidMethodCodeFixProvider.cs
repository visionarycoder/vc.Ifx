using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Ifx.Analyzers.Abstractions;

namespace Ifx.CodeFixes.Providers;

/// <summary>Changes reviewed async void methods to async Task methods.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ifx1200NoAsyncVoidMethodCodeFixProvider))]
[Shared]
public sealed class Ifx1200NoAsyncVoidMethodCodeFixProvider : CodeFixProvider
{
    private const string Title = "Change 'async void' to 'async Task'";
    private const string EquivalenceKey = "Ifx.IFX1200.AsyncVoidToTask";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.Ifx1200NoAsyncVoidMethod];

    /// <inheritdoc />
    public override FixAllProvider? GetFixAllProvider() => null;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;
        var diagnostic = context.Diagnostics[0];
        if (diagnostic.Id != DiagnosticIds.Ifx1200NoAsyncVoidMethod ||
            diagnostic.Location.SourceTree != root.SyntaxTree ||
            root.ContainsDiagnostics ||
            root is not CompilationUnitSyntax compilationUnit)
        {
            return;
        }

        var method = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method?.ReturnType is not PredefinedTypeSyntax returnType ||
            !method.Modifiers.Any(SyntaxKind.AsyncKeyword) ||
            !returnType.Keyword.IsKind(SyntaxKind.VoidKeyword))
        {
            return;
        }

        context.RegisterCodeFix(CodeAction.Create(Title, token => ApplyAsync(context.Document, compilationUnit, method, token), EquivalenceKey), diagnostic);
    }

    private static Task<Document> ApplyAsync(Document document, CompilationUnitSyntax root, MethodDeclarationSyntax method, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var taskType = SyntaxFactory.IdentifierName("Task").WithTriviaFrom(method.ReturnType);
        var updatedMethod = method.WithReturnType(taskType);
        var updatedRoot = root.ReplaceNode(method, updatedMethod);
        if (!HasTaskUsing(updatedRoot))
        {
            updatedRoot = updatedRoot.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")));
        }

        return Task.FromResult(document.WithSyntaxRoot(updatedRoot));
    }

    private static bool HasTaskUsing(CompilationUnitSyntax root) =>
        root.Usings.Any(usingDirective =>
            usingDirective.Alias is null &&
            !usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword) &&
            usingDirective.Name is { } name &&
            name.ToString() == "System.Threading.Tasks");
}
