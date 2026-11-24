namespace Ifx.Proxy.Interceptor.Caching.Providers;

internal interface IProxyCacheInvalidationChannel
{
    Task<long> GetGenerationAsync(CancellationToken cancellationToken);

    Task<long> AdvanceGenerationAsync(CancellationToken cancellationToken);

    Task SubscribeAsync(Action<string> handler, CancellationToken cancellationToken);

    Task UnsubscribeAsync(Action<string> handler, CancellationToken cancellationToken);

    Task PublishAsync(string payload, CancellationToken cancellationToken);
}
