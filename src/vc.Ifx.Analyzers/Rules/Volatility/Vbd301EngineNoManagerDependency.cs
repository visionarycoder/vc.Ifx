using System;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility;

public static class Vbd301EngineNoManagerDependency
{

    public static readonly DiagnosticDescriptor Rule = new(DiagnosticIds.Vbd301EngineCannotDependOnManager, "Engines cannot depend upon managers", "Engine '{0}' contains reference to manager", "Architecture", DiagnosticSeverity.Error, isEnabledByDefault: true, helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-vbd-policy");

    public static void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var namedType = (INamedTypeSymbol)context.Symbol;

        var assemblyName = context.Compilation.Assembly.Name;

        if (ProjectAnalyzer.GetLayer(assemblyName!) != Layer.Engine)
            return;

        if (ReferencesManager(namedType.BaseType, context.CancellationToken))
        {
            Report(context, namedType, namedType.Locations[0]);
            return;
        }

        foreach (var iface in namedType.Interfaces)
        {
            if (!ReferencesManager(iface, context.CancellationToken))
                continue;

            Report(context, namedType, namedType.Locations[0]);
            return;
        }

        foreach (var member in namedType.GetMembers())
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            switch (member)
            {
                case IFieldSymbol field when ReferencesManager(field.Type, context.CancellationToken):
                    Report(context, namedType, field.Locations[0]);
                    return;
                case IPropertySymbol property when ReferencesManager(property.Type, context.CancellationToken):
                    Report(context, namedType, property.Locations[0]);
                    return;
            }
        }
    }

    private static void Report(SymbolAnalysisContext context, INamedTypeSymbol engineType, Location location)
    {
        var diagnostic = Diagnostic.Create(Rule, location, engineType.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool ReferencesManager(ITypeSymbol? typeSymbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (typeSymbol == null)
            return false;

        if (IsManagerType(typeSymbol))
            return true;

        switch (typeSymbol)
        {
            case IArrayTypeSymbol arrayType:
                return ReferencesManager(arrayType.ElementType, cancellationToken);
            case INamedTypeSymbol { IsGenericType: true } namedType:
                foreach (var typeArgument in namedType.TypeArguments)
                {
                    if (ReferencesManager(typeArgument, cancellationToken))
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
