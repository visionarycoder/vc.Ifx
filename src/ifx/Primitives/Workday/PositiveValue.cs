namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>A numeric value that is finite and greater than zero.</summary>
public sealed record PositiveValue<T> where T : struct, INumber<T>
{
    public T Value { get; }

    [JsonConstructor]
    public PositiveValue(T value)
    {
        if (!IsValid(value))
            throw new ArgumentOutOfRangeException(nameof(value), value, "Must be finite and greater than zero.");
        Value = value;
    }

    public static bool IsValid(T value) => T.IsFinite(value) && value > T.Zero;

    public static bool TryCreate(T value, [NotNullWhen(true)] out PositiveValue<T>? result)
    {
        result = IsValid(value) ? new(value) : null;
        return result is not null;
    }

    public override string ToString() => Value.ToString(null, CultureInfo.InvariantCulture);
    public static explicit operator T(PositiveValue<T> value) => value.Value;
    public static explicit operator PositiveValue<T>(T value) => new(value);
}
