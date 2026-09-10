using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility
{

    /// <summary>
    /// VBD501: Contract project cannot depend on Service project
    /// Volatility-Based Decomposition: Stable contracts should not depend on volatile implementations
    /// </summary>
    public static class Vbd501ContractCannotDependOnService
    {
        public static readonly DiagnosticDescriptor Rule = new(id: DiagnosticIds.Vbd501ContractCannotDependOnService, title: "Contract project cannot depend on Service project", messageFormat: "Contract project '{0}' cannot depend on Service project '{1}'. Contracts are stable and should not depend on volatile implementations.", category: "Architecture.Volatility", defaultSeverity: DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Contract projects define stable interfaces and should not depend on volatile service implementations. This violates the Stable Dependencies Principle.", helpLinkUri: "Docs/Volatility/vbd501.md");

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

            if (ProjectAnalyzer.GetProjectType(assemblyName!) != ProjectType.Contract)
                return;

            var violation = FindServiceDependency(namedType, assemblyName!);
            if (violation == null)
                return;

            var (source, referencedAssembly) = violation.Value;
            var location = source.Locations.FirstOrDefault()
                           ?? namedType.Locations.FirstOrDefault()
                           ?? Location.None;

            var diagnostic = Diagnostic.Create(Rule, location, assemblyName!, referencedAssembly);
            context.ReportDiagnostic(diagnostic);
        }

        private static (ISymbol Source, string TargetAssembly)? FindServiceDependency(INamedTypeSymbol type, string currentAssembly)
        {
            foreach (var (source, referencedType) in EnumerateDependencies(type))
            {
                var referencedAssembly = referencedType.ContainingAssembly?.Name;
                if (string.IsNullOrWhiteSpace(referencedAssembly))
                    continue;

                if (referencedAssembly!.Equals(currentAssembly, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ProjectAnalyzer.GetProjectType(referencedAssembly) == ProjectType.Service)
                    return (source, referencedAssembly);
            }

            return null;
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
