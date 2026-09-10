using System.Text.Json;
using System.Text.Json.Serialization;

namespace VisionaryCoder.Framework.Primitives;

public sealed class EntityIdJsonConverterFactory : JsonConverterFactory
{

    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(EntityId<,>);

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        Type[] args = type.GetGenericArguments(); // [TEntity, TKey]
        Type convType = typeof(EntityIdJsonConverter<,>).MakeGenericType(args[0], args[1]);
        return (JsonConverter) Activator.CreateInstance(convType)!;
    }

    private sealed class EntityIdJsonConverter<TEntity, TKey> : JsonConverter<EntityId<TEntity, TKey>>
        where TEntity : class
        where TKey : notnull
    {

        public override EntityId<TEntity, TKey> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            if (typeof(TKey) == typeof(Guid))
            {
                return new((TKey)(object)reader.GetGuid());
            }
            
            if (typeof(TKey) == typeof(string))
                return new((TKey)(object)(reader.GetString() ?? string.Empty));
            if (typeof(TKey) == typeof(int))
                return new((TKey)(object)reader.GetInt32());
            if (typeof(TKey) == typeof(long))
                return new((TKey)(object)reader.GetInt64());
            if (typeof(TKey) == typeof(short))
                return new((TKey)(object)reader.GetInt16());

            // Fallback: read as string then Parse
            string str = reader.GetString() ?? throw new JsonException("Null ID.");
            return EntityId<TEntity, TKey>.Parse(str);

        }

        public override void Write(Utf8JsonWriter writer, EntityId<TEntity, TKey> value, JsonSerializerOptions options)
        {

            if (typeof(TKey) == typeof(Guid))
            {
                writer.WriteStringValue((Guid)(object)value.Value);
                return;
            }

            if (typeof(TKey) == typeof(string))
            {
                writer.WriteStringValue((string)(object)value.Value);
                return;
            }

            if (typeof(TKey) == typeof(int))
            {
                writer.WriteNumberValue((int)(object)value.Value);
                return;
            }

            if (typeof(TKey) == typeof(long))
            {
                writer.WriteNumberValue((long)(object)value.Value);
                return;
            }

            if (typeof(TKey) == typeof(short))
            {
                writer.WriteNumberValue((short)(object)value.Value);
                return;
            }

            writer.WriteStringValue(value.ToString());
        }

    }

}
