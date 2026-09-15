using System.Collections.Immutable;
using Ifx.Analyzers.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Ifx.Analyzers.Rules;

/// <summary>
/// Reports direct calls to <c>IMemoryCache</c>, <c>IDistributedCache</c>, or <c>HybridCache</c>
/// members outside the Ifx framework, steering callers toward the <c>[Cacheable]</c>
/// attribute handled by <c>CachingInterceptor</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidManualCachingAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        id: DiagnosticIds.Ifx1500AvoidManualCaching,
        title: "Avoid manual cache invocation",
        messageFormat: "'{0}' calls '{1}' directly; apply [Cacheable] on the boundary-intercepted contract instead",
        category: "Ifx.Proxies",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Ifx.Proxies exposes a CachingInterceptor driven by [Cacheable]. Calling IMemoryCache, IDistributedCache, or HybridCache directly duplicates that concern and bypasses key resolution, tiering, and telemetry.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var hybridCacheType = context.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Caching.Hybrid.HybridCache");
        var memoryCacheType = context.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Caching.Memory.IMemoryCache");
        var distributedCacheType = context.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Caching.Distributed.IDistributedCache");

        if (hybridCacheType is null && memoryCacheType is null && distributedCacheType is null)
        {
            return;
        }

        context.RegisterOperationAction(
            operationContext => AnalyzeInvocation(operationContext, hybridCacheType, memoryCacheType, distributedCacheType),
            OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(
        OperationAnalysisContext context,
        INamedTypeSymbol? hybridCacheType,
        INamedTypeSymbol? memoryCacheType,
        INamedTypeSymbol? distributedCacheType)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var receiverType = invocation.TargetMethod.ReceiverType;

        if (!IsCacheType(receiverType, hybridCacheType, memoryCacheType, distributedCacheType))
        {
            return;
        }

        var containingSymbol = context.ContainingSymbol;
        if (IfxFrameworkNamespace.Contains(containingSymbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            invocation.Syntax.GetLocation(),
            containingSymbol.Name,
            invocation.TargetMethod.Name));
    }

    private static bool IsCacheType(
        ITypeSymbol? receiverType,
        INamedTypeSymbol? hybridCacheType,
        INamedTypeSymbol? memoryCacheType,
        INamedTypeSymbol? distributedCacheType)
    {
        if (receiverType is null)
        {
            return false;
        }

        if (SymbolEqualityComparer.Default.Equals(receiverType, hybridCacheType)
            || SymbolEqualityComparer.Default.Equals(receiverType, memoryCacheType)
            || SymbolEqualityComparer.Default.Equals(receiverType, distributedCacheType))
        {
            return true;
        }

        foreach (var implementedInterface in receiverType.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(implementedInterface, memoryCacheType)
                || SymbolEqualityComparer.Default.Equals(implementedInterface, distributedCacheType))
            {
                return true;
            }
        }

        return false;
    }
}
