using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Ifx.Proxy;
using Ifx.Proxy.Interceptor.Caching;
using Ifx.Proxy.Interceptor.Caching.Providers;

namespace Ifx.Integrations.Benchmarks.Proxy;

public class ProxyCacheBenchmarks
{
    private MemoryCache memory = null!;
    private MemoryProxyCache cache = null!;
    private ProxyResponse<byte[]> response = null!;

    [Params(64, 65536)] public int PayloadBytes { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        memory = new MemoryCache(new MemoryCacheOptions { SizeLimit = 4 });
        cache = new MemoryProxyCache(memory);
        response = ProxyResponse<byte[]>.Success(new byte[PayloadBytes]);
        await cache.SetAsync("hit", response, TimeSpan.FromHours(1));
        await Replace();
        if ((await Hit())?.Data?.Length != PayloadBytes || await Miss() is not null)
            throw new InvalidOperationException("Cache fixture did not establish hit/miss state.");
    }

    [Benchmark] public Task<ProxyResponse<byte[]>?> Hit() => cache.GetAsync<byte[]>("hit");
    [Benchmark] public Task<ProxyResponse<byte[]>?> Miss() => cache.GetAsync<byte[]>("absent");
    [Benchmark] public Task Replace() => cache.SetAsync("replace", response, TimeSpan.FromHours(1));
    [GlobalCleanup] public void Cleanup() => memory.Dispose();
}
