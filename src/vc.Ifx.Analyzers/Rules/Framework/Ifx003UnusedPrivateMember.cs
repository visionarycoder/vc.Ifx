using System.Collections.Concurrent;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Framework;

public static class Ifx003UnusedPrivateMember
{
    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticIds.Ifx003UnusedPrivateMember,
        title: "Unused private member",
        messageFormat: "Private member '{0}' is never used",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Detects private fields, properties, and methods that are never referenced in the project. Remove the member or add a real usage.",
        helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-ifx-inventory",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public static void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(startContext =>
        {
            startContext.CancellationToken.ThrowIfCancellationRequested();
            var declaredSymbols = new ConcurrentDictionary<ISymbol, Location>(SymbolEqualityComparer.Default);
            var usedSymbols = new ConcurrentDictionary<ISymbol, byte>(SymbolEqualityComparer.Default);

            startContext.RegisterSymbolAction(symbolContext =>
            {
                symbolContext.CancellationToken.ThrowIfCancellationRequested();
                if (!IsCandidate(symbolContext.Symbol))
                {
                    return;
                }

                var location = symbolContext.Symbol.Locations[0];
                declaredSymbols.TryAdd(symbolContext.Symbol, location);
            }, SymbolKind.Field, SymbolKind.Property, SymbolKind.Method);

            startContext.RegisterOperationAction(operationContext =>
            {
                operationContext.CancellationToken.ThrowIfCancellationRequested();
                var fieldReference = (IFieldReferenceOperation)operationContext.Operation;
                MarkUsed(fieldReference.Field, usedSymbols);
            }, OperationKind.FieldReference);

            startContext.RegisterOperationAction(operationContext =>
            {
                operationContext.CancellationToken.ThrowIfCancellationRequested();
                var propertyReference = (IPropertyReferenceOperation)operationContext.Operation;
                MarkUsed(propertyReference.Property, usedSymbols);
            }, OperationKind.PropertyReference);

            startContext.RegisterOperationAction(operationContext =>
            {
                operationContext.CancellationToken.ThrowIfCancellationRequested();
                var invocation = (IInvocationOperation)operationContext.Operation;
                MarkUsed(invocation.TargetMethod, usedSymbols);
            }, OperationKind.Invocation);

            startContext.RegisterOperationAction(operationContext =>
            {
                operationContext.CancellationToken.ThrowIfCancellationRequested();
                var methodReference = (IMethodReferenceOperation)operationContext.Operation;
                MarkUsed(methodReference.Method, usedSymbols);
            }, OperationKind.MethodReference);

            startContext.RegisterCompilationEndAction(endContext =>
            {
                endContext.CancellationToken.ThrowIfCancellationRequested();
                foreach (var declaredSymbol in declaredSymbols.OrderBy(pair => pair.Key.ToDisplayString(), System.StringComparer.Ordinal))
                {
                    endContext.CancellationToken.ThrowIfCancellationRequested();
                    if (usedSymbols.ContainsKey(declaredSymbol.Key))
                    {
                        continue;
                    }

                    endContext.ReportDiagnostic(Diagnostic.Create(Rule, declaredSymbol.Value, declaredSymbol.Key.Name));
                }
            });
        });
    }

    private static void MarkUsed(ISymbol symbol, ConcurrentDictionary<ISymbol, byte> usedSymbols)
    {
        if (!IsCandidate(symbol))
        {
            return;
        }

        usedSymbols.TryAdd(symbol.OriginalDefinition, 0);
    }

    private static bool IsCandidate(ISymbol symbol)
    {
        if (symbol.IsImplicitlyDeclared || symbol.DeclaredAccessibility != Accessibility.Private)
        {
            return false;
        }

        return symbol switch
        {
            IFieldSymbol => true,
            IPropertySymbol propertySymbol => !propertySymbol.IsIndexer,
            _ => (IMethodSymbol)symbol is { MethodKind: MethodKind.Ordinary, IsOverride: false, IsExtern: false }
        };
    }
}
