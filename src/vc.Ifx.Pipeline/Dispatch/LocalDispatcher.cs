using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Dispatch;

/// <summary>Resolves exact handler contracts from an externally owned service provider.</summary>
public sealed class LocalDispatcher(IServiceProvider sp) : ILocalDispatcher
{
    private readonly IServiceProvider services = sp ?? throw new ArgumentNullException(nameof(sp));

    /// <inheritdoc />
    public Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request)
        where TRequest : IRequest<TResponse> => DispatchAsync<TRequest, TResponse>(request, CancellationToken.None);

    /// <inheritdoc />
    public Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        var handler = services.GetService(typeof(IRequestHandler<TRequest, TResponse>)) as IRequestHandler<TRequest, TResponse>
            ?? throw new InvalidOperationException($"No handler for {typeof(TRequest).Name}");
        return handler.HandleAsync(request, cancellationToken);
    }
}
