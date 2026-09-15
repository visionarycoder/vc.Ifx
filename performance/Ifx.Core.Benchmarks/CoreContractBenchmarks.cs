using BenchmarkDotNet.Attributes;
using System.Text.Json;
using Ifx.Component;
using Ifx.Primitives;

namespace Ifx.Core.Benchmarks;

[MemoryDiagnoser]
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

[MemoryDiagnoser]
public class IdentifierBenchmarks
{
    private readonly Guid key = new("e1af00ea-d529-4a90-94b5-a45fde018d76");
    private readonly JsonSerializerOptions options = new() { Converters = { new EntityIdentifierJsonConverterFactory() } };
    private EntityIdentifier<Customer, Guid> identifier;
    private EntityIdentifier<Customer, Guid> equal;
    private string text = string.Empty;
    private string json = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        identifier = EntityIdentifier<Customer, Guid>.Create(key);
        equal = EntityIdentifier<Customer, Guid>.Create(key);
        text = identifier.ToString();
        json = JsonSerializer.Serialize(identifier, options);
        if (Deserialize() != identifier || !EqualIdentifiers() || !TryParseValid() || TryParseInvalid())
            throw new InvalidOperationException("Identifier fixture does not satisfy the expected contract.");
    }

    [Benchmark] public EntityIdentifier<Customer, Guid> Construct() => EntityIdentifier<Customer, Guid>.Create(key);
    [Benchmark] public bool TryParseValid() => EntityIdentifier<Customer, Guid>.TryParse(text, out var parsed);
    [Benchmark] public bool TryParseInvalid() => EntityIdentifier<Customer, Guid>.TryParse("not-a-guid", out var parsed);
    [Benchmark] public bool EqualIdentifiers() => identifier == equal;
    [Benchmark] public string Serialize() => JsonSerializer.Serialize(identifier, options);
    [Benchmark] public EntityIdentifier<Customer, Guid> Deserialize() => JsonSerializer.Deserialize<EntityIdentifier<Customer, Guid>>(json, options);

    public sealed class Customer;
}
