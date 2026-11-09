namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>A concrete network port. Zero (automatic binding) is deliberately excluded.</summary>
public sealed record PortNumber
{
    public int Value { get; }

    [JsonConstructor]
    public PortNumber(int value)
    {
        if (!IsValid(value))
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value is outside the supported range.");
        Value = value;
    }

    public static bool IsValid(int value) => value >= 1 && value <= 65535;

    public static bool TryCreate(int value, [NotNullWhen(true)] out PortNumber? result)
    {
        result = IsValid(value) ? new(value) : null;
        return result is not null;
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    public static explicit operator int(PortNumber value) => value.Value;
    public static explicit operator PortNumber(int value) => new(value);
}
