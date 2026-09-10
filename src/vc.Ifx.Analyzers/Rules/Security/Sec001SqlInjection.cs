using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Security;

/// <summary>
/// SEC001 Rule: Detects potential SQL injection vectors when unsafe SQL operations
/// are called with non-literal arguments. This generic rule applies to all code.
/// </summary>
internal static class Sec001SqlInjection
{

    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.Sec001SqlInjectionVector,
        title: "SQL Injection Vector Detected",
        messageFormat: "Potential SQL injection vector: unsafe SQL method '{0}' called with non-literal argument. Use a parameterized SQL API or LINQ; do not concatenate untrusted input.",
        category: "Security",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Flags non-literal SQL arguments at recognized raw SQL call names. This syntactic policy is not a complete security or data-flow analysis.",
        helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-security-policy");

    public static void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (!SqlOperationDetector.IsUnsafeSqlOperation(invocation))
            return;

        var sqlArg = SqlOperationDetector.GetSqlCommandArgument(invocation);
        if (sqlArg == null)
            return;

        var methodName = SqlOperationDetector.GetMethodName(invocation)!;

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
                return;
            case InterpolatedStringExpressionSyntax interpolated when !interpolated.Contents.OfType<InterpolationSyntax>().Any():
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
