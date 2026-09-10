using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;
using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Exceptions;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Retry;

/// <summary>
/// Interceptor that implements retry logic for failed invocations.
/// Uses the RetryAttribute to determine retry configuration.
/// Uses ITransientExceptionDetector to identify exceptions that can be retried.
/// </summary>
public sealed class RetryInterceptor(ITransientExceptionDetector detector) : IProxyInterceptor
{
    /// <summary>
    /// Executes the interceptor to implement retry logic on method invocation.
    /// </summary>
    public async ValueTask<object?> InvokeAsync(MethodContext context, HandlerDelegate next)
    {
        var attribute = context.GetAttribute<RetryAttribute>();

        if (attribute is null)
            return await next().ConfigureAwait(false);

        // Attempt the invocation with retries on transient exceptions
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await next().ConfigureAwait(false);
            }
            catch (Exception exception) when (attempt < attribute.MaxAttempts && detector.IsTransient(exception))
            {
                // Delay before retrying
                await Task.Delay(attribute.Delay).ConfigureAwait(false);
            }
        }
    }
}
