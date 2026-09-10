using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Timeout;

/// <summary>
/// Interceptor that enforces timeout constraints on method invocations.
/// Uses the TimeoutAttribute to determine the timeout duration.
/// Throws OperationCanceledException if the timeout is exceeded.
/// </summary>
public sealed class TimeoutInterceptor : IProxyInterceptor
{
    /// <summary>
    /// Executes the interceptor to enforce timeout on method invocation.
    /// </summary>
    public async ValueTask<object?> InvokeAsync(MethodContext context, HandlerDelegate next)
    {
        var attribute = context.GetAttribute<TimeoutAttribute>();
        if (attribute is null)
            return await next().ConfigureAwait(false);

        // Apply timeout to the invocation
        return await next().AsTask().WaitAsync(attribute.Timeout).ConfigureAwait(false);
    }
}
