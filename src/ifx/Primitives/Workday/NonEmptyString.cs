namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>Nonblank text; preserves original whitespace, case, and Unicode.</summary>
public sealed record NonEmptyString
{
    public string Value { get; }

    [JsonConstructor]
    public NonEmptyString(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public static bool TryCreate(string? value, [NotNullWhen(true)] out NonEmptyString? result)
    {
        result = string.IsNullOrWhiteSpace(value) ? null : new(value);
        return result is not null;
    }

    public override string ToString() => Value;
    public static explicit operator string(NonEmptyString value) => value.Value;
    public static explicit operator NonEmptyString(string value) => new(value);
}
