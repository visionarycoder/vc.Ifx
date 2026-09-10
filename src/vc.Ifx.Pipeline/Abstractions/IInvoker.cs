namespace VisionaryCoder.Framework.Pipeline.Abstractions;

public interface IInvoker
{
    Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request)
        where TRequest : IRequest<TResponse>;
}