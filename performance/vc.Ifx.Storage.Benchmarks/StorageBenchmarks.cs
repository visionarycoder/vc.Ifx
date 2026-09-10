using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using VisionaryCoder.Framework.Storage.Local;

namespace vc.Ifx.Storage.Benchmarks;

[MemoryDiagnoser]
public class StorageBenchmarks
{
    private readonly LocalStorageProvider provider = new(NullLogger<LocalStorageProvider>.Instance);

    [Benchmark]
    public string ProviderTypeName() => provider.GetType().FullName ?? string.Empty;
}
