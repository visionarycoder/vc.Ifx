using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Auditing;

/// <summary>
/// Records audit entries for all method invocations.
/// Logs both successful and failed invocations with correlation ID, method details, and error information.
/// </summary>
public sealed class AuditInterceptor(IAuditSink auditSink) : IProxyInterceptor
{
    /// <summary>
    /// Executes the operation auditor to record audit entries for method invocations.
    /// </summary>
    public async ValueTask<object?> InvokeAsync(MethodContext context, HandlerDelegate next)
    {
        // Retrieve correlation ID and redacted arguments from context
        context.Items.TryGetValue(InterceptorConstants.CorrelationId, out var correlationValue);
        context.Items.TryGetValue(InterceptorConstants.SafeArguments, out var arguments);
        var correlationId = correlationValue?.ToString();

        try
        {
            var result = await next().ConfigureAwait(false);

            // Write successful audit entry
            await auditSink.WriteAsync(new AuditEntry(
                correlationId,
                context.ContractType.FullName!,
                context.MethodInfo.Name,
                arguments,
                true,
                null,
                DateTimeOffset.UtcNow));

            return result;
        }
        catch (Exception exception)
        {
            // Write failed audit entry with error message
            await auditSink.WriteAsync(new AuditEntry(
                correlationId,
                context.ContractType.FullName!,
                context.MethodInfo.Name,
                arguments,
                false,
                exception.Message,
                DateTimeOffset.UtcNow));

            throw;
        }
    }
}
