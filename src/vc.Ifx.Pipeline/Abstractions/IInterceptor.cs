namespace VisionaryCoder.Framework.Pipeline.Abstractions;

/// <summary>A policy boundary around a request continuation.</summary>
public interface IInterceptor
{
    /// <summary>Invokes the legacy continuation.</summary>
    Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, Task<TResponse>> next)
        where TRequest : IRequest<TResponse>;

    /// <summary>Bridges legacy interceptors while forwarding cancellation to the continuation.</summary>
    Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request,
        Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        PipelineGuard.Call(request, next, cancellationToken);
        return InvokeAsync<TRequest, TResponse>(request, item =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return next(item, cancellationToken);
        });
    }
}
