using BenchmarkDotNet.Attributes;
using System.Text.Json;
using Ifx.Component;
using Ifx.Primitives;

namespace Ifx.Benchmarks.Core;

public class CoreContractBenchmarks
{
    [Benchmark]
    public ComponentResult SuccessfulResult() => ComponentResult.Success();

    [Benchmark]
    public ComponentResult<int> SuccessfulGenericResult() => ComponentResult<int>.Success(42);

    [Benchmark]
    public ComponentResult FailedResult() => ComponentResult.Failure("Rejected input");

    [Benchmark]
    public ComponentResult<int> FailedGenericResult() => ComponentResult<int>.Failure("Rejected input");
}
