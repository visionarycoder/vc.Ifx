using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors;

/// <summary>
/// Defines the contract for implementing method interception logic.
/// Interceptors form a pipeline that executes in sequence before and after method invocation.
/// </summary>
public interface IProxyInterceptor
{
    /// <summary>
    /// Invokes the interceptor logic with access to the invocation context and the next handler in the pipeline.
    /// </summary>
    /// <param name="context">The context of the current invocation, including method, arguments, and target.</param>
    /// <param name="next">A delegate to invoke the next handler in the pipeline.</param>
    /// <returns>A ValueTask containing the result of the invocation or pipeline.</returns>
    ValueTask<object?> InvokeAsync(MethodContext context, HandlerDelegate next);
}
