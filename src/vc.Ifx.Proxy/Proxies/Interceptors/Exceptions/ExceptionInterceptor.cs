using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Exceptions;

/// <summary>
/// Interceptor that applies custom exception handling to method invocations.
/// Uses IExceptionHandler to transform or wrap exceptions.
/// </summary>
public sealed class ExceptionInterceptor(IExceptionHandler exceptionHandler) : IProxyInterceptor
{
    /// <summary>
    /// Executes the interceptor to apply custom exception handling.
    /// </summary>
    public async ValueTask<object?> InvokeAsync(MethodContext context, HandlerDelegate next)
    {
        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw exceptionHandler.Handle(context, exception);
        }
    }
}
