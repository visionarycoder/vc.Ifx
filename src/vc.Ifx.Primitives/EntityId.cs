using System.Globalization;

namespace VisionaryCoder.Framework.Primitives;
public readonly record struct EntityId<TEntity, TKey>(TKey Value) : IEntityId
    where TEntity : class
    where TKey : notnull
{
    
    public static EntityId<TEntity, TKey> Create(TKey value)
    {
        if (EqualityComparer<TKey>.Default.Equals(value, default!))
            throw new ArgumentException("ID cannot be the default value.", nameof(value));

        if (value is string s && string.IsNullOrWhiteSpace(s))
            throw new ArgumentException("ID cannot be empty/whitespace.", nameof(value));

        return new(value);
    }
    
    public override string ToString() => Value?.ToString() ?? string.Empty;

    // Boxing for infra
    Type IEntityId.ValueType => typeof(TKey);

    object IEntityId.BoxedValue => Value;

    // Conversions
    public static implicit operator EntityId<TEntity, TKey>(TKey value) => Create(value);
    public static explicit operator TKey(EntityId<TEntity, TKey> id) => id.Value;

    public static EntityId<TEntity, TKey> Parse(string text) => TryParse(text, out EntityId<TEntity, TKey> id) 
        ? id 
        : throw new FormatException($"Invalid {typeof(TKey).Name}.");

    public static bool TryParse(string text, out EntityId<TEntity, TKey> id)
    {

        id = default;
        if (typeof(TKey) == typeof(Guid) && Guid.TryParse(text, out Guid g))
        {
            id = new((TKey)(object)g);
            return true;
        }
        if (typeof(TKey) == typeof(string))
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                id = new((TKey)(object)text);
                return true;
            }
            return false;
        }

        if (typeof(TKey) == typeof(int) && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
        {
            id = new((TKey)(object)i);
            return true;
        }
        if (typeof(TKey) == typeof(long) && long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l))
        {
            id = new((TKey)(object)l);
            return true;
        }
        if (typeof(TKey) == typeof(short) && short.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out short s))
        {
            id = new((TKey)(object)s);
            return true;
        }
        return false;

    }
    
}
