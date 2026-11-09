namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>A positive page size. Application-specific limits belong at the boundary.</summary>
public sealed record PageSize
{
    public int Value { get; }

    [JsonConstructor]
    public PageSize(int value)
    {
        if (!IsValid(value))
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value is outside the supported range.");
        Value = value;
    }

    public static bool IsValid(int value) => value >= 1 && value <= int.MaxValue;

    public static bool TryCreate(int value, [NotNullWhen(true)] out PageSize? result)
    {
        result = IsValid(value) ? new(value) : null;
        return result is not null;
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    public static explicit operator int(PageSize value) => value.Value;
    public static explicit operator PageSize(int value) => new(value);
}
