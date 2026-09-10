using System;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace vc.Ifx.Analyzers.Helpers
{

    public static class CodeQualityMetrics
    {
        public static int CountStatements(MethodDeclarationSyntax method)
        {
            if (method.Body != null)
                return method.Body.Statements.Count;
            return method.ExpressionBody != null ? 1 : 0;
        }

        public static int CountParameters(MethodDeclarationSyntax method) => method.ParameterList?.Parameters.Count ?? 0;

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
            var maxDepth = 0;
            var current = 0;

            foreach (var node in method.DescendantNodes())
            {
                if (node is BlockSyntax)
                {
                    current++;
                    maxDepth = Math.Max(maxDepth, current);
                }
                if (node is StatementSyntax && node.Parent is BlockSyntax)
                {
                    current--;
                }
            }

            return maxDepth;
        }
    }
}
