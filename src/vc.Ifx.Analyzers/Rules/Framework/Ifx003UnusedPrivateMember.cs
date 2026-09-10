using System.Collections.Concurrent;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Framework
{

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
            helpLinkUri: "Docs/Framework/ifx003.md",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public static void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                var declaredSymbols = new ConcurrentDictionary<ISymbol, Location>(SymbolEqualityComparer.Default);
                var usedSymbols = new ConcurrentDictionary<ISymbol, byte>(SymbolEqualityComparer.Default);

                startContext.RegisterSymbolAction(symbolContext =>
                {
                    if (!IsCandidate(symbolContext.Symbol))
                    {
                        return;
                    }

                    var location = symbolContext.Symbol.Locations.FirstOrDefault(l => l.IsInSource);
                    if (location is null)
                    {
                        return;
                    }

                    declaredSymbols.TryAdd(symbolContext.Symbol, location);
                }, SymbolKind.Field, SymbolKind.Property, SymbolKind.Method);

                startContext.RegisterOperationAction(operationContext =>
                {
                    var fieldReference = (IFieldReferenceOperation)operationContext.Operation;
                    MarkUsed(fieldReference.Field, usedSymbols);
                }, OperationKind.FieldReference);

                startContext.RegisterOperationAction(operationContext =>
                {
                    var propertyReference = (IPropertyReferenceOperation)operationContext.Operation;
                    MarkUsed(propertyReference.Property, usedSymbols);
                }, OperationKind.PropertyReference);

                startContext.RegisterOperationAction(operationContext =>
                {
                    var invocation = (IInvocationOperation)operationContext.Operation;
                    MarkUsed(invocation.TargetMethod, usedSymbols);
                }, OperationKind.Invocation);

                startContext.RegisterOperationAction(operationContext =>
                {
                    var methodReference = (IMethodReferenceOperation)operationContext.Operation;
                    MarkUsed(methodReference.Method, usedSymbols);
                }, OperationKind.MethodReference);

                startContext.RegisterCompilationEndAction(endContext =>
                {
                    foreach (var declaredSymbol in declaredSymbols)
                    {
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

            usedSymbols.TryAdd(symbol, 0);
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
                IMethodSymbol methodSymbol => methodSymbol is { MethodKind: MethodKind.Ordinary, IsOverride: false, IsExtern: false },
                _ => false
            };
        }
    }
}
