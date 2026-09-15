using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Ifx.Analyzers.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ifx.Analyzers.Rules;

/// <summary>
/// Reports hand-rolled retry loops (a <c>for</c>, <c>while</c>, or <c>do</c> loop containing
/// a <c>try</c>/<c>catch</c> and a call to <c>Task.Delay</c>) outside the Ifx framework,
/// steering callers toward the <c>[Retry]</c> attribute handled by <c>RetryInterceptor</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidManualRetryLoopAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        id: DiagnosticIds.Ifx1501AvoidManualRetryLoop,
        title: "Avoid manual retry loop",
        messageFormat: "'{0}' retries manually with a loop, try/catch, and 'Task.Delay'; apply [Retry] on the boundary-intercepted contract instead",
        category: "Ifx.Proxies",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Ifx.Proxies exposes a RetryInterceptor driven by [Retry] and backed by Polly. A hand-rolled loop with try/catch and Task.Delay duplicates that concern without backoff, jitter, telemetry, or exception classification.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeLoop,
            SyntaxKind.ForStatement,
            SyntaxKind.WhileStatement,
            SyntaxKind.DoStatement);
    }

    private static void AnalyzeLoop(SyntaxNodeAnalysisContext context)
    {
        var loop = context.Node;

        if (!ContainsTryCatch(loop))
        {
            return;
        }

        var containingSymbol = context.ContainingSymbol;
        if (IfxFrameworkNamespace.Contains(containingSymbol))
        {
            return;
        }

        if (!ContainsTaskDelay(loop, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, loop.GetLocation(), containingSymbol!.Name));
    }

    private static bool ContainsTryCatch(SyntaxNode loop)
    {
        return loop.DescendantNodes()
            .OfType<TryStatementSyntax>()
            .Any(tryStatement => tryStatement.Catches.Count > 0);
    }

    private static bool ContainsTaskDelay(SyntaxNode loop, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        foreach (var invocation in loop.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (!(ModelExtensions.GetSymbolInfo(semanticModel, invocation, cancellationToken).Symbol is IMethodSymbol method))
            {
                continue;
            }

            if (method.Name != "Delay")
            {
                continue;
            }

            if (string.Equals(method.ContainingType.ToDisplayString(), "System.Threading.Tasks.Task", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
