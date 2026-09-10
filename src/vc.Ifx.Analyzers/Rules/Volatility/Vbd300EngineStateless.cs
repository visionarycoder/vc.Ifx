using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility
{

    public static class Vbd300EngineStateless
    {
        public static readonly DiagnosticDescriptor Rule = new(DiagnosticIds.Vbd300EngineStateless, "Engines must be stateless", "Engine '{0}' contains state", "Architecture", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: "Docs/Volatility/vbd300.md");

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

            if (namedType.TypeKind is not (TypeKind.Class or TypeKind.Struct))
                return;

            var stateMember = FindStateMember(namedType);
            if (stateMember == null)
                return;

            var location = stateMember.Locations.FirstOrDefault()
                           ?? namedType.Locations.FirstOrDefault()
                           ?? Location.None;

            var diagnostic = Diagnostic.Create(Rule, location, namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private static ISymbol? FindStateMember(INamedTypeSymbol type)
        {
            foreach (var member in type.GetMembers())
            {
                if (member is IFieldSymbol field && IsMutableInstanceField(field))
                {
                    return field;
                }

                if (member is IPropertySymbol property && IsMutableProperty(property))
                {
                    return property;
                }
            }

            return null;
        }

        // VBD300 targets genuinely mutable engine state: a non-static, non-const, non-readonly
        // instance field that can change across method calls. Excludes:
        //  - readonly fields � injected dependencies / set-once values are not mutable state;
        //  - compiler-generated backing fields (auto-properties, primary-constructor captures) �
        //    the property itself is reported via IsMutableProperty, so flagging its backing field
        //    too would double-report the same member.
        private static bool IsMutableInstanceField(IFieldSymbol field)
        {
            if (field.IsStatic || field.IsConst || field.IsReadOnly)
                return false;

            if (field.IsImplicitlyDeclared || field.AssociatedSymbol is IPropertySymbol)
                return false;

            return true;
        }

        // A settable, non-init, non-static instance property is mutable engine state. This deliberately
        // mirrors VBD500's contract-immutability check: a `{ get; set; }` member in an engine is both a
        // contract concern (VBD500) and mutable state (VBD300), and is intentionally flagged under both.
        private static bool IsMutableProperty(IPropertySymbol property)
        {
            if (property.IsStatic)
                return false;

            var setter = property.SetMethod;
            if (setter == null || setter.IsInitOnly)
                return false;

            return true;
        }
    }
}
