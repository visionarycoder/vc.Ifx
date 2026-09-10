using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.CodeQuality
{

    public static class Cq103TooManyLocalVariables
    {
        private const int MaxVariables = 10;

        public static readonly DiagnosticDescriptor Rule = new(
            id: DiagnosticIds.Cq103TooManyLocals,
            title: "Method has too many local variables",
            messageFormat: "Method '{0}' declares {1} local variables (max allowed: {2})",
            category: "CodeQuality",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: "Docs/CodeQuality/cq103.md");

        public static void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.MethodDeclaration);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;

            var count = CodeQualityMetrics.CountLocalVariables(method);
            if (count <= MaxVariables)
                return;

            var diagnostic = Diagnostic.Create(Rule, method.Identifier.GetLocation(),
                method.Identifier.Text, count, MaxVariables);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
