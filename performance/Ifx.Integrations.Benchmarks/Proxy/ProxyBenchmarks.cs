using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Ifx.Proxy;
using Ifx.Proxy.Interceptor.Caching;
using Ifx.Proxy.Interceptor.Caching.Providers;

namespace Ifx.Integrations.Benchmarks.Proxy;

public class ProxyBenchmarks
{
    private readonly DefaultCacheKeyProvider provider = new();
    private ProxyContext context = null!;

    [Params(0, 16)] public int HeaderCount { get; set; }

    [GlobalSetup]
    public void Setup() => context = new()
    {
        OperationName = "GetCustomers",
        Method = "GET",
        Url = "https://example.test/customers",
        Headers = Enumerable.Range(0, HeaderCount).ToDictionary(index => $"X-Header-{index}", index => $"Value-{index}")
    };

    [Benchmark]
    public string GenerateCacheKey() => provider.GenerateKey(context);
}
