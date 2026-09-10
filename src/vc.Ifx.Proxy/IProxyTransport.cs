namespace VisionaryCoder.Framework.Proxy;
/// <summary>
/// Defines the contract for proxy transport implementations.
/// </summary>
public interface IProxyTransport
{
    /// <summary>
    /// Sends a request through the transport layer and returns a response.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="context">The proxy context containing request information.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    Task<ProxyResponse<T>> SendCoreAsync<T>(ProxyContext context, CancellationToken cancellationToken = default);
}
