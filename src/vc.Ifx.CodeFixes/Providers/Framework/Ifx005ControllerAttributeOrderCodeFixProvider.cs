using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace vc.Ifx.CodeFixes.Providers;

/// <summary>Reorders recognized controller attribute lists without removing authorization metadata.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ifx005ControllerAttributeOrderCodeFixProvider))]
[Shared]
public sealed class Ifx005ControllerAttributeOrderCodeFixProvider : CodeFixProvider
{
    private static readonly ImmutableArray<string> Order = ImmutableArray.Create(
        "Microsoft.AspNetCore.Authorization.AuthorizeAttribute",
        "Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute",
        "Microsoft.AspNetCore.Mvc.RouteAttribute",
        "Microsoft.AspNetCore.Mvc.ApiControllerAttribute");

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("IFX005");

    /// <inheritdoc />
    public override FixAllProvider? GetFixAllProvider() => null;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;
        var diagnostic = context.Diagnostics[0];
        if (diagnostic.Id != "IFX005" || diagnostic.Location.SourceTree != root.SyntaxTree || root.ContainsDiagnostics)
            return;
        var declaration = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (declaration == null || declaration.AttributeLists.Count < 2 || declaration.ContainsDirectives)
            return;
        var model = (await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false))!;
        var ranked = new List<(AttributeListSyntax List, int Rank)>();
        foreach (var list in declaration.AttributeLists)
        {
            if (list.Attributes.Count != 1 || list.Target != null || list.DescendantTrivia().Any(trivia => trivia.HasStructure))
                return;
            var type = (model.GetSymbolInfo(list.Attributes[0], context.CancellationToken).Symbol as IMethodSymbol)?.ContainingType;
            if (type == null || !type.ContainingAssembly.Name.StartsWith("Microsoft.AspNetCore.", StringComparison.Ordinal))
                return;
            var rank = Order.IndexOf(type.ToDisplayString());
            if (rank < 0)
                return;
            ranked.Add((list, rank));
        }
        var sorted = ranked.OrderBy(item => item.Rank).Select(item => item.List).ToArray();
        if (sorted.SequenceEqual(declaration.AttributeLists))
            return;
        context.RegisterCodeFix(CodeAction.Create("Reorder recognized controller attributes (review metadata order)", token =>
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(declaration, declaration.WithAttributeLists(SyntaxFactory.List(sorted)))));
        }, "Ifx.IFX005.AttributeOrder"), diagnostic);
    }
}
