using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Framework
{

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
            helpLinkUri: "Docs/Framework/ifx006.md");

        public static void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxTreeAction(Analyze);
        }

        private static void Analyze(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetRoot(context.CancellationToken) as CompilationUnitSyntax;
            if (root is null)
            {
                return;
            }

            var topLevelClasses = GetTopLevelClasses(root);

            if (topLevelClasses.Count <= 1)
            {
                return;
            }

            foreach (var classDecl in topLevelClasses.Skip(1))
            {
                var diagnostic = Diagnostic.Create(Rule, classDecl.Identifier.GetLocation(), topLevelClasses.Count);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static List<ClassDeclarationSyntax> GetTopLevelClasses(CompilationUnitSyntax root)
        {
            var classes = new List<ClassDeclarationSyntax>();

            foreach (var member in root.Members)
            {
                if (member is ClassDeclarationSyntax classDecl)
                {
                    classes.Add(classDecl);
                }
                else if (member is NamespaceDeclarationSyntax namespaceDecl)
                {
                    classes.AddRange(GetClassesInNamespace(namespaceDecl));
                }
                else if (member is FileScopedNamespaceDeclarationSyntax fileScopedNamespace)
                {
                    classes.AddRange(GetClassesInFileScopedNamespace(fileScopedNamespace));
                }
            }

            return classes;
        }

        private static List<ClassDeclarationSyntax> GetClassesInNamespace(NamespaceDeclarationSyntax namespaceDecl)
        {
            var classes = new List<ClassDeclarationSyntax>();

            foreach (var member in namespaceDecl.Members)
            {
                if (member is ClassDeclarationSyntax classDecl)
                {
                    classes.Add(classDecl);
                }
            }

            return classes;
        }

        private static List<ClassDeclarationSyntax> GetClassesInFileScopedNamespace(FileScopedNamespaceDeclarationSyntax fileScopedNamespace)
        {
            var classes = new List<ClassDeclarationSyntax>();

            foreach (var member in fileScopedNamespace.Members)
            {
                if (member is ClassDeclarationSyntax classDecl)
                {
                    classes.Add(classDecl);
                }
            }

            return classes;
        }
    }
}
