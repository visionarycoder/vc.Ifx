using BenchmarkDotNet.Attributes;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;

namespace vc.Ifx.Proxy.Benchmarks;

[MemoryDiagnoser]
public class ProxyBenchmarks
{
    private readonly DefaultCacheKeyProvider provider = new();
    private readonly ProxyContext context = new()
    {
        OperationName = "GetCustomers",
        Method = "GET",
        Url = "https://example.test/customers",
        Headers = new Dictionary<string, string>
        {
            ["Accept"] = "application/json",
            ["X-API-Version"] = "2026-09-10"
        }
    };

    [Benchmark]
    public string GenerateCacheKey() => provider.GenerateKey(context);
}
