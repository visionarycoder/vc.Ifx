using BenchmarkDotNet.Attributes;
using System.Text.Json;
using VisionaryCoder.Framework;
using VisionaryCoder.Framework.Primitives;

namespace vc.Ifx.Core.Benchmarks;

[MemoryDiagnoser]
public class CoreContractBenchmarks
{
    [Benchmark]
    public ServiceResult SuccessfulResult() => ServiceResult.Success();

    [Benchmark]
    public ServiceResult<int> SuccessfulGenericResult() => ServiceResult<int>.Success(42);

    [Benchmark]
    public ServiceResult FailedResult() => ServiceResult.Failure("Rejected input");

    [Benchmark]
    public ServiceResult<int> FailedGenericResult() => ServiceResult<int>.Failure("Rejected input");
}

[MemoryDiagnoser]
public class IdentifierBenchmarks
{
    private readonly Guid key = new("e1af00ea-d529-4a90-94b5-a45fde018d76");
    private readonly JsonSerializerOptions options = new() { Converters = { new EntityIdJsonConverterFactory() } };
    private EntityId<Customer, Guid> identifier;
    private EntityId<Customer, Guid> equal;
    private string text = string.Empty;
    private string json = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        identifier = EntityId<Customer, Guid>.Create(key);
        equal = EntityId<Customer, Guid>.Create(key);
        text = identifier.ToString();
        json = JsonSerializer.Serialize(identifier, options);
        if (Deserialize() != identifier || !EqualIdentifiers() || !TryParseValid() || TryParseInvalid())
            throw new InvalidOperationException("Identifier fixture does not satisfy the expected contract.");
    }

    [Benchmark] public EntityId<Customer, Guid> Construct() => EntityId<Customer, Guid>.Create(key);
    [Benchmark] public bool TryParseValid() => EntityId<Customer, Guid>.TryParse(text, out var parsed);
    [Benchmark] public bool TryParseInvalid() => EntityId<Customer, Guid>.TryParse("not-a-guid", out var parsed);
    [Benchmark] public bool EqualIdentifiers() => identifier == equal;
    [Benchmark] public string Serialize() => JsonSerializer.Serialize(identifier, options);
    [Benchmark] public EntityId<Customer, Guid> Deserialize() => JsonSerializer.Deserialize<EntityId<Customer, Guid>>(json, options);

    public sealed class Customer;
}
