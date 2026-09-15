using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ifx.Primitives;

/// <summary>Serializes supported typed identifiers as scalar JSON values.</summary>
public sealed class EntityIdentifierJsonConverterFactory : JsonConverterFactory
{
    private static readonly Type[] SupportedKeys = [typeof(Guid), typeof(string), typeof(int), typeof(long), typeof(short)];

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        return typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(EntityIdentifier<,>)
            && !typeToConvert.ContainsGenericParameters;
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!CanConvert(type))
            throw new ArgumentException("A closed EntityIdentifier type is required.", nameof(type));

        Type[] arguments = type.GetGenericArguments();
        if (!SupportedKeys.Contains(arguments[1]))
            throw new NotSupportedException($"EntityIdentifier JSON keys of type {arguments[1]} are not supported.");

        Type converter = typeof(EntityIdJsonConverter<,>).MakeGenericType(arguments);
        return (JsonConverter)Activator.CreateInstance(converter)!;
    }

    private sealed class EntityIdJsonConverter<TEntity, TKey> : JsonConverter<EntityIdentifier<TEntity, TKey>>
        where TEntity : class
        where TKey : notnull
    {
        public override EntityIdentifier<TEntity, TKey> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            TKey? value = JsonSerializer.Deserialize<TKey>(ref reader, options);
            // Preserve the legacy null-string representation; numeric/Guid nulls are rejected by their converters.
            return new(value is null ? (TKey)(object)string.Empty : value);
        }

        public override void Write(Utf8JsonWriter writer, EntityIdentifier<TEntity, TKey> value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value.Value, options);
    }
}
