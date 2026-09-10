using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Exceptions;

/// <summary>
/// Defines a contract for custom exception handling during method invocation.
/// </summary>
public interface IExceptionHandler
{
    /// <summary>
    /// Handles an exception that occurred during method invocation.
    /// Can transform, wrap, or replace the exception.
    /// </summary>
    /// <param name="context">The invocation context.</param>
    /// <param name="exception">The exception that was thrown.</param>
    /// <returns>The exception to be thrown to the caller.</returns>
    Exception Handle(MethodContext context, Exception exception);
}
