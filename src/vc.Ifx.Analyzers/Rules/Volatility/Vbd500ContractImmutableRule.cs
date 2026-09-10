using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility
{

    public static class Vbd500ContractImmutableRule
    {
        public static readonly DiagnosticDescriptor Rule = new(DiagnosticIds.Vbd500ContractImmutable, "Contracts must be immutable", "Contract '{0}' contains mutable members", "Architecture", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: "Docs/Volatility/vbd500.md");

        public static void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeContract, SymbolKind.NamedType);
        }

        private static void AnalyzeContract(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol namedType)
                return;

            // Only enforce on contract projects
            var assemblyName = namedType.ContainingAssembly?.Name;
            if (string.IsNullOrWhiteSpace(assemblyName))
                return;

            if (ProjectAnalyzer.GetProjectType(assemblyName!) != ProjectType.Contract)
                return;

            // Skip interfaces, enums, delegates, etc.
            if (namedType.TypeKind is TypeKind.Interface or TypeKind.Enum or TypeKind.Delegate)
                return;

            var mutableMember = FindFirstMutableMember(namedType);
            if (mutableMember == null)
                return;

            var location = mutableMember.Locations.Length > 0
                ? mutableMember.Locations[0]
                : (namedType.Locations.Length > 0 ? namedType.Locations[0] : Location.None);

            var diagnostic = Diagnostic.Create(Rule, location, namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private static ISymbol? FindFirstMutableMember(INamedTypeSymbol namedType)
        {
            foreach (var member in namedType.GetMembers())
            {
                if (member is IPropertySymbol property && IsMutableProperty(property))
                {
                    return property;
                }

                if (member is IFieldSymbol field && IsMutableField(field))
                {
                    return field;
                }
            }

            return null;
        }

        private static bool IsMutableProperty(IPropertySymbol property)
        {
            if (property.IsStatic)
                return false;

            var setter = property.SetMethod;
            if (setter == null)
                return false;

            // init-only setters are considered immutable
            if (setter.IsInitOnly)
                return false;

            return true;
        }

        private static bool IsMutableField(IFieldSymbol field)
        {
            if (field.IsStatic)
                return false;

            if (field.IsConst || field.IsReadOnly)
                return false;

            return true;
        }
    }
}
