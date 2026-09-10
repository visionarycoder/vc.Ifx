using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Status;

/// <summary>
/// Publishes status updates at key points in method invocation.
/// Reports Started, Completed, and Failed status events with correlation and error details.
/// </summary>
public sealed class StatusInterceptor(IStatusSink statusSink) : IProxyInterceptor
{
    /// <summary>
    /// Executes the recorder to publish invocation status updates.
    /// </summary>
    public async ValueTask<object?> InvokeAsync(MethodContext context, HandlerDelegate next)
    {
        // Retrieve correlation ID from context
        context.Items.TryGetValue(InterceptorConstants.CorrelationId, out var correlationValue);
        var correlationId = correlationValue?.ToString();

        // Publish Started status
        await statusSink.PublishAsync(CreateUpdate(context, correlationId, Status.Started));

        try
        {
            var result = await next().ConfigureAwait(false);

            // Publish Completed status on success
            await statusSink.PublishAsync(CreateUpdate(context, correlationId, Status.Completed));

            return result;
        }
        catch (Exception exception)
        {
            // Publish Failed status with error message
            await statusSink.PublishAsync(CreateUpdate(context, correlationId, Status.Failed, exception.Message));

            throw;
        }
    }

    /// <summary>
    /// Creates an InvocationStatusUpdate record for the given context and status.
    /// </summary>
    private static StatusUpdated CreateUpdate(MethodContext context, string? correlationId, Status status, string? error = null)
        => new(
            correlationId,
            context.ContractType.FullName ?? context.ContractType.Name,
            context.MethodInfo.Name,
            status,
            error,
            DateTimeOffset.UtcNow);
}
