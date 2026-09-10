using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility
{

    /// <summary>
    /// VBD400: Access projects must not depend on other Access projects.
    /// This prevents access-layer implementations from taking dependencies on other access implementations.
    /// </summary>
    public static class Vbd400NoAccessToAccessCalls
    {
        public static readonly DiagnosticDescriptor Rule = new(DiagnosticIds.Vbd400NoAccessToAccessCalls, "Access projects cannot depend on other Access projects", "Access project '{0}' depends on Access project '{1}'", "Architecture", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: "Docs/Volatility/vbd400.md");

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

            if (ProjectAnalyzer.GetLayer(assemblyName!) != Layer.Access)
                return;

            var violation = FindAccessDependency(namedType, assemblyName!);
            if (violation == null)
                return;

            var (member, referencedAssembly) = violation.Value;
            var location = member.Locations.FirstOrDefault() ?? namedType.Locations.FirstOrDefault() ?? Location.None;
            var diagnostic = Diagnostic.Create(Rule, location, assemblyName!, referencedAssembly);
            context.ReportDiagnostic(diagnostic);
        }

        private static (ISymbol Source, string TargetAssembly)? FindAccessDependency(INamedTypeSymbol type, string currentAssembly)
        {
            foreach (var (source, referencedType) in EnumerateDependencies(type))
            {
                var targetAssembly = referencedType.ContainingAssembly?.Name;
                if (string.IsNullOrWhiteSpace(targetAssembly))
                    continue;

                if (targetAssembly!.Equals(currentAssembly, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ProjectAnalyzer.GetLayer(targetAssembly) == Layer.Access && ProjectAnalyzer.GetProjectType(targetAssembly) == ProjectType.Service)
                    return (source, targetAssembly);
            }

            return null;
        }

        private static IEnumerable<(ISymbol Source, ITypeSymbol Type)> EnumerateDependencies(INamedTypeSymbol type)
        {
            var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var stack = new Stack<(ISymbol Source, ITypeSymbol Type)>();

            void Push(ISymbol source, ITypeSymbol? typeSymbol)
            {
                if (typeSymbol != null)
                    stack.Push((source, typeSymbol));
            }

            if (type.BaseType != null && type.BaseType.SpecialType != SpecialType.System_Object)
                Push(type, type.BaseType);

            foreach (var namedInterfaces in type.Interfaces)
                Push(type, namedInterfaces);

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
                        foreach (var constraint in methodSymbol.TypeParameters.SelectMany(typeParameter => typeParameter.ConstraintTypes))
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
                    case INamedTypeSymbol { IsGenericType: true } namedType:
                        foreach (var argument in namedType.TypeArguments)
                            Push(current.Source, argument);
                        break;
                }
            }
        }
    }
}
