using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Security
{

    /// <summary>
    /// SEC001 Rule: Detects potential SQL injection vectors when unsafe SQL operations
    /// are called with non-literal arguments. This generic rule applies to all code.
    /// </summary>
    internal static class Sec001SqlInjection
    {

        public static readonly DiagnosticDescriptor Rule = new(
            DiagnosticIds.Sec001SqlInjectionVector,
            title: "SQL Injection Vector Detected",
            messageFormat: "Potential SQL injection vector: unsafe SQL method '{0}' called with non-literal argument. Use FromSqlInterpolated, LINQ, or sanitize with SqlInjectionSanitizer.RemoveInjectionVectors().",
            category: "Security",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Detects SQL injection vulnerabilities in Entity Framework Core operations.",
            helpLinkUri: "Docs/Security/sec001.md");

        public static void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not InvocationExpressionSyntax invocation)
                return;

            if (!SqlOperationDetector.IsUnsafeSqlOperation(invocation))
                return;

            var sqlArg = SqlOperationDetector.GetSqlCommandArgument(invocation);
            if (sqlArg == null)
                return;

            var methodName = SqlOperationDetector.GetMethodName(invocation) ?? "Unknown";

            AnalyzeSqlArgument(sqlArg, out var hasVulnerability);
            if (!hasVulnerability)
            {
                return;
            }

            var diagnostic = Diagnostic.Create(Sec001SqlInjection.Rule, invocation.GetLocation(), methodName);
            context.ReportDiagnostic(diagnostic);
        }

        private static void AnalyzeSqlArgument(ExpressionSyntax expression, out bool hasVulnerability)
        {
            hasVulnerability = false;
            switch (expression)
            {
                case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression):
                case InterpolatedStringExpressionSyntax:
                    return;
                case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AddExpression):
                    {
                        var components = SqlOperationDetector.GetConcatenationComponents(binary);
                        var allLiterals = components.All(c => c is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression));
                        if (allLiterals)
                        {
                            return;
                        }
                        break;
                    }
            }

            hasVulnerability = true;
        }
    }
}
