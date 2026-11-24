using StackExchange.Redis;

namespace Ifx.Proxy.Interceptor.Caching.Providers;

internal sealed class RedisProxyCacheInvalidationChannel(
    ISubscriber subscriber,
    IDatabase database,
    string channelName) : IProxyCacheInvalidationChannel
{
    private readonly Dictionary<Action<string>, Action<RedisChannel, RedisValue>> handlers = [];
    private readonly object sync = new();
    private readonly RedisChannel channel = RedisChannel.Literal(channelName);
    private readonly RedisKey generationKey = $"{channelName}:generation";

    public async Task<long> GetGenerationAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RedisValue current = await database.StringGetAsync(generationKey).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return current.IsNull ? 0 : (long)current;
    }

    public async Task<long> AdvanceGenerationAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        long next = await database.StringIncrementAsync(generationKey).ConfigureAwait(false);
        return next;
    }

    public async Task SubscribeAsync(Action<string> handler, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(handler);
        cancellationToken.ThrowIfCancellationRequested();

        Action<RedisChannel, RedisValue> redisHandler = (_, value) => handler(value.ToString());
        lock (sync)
        {
            if (!handlers.TryAdd(handler, redisHandler))
            {
                return;
            }
        }

        try
        {
            await subscriber.SubscribeAsync(channel, redisHandler).ConfigureAwait(false);
        }
        catch
        {
            lock (sync)
            {
                handlers.Remove(handler);
            }

            throw;
        }
    }

    public async Task UnsubscribeAsync(Action<string> handler, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(handler);
        cancellationToken.ThrowIfCancellationRequested();

        Action<RedisChannel, RedisValue>? redisHandler;
        lock (sync)
        {
            if (!handlers.Remove(handler, out redisHandler))
            {
                return;
            }
        }

        await subscriber.UnsubscribeAsync(channel, redisHandler).ConfigureAwait(false);
    }

    public async Task PublishAsync(string payload, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(payload);
        cancellationToken.ThrowIfCancellationRequested();
        _ = await subscriber.PublishAsync(channel, payload).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }
}
