using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility
{

    /// <summary>
    /// VBD102: Infrastructure project cannot depend on domain projects
    /// Volatility-Based Decomposition: Infrastructure must remain stable and domain-agnostic
    /// </summary>
    public static class Vbd102InfrastructureCannotDependOnDomain
    {
        public static readonly DiagnosticDescriptor Rule = new(
            id: DiagnosticIds.Vbd102InfrastructureCannotDependOnDomain,
            title: "Infrastructure project cannot depend on domain projects",
            messageFormat: "Infrastructure project '{0}' cannot depend on domain project '{1}'. Infrastructure must remain stable and domain-agnostic.",
            category: "Architecture.Volatility",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Infrastructure projects (Ifx.*) are very stable and should not depend on volatile domain projects. This ensures infrastructure remains reusable across domains.",
            helpLinkUri: "Docs/Volatility/vbd102.md");

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

            if (ProjectAnalyzer.GetProjectType(assemblyName!) != ProjectType.Infrastructure)
                return;

            var violation = FindDomainDependency(namedType);
            if (violation is null)
                return;

            var (source, target) = violation.Value;
            var location = source.Locations.FirstOrDefault() ?? namedType.Locations.FirstOrDefault() ?? Location.None;
            var diagnostic = Diagnostic.Create(Rule, location, assemblyName!, target.ContainingAssembly?.Name ?? target.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            context.ReportDiagnostic(diagnostic);
        }

        private static (ISymbol Source, ITypeSymbol Target)? FindDomainDependency(INamedTypeSymbol type)
        {
            var inspectedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            bool IsDomain(ITypeSymbol symbol)
            {
                var targetAssembly = symbol.ContainingAssembly?.Name;
                if (string.IsNullOrWhiteSpace(targetAssembly))
                    return false;

                var projectType = ProjectAnalyzer.GetProjectType(targetAssembly!);
                return projectType is ProjectType.Service or ProjectType.Contract or ProjectType.Orm;
            }

            foreach (var dependency in EnumerateMemberDependencies(type))
            {
                foreach (var referenced in EnumerateReferencedTypes(dependency.Type))
                {
                    if (!inspectedTypes.Add(referenced))
                        continue;

                    if (IsDomain(referenced))
                        return (dependency.Source, referenced);
                }
            }

            return null;
        }

        private static IEnumerable<(ISymbol Source, ITypeSymbol Type)> EnumerateMemberDependencies(INamedTypeSymbol type)
        {
            if (type.BaseType != null && type.BaseType.SpecialType != SpecialType.System_Object)
                yield return (type, type.BaseType);

            foreach (var iface in type.Interfaces)
                yield return (type, iface);

            foreach (var member in type.GetMembers())
            {
                switch (member)
                {
                    case IFieldSymbol fieldSymbol:
                        yield return (fieldSymbol, fieldSymbol.Type);
                        break;
                    case IPropertySymbol propertySymbol:
                        yield return (propertySymbol, propertySymbol.Type);
                        foreach (var parameter in propertySymbol.Parameters)
                            yield return (propertySymbol, parameter.Type);
                        break;
                    case IMethodSymbol methodSymbol:
                        yield return (methodSymbol, methodSymbol.ReturnType);
                        foreach (var parameter in methodSymbol.Parameters)
                            yield return (methodSymbol, parameter.Type);
                        foreach (var typeParameter in methodSymbol.TypeParameters)
                            foreach (var constraint in typeParameter.ConstraintTypes)
                                yield return (methodSymbol, constraint);
                        break;
                    case IEventSymbol eventSymbol:
                        yield return (eventSymbol, eventSymbol.Type);
                        break;
                }
            }
        }

        private static IEnumerable<ITypeSymbol> EnumerateReferencedTypes(ITypeSymbol type)
        {
            var stack = new Stack<ITypeSymbol>();
            var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            stack.Push(type);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!visited.Add(current))
                    continue;

                yield return current;

                switch (current)
                {
                    case IArrayTypeSymbol arrayType:
                        stack.Push(arrayType.ElementType);
                        break;
                    case IPointerTypeSymbol pointerType:
                        stack.Push(pointerType.PointedAtType);
                        break;
                    case INamedTypeSymbol { IsGenericType: true } namedType:
                        foreach (var argument in namedType.TypeArguments)
                            stack.Push(argument);
                        break;
                }
            }
        }
    }
}
