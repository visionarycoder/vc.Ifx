namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>A case-sensitive application code, independent of HTTP status and display text.</summary>
public sealed record ResponseCode
{
    public string Value { get; }

    [JsonConstructor]
    public ResponseCode(string value)
    {
        if (!IsValid(value))
            throw new ArgumentException("Use 1–128 ASCII letters, digits, dots, hyphens or underscores; start with a letter.", nameof(value));
        Value = value;
    }

    public static bool IsValid(string? value) =>
        value is { Length: > 0 and <= 128 } &&
        char.IsAsciiLetter(value[0]) &&
        value.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '_');

    public static bool TryCreate(string? value, [NotNullWhen(true)] out ResponseCode? result)
    {
        result = IsValid(value) ? new(value!) : null;
        return result is not null;
    }

    public override string ToString() => Value;
    public static explicit operator string(ResponseCode value) => value.Value;
    public static explicit operator ResponseCode(string value) => new(value);
}
