namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>An instant normalized to UTC, independent of the original offset.</summary>
public sealed record UtcTimestamp
{
    public DateTimeOffset Value { get; }

    [JsonConstructor]
    public UtcTimestamp(DateTimeOffset value) => Value = value.ToUniversalTime();

    public static UtcTimestamp Now(TimeProvider? clock = null) =>
        new((clock ?? TimeProvider.System).GetUtcNow());

    public UtcTimestamp Add(TimeSpan duration) => new(Value.Add(duration));
    public override string ToString() => Value.ToString("O", CultureInfo.InvariantCulture);
    public static explicit operator DateTimeOffset(UtcTimestamp value) => value.Value;
    public static explicit operator UtcTimestamp(DateTimeOffset value) => new(value);
}
