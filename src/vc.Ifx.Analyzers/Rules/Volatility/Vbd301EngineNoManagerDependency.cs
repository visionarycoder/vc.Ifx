using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility
{

    public static class Vbd301EngineNoManagerDependency
    {

        public static readonly DiagnosticDescriptor Rule = new(DiagnosticIds.Vbd301EngineCannotDependOnManager, "Engines cannot depend upon managers", "Engine '{0}' contains reference to manager", "Architecture", DiagnosticSeverity.Error, isEnabledByDefault: true, helpLinkUri: "Docs/Volatility/vbd301.md");

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

            if (ProjectAnalyzer.GetLayer(assemblyName!) != Layer.Engine)
                return;

            if (ReferencesManager(namedType.BaseType))
            {
                Report(context, namedType, namedType.Locations.FirstOrDefault() ?? Location.None);
                return;
            }

            foreach (var iface in namedType.Interfaces)
            {
                if (!ReferencesManager(iface))
                    continue;

                Report(context, namedType, namedType.Locations.FirstOrDefault() ?? Location.None);
                return;
            }

            foreach (var member in namedType.GetMembers())
            {
                switch (member)
                {
                    case IFieldSymbol field when ReferencesManager(field.Type):
                        Report(context, namedType, field.Locations.FirstOrDefault() ?? Location.None);
                        return;
                    case IPropertySymbol property when ReferencesManager(property.Type):
                        Report(context, namedType, property.Locations.FirstOrDefault() ?? Location.None);
                        return;
                }
            }
        }

        private static void Report(SymbolAnalysisContext context, INamedTypeSymbol engineType, Location location)
        {
            var diagnostic = Diagnostic.Create(Rule, location, engineType.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private static bool ReferencesManager(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol == null)
                return false;

            if (IsManagerType(typeSymbol))
                return true;

            switch (typeSymbol)
            {
                case IArrayTypeSymbol arrayType:
                    return ReferencesManager(arrayType.ElementType);
                case INamedTypeSymbol { IsGenericType: true } namedType:
                    foreach (var typeArgument in namedType.TypeArguments)
                    {
                        if (ReferencesManager(typeArgument))
                            return true;
                    }

                    break;
            }

            return false;
        }

        private static bool IsManagerType(ITypeSymbol typeSymbol)
        {
            var containingAssembly = typeSymbol.ContainingAssembly?.Name;
            if (!string.IsNullOrWhiteSpace(containingAssembly) &&
                ProjectAnalyzer.GetLayer(containingAssembly!) == Layer.Manager)
            {
                return true;
            }
            var ns = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(ns) && ns.StartsWith("Manager.", StringComparison.Ordinal);
        }

    }
}
