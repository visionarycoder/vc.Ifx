using BenchmarkDotNet.Attributes;
using System.Linq.Expressions;
using System.Text.Json;
using Ifx.Filtering;

namespace Ifx.Benchmarks.Filtering;

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
