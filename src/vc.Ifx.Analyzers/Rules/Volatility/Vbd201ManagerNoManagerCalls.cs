using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility;

public static class Vbd201ManagerNoManagerCalls
{
    public static readonly DiagnosticDescriptor Rule = new(DiagnosticIds.Vbd201ManagerNoManagerCalls, "Managers cannot call other managers", "Manager '{0}' references another manager", "Architecture", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-vbd-policy");

    public static void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken)!;

        var assemblyName = context.Compilation.Assembly.Name;

        if (ProjectAnalyzer.GetLayer(assemblyName!) != Layer.Manager)
            return;

        var invocations = GetInvocationExpressions(methodDeclaration).ToList();
        if (invocations.Count == 0)
            return;

        foreach (var invocation in invocations)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var targetMethod = GetTargetMethodSymbol(context, invocation);
            if (targetMethod == null)
                continue;

            var targetType = targetMethod.ContainingType;

            if (SymbolEqualityComparer.Default.Equals(targetType, methodSymbol.ContainingType))
                continue;

            var targetAssembly = targetType.ContainingAssembly.Name;

            if (targetAssembly!.Equals(assemblyName, System.StringComparison.OrdinalIgnoreCase))
                continue;

            if (ProjectAnalyzer.GetLayer(targetAssembly!) != Layer.Manager)
                continue;

            var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodSymbol.ContainingType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static IEnumerable<InvocationExpressionSyntax> GetInvocationExpressions(MethodDeclarationSyntax method)
    {
        if (method.Body != null)
        {
            foreach (var invocation in method.Body.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                yield return invocation;
            }
        }

        if (method.ExpressionBody?.Expression != null)
        {
            foreach (var invocation in method.ExpressionBody.Expression.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
            {
                yield return invocation;
            }
        }
    }

    private static IMethodSymbol? GetTargetMethodSymbol(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
    {
        return context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;
    }
}
