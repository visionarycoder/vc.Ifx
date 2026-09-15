using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ifx.Primitives;

/// <summary>Serializes <see cref="Identifier{TOwner}"/> values as scalar GUID strings.</summary>
public sealed class IdentifierJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        return typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(Identifier<>)
            && !typeToConvert.ContainsGenericParameters;
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!CanConvert(type))
            throw new ArgumentException("A closed Identifier type is required.", nameof(type));

        Type owner = type.GetGenericArguments()[0];
        Type converter = typeof(IdentifierConverter<>).MakeGenericType(owner);
        return (JsonConverter)Activator.CreateInstance(converter)!;
    }

    private sealed class IdentifierConverter<TOwner> : JsonConverter<Identifier<TOwner>>
        where TOwner : class
    {
        public override Identifier<TOwner> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Guid value = JsonSerializer.Deserialize<Guid>(ref reader, options);
            return Identifier<TOwner>.Create(value);
        }

        public override void Write(Utf8JsonWriter writer, Identifier<TOwner> value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value.Value, options);
    }
}
