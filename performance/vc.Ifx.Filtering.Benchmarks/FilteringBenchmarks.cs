using BenchmarkDotNet.Attributes;
using VisionaryCoder.Framework.Filtering;
using VisionaryCoder.Framework.Filtering.Abstractions;

namespace vc.Ifx.Filtering.Benchmarks;

[MemoryDiagnoser]
public class FilteringBenchmarks
{
    [Benchmark]
    public FilterNode BuildSimplePredicate() =>
        Filter.For<BenchmarkCustomer>()
            .Where(customer => customer.Age >= 18 && customer.Name.Contains("Smith"))
            .Build();

    private sealed class BenchmarkCustomer
    {
        public int Age { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
