using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.CodeQuality;

public static class Cq101TooManyParameters
{
    private const int MaxParameters = 5;

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticIds.Cq101TooManyParameters,
        title: "Method has too many parameters",
        messageFormat: "Method '{0}' has {1} parameters (max allowed: {2})",
        category: "CodeQuality",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-metrics-policy");

    public static void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.MethodDeclaration);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var method = (MethodDeclarationSyntax)context.Node;

        var count = CodeQualityMetrics.CountParameters(method);
        if (count <= MaxParameters)
            return;

        var diagnostic = Diagnostic.Create(Rule, method.Identifier.GetLocation(),
            method.Identifier.Text, count, MaxParameters);

        context.ReportDiagnostic(diagnostic);
    }
}
