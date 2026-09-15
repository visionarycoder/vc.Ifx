using System.Collections.Immutable;
using Ifx.Analyzers.Abstractions;
using Ifx.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ifx.Analyzers.Rules;

/// <summary>Flags private fields, parameters, and locals that use a leading underscore prefix.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Ifx1400NoUnderscorePrefixedIdentifier : DiagnosticAnalyzer
{
    /// <summary>Descriptor for the underscore-prefixed identifier naming policy.</summary>
    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticIds.Ifx1400NoUnderscorePrefixedIdentifier,
        title: "Identifier should not use a leading underscore prefix",
        messageFormat: "'{0}' should not use a leading underscore; rename without the underscore prefix",
        category: DiagnosticCategories.Naming,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Private fields, parameters, and local variables should not use underscore-prefixed identifiers.",
        helpLinkUri: "https://github.com/visionarycoder/Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#ifx1400");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
        context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclarator, SyntaxKind.VariableDeclarator);
    }

    private static void AnalyzeField(SymbolAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var field = (IFieldSymbol)context.Symbol;
        if (field.IsImplicitlyDeclared || field.DeclaredAccessibility != Accessibility.Private || !HasUnderscorePrefix(field.Name))
        {
            return;
        }

        var location = field.Locations[0];
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, field.Name));
    }

    private static void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var parameter = (ParameterSyntax)context.Node;
        if (!HasUnderscorePrefix(parameter.Identifier.ValueText))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Identifier.GetLocation(), parameter.Identifier.ValueText));
    }

    private static void AnalyzeVariableDeclarator(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var variable = (VariableDeclaratorSyntax)context.Node;
        if (!IsLocalVariable(variable) || !HasUnderscorePrefix(variable.Identifier.ValueText))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Identifier.GetLocation(), variable.Identifier.ValueText));
    }

    private static bool HasUnderscorePrefix(string name) => name.Length > 1 && name[0] == '_';

    private static bool IsLocalVariable(VariableDeclaratorSyntax variable)
    {
        if (variable.Parent is not VariableDeclarationSyntax declaration)
        {
            return false;
        }

        return declaration.Parent is LocalDeclarationStatementSyntax or ForStatementSyntax or UsingStatementSyntax or FixedStatementSyntax;
    }
}
