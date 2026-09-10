using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Security
{

    /// <summary>
    /// SEC002 Rule: Detects potential SQL injection vectors when unsanitized tainted parameters
    /// (from controller bindings) are passed to logging methods or Activity tags without sanitization.
    /// This rule applies to WebAPI controllers and prevents log-based injection attacks.
    /// </summary>
    internal static class Sec002ControllerLoggingInjection
    {

        public static readonly DiagnosticDescriptor Rule = new(
            DiagnosticIds.Sec002LoggingSqlInjectionVector,
            title: "Logging SQL Injection Vector Detected",
            messageFormat: "Potential SQL injection vector: unsanitized tainted parameter '{0}' passed to logging method. Sanitize with SqlInjectionSanitizer.RemoveInjectionVectors() before logging.",
            category: "Security",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Detects SQL injection vulnerabilities when untrusted parameters flow into logging statements without sanitization.",
            helpLinkUri: "Docs/Security/sec002.md");

        public static void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var method = context.Node as MethodDeclarationSyntax;
            if (method == null)
                return;

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
                if (tracker.FindLoggingInjectionVectorsInExpression(argument.Expression))
                {
                    var taintedName = ControllerSecurityHelpers.ExtractIdentifierName(argument.Expression) ?? "tainted parameter";
                    var diagnostic = Diagnostic.Create(Sec002ControllerLoggingInjection.Rule, argument.GetLocation(), taintedName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
