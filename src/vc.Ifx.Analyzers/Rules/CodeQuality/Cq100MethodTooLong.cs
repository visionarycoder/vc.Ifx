using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.CodeQuality;

public static class Cq100MethodTooLong
{

    private const int MaxStatements = 80;

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticIds.Cq100MethodTooLong,
        title: "Method is too long",
        messageFormat: "Method '{0}' contains {1} statements (max allowed: {2})",
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
        var statements = CodeQualityMetrics.CountStatements(method);
        if (statements <= MaxStatements)
            return;

        var diagnostic = Diagnostic.Create(Rule, method.Identifier.GetLocation(), method.Identifier.Text, statements, MaxStatements);
        context.ReportDiagnostic(diagnostic);
    }
}
