using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Ifx.Analyzers.Abstractions;

namespace Ifx.CodeFixes.Providers;

/// <summary>Renames underscore-prefixed private fields, parameters, and locals to remove the prefix.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider))]
[Shared]
public sealed class Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier
    ];

    /// <inheritdoc />
    public override FixAllProvider? GetFixAllProvider() => null;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;
        var diagnostic = context.Diagnostics[0];
        if (diagnostic.Id != DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier || diagnostic.Location.SourceTree != root.SyntaxTree || root.ContainsDiagnostics)
        {
            return;
        }

        var tokenParent = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent!;

        var model = (await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false))!;
        ISymbol? symbol = GetDeclaredSymbol(tokenParent, model, context.CancellationToken);
        if (symbol is null || !CanRename(symbol))
        {
            return;
        }

        string? newName = GetReplacementName(symbol.Name);
        if (newName is null)
        {
            return;
        }

        string replacementName = newName;
        if (HasCollision(symbol, tokenParent, replacementName))
        {
            return;
        }

        context.RegisterCodeFix(CodeAction.Create(
            "Remove underscore prefix from '" + symbol.Name + "'",
            cancellationToken => RenameAsync(context.Document.Project.Solution, symbol, replacementName, cancellationToken),
            "Ifx.IFX1400.RemoveUnderscorePrefix"), diagnostic);
    }

    private static bool CanRename(ISymbol symbol) => symbol switch
    {
        IFieldSymbol field => !field.IsImplicitlyDeclared && field.DeclaredAccessibility == Accessibility.Private,
        ILocalSymbol => true,
        IParameterSymbol => true,
        _ => false
    };

    private static ISymbol? GetDeclaredSymbol(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (node is ParameterSyntax parameter)
        {
            return model.GetDeclaredSymbol(parameter, cancellationToken);
        }

        if (node is VariableDeclaratorSyntax variable)
        {
            return model.GetDeclaredSymbol(variable, cancellationToken);
        }

        return null;
    }

    private static string? GetReplacementName(string name)
    {
        int index = 0;
        while (index < name.Length && name[index] == '_')
        {
            index++;
        }

        return index >= name.Length ? null : name.Substring(index);
    }

    private static async Task<Solution> RenameAsync(Solution solution, ISymbol symbol, string newName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var options = new SymbolRenameOptions(false, false, false, false);
        return await Renamer.RenameSymbolAsync(solution, symbol, options, newName, cancellationToken).ConfigureAwait(false);
    }

    private static bool HasCollision(ISymbol symbol, SyntaxNode declarationNode, string newName)
    {
        return symbol switch
        {
            IFieldSymbol field => field.ContainingType.GetMembers(newName).Any(member => !SymbolEqualityComparer.Default.Equals(member, field)),
            IParameterSymbol => HasScopeCollision(declarationNode, newName),
            ILocalSymbol => HasScopeCollision(declarationNode, newName),
            _ => true
        };
    }

    private static bool HasScopeCollision(SyntaxNode declarationNode, string newName)
    {
        SyntaxNode? scope = GetContainingExecutableScope(declarationNode);
        if (scope is null)
        {
            return true;
        }

        foreach (var parameter in scope.DescendantNodes().OfType<ParameterSyntax>())
        {
            if (!ReferenceEquals(parameter, declarationNode) && parameter.Identifier.ValueText == newName)
            {
                return true;
            }
        }

        foreach (var variable in scope.DescendantNodes().OfType<VariableDeclaratorSyntax>())
        {
            if (!ReferenceEquals(variable, declarationNode) && IsLocalVariable(variable) && variable.Identifier.ValueText == newName)
            {
                return true;
            }
        }

        foreach (var localFunction in scope.DescendantNodes().OfType<LocalFunctionStatementSyntax>())
        {
            if (localFunction.Identifier.ValueText == newName)
            {
                return true;
            }
        }

        foreach (var iteration in scope.DescendantNodes().OfType<ForEachStatementSyntax>())
        {
            if (iteration.Identifier.ValueText == newName)
            {
                return true;
            }
        }

        foreach (var declaration in scope.DescendantNodes().OfType<CatchDeclarationSyntax>())
        {
            if (declaration.Identifier.ValueText == newName)
            {
                return true;
            }
        }

        foreach (var designation in scope.DescendantNodes().OfType<SingleVariableDesignationSyntax>())
        {
            if (designation.Identifier.ValueText == newName)
            {
                return true;
            }
        }

        return false;
    }

    private static SyntaxNode? GetContainingExecutableScope(SyntaxNode declarationNode)
    {
        SyntaxNode? localFunction = declarationNode.FirstAncestorOrSelf<LocalFunctionStatementSyntax>();
        if (localFunction is not null)
        {
            return localFunction;
        }

        SyntaxNode? anonymousFunction = declarationNode.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>();
        if (anonymousFunction is not null)
        {
            return anonymousFunction;
        }

        SyntaxNode? method = declarationNode.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>();
        if (method is not null)
        {
            return method;
        }

        return declarationNode.FirstAncestorOrSelf<AccessorDeclarationSyntax>();
    }

    private static bool IsLocalVariable(VariableDeclaratorSyntax variable)
    {
        if (variable.Parent is not VariableDeclarationSyntax declaration)
        {
            return false;
        }

        var parent = declaration.Parent;
        if (parent is LocalDeclarationStatementSyntax)
        {
            return true;
        }

        if (parent is ForStatementSyntax)
        {
            return true;
        }

        if (parent is UsingStatementSyntax)
        {
            return true;
        }

        return parent is FixedStatementSyntax;
    }
}
