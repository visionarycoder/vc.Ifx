namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>A percentage from 0 through 100 inclusive; 15 means 15 percent.</summary>
public sealed record Percentage
{
    public decimal Value { get; }

    [JsonConstructor]
    public Percentage(decimal value)
    {
        if (!IsValid(value))
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value is outside the supported range.");
        Value = value;
    }

    public static bool IsValid(decimal value) => value >= 0m && value <= 100m;

    public static bool TryCreate(decimal value, [NotNullWhen(true)] out Percentage? result)
    {
        result = IsValid(value) ? new(value) : null;
        return result is not null;
    }

    public Ratio ToRatio() => new(Value / 100m);
    public decimal ApplyTo(decimal value) => value * (Value / 100m);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    public static explicit operator decimal(Percentage value) => value.Value;
    public static explicit operator Percentage(decimal value) => new(value);
}
