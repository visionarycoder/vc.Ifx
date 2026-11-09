namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>A normalized fraction from 0 through 1 inclusive.</summary>
public sealed record Ratio
{
    public decimal Value { get; }

    [JsonConstructor]
    public Ratio(decimal value)
    {
        if (!IsValid(value))
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value is outside the supported range.");
        Value = value;
    }

    public static bool IsValid(decimal value) => value >= 0m && value <= 1m;

    public static bool TryCreate(decimal value, [NotNullWhen(true)] out Ratio? result)
    {
        result = IsValid(value) ? new(value) : null;
        return result is not null;
    }

    public Percentage ToPercentage() => new(Value * 100m);
    public decimal ApplyTo(decimal value) => value * Value;

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    public static explicit operator decimal(Ratio value) => value.Value;
    public static explicit operator Ratio(decimal value) => new(value);
}
