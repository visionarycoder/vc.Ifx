using BenchmarkDotNet.Attributes;
using VisionaryCoder.Framework.Querying.Serialization;

namespace vc.Ifx.Filtering.Benchmarks;

[MemoryDiagnoser]
public class QuerySerializationBenchmarks
{
    private FilterNode query = null!;
    private string json = string.Empty;

    [Params(1, 16)] public int PredicateCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        query = new CompositeFilter("And", Enumerable.Range(0, PredicateCount)
            .Select(index => (FilterNode)new PropertyFilter("Equals", "Name", "Customer")).ToList());
        json = Serialize();
        if (Deserialize() is not CompositeFilter composite || composite.Children.Count != PredicateCount)
            throw new InvalidOperationException("Query fixture did not roundtrip.");
    }

    [Benchmark] public string Serialize() => QueryFilterSerializer.Serialize(query);
    [Benchmark] public FilterNode? Deserialize() => QueryFilterSerializer.Deserialize(json);
}
