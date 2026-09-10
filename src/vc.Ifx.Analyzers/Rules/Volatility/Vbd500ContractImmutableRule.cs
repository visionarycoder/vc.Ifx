using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility;

public static class Vbd500ContractImmutableRule
{
    public static readonly DiagnosticDescriptor Rule = new(DiagnosticIds.Vbd500ContractImmutable, "Contracts must be immutable", "Contract '{0}' contains mutable members", "Architecture", DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-vbd-policy");

    public static void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeContract, SymbolKind.NamedType);
    }

    private static void AnalyzeContract(SymbolAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var namedType = (INamedTypeSymbol)context.Symbol;

        // Only enforce on contract projects
        var assemblyName = context.Compilation.Assembly.Name;

        if (ProjectAnalyzer.GetProjectType(assemblyName!) != ProjectType.Contract)
            return;

        // Skip interfaces, enums, delegates, etc.
        if (namedType.TypeKind is TypeKind.Interface or TypeKind.Enum or TypeKind.Delegate)
            return;

        var mutableMember = FindFirstMutableMember(namedType, context.CancellationToken);
        if (mutableMember == null)
            return;

        var location = mutableMember.Locations[0];

        var diagnostic = Diagnostic.Create(Rule, location, namedType.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static ISymbol? FindFirstMutableMember(INamedTypeSymbol namedType, CancellationToken cancellationToken)
    {
        foreach (var member in namedType.GetMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();
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

        if (field.IsReadOnly)
            return false;

        return true;
    }
}
