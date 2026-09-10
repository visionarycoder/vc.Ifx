using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility
{

    /// <summary>
    /// VBD700: Service project should not depend on other Service projects
    /// Volatility-Based Decomposition: Services should depend on stable contracts, not volatile implementations
    /// </summary>
    public static class Vbd700ServiceCannotDependOnOtherService
    {
        public static readonly DiagnosticDescriptor Rule = new(id: DiagnosticIds.Vbd700ServiceCannotDependOnOtherService, title: "Service project should not depend on other Service projects", messageFormat: "Service project '{0}' should not depend on Service project '{1}'. Services should depend on Contracts, not other Services.", category: "Architecture.Volatility", defaultSeverity: DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "Service projects should depend on Contract interfaces, not on other Service implementations. This improves testability and reduces coupling.", helpLinkUri: "Docs/Volatility/vbd700.md", customTags: WellKnownDiagnosticTags.CompilationEnd);

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

            if (ProjectAnalyzer.GetProjectType(assemblyName!) != ProjectType.Service)
                return;

            var violation = FindServiceDependency(namedType, assemblyName!);
            if (violation == null)
                return;

            var (member, referencedAssembly) = violation.Value;
            var location = member.Locations.FirstOrDefault() ?? namedType.Locations.FirstOrDefault() ?? Location.None;
            var diagnostic = Diagnostic.Create(Rule, location, assemblyName!, referencedAssembly);
            context.ReportDiagnostic(diagnostic);
        }

        private static (ISymbol Source, string TargetAssembly)? FindServiceDependency(INamedTypeSymbol type, string currentAssembly)
        {
            foreach (var (source, referencedType) in EnumerateDependencies(type))
            {
                var targetAssembly = referencedType.ContainingAssembly?.Name;
                if (string.IsNullOrWhiteSpace(targetAssembly))
                    continue;

                if (targetAssembly!.Equals(currentAssembly, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ProjectAnalyzer.GetProjectType(targetAssembly) == ProjectType.Service)
                    return (source, targetAssembly);
            }

            return null;
        }

        private static IEnumerable<(ISymbol Source, ITypeSymbol Type)> EnumerateDependencies(INamedTypeSymbol type)
        {
            var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var stack = new Stack<(ISymbol Source, ITypeSymbol Type)>();

            void Push(ISymbol source, ITypeSymbol typeSymbol)
            {
                if (typeSymbol != null)
                    stack.Push((source, typeSymbol));
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
                    case INamedTypeSymbol { IsGenericType: true } namedType:
                        foreach (var argument in namedType.TypeArguments)
                            Push(current.Source, argument);
                        break;
                }
            }
        }
    }
}
