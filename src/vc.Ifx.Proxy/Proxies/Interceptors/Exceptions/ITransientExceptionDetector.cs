namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Exceptions;

/// <summary>
/// Defines a contract for determining whether an exception represents a transient error
/// that can be safely retried.
/// </summary>
public interface ITransientExceptionDetector
{
    /// <summary>
    /// Determines whether the given exception is transient and can be retried.
    /// </summary>
    /// <param name="exception">The exception to evaluate.</param>
    /// <returns>True if the exception is transient; otherwise false.</returns>
    bool IsTransient(Exception exception);
}
