using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Framework;

public static class Ifx006SingleClassPerFile
{
    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticIds.Ifx006SingleClassPerFile,
        title: "Multiple top-level classes in single file",
        messageFormat: "File contains {0} top-level classes (only 1 allowed, nested classes are exempt)",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Each file should contain at most one top-level class to improve maintainability and discoverability. Nested classes are permitted.",
        helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-ifx-inventory");

    public static void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxTreeAction(Analyze);
    }

    private static void Analyze(SyntaxTreeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var root = (CompilationUnitSyntax)context.Tree.GetRoot(context.CancellationToken);

        var topLevelClasses = root.DescendantNodes(node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax)
            .OfType<ClassDeclarationSyntax>().ToList();

        if (topLevelClasses.Count <= 1)
        {
            return;
        }

        foreach (var classDecl in topLevelClasses.Skip(1))
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var diagnostic = Diagnostic.Create(Rule, classDecl.Identifier.GetLocation(), topLevelClasses.Count);
            context.ReportDiagnostic(diagnostic);
        }
    }

}
