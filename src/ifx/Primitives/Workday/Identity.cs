namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>A nonempty GUID whose marker type identifies its domain.</summary>
public sealed record Identity<T>
{
    public Guid Value { get; }

    [JsonConstructor]
    public Identity(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Identity cannot be empty.", nameof(value));
        Value = value;
    }

    public static Identity<T> New() => new(Guid.CreateVersion7());
    public static Identity<T> Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string? value, [NotNullWhen(true)] out Identity<T>? result)
    {
        result = Guid.TryParse(value, out var id) && id != Guid.Empty ? new(id) : null;
        return result is not null;
    }

    public override string ToString() => Value.ToString("D");
    public static explicit operator Guid(Identity<T> value) => value.Value;
    public static explicit operator Identity<T>(Guid value) => new(value);
}
