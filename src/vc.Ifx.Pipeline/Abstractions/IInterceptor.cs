namespace VisionaryCoder.Framework.Pipeline.Abstractions;

public interface IInterceptor
{
    Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, Task<TResponse>> next) where TRequest : IRequest<TResponse>;
}
