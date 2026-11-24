using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Ifx.Proxy;
using Ifx.Proxy.Interceptor.Caching;
using Ifx.Proxy.Interceptor.Caching.Providers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;

namespace Ifx.Tests.Proxy.Interceptors.Caching.Providers;

[TestClass]
public sealed class DistributedProxyCacheTests
{
    [TestMethod]
    public async Task SetAndGetAsync_RoundTripTypedResponsesWithoutExposingCallerKey()
    {
        var storage = new MemoryDistributedCache();
        DistributedProxyCache cache = await CreateCache(storage);

        await cache.SetAsync("sensitive/raw-key", ProxyResponse<string>.Success("value"), TimeSpan.FromMinutes(1));

        int readsBeforeLocalHit = storage.GetCalls;
        ProxyResponse<string>? result = await cache.GetAsync<string>("sensitive/raw-key");
        int readsAfterFirstRead = storage.GetCalls;
        result = await cache.GetAsync<string>("sensitive/raw-key");
        Assert.AreEqual("value", result?.Data);
        Assert.IsTrue(result?.IsSuccess);
        Assert.AreEqual(readsBeforeLocalHit, readsAfterFirstRead);
        Assert.AreEqual(readsAfterFirstRead, storage.GetCalls);
        Assert.IsFalse(storage.Keys.Any(key => key.Contains("sensitive/raw-key", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task ClearAsync_ChangesNamespaceWithoutDeletingSharedCacheEntries()
    {
        var storage = new MemoryDistributedCache();
        DistributedProxyCache cache = await CreateCache(storage);
        await cache.SetAsync("key", ProxyResponse<int>.Success(1), TimeSpan.FromMinutes(1));
        string cachedEntryKey = storage.Keys.Single(key => key.Contains(":0:", StringComparison.Ordinal));

        await cache.ClearAsync();

        Assert.IsNull(await cache.GetAsync<int>("key"));
        Assert.IsTrue(storage.Keys.Contains(cachedEntryKey, StringComparer.Ordinal));
    }

    [TestMethod]
    public async Task RemoveAndExistsAsync_UseCurrentGeneration()
    {
        DistributedProxyCache cache = await CreateCache(new MemoryDistributedCache());
        await cache.SetAsync("key", ProxyResponse<int>.Success(1), TimeSpan.FromMinutes(1));
        Assert.IsTrue(await cache.ExistsAsync("key"));

        await cache.RemoveAsync("key");

        Assert.IsFalse(await cache.ExistsAsync("key"));
    }

    [TestMethod]
    public async Task Operations_RejectInvalidArgumentsAndPreCanceledTokens()
    {
        DistributedProxyCache cache = await CreateCache(new MemoryDistributedCache());
        using var canceled = new CancellationTokenSource();
        await canceled.CancelAsync();

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => cache.GetAsync<int>(" "));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => cache.SetAsync<int>("key", null!, TimeSpan.FromMinutes(1)));
        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() => cache.SetAsync("key", ProxyResponse<int>.Success(1), TimeSpan.Zero));
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => cache.ExistsAsync("key", canceled.Token));
        Assert.ThrowsExactly<ArgumentException>(() =>
            new DistributedProxyCacheOptions { KeyPrefix = "invalid:prefix" }.Validate());
    }

    [TestMethod]
    public async Task AddRedisProxyCaching_RegistersDistributedProvider()
    {
        var services = new ServiceCollection();
        Action<RedisChannel, RedisValue>? invalidationHandler = null;
        var subscriber = new Mock<ISubscriber>(MockBehavior.Strict);
        subscriber.Setup(value => value.SubscribeAsync(
                It.IsAny<RedisChannel>(),
                It.IsAny<Action<RedisChannel, RedisValue>>(),
                It.IsAny<CommandFlags>()))
            .Callback<RedisChannel, Action<RedisChannel, RedisValue>, CommandFlags>((_, handler, _) =>
                invalidationHandler = handler)
            .Returns(Task.CompletedTask);
        subscriber.Setup(value => value.UnsubscribeAsync(
                It.IsAny<RedisChannel>(),
                It.IsAny<Action<RedisChannel, RedisValue>>(),
                It.IsAny<CommandFlags>()))
            .Returns(Task.CompletedTask);
        subscriber.Setup(value => value.PublishAsync(
                It.IsAny<RedisChannel>(),
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);
        var database = new Mock<IDatabase>(MockBehavior.Strict);
        database.Setup(value => value.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);
        database.Setup(value => value.StringIncrementAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<long>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);
        var multiplexer = new Mock<IConnectionMultiplexer>(MockBehavior.Strict);
        multiplexer.Setup(value => value.GetSubscriber(null)).Returns(subscriber.Object);
        multiplexer.Setup(value => value.GetDatabase(-1, null)).Returns(database.Object);
        services.AddRedisProxyCaching(multiplexer.Object);
        var distributedCache = new MemoryDistributedCache();
        services.AddSingleton<IDistributedCache>(distributedCache);
        using ServiceProvider provider = services.BuildServiceProvider();

        IProxyCache cache = provider.GetRequiredService<IProxyCache>();
        Assert.IsInstanceOfType<DistributedProxyCache>(cache);
        await ((DistributedProxyCache)cache).StartAsync(CancellationToken.None);
        await cache.SetAsync("key", ProxyResponse<string>.Success("value"), TimeSpan.FromMinutes(1));
        Assert.AreEqual("value", (await cache.GetAsync<string>("key"))?.Data);
        int readsBeforeInvalidation = distributedCache.GetCalls;
        string keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("key")));
        invalidationHandler!(
            RedisChannel.Literal("ifx:proxy-cache:default:invalidation"),
            $$"""{"instanceId":"other","generation":0,"keyHash":"{{keyHash}}","isClear":false}""");
        Assert.AreEqual("value", (await cache.GetAsync<string>("key"))?.Data);
        Assert.AreEqual(readsBeforeInvalidation + 1, distributedCache.GetCalls);
        await cache.ClearAsync();
        await ((DistributedProxyCache)cache).StopAsync(CancellationToken.None);
        database.Verify(value => value.StringIncrementAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<long>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [TestMethod]
    public async Task RedisChannel_InvalidatesL1EntriesAcrossCacheInstances()
    {
        var storage = new MemoryDistributedCache();
        var channel = new MemoryInvalidationChannel();
        DistributedProxyCache first = await CreateCache(storage, channel);
        DistributedProxyCache second = await CreateCache(storage, channel);

        await first.SetAsync("key", ProxyResponse<string>.Success("old"), TimeSpan.FromMinutes(1));
        Assert.AreEqual("old", (await second.GetAsync<string>("key"))?.Data);

        await first.SetAsync("key", ProxyResponse<string>.Success("new"), TimeSpan.FromMinutes(1));
        Assert.AreEqual("new", (await second.GetAsync<string>("key"))?.Data);

        await first.RemoveAsync("key");
        Assert.IsNull(await second.GetAsync<string>("key"));

        await first.SetAsync("key", ProxyResponse<string>.Success("after-remove"), TimeSpan.FromMinutes(1));
        Assert.AreEqual("after-remove", (await second.GetAsync<string>("key"))?.Data);
        await first.ClearAsync();
        Assert.IsNull(await second.GetAsync<string>("key"));

        await first.ClearAsync();
        await first.SetAsync("key", ProxyResponse<string>.Success("current"), TimeSpan.FromMinutes(1));
        await channel.PublishAsync(
            """{"instanceId":"other","generation":1,"keyHash":null,"isClear":true}""",
            CancellationToken.None);
        Assert.AreEqual("current", (await second.GetAsync<string>("key"))?.Data);

        await first.StopAsync(CancellationToken.None);
        await second.StopAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task GetAsync_DoesNotRefillL1WithPayloadInvalidatedDuringL2Read()
    {
        var storage = new MemoryDistributedCache();
        var channel = new MemoryInvalidationChannel();
        DistributedProxyCache first = await CreateCache(storage, channel);
        DistributedProxyCache second = await CreateCache(storage, channel);
        await first.SetAsync("key", ProxyResponse<string>.Success("old"), TimeSpan.FromMinutes(1));
        string cacheKey = storage.Keys.Single();
        byte[] oldPayload = storage.Get(cacheKey)!;

        var readStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseRead = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int reads = 0;
        storage.GetAsyncOverride = async (key, cancellationToken) =>
        {
            if (Interlocked.Increment(ref reads) == 1)
            {
                readStarted.SetResult();
                await releaseRead.Task.WaitAsync(cancellationToken);
                return oldPayload;
            }

            return storage.Get(key);
        };

        Task<ProxyResponse<string>?> pendingRead = second.GetAsync<string>("key");
        await readStarted.Task;
        await first.SetAsync("key", ProxyResponse<string>.Success("new"), TimeSpan.FromMinutes(1));
        releaseRead.SetResult();

        Assert.AreEqual("new", (await pendingRead)!.Data);
        await first.StopAsync(CancellationToken.None);
        await second.StopAsync(CancellationToken.None);
    }

    private static async Task<DistributedProxyCache> CreateCache(
        IDistributedCache cache,
        IProxyCacheInvalidationChannel? invalidationChannel = null)
    {
        var proxyCache = new DistributedProxyCache(
            cache,
            new MemoryCache(new MemoryCacheOptions()),
            Microsoft.Extensions.Options.Options.Create(new DistributedProxyCacheOptions { KeyPrefix = "tests" }),
            invalidationChannel ?? new MemoryInvalidationChannel());
        await proxyCache.StartAsync(CancellationToken.None);
        return proxyCache;
    }

    private sealed class MemoryDistributedCache : IDistributedCache
    {
        private readonly ConcurrentDictionary<string, byte[]> entries = new(StringComparer.Ordinal);

        public IReadOnlyCollection<string> Keys => entries.Keys.ToArray();
        public DistributedCacheEntryOptions? LastEntryOptions { get; private set; }
        public int GetCalls { get; private set; }
        public Func<string, CancellationToken, Task<byte[]?>>? GetAsyncOverride { get; set; }

        public byte[]? Get(string key)
        {
            GetCalls++;
            return entries.TryGetValue(key, out byte[]? value) ? value : null;
        }

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (GetAsyncOverride is not null)
            {
                return GetAsyncOverride(key, token);
            }

            return Task.FromResult(Get(key));
        }

        public void Refresh(string key) { }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public void Remove(string key) => entries.TryRemove(key, out _);

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Remove(key);
            return Task.CompletedTask;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            LastEntryOptions = options;
            entries[key] = value.ToArray();
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Set(key, value, options);
            return Task.CompletedTask;
        }
    }

    private sealed class MemoryInvalidationChannel : IProxyCacheInvalidationChannel
    {
        private readonly List<Action<string>> handlers = [];
        private long generation;

        public Task<long> GetGenerationAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(generation);
        }

        public Task<long> AdvanceGenerationAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(++generation);
        }

        public Task SubscribeAsync(Action<string> handler, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            handlers.Add(handler);
            return Task.CompletedTask;
        }

        public Task UnsubscribeAsync(Action<string> handler, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            handlers.Remove(handler);
            return Task.CompletedTask;
        }

        public Task PublishAsync(string payload, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (Action<string> handler in handlers.ToArray())
            {
                handler(payload);
            }

            return Task.CompletedTask;
        }
    }
}
