using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace vc.Ifx.Analyzers.Helpers
{

    /// <summary>
    /// Detects SQL operations, safe patterns, and sanitization methods in code.
    /// Supports Entity Framework Core patterns and common SQL execution methods.
    /// </summary>
    internal static class SqlOperationDetector
    {
        /// <summary>
        /// SQL methods that are unsafe when used with non-literal/non-interpolated arguments.
        /// </summary>
        private static readonly string[] unsafeSqlMethods = ["FromSqlRaw", "ExecuteSqlRaw"];

        /// <summary>
        /// SQL methods that are safe (parameterized or interpolated).
        /// </summary>
        private static readonly string[] safeSqlMethods = ["FromSqlInterpolated", "FromSql"];

        /// <summary>
        /// Methods known to sanitize or validate input for SQL injection prevention.
        /// </summary>
        private static readonly string[] sanitizingMethods = ["RemoveInjectionVectors"];

        /// <summary>
        /// Checks if an invocation is an unsafe SQL operation.
        /// </summary>
        public static bool IsUnsafeSqlOperation(InvocationExpressionSyntax invocation)
        {
            var methodName = GetMethodName(invocation);
            return methodName != null && unsafeSqlMethods.Contains(methodName);
        }

        /// <summary>
        /// Checks if an invocation is a safe SQL operation.
        /// </summary>
        public static bool IsSafeSqlOperation(InvocationExpressionSyntax invocation)
        {
            var methodName = GetMethodName(invocation);
            return methodName != null && safeSqlMethods.Contains(methodName);
        }

        /// <summary>
        /// Checks if an invocation is a sanitizing method that removes injection vectors.
        /// </summary>
        public static bool IsSanitizingMethod(InvocationExpressionSyntax invocation)
        {
            var methodName = GetMethodName(invocation);
            return methodName != null && sanitizingMethods.Contains(methodName);
        }

        /// <summary>
        /// Extracts the SQL command argument from an unsafe SQL operation.
        /// </summary>
        public static ExpressionSyntax? GetSqlCommandArgument(InvocationExpressionSyntax invocation)
        {
            return invocation.ArgumentList.Arguments.Count > 0
                ? invocation.ArgumentList.Arguments[0].Expression
                : null;
        }

        /// <summary>
        /// Extracts components from string concatenation expressions.
        /// </summary>
        public static List<ExpressionSyntax> GetConcatenationComponents(BinaryExpressionSyntax binary)
        {
            var components = new List<ExpressionSyntax>();
            var current = binary;

            while (current is { } binaryExpr)
            {
                components.Add(binaryExpr.Right);
                current = binaryExpr.Left as BinaryExpressionSyntax;
            }

            if (current != null) components.Add(current);
            components.Reverse();
            return components;
        }

        /// <summary>
        /// Extracts the method name from an invocation expression.
        /// </summary>
        public static string? GetMethodName(InvocationExpressionSyntax invocation)
        {
            return invocation.Expression switch
            {
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
                GenericNameSyntax genericName => genericName.Identifier.Text,
                IdentifierNameSyntax identifierName => identifierName.Identifier.Text,
                _ => null
            };
        }
    }
}
