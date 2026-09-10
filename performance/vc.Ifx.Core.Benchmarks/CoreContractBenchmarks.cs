using BenchmarkDotNet.Attributes;
using VisionaryCoder.Framework;

namespace vc.Ifx.Core.Benchmarks;

[MemoryDiagnoser]
public class CoreContractBenchmarks
{
    [Benchmark]
    public ServiceResult SuccessfulResult() => ServiceResult.Success();

    [Benchmark]
    public ServiceResult<int> SuccessfulGenericResult() => ServiceResult<int>.Success(42);
}
