using System.Collections.Concurrent;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Framework
{

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
            helpLinkUri: "Docs/Framework/ifx004.md",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        public static void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                var declaredTypes = new ConcurrentDictionary<INamedTypeSymbol, Location>(SymbolEqualityComparer.Default);
                var usedTypes = new ConcurrentDictionary<INamedTypeSymbol, byte>(SymbolEqualityComparer.Default);

                startContext.RegisterSymbolAction(symbolContext =>
                {
                    if (symbolContext.Symbol is not INamedTypeSymbol typeSymbol || !IsCandidate(typeSymbol))
                    {
                        return;
                    }

                    var location = typeSymbol.Locations.FirstOrDefault(l => l.IsInSource);
                    if (location is null)
                    {
                        return;
                    }

                    declaredTypes.TryAdd(typeSymbol, location);
                }, SymbolKind.NamedType);

                startContext.RegisterOperationAction(operationContext =>
                {
                    var objectCreation = (IObjectCreationOperation)operationContext.Operation;
                    MarkUsed(objectCreation.Type as INamedTypeSymbol, usedTypes);
                }, OperationKind.ObjectCreation);

                startContext.RegisterOperationAction(operationContext =>
                {
                    var invocation = (IInvocationOperation)operationContext.Operation;
                    MarkUsed(invocation.TargetMethod.ContainingType, usedTypes);

                    foreach (var typeArgument in invocation.TargetMethod.TypeArguments)
                    {
                        MarkUsed(typeArgument as INamedTypeSymbol, usedTypes);
                    }
                }, OperationKind.Invocation);

                startContext.RegisterOperationAction(operationContext =>
                {
                    var fieldReference = (IFieldReferenceOperation)operationContext.Operation;
                    MarkUsed(fieldReference.Field.Type as INamedTypeSymbol, usedTypes);
                }, OperationKind.FieldReference);

                startContext.RegisterOperationAction(operationContext =>
                {
                    var propertyReference = (IPropertyReferenceOperation)operationContext.Operation;
                    MarkUsed(propertyReference.Property.Type as INamedTypeSymbol, usedTypes);
                }, OperationKind.PropertyReference);

                startContext.RegisterOperationAction(operationContext =>
                {
                    var conversion = (IConversionOperation)operationContext.Operation;
                    MarkUsed(conversion.Type as INamedTypeSymbol, usedTypes);
                }, OperationKind.Conversion);

                startContext.RegisterOperationAction(operationContext =>
                {
                    var typeOf = (ITypeOfOperation)operationContext.Operation;
                    MarkUsed(typeOf.TypeOperand as INamedTypeSymbol, usedTypes);
                }, OperationKind.TypeOf);

                startContext.RegisterSymbolAction(symbolContext =>
                {
                    if (symbolContext.Symbol is not INamedTypeSymbol namedType)
                    {
                        return;
                    }

                    MarkUsed(namedType.BaseType, usedTypes);
                    foreach (var @interface in namedType.Interfaces)
                    {
                        MarkUsed(@interface, usedTypes);
                    }
                }, SymbolKind.NamedType);

                startContext.RegisterSymbolAction(symbolContext =>
                {
                    var methodSymbol = (IMethodSymbol)symbolContext.Symbol;
                    MarkUsed(methodSymbol.ReturnType as INamedTypeSymbol, usedTypes);
                    foreach (var parameter in methodSymbol.Parameters)
                    {
                        MarkUsed(parameter.Type as INamedTypeSymbol, usedTypes);
                    }
                }, SymbolKind.Method);

                startContext.RegisterSymbolAction(symbolContext =>
                {
                    var propertySymbol = (IPropertySymbol)symbolContext.Symbol;
                    MarkUsed(propertySymbol.Type as INamedTypeSymbol, usedTypes);
                }, SymbolKind.Property);

                startContext.RegisterSymbolAction(symbolContext =>
                {
                    var fieldSymbol = (IFieldSymbol)symbolContext.Symbol;
                    MarkUsed(fieldSymbol.Type as INamedTypeSymbol, usedTypes);
                }, SymbolKind.Field);

                startContext.RegisterCompilationEndAction(endContext =>
                {
                    foreach (var declaredType in declaredTypes)
                    {
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
                    usedTypes.TryAdd(current, 0);
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

            if (symbol.Name.EndsWith("Attribute"))
            {
                return false;
            }

            if (symbol.GetAttributes().Any(IsReflectionDiscoveryAttribute))
            {
                return false;
            }

            return !symbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute");
        }

        private static bool IsReflectionDiscoveryAttribute(AttributeData attribute)
        {
            var attributeTypeName = attribute.AttributeClass?.ToDisplayString();
            if (string.IsNullOrWhiteSpace(attributeTypeName))
            {
                return false;
            }

            return attributeTypeName!.StartsWith("System.Text.Json.Serialization.")
                || attributeTypeName.StartsWith("Newtonsoft.Json.")
                || attributeTypeName.StartsWith("System.Runtime.Serialization.")
                || attributeTypeName.StartsWith("System.ComponentModel.DataAnnotations.")
                || attributeTypeName.StartsWith("Microsoft.EntityFrameworkCore.");
        }
    }
}
