using BenchmarkDotNet.Attributes;
using Ifx.Querying.Serialization;
using SerializedFilterNode = Ifx.Querying.Serialization.FilterNode;

namespace Ifx.Benchmarks.Filtering;

[MemoryDiagnoser]
public class QuerySerializationBenchmarks
{
    private SerializedFilterNode query = null!;
    private string json = string.Empty;

    [Params(1, 16)] public int PredicateCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        query = new CompositeFilter("And", Enumerable.Range(0, PredicateCount)
            .Select(index => (SerializedFilterNode)new PropertyFilter("Equals", "Name", "Customer")).ToList());
        json = Serialize();
        if (Deserialize() is not CompositeFilter composite || composite.Children.Count != PredicateCount)
            throw new InvalidOperationException("Query fixture did not roundtrip.");
    }

    [Benchmark] public string Serialize() => QueryFilterSerializer.Serialize(query);
    [Benchmark] public SerializedFilterNode? Deserialize() => QueryFilterSerializer.Deserialize(json);
}
