using System.Collections.Concurrent;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Framework;

public static class Ifx004UnusedType
{
    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticIds.Ifx004UnusedType,
        title: "Unused type",
        messageFormat: "Type '{0}' is never used",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Detects internal/private classes, records, and structs that are never referenced. Remove unused types or reference them from production code.",
        helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-ifx-inventory",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public static void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(startContext =>
        {
            startContext.CancellationToken.ThrowIfCancellationRequested();
            var declaredTypes = new ConcurrentDictionary<INamedTypeSymbol, Location>(SymbolEqualityComparer.Default);
            var usedTypes = new ConcurrentDictionary<INamedTypeSymbol, byte>(SymbolEqualityComparer.Default);

            startContext.RegisterSymbolAction(symbolContext =>
            {
                symbolContext.CancellationToken.ThrowIfCancellationRequested();
                var typeSymbol = (INamedTypeSymbol)symbolContext.Symbol;
                if (!IsCandidate(typeSymbol))
                {
                    return;
                }

                var location = typeSymbol.Locations[0];
                declaredTypes.TryAdd(typeSymbol, location);
            }, SymbolKind.NamedType);

            startContext.RegisterOperationAction(operationContext =>
            {
                operationContext.CancellationToken.ThrowIfCancellationRequested();
                var objectCreation = (IObjectCreationOperation)operationContext.Operation;
                MarkUsed(objectCreation.Type as INamedTypeSymbol, usedTypes);
            }, OperationKind.ObjectCreation);

            startContext.RegisterOperationAction(operationContext =>
            {
                operationContext.CancellationToken.ThrowIfCancellationRequested();
                var invocation = (IInvocationOperation)operationContext.Operation;
                MarkUsed(invocation.TargetMethod.ContainingType, usedTypes);

                foreach (var typeArgument in invocation.TargetMethod.TypeArguments)
                {
                    MarkUsed(typeArgument as INamedTypeSymbol, usedTypes);
                }
            }, OperationKind.Invocation);

            startContext.RegisterOperationAction(operationContext =>
            {
                operationContext.CancellationToken.ThrowIfCancellationRequested();
                var fieldReference = (IFieldReferenceOperation)operationContext.Operation;
                MarkUsed(fieldReference.Field.Type as INamedTypeSymbol, usedTypes);
            }, OperationKind.FieldReference);

            startContext.RegisterOperationAction(operationContext =>
            {
                operationContext.CancellationToken.ThrowIfCancellationRequested();
                var propertyReference = (IPropertyReferenceOperation)operationContext.Operation;
                MarkUsed(propertyReference.Property.Type as INamedTypeSymbol, usedTypes);
            }, OperationKind.PropertyReference);

            startContext.RegisterOperationAction(operationContext =>
            {
                operationContext.CancellationToken.ThrowIfCancellationRequested();
                var conversion = (IConversionOperation)operationContext.Operation;
                MarkUsed(conversion.Type as INamedTypeSymbol, usedTypes);
            }, OperationKind.Conversion);

            startContext.RegisterOperationAction(operationContext =>
            {
                operationContext.CancellationToken.ThrowIfCancellationRequested();
                var typeOf = (ITypeOfOperation)operationContext.Operation;
                MarkUsed(typeOf.TypeOperand as INamedTypeSymbol, usedTypes);
            }, OperationKind.TypeOf);

            startContext.RegisterSymbolAction(symbolContext =>
            {
                symbolContext.CancellationToken.ThrowIfCancellationRequested();
                var namedType = (INamedTypeSymbol)symbolContext.Symbol;
                MarkUsed(namedType.BaseType, usedTypes);
                foreach (var @interface in namedType.Interfaces)
                {
                    MarkUsed(@interface, usedTypes);
                }
            }, SymbolKind.NamedType);

            startContext.RegisterSymbolAction(symbolContext =>
            {
                symbolContext.CancellationToken.ThrowIfCancellationRequested();
                var methodSymbol = (IMethodSymbol)symbolContext.Symbol;
                MarkUsed(methodSymbol.ReturnType as INamedTypeSymbol, usedTypes);
                foreach (var parameter in methodSymbol.Parameters)
                {
                    MarkUsed(parameter.Type as INamedTypeSymbol, usedTypes);
                }
            }, SymbolKind.Method);

            startContext.RegisterSymbolAction(symbolContext =>
            {
                symbolContext.CancellationToken.ThrowIfCancellationRequested();
                var propertySymbol = (IPropertySymbol)symbolContext.Symbol;
                MarkUsed(propertySymbol.Type as INamedTypeSymbol, usedTypes);
            }, SymbolKind.Property);

            startContext.RegisterSymbolAction(symbolContext =>
            {
                symbolContext.CancellationToken.ThrowIfCancellationRequested();
                var fieldSymbol = (IFieldSymbol)symbolContext.Symbol;
                MarkUsed(fieldSymbol.Type as INamedTypeSymbol, usedTypes);
            }, SymbolKind.Field);

            startContext.RegisterCompilationEndAction(endContext =>
            {
                endContext.CancellationToken.ThrowIfCancellationRequested();
                foreach (var declaredType in declaredTypes.OrderBy(pair => pair.Key.ToDisplayString(), System.StringComparer.Ordinal))
                {
                    endContext.CancellationToken.ThrowIfCancellationRequested();
                    if (usedTypes.ContainsKey(declaredType.Key))
                    {
                        continue;
                    }

                    endContext.ReportDiagnostic(Diagnostic.Create(Rule, declaredType.Value, declaredType.Key.Name));
                }
            });
        });
    }

    private static void MarkUsed(INamedTypeSymbol? typeSymbol, ConcurrentDictionary<INamedTypeSymbol, byte> usedTypes)
    {
        if (typeSymbol is null)
        {
            return;
        }

        var current = typeSymbol;
        while (current is not null)
        {
            if (IsCandidate(current))
            {
                usedTypes.TryAdd(current.OriginalDefinition, 0);
            }

            current = current.ContainingType;
        }
    }

    private static bool IsCandidate(INamedTypeSymbol symbol)
    {
        if (symbol.IsImplicitlyDeclared || symbol.TypeKind is not (TypeKind.Class or TypeKind.Struct))
        {
            return false;
        }

        if (symbol.DeclaredAccessibility != Accessibility.Private && symbol.DeclaredAccessibility != Accessibility.Internal)
        {
            return false;
        }

        if (symbol.IsAbstract || symbol.IsStatic)
        {
            return false;
        }

        if (symbol.Name.EndsWith("Attribute", System.StringComparison.Ordinal))
        {
            return false;
        }

        return !symbol.GetAttributes().Any(IsReflectionDiscoveryAttribute);
    }

    private static bool IsReflectionDiscoveryAttribute(AttributeData attribute)
    {
        var attributeTypeName = attribute.AttributeClass?.ToDisplayString();
        if (string.IsNullOrWhiteSpace(attributeTypeName))
        {
            return false;
        }

        return attributeTypeName!.StartsWith("System.Text.Json.Serialization.", System.StringComparison.Ordinal)
            || attributeTypeName.StartsWith("Newtonsoft.Json.", System.StringComparison.Ordinal)
            || attributeTypeName.StartsWith("System.Runtime.Serialization.", System.StringComparison.Ordinal)
            || attributeTypeName.StartsWith("System.ComponentModel.DataAnnotations.", System.StringComparison.Ordinal)
            || attributeTypeName.StartsWith("Microsoft.EntityFrameworkCore.", System.StringComparison.Ordinal)
            || attributeTypeName == "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute";
    }
}
