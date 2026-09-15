using System.Collections.Immutable;
using System.Threading;
using Ifx.Analyzers.Abstractions;
using Ifx.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ifx.Analyzers.Rules;

/// <summary>Flags non-event async void methods that hide failures from callers.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Ifx1200NoAsyncVoidMethod : DiagnosticAnalyzer
{
    /// <summary>Descriptor for async void methods outside recognized exceptions.</summary>
    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticIds.Ifx1200NoAsyncVoidMethod,
        title: "Avoid async void methods",
        messageFormat: "Method '{0}' is declared 'async void'; use 'async Task' unless this is a recognized event handler",
        category: DiagnosticCategories.Reliability,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Use async Task instead of async void except for recognized event handlers and required framework contracts.",
        helpLinkUri: "https://github.com/visionarycoder/Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#ifx1200");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var declaration = (MethodDeclarationSyntax)context.Node;
        if (!declaration.Modifiers.Any(SyntaxKind.AsyncKeyword) ||
            declaration.ReturnType is not PredefinedTypeSyntax returnType ||
            !returnType.Keyword.IsKind(SyntaxKind.VoidKeyword))
        {
            return;
        }

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken);
        if (methodSymbol is null || methodSymbol.OverriddenMethod != null || ImplementsInterfaceMember(methodSymbol, context.CancellationToken))
        {
            return;
        }

        if (IsRecognizedEventHandler(methodSymbol, context.Compilation, context.CancellationToken))
        {
            return;
        }

        context.CancellationToken.ThrowIfCancellationRequested();
        context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Identifier.GetLocation(), declaration.Identifier.ValueText));
    }

    private static bool ImplementsInterfaceMember(IMethodSymbol methodSymbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (methodSymbol.ExplicitInterfaceImplementations.Length > 0)
        {
            return true;
        }

        var containingType = methodSymbol.ContainingType;
        foreach (var interfaceType in containingType.AllInterfaces)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var member in interfaceType.GetMembers(methodSymbol.Name))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (member is IMethodSymbol interfaceMethod &&
                    SymbolEqualityComparer.Default.Equals(containingType.FindImplementationForInterfaceMember(interfaceMethod), methodSymbol))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsRecognizedEventHandler(IMethodSymbol methodSymbol, Compilation compilation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (methodSymbol.Parameters.Length != 2)
        {
            return false;
        }

        var sender = methodSymbol.Parameters[0];
        var args = methodSymbol.Parameters[1];
        if (sender.RefKind != RefKind.None || args.RefKind != RefKind.None || !sender.Type.IsReferenceType)
        {
            return false;
        }

        var eventArgsType = compilation.GetTypeByMetadataName("System.EventArgs");
        if (eventArgsType is null)
        {
            return false;
        }

        return InheritsFromOrMatches(args.Type, eventArgsType, cancellationToken);
    }

    private static bool InheritsFromOrMatches(ITypeSymbol candidateType, INamedTypeSymbol targetType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        for (ITypeSymbol? current = candidateType; current is not null; current = current.BaseType)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (SymbolEqualityComparer.Default.Equals(current, targetType))
            {
                return true;
            }
        }

        return false;
    }
}
