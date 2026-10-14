using BenchmarkDotNet.Attributes;
using System.Linq.Expressions;
using System.Text.Json;
using Ifx.Filtering;
using Ifx.Filtering.Abstractions;

namespace Ifx.Filtering.Benchmarks;

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
