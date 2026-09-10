namespace VisionaryCoder.Framework.Pipeline.Abstractions;

/// <summary>Compatible request dispatch boundary with cooperative cancellation support.</summary>
public interface IRemoteDispatcher
{
    /// <summary>Invokes the legacy operation without an explicit cancellation token.</summary>
    Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request, EndpointResolution endpoint)
        where TRequest : IRequest<TResponse>;

    /// <summary>Checks cancellation before invoking a legacy implementation; override to cancel in-flight work.</summary>
    Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request, EndpointResolution endpoint, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        cancellationToken.ThrowIfCancellationRequested();
        return DispatchAsync<TRequest, TResponse>(request, endpoint);
    }
}
