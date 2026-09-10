using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Dispatch;

public sealed class LocalDispatcher(IServiceProvider sp) : ILocalDispatcher
{
    public Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request)
        where TRequest : IRequest<TResponse>
    {
        var handler = sp.GetService(typeof(IRequestHandler<TRequest, TResponse>))
            as IRequestHandler<TRequest, TResponse>;
        if (handler == null)
            throw new InvalidOperationException($"No handler for {typeof(TRequest).Name}");
        return handler.HandleAsync(request, CancellationToken.None);
    }
}