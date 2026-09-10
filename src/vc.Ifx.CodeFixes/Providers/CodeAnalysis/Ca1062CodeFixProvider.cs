using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace vc.Ifx.CodeFixes.Providers;

/// <summary>Offers a reviewed null guard for an unambiguous synchronous reference parameter.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ca1062CodeFixProvider))]
[Shared]
public sealed class Ca1062CodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("CA1062");

    /// <inheritdoc />
    public override FixAllProvider? GetFixAllProvider() => null;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;
        var diagnostic = context.Diagnostics[0];
        if (diagnostic.Id != "CA1062" || diagnostic.Location.SourceTree != root.SyntaxTree || root.ContainsDiagnostics)
            return;
        var method = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method?.Body == null || method.ContainsDirectives || method.Modifiers.Any(SyntaxKind.AsyncKeyword) ||
            method.ParameterList.Parameters.Count != 1 || method.Body.DescendantNodes().Any(node => node is IfStatementSyntax or YieldStatementSyntax))
            return;
        var model = (await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false))!;
        var parameter = (IParameterSymbol)model.GetDeclaredSymbol(method.ParameterList.Parameters[0], context.CancellationToken)!;
        if (!method.Modifiers.Any(SyntaxKind.PublicKeyword) || !parameter.Type.IsReferenceType ||
            parameter.Type.TypeKind == TypeKind.Error || parameter.NullableAnnotation == NullableAnnotation.Annotated ||
            parameter.RefKind != RefKind.None || parameter.IsOptional)
            return;
        var exception = model.Compilation.GetTypeByMetadataName("System.ArgumentNullException");
        if (exception == null || exception.Locations.Any(location => location.IsInSource) || exception.GetMembers("ThrowIfNull").Length == 0)
            return;
        if (method.Body.DescendantNodes().OfType<InvocationExpressionSyntax>().Any(invocation =>
            (model.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol)?.ContainingType.Equals(exception, SymbolEqualityComparer.Default) == true))
            return;
        var name = method.ParameterList.Parameters[0].Identifier.Text;
        var statement = SyntaxFactory.ParseStatement("global::System.ArgumentNullException.ThrowIfNull(" + name + ");")
            .WithAdditionalAnnotations(Formatter.Annotation);
        context.RegisterCodeFix(CodeAction.Create("Validate argument (changes null-input behavior)", token =>
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(method.Body, method.Body.WithStatements(method.Body.Statements.Insert(0, statement)))));
        }, "Ifx.CA1062.NullGuard"), diagnostic);
    }
}
