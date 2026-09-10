namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Exceptions;

/// <summary>
/// Default implementation of transient exception detection.
/// Considers TimeoutException and IOException as transient.
/// </summary>
public sealed class DefaultTransientExceptionDetector : ITransientExceptionDetector
{
    /// <summary>
    /// Determines whether the exception is transient.
    /// </summary>
    public bool IsTransient(Exception exception)
        => exception is TimeoutException or IOException;
}
