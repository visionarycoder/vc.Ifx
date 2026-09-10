namespace VisionaryCoder.Framework.Pipeline.Abstractions;

public interface IRemoteDispatcher
{
    Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request, EndpointResolution endpoint)
        where TRequest : IRequest<TResponse>;
}
