using System;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace vc.Ifx.Analyzers.Helpers;

public static class CodeQualityMetrics
{
    public static int CountStatements(MethodDeclarationSyntax method)
    {
        if (method.Body != null)
            return method.Body.Statements.Count;
        return method.ExpressionBody != null ? 1 : 0;
    }

    public static int CountParameters(MethodDeclarationSyntax method) => method.ParameterList.Parameters.Count;

    public static int CountLocalVariables(MethodDeclarationSyntax method) => method.DescendantNodes().OfType<VariableDeclaratorSyntax>().Count();

    public static int ComputeCyclomaticComplexity(MethodDeclarationSyntax method)
    {
        return method.DescendantNodes().Count(n =>
            n is IfStatementSyntax
                or ForStatementSyntax
                or ForEachStatementSyntax
                or WhileStatementSyntax
                or DoStatementSyntax
                or CaseSwitchLabelSyntax
                or ConditionalExpressionSyntax
                or BinaryExpressionSyntax { OperatorToken.RawKind: (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.AmpersandAmpersandToken or (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.BarBarToken }
        ) + 1;
    }

    public static int ComputeNestingDepth(MethodDeclarationSyntax method)
    {
        // The method body is depth one; sibling blocks never increase each other's depth.
        return method.DescendantNodes(node => node is not (LocalFunctionStatementSyntax or AnonymousFunctionExpressionSyntax))
            .OfType<BlockSyntax>()
            .Select(block => block.AncestorsAndSelf().OfType<BlockSyntax>().Count())
            .DefaultIfEmpty(0)
            .Max();
    }
}
