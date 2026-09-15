using System;
using System.Collections.Immutable;
using Ifx.Analyzers.Abstractions;
using Ifx.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Ifx.Analyzers.Rules;

/// <summary>Reports synchronous blocking on task-based asynchronous operations.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Ifx1300NoSyncOverAsyncBlocking : DiagnosticAnalyzer
{
    /// <summary>Descriptor for sync-over-async blocking on Task and ValueTask flows.</summary>
    public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        id: DiagnosticIds.Ifx1300NoSyncOverAsyncBlocking,
        title: "Do not block on asynchronous work",
        messageFormat: "Blocking call '{0}' on an asynchronous operation can cause deadlocks; use 'await' instead",
        category: DiagnosticCategories.Reliability,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Avoid synchronous blocking on Task and ValueTask operations. Propagate async and use await instead of Result, Wait, WaitAll, WaitAny, or GetAwaiter().GetResult().",
        helpLinkUri: "https://github.com/visionarycoder/Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#ifx1300");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(RegisterCompilationActions);
    }

    private static void RegisterCompilationActions(CompilationStartAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        INamedTypeSymbol? taskType = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        INamedTypeSymbol? genericTaskType = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        INamedTypeSymbol? valueTaskType = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        INamedTypeSymbol? genericValueTaskType = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");

        if (taskType == null && genericTaskType == null && valueTaskType == null && genericValueTaskType == null)
        {
            return;
        }

        context.RegisterOperationAction(
            operationContext => AnalyzePropertyReference(operationContext, genericTaskType),
            OperationKind.PropertyReference);
        context.RegisterOperationAction(
            operationContext => AnalyzeInvocation(operationContext, taskType, genericTaskType, valueTaskType, genericValueTaskType),
            OperationKind.Invocation);
    }

    private static void AnalyzePropertyReference(OperationAnalysisContext context, INamedTypeSymbol? genericTaskType)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        IPropertyReferenceOperation propertyReference = (IPropertyReferenceOperation)context.Operation;
        if (!string.Equals(propertyReference.Property.Name, "Result", StringComparison.Ordinal))
        {
            return;
        }

        if (!IsConstructedFrom(propertyReference.Property.ContainingType, genericTaskType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, GetReportedLocation(propertyReference), propertyReference.Property.Name));
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, INamedTypeSymbol? taskType, INamedTypeSymbol? genericTaskType,
        INamedTypeSymbol? valueTaskType, INamedTypeSymbol? genericValueTaskType)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        IInvocationOperation invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol targetMethod = invocation.TargetMethod;

        if (string.Equals(targetMethod.Name, "Wait", StringComparison.Ordinal)
            && !targetMethod.IsStatic
            && IsTaskType(targetMethod.ContainingType, taskType, genericTaskType))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, GetReportedLocation(invocation), targetMethod.Name));
            return;
        }

        if ((string.Equals(targetMethod.Name, "WaitAll", StringComparison.Ordinal)
                || string.Equals(targetMethod.Name, "WaitAny", StringComparison.Ordinal))
            && targetMethod.IsStatic
            && taskType != null
            && SymbolEqualityComparer.Default.Equals(targetMethod.ContainingType, taskType))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, GetReportedLocation(invocation), targetMethod.Name));
            return;
        }

        if (!string.Equals(targetMethod.Name, "GetResult", StringComparison.Ordinal))
        {
            return;
        }

        IInvocationOperation? getAwaiterInvocation = invocation.Instance as IInvocationOperation;
        if (getAwaiterInvocation == null || !string.Equals(getAwaiterInvocation.TargetMethod.Name, "GetAwaiter", StringComparison.Ordinal))
        {
            return;
        }

        if (!IsTaskLikeType(getAwaiterInvocation.Instance == null ? null : getAwaiterInvocation.Instance.Type,
            taskType, genericTaskType, valueTaskType, genericValueTaskType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, GetReportedLocation(invocation), targetMethod.Name));
    }

    private static bool IsTaskLikeType(ITypeSymbol? type, INamedTypeSymbol? taskType, INamedTypeSymbol? genericTaskType,
        INamedTypeSymbol? valueTaskType, INamedTypeSymbol? genericValueTaskType)
    {
        return IsTaskType(type as INamedTypeSymbol, taskType, genericTaskType)
            || IsValueTaskType(type as INamedTypeSymbol, valueTaskType, genericValueTaskType);
    }

    private static bool IsTaskType(INamedTypeSymbol? type, INamedTypeSymbol? taskType, INamedTypeSymbol? genericTaskType)
    {
        return IsExactType(type, taskType) || IsConstructedFrom(type, genericTaskType);
    }

    private static bool IsValueTaskType(INamedTypeSymbol? type, INamedTypeSymbol? valueTaskType, INamedTypeSymbol? genericValueTaskType)
    {
        return IsExactType(type, valueTaskType) || IsConstructedFrom(type, genericValueTaskType);
    }

    private static bool IsExactType(INamedTypeSymbol? type, INamedTypeSymbol? expectedType)
    {
        return type != null && expectedType != null && SymbolEqualityComparer.Default.Equals(type, expectedType);
    }

    private static bool IsConstructedFrom(INamedTypeSymbol? type, INamedTypeSymbol? genericType)
    {
        return type != null
            && genericType != null
            && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, genericType);
    }

    private static Location GetReportedLocation(IOperation operation)
    {
        if (operation.Syntax is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Name.GetLocation();
        }

        if (operation.Syntax is InvocationExpressionSyntax invocation
            && invocation.Expression is MemberAccessExpressionSyntax invokedMemberAccess)
        {
            return invokedMemberAccess.Name.GetLocation();
        }

        return operation.Syntax.GetLocation();
    }
}
