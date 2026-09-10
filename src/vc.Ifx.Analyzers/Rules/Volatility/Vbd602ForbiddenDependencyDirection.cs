using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility
{

    public static class Vbd602ForbiddenDependencyDirection
    {
        public static readonly DiagnosticDescriptor Rule = new(DiagnosticIds.Vbd602ForbiddenDependencyDirection, "Forbidden dependency direction", "Type '{0}' depends on forbidden type '{1}'", "Architecture", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: "Docs/Volatility/vbd602.md");

        public static void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeDependencies, SymbolKind.NamedType);
        }

        private static void AnalyzeDependencies(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol namedType)
                return;

            var assemblyName = namedType.ContainingAssembly?.Name;
            if (string.IsNullOrWhiteSpace(assemblyName))
                return;

            var currentLayer = ProjectAnalyzer.GetLayer(assemblyName!);

            // Only enforce for recognized layered projects
            if (currentLayer is Layer.Unknown or Layer.Infrastructure)
                return;

            var reported = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            void InspectDependency(ITypeSymbol? dependencyType, ISymbol source)
            {
                if (dependencyType == null)
                {
                    return;
                }

                foreach (var candidate in EnumerateReferencedTypes(dependencyType))
                {
                    if (!reported.Add(candidate))
                    {
                        continue;
                    }

                    var targetAssembly = candidate.ContainingAssembly?.Name;
                    if (string.IsNullOrWhiteSpace(targetAssembly))
                    {
                        continue;
                    }

                    var targetLayer = ProjectAnalyzer.GetLayer(targetAssembly!);
                    if (targetLayer == Layer.Unknown)
                    {
                        continue;
                    }

                    if (ProjectAnalyzer.IsLayerDependencyAllowed(currentLayer, targetLayer))
                    {
                        continue;
                    }

                    var location = source.Locations.FirstOrDefault() ?? namedType.Locations.FirstOrDefault() ?? Location.None;
                    var diagnostic = Diagnostic.Create(Rule, location, namedType.Name, candidate.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // Base type & interfaces
            if (namedType.BaseType != null && namedType.BaseType.SpecialType != SpecialType.System_Object)
            {
                InspectDependency(namedType.BaseType, namedType);
            }

            foreach (var iface in namedType.Interfaces)
            {
                InspectDependency(iface, namedType);
            }

            foreach (var member in namedType.GetMembers())
            {
                switch (member)
                {
                    case IFieldSymbol fieldSymbol:
                        InspectDependency(fieldSymbol.Type, fieldSymbol);
                        break;
                    case IPropertySymbol propertySymbol:
                        InspectDependency(propertySymbol.Type, propertySymbol);
                        foreach (var parameter in propertySymbol.Parameters)
                        {
                            InspectDependency(parameter.Type, propertySymbol);
                        }

                        break;
                    case IMethodSymbol methodSymbol:
                        InspectDependency(methodSymbol.ReturnType, methodSymbol);
                        foreach (var parameter in methodSymbol.Parameters)
                        {
                            InspectDependency(parameter.Type, methodSymbol);
                        }

                        foreach (var typeParameter in methodSymbol.TypeParameters)
                        {
                            foreach (var constraint in typeParameter.ConstraintTypes)
                            {
                                InspectDependency(constraint, methodSymbol);
                            }
                        }

                        break;
                    case IEventSymbol eventSymbol:
                        InspectDependency(eventSymbol.Type, eventSymbol);
                        break;
                }
            }
        }

        private static IEnumerable<ITypeSymbol> EnumerateReferencedTypes(ITypeSymbol typeSymbol)
        {
            var stack = new Stack<ITypeSymbol>();
            var seen = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            stack.Push(typeSymbol);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!seen.Add(current))
                {
                    continue;
                }

                yield return current;

                switch (current)
                {
                    case IArrayTypeSymbol arrayTypeSymbol:
                        stack.Push(arrayTypeSymbol.ElementType);
                        break;
                    case IPointerTypeSymbol pointerTypeSymbol:
                        stack.Push(pointerTypeSymbol.PointedAtType);
                        break;
                    case INamedTypeSymbol namedTypeSymbol:
                        foreach (var argument in namedTypeSymbol.TypeArguments)
                        {
                            stack.Push(argument);
                        }

                        break;
                }
            }
        }
    }
}
