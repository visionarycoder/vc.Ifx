using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Framework
{

    internal class Ifx002Refactor
    {
        public static readonly DiagnosticDescriptor Rule = new(
            id: DiagnosticIds.Ifx002RefactorRequired,
            title: "Refactoring Required",
            messageFormat: "{0} - Submitted by {1}: {2}",
            category: "Refactoring",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "This symbol is marked for refactoring. Apply the requested refactor and remove the RefactorAttribute when complete.",
            helpLinkUri: "Docs/Framework/ifx002.md");

        public static void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method, SymbolKind.Property, SymbolKind.Field, SymbolKind.Event, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var symbol = context.Symbol;

            var attribute = symbol
                .GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == "Wsdot.Idl.Ifx.Attributes.RefactorAttribute");

            if (attribute == null)
            {
                return;
            }

            var reason = attribute.ConstructorArguments.Length > 0
                ? attribute.ConstructorArguments[0].Value?.ToString() ?? "No reason provided"
                : "No reason provided";

            var submitter = attribute.ConstructorArguments.Length > 1
                ? attribute.ConstructorArguments[1].Value?.ToString() ?? "Unknown"
                : "Unknown";

            //var actionType = "Refactoring needed";
            if (attribute.ConstructorArguments.Length > 2 && attribute.ConstructorArguments[2].Value is int typeValue)
            {
                if (attribute.ConstructorArguments[2].Type is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType)
                {
                    var enumMember = enumType.GetMembers()
                        .OfType<IFieldSymbol>()
                        .FirstOrDefault(m => m.HasConstantValue && (int)m.ConstantValue == typeValue);
                }
            }
            // Report diagnostic
            var diagnostic = Diagnostic.Create(Rule, symbol.Locations.FirstOrDefault() ?? Location.None, symbol.Name, submitter, reason);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
