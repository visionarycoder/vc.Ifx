using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Security;

/// <summary>
/// SEC002 compatibility policy: tracks controller parameter names passed to logging sinks.
/// This syntax-only policy does not prove SQL safety or comprehensive data-flow safety.
/// </summary>
internal static class Sec002ControllerLoggingInjection
{

    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.Sec002LoggingSqlInjectionVector,
        title: "Unreviewed controller input in logging",
        messageFormat: "Controller input '{0}' is passed to a logging sink. Review sensitive data and use structured logging with appropriate output encoding.",
        category: "Security",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Flags recognized controller parameter names and assignment aliases at logging sinks. This compatibility policy is not a complete taint analysis.",
        helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-security-policy");

    public static void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var method = (MethodDeclarationSyntax)context.Node;

        if (!ControllerSecurityHelpers.IsControllerMethod(method))
            return;

        var tracker = new ControllerParameterTracker(method);
        AnalyzeMethodBody(method.Body, tracker, context);
    }

    private static void AnalyzeMethodBody(BlockSyntax? body, ControllerParameterTracker tracker, SyntaxNodeAnalysisContext context)
    {
        if (body == null)
            return;

        foreach (var statement in body.Statements)
        {
            AnalyzeStatement(statement, tracker, context);
        }
    }

    private static void AnalyzeStatement(StatementSyntax statement, ControllerParameterTracker tracker, SyntaxNodeAnalysisContext context)
    {
        var descendants = statement.DescendantNodes();

        foreach (var node in descendants)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            if (node is AssignmentExpressionSyntax assignment)
            {
                tracker.TrackAssignment(assignment);
            }
            else if (node is InvocationExpressionSyntax invocation)
            {
                if (ControllerSecurityHelpers.IsLoggingOrActivityMethod(invocation))
                {
                    AnalyzeLoggingInvocation(invocation, tracker, context);
                }
            }
        }
    }

    private static void AnalyzeLoggingInvocation(InvocationExpressionSyntax invocation, ControllerParameterTracker tracker, SyntaxNodeAnalysisContext context)
    {
        var arguments = invocation.ArgumentList.Arguments;
        foreach (var argument in arguments)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            if (tracker.FindLoggingInjectionVectorsInExpression(argument.Expression))
            {
                var taintedName = ControllerSecurityHelpers.ExtractIdentifierName(argument.Expression) ?? "tainted parameter";
                var diagnostic = Diagnostic.Create(Sec002ControllerLoggingInjection.Rule, argument.GetLocation(), taintedName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
