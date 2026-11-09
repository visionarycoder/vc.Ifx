namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>A numeric value that is finite and greater than or equal to zero.</summary>
public sealed record NonNegativeValue<T> where T : struct, INumber<T>
{
    public T Value { get; }

    [JsonConstructor]
    public NonNegativeValue(T value)
    {
        if (!IsValid(value))
            throw new ArgumentOutOfRangeException(nameof(value), value, "Must be finite and greater than or equal to zero.");
        Value = value;
    }

    public static bool IsValid(T value) => T.IsFinite(value) && value >= T.Zero;

    public static bool TryCreate(T value, [NotNullWhen(true)] out NonNegativeValue<T>? result)
    {
        result = IsValid(value) ? new(value) : null;
        return result is not null;
    }

    public override string ToString() => Value.ToString(null, CultureInfo.InvariantCulture);
    public static explicit operator T(NonNegativeValue<T> value) => value.Value;
    public static explicit operator NonNegativeValue<T>(T value) => new(value);
}
