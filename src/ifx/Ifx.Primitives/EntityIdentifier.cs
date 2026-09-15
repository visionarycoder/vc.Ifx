using System.Globalization;
using Ifx.Abstractions;

namespace Ifx.Primitives;

public readonly record struct EntityIdentifier<TEntity, TKey>(TKey Value) : IEntityIdentifier, IPrimitiveValue<TKey>
    where TEntity : class
    where TKey : notnull
{
    
    public static EntityIdentifier<TEntity, TKey> Create(TKey value)
    {
        if (EqualityComparer<TKey>.Default.Equals(value, default!))
            throw new ArgumentException("ID cannot be the default value.", nameof(value));

        if (value is string s && string.IsNullOrWhiteSpace(s))
            throw new ArgumentException("ID cannot be empty/whitespace.", nameof(value));

        return new(value);
    }
    
    public override string ToString() => Convert.ToString(Value, CultureInfo.InvariantCulture) ?? string.Empty;

    // Boxing for infra (single implementation satisfies both IEntityIdentifier and IPrimitiveValue,
    // since IEntityIdentifier extends IPrimitiveValue without redeclaring members).
    Type IPrimitiveValue.ValueType => typeof(TKey);

    object IPrimitiveValue.BoxedValue => Value;

    // Conversions
    public static implicit operator EntityIdentifier<TEntity, TKey>(TKey value) => Create(value);
    public static explicit operator TKey(EntityIdentifier<TEntity, TKey> identifier) => identifier.Value;

    public static EntityIdentifier<TEntity, TKey> Parse(string text) => TryParse(text, out EntityIdentifier<TEntity, TKey> id) 
        ? id 
        : throw new FormatException($"Invalid {typeof(TKey).Name}.");

    public static bool TryParse(string text, out EntityIdentifier<TEntity, TKey> identifier)
    {

        identifier = default;
        if (typeof(TKey) == typeof(Guid) && Guid.TryParse(text, out Guid g))
        {
            identifier = new((TKey)(object)g);
            return true;
        }
        if (typeof(TKey) == typeof(string))
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                identifier = new((TKey)(object)text);
                return true;
            }
            return false;
        }

        if (typeof(TKey) == typeof(int) && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
        {
            identifier = new((TKey)(object)i);
            return true;
        }
        if (typeof(TKey) == typeof(long) && long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l))
        {
            identifier = new((TKey)(object)l);
            return true;
        }
        if (typeof(TKey) == typeof(short) && short.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out short s))
        {
            identifier = new((TKey)(object)s);
            return true;
        }
        return false;

    }
    
}
