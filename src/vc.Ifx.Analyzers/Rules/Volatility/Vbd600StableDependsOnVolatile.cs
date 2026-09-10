using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility
{

    /// <summary>
    /// VBD600: Stable component cannot depend on more volatile component
    /// Volatility-Based Decomposition: Core principle - dependencies flow from volatile to stable
    /// </summary>
    public static class Vbd600StableDependsOnVolatile
    {
        public static readonly DiagnosticDescriptor Rule = new(id: DiagnosticIds.Vbd600StableDependsOnVolatile, title: "Stable component cannot depend on more volatile component", messageFormat: "{0} project '{1}' cannot depend on {2} project '{3}'. Dependencies must flow from volatile to stable components.", category: "Architecture.Volatility", defaultSeverity: DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "Dependencies should flow from volatile (frequently changing) to stable (rarely changing) components. This is the fundamental principle of Volatility-Based Decomposition.", helpLinkUri: "Docs/Volatility/vbd600.md");

        public static void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol namedType)
                return;

            var assemblyName = namedType.ContainingAssembly?.Name;
            if (string.IsNullOrWhiteSpace(assemblyName))
                return;

            var currentProjectType = ProjectAnalyzer.GetProjectType(assemblyName!);
            if (currentProjectType is ProjectType.Unknown or ProjectType.Test)
                return;

            foreach (var (source, referencedType) in EnumerateDependencies(namedType))
            {
                var referencedAssembly = referencedType.ContainingAssembly?.Name;
                if (string.IsNullOrWhiteSpace(referencedAssembly))
                    continue;

                var referencedProjectType = ProjectAnalyzer.GetProjectType(referencedAssembly!);
                if (referencedProjectType == ProjectType.Unknown)
                    continue;

                if (ProjectAnalyzer.IsDependencyAllowed(currentProjectType, referencedProjectType))
                    continue;

                var location = source.Locations.FirstOrDefault() ?? namedType.Locations.FirstOrDefault() ?? Location.None;
                var diagnostic = Diagnostic.Create(Rule, location, ProjectAnalyzer.GetProjectTypeName(currentProjectType),
                    assemblyName!,
                    ProjectAnalyzer.GetProjectTypeName(referencedProjectType),
                    referencedAssembly!);
                context.ReportDiagnostic(diagnostic);
                return;
            }
        }

        private static IEnumerable<(ISymbol Source, ITypeSymbol Type)> EnumerateDependencies(INamedTypeSymbol type)
        {
            var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var stack = new Stack<(ISymbol Source, ITypeSymbol Type)>();

            void Push(ISymbol source, ITypeSymbol? dependency)
            {
                if (dependency != null)
                    stack.Push((source, dependency));
            }

            if (type.BaseType != null && type.BaseType.SpecialType != SpecialType.System_Object)
                Push(type, type.BaseType);

            foreach (var iface in type.Interfaces)
                Push(type, iface);

            foreach (var member in type.GetMembers())
            {
                switch (member)
                {
                    case IFieldSymbol fieldSymbol:
                        Push(fieldSymbol, fieldSymbol.Type);
                        break;
                    case IPropertySymbol propertySymbol:
                        Push(propertySymbol, propertySymbol.Type);
                        foreach (var parameter in propertySymbol.Parameters)
                            Push(propertySymbol, parameter.Type);
                        break;
                    case IMethodSymbol methodSymbol:
                        Push(methodSymbol, methodSymbol.ReturnType);
                        foreach (var parameter in methodSymbol.Parameters)
                            Push(methodSymbol, parameter.Type);
                        foreach (var typeParameter in methodSymbol.TypeParameters)
                            foreach (var constraint in typeParameter.ConstraintTypes)
                                Push(methodSymbol, constraint);
                        break;
                    case IEventSymbol eventSymbol:
                        Push(eventSymbol, eventSymbol.Type);
                        break;
                }
            }

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!visited.Add(current.Type))
                    continue;

                yield return current;

                switch (current.Type)
                {
                    case IArrayTypeSymbol arrayType:
                        Push(current.Source, arrayType.ElementType);
                        break;
                    case IPointerTypeSymbol pointerType:
                        Push(current.Source, pointerType.PointedAtType);
                        break;
                    case INamedTypeSymbol { IsGenericType: true } namedTypeSymbol:
                        foreach (var argument in namedTypeSymbol.TypeArguments)
                            Push(current.Source, argument);
                        break;
                }
            }
        }
    }
}
