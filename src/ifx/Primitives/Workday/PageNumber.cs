namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>A one-based page number.</summary>
public sealed record PageNumber
{
    public int Value { get; }

    [JsonConstructor]
    public PageNumber(int value)
    {
        if (!IsValid(value))
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value is outside the supported range.");
        Value = value;
    }

    public static bool IsValid(int value) => value >= 1 && value <= int.MaxValue;

    public static bool TryCreate(int value, [NotNullWhen(true)] out PageNumber? result)
    {
        result = IsValid(value) ? new(value) : null;
        return result is not null;
    }

    public long GetOffset(PageSize size)
    {
        ArgumentNullException.ThrowIfNull(size);
        return ((long)Value - 1) * size.Value;
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    public static explicit operator int(PageNumber value) => value.Value;
    public static explicit operator PageNumber(int value) => new(value);
}
