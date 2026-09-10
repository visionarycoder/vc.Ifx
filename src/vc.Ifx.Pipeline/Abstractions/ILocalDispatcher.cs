namespace VisionaryCoder.Framework.Pipeline.Abstractions;

public interface ILocalDispatcher
{
    Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request)
        where TRequest : IRequest<TResponse>;
}