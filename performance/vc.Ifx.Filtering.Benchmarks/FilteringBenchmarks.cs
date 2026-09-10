using BenchmarkDotNet.Attributes;
using System.Linq.Expressions;
using System.Text.Json;
using VisionaryCoder.Framework.Filtering;
using VisionaryCoder.Framework.Filtering.Abstractions;

namespace vc.Ifx.Filtering.Benchmarks;

[MemoryDiagnoser]
public class FilteringBenchmarks
{
    private FilterNode node = null!;
    private string json = string.Empty;

    [Params(1, 8)] public int PredicateCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        node = BuildNestedPredicate();
        json = SerializePortableFilter();
        if (DeserializePortableFilter() is null) throw new InvalidOperationException("Missing roundtrip filter.");
    }

    [Benchmark]
    public FilterNode BuildSimplePredicate() =>
        Filter.For<BenchmarkCustomer>()
            .Where(customer => customer.Age >= 18 && customer.Name.Contains("Smith"))
            .Build();

    [Benchmark]
    public FilterNode BuildNestedPredicate()
    {
        var specification = new FilterSpec<BenchmarkCustomer>(customer => customer.Orders.Any(order => order.Amount >= 100));
        for (int index = 0; index < PredicateCount; index++)
        {
            int minimum = index + 18;
            specification = specification.Where(customer => customer.Age >= minimum);
        }
        return specification.ToFilterNode();
    }

    [Benchmark] public Expression<Func<BenchmarkCustomer, bool>> TranslatePortableFilter() => FilterExpression.Create<BenchmarkCustomer>(node);
    [Benchmark] public string SerializePortableFilter() => JsonSerializer.Serialize(node);
    [Benchmark] public FilterNode? DeserializePortableFilter() => JsonSerializer.Deserialize<FilterNode>(json);

    public sealed class BenchmarkCustomer
    {
        public int Age { get; set; }

        public string Name { get; set; } = string.Empty;
        public Order[] Orders { get; set; } = [];
    }

    public sealed record Order(int Amount);
}

[MemoryDiagnoser]
public class FilteringExecutionBenchmarks
{
    private FilteringBenchmarks.BenchmarkCustomer[] customers = [];
    private readonly Expression<Func<FilteringBenchmarks.BenchmarkCustomer, bool>> expression =
        customer => customer.Age >= 18 && customer.Orders.Any(order => order.Amount >= 100);
    private Func<FilteringBenchmarks.BenchmarkCustomer, bool> direct = null!;
    private Func<FilteringBenchmarks.BenchmarkCustomer, bool> portable = null!;
    private FilterNode node = null!;

    [Params(100, 10000)] public int CustomerCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        customers = Enumerable.Range(0, CustomerCount).Select(index => new FilteringBenchmarks.BenchmarkCustomer
        {
            Age = index % 80, Name = "Customer", Orders = [new(index % 200 + 50), new(index % 50)]
        }).ToArray();
        direct = expression.Compile();
        node = new FilterSpec<FilteringBenchmarks.BenchmarkCustomer>(expression).ToFilterNode();
        portable = FilterExpression.Create<FilteringBenchmarks.BenchmarkCustomer>(node).Compile();
        if (DirectLinq() != CompiledPortablePredicate() || DirectLinq() == 0 || DirectLinq() == CustomerCount)
            throw new InvalidOperationException("Expected a mixed, equivalent filtering workload.");
    }

    [Benchmark(Baseline = true)] public int DirectLinq() => customers.Count(direct);
    [Benchmark] public int CompiledPortablePredicate() => customers.Count(portable);
    [Benchmark] public int TranslateCompileAndExecute() => customers.Count(FilterExpression.Create<FilteringBenchmarks.BenchmarkCustomer>(node).Compile());
}
