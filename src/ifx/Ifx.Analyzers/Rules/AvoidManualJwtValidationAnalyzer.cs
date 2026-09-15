using System.Collections.Immutable;
using Ifx.Analyzers.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Ifx.Analyzers.Rules;

/// <summary>
/// Reports direct calls to <c>JwtSecurityTokenHandler.ValidateToken</c> or
/// <c>ValidateTokenAsync</c> outside the Ifx framework, steering callers toward
/// <c>[AuthenticationScheme(AuthenticationSchemeKind.Jwt)]</c> handled by
/// <c>AuthenticationInterceptor</c> and <c>JwtSchemeValidator</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidManualJwtValidationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        id: DiagnosticIds.Ifx1502AvoidManualJwtValidation,
        title: "Avoid manual JWT validation",
        messageFormat: "'{0}' calls 'JwtSecurityTokenHandler.{1}' directly; apply [AuthenticationScheme(AuthenticationSchemeKind.Jwt)] on the boundary-intercepted contract instead",
        category: "Ifx.Proxies",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Ifx.Proxies exposes an AuthenticationInterceptor with a JwtSchemeValidator driven by [AuthenticationScheme]. Calling JwtSecurityTokenHandler directly duplicates token validation outside the interceptor pipeline and its principal propagation.");

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
        var handlerType = context.Compilation.GetTypeByMetadataName("System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler");
        if (handlerType is null)
        {
            return;
        }

        context.RegisterOperationAction(
            operationContext => AnalyzeInvocation(operationContext, handlerType),
            OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, INamedTypeSymbol handlerType)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var methodName = invocation.TargetMethod.Name;

        if (methodName != "ValidateToken" && methodName != "ValidateTokenAsync")
        {
            return;
        }

        var receiverType = invocation.TargetMethod.ReceiverType;
        if (!SymbolEqualityComparer.Default.Equals(receiverType, handlerType))
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
            methodName));
    }
}
