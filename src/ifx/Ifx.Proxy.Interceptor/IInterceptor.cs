namespace Ifx.Proxy.Interceptor;

/// <summary>
/// Base interface for all proxy interceptors.
/// </summary>
public interface IInterceptor : Ifx.Pipeline.Abstractions.IInterceptor
{
    /// <summary>
    /// Gets the order in which this interceptor should be executed.
    /// Lower numbers execute first.
    /// </summary>
    int Order { get; }
}