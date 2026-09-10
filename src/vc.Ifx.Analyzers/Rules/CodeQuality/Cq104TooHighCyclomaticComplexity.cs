using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.CodeQuality
{

    public static class Cq104TooHighCyclomaticComplexity
    {
        private const int MaxComplexity = 10;

        public static readonly DiagnosticDescriptor Rule = new(
            id: DiagnosticIds.Cq104TooComplex,
            title: "Method cyclomatic complexity is too high",
            messageFormat: "Method '{0}' has cyclomatic complexity {1} (max allowed: {2})",
            category: "CodeQuality",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: "Docs/CodeQuality/cq104.md");

        public static void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.MethodDeclaration);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;
            var complexity = CodeQualityMetrics.ComputeCyclomaticComplexity(method);
            if (complexity <= MaxComplexity)
                return;
            var diagnostic = Diagnostic.Create(Rule, method.Identifier.GetLocation(), method.Identifier.Text, complexity, MaxComplexity);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
