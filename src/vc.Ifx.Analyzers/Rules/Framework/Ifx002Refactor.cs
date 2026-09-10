using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Framework;

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
        helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-ifx-inventory");

    public static void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method, SymbolKind.Property, SymbolKind.Field, SymbolKind.Event, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var symbol = context.Symbol;
        var refactorAttribute = context.Compilation.GetTypeByMetadataName("Wsdot.Idl.Ifx.Attributes.RefactorAttribute");
        if (refactorAttribute is null)
            return;

        var attribute = symbol
            .GetAttributes()
            .FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, refactorAttribute));

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

        var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name, submitter, reason);
        context.ReportDiagnostic(diagnostic);
    }
}
