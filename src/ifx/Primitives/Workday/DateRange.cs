namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>A closed range of calendar dates; both endpoints are included.</summary>
public sealed record DateRange
{
    public DateOnly Start { get; }
    public DateOnly End { get; }

    [JsonConstructor]
    public DateRange(DateOnly start, DateOnly end)
    {
        if (end < start)
            throw new ArgumentException("End must be on or after start.", nameof(end));
        Start = start;
        End = end;
    }

    [JsonIgnore]
    public int Days => End.DayNumber - Start.DayNumber + 1;

    public bool Contains(DateOnly date) => date >= Start && date <= End;

    public bool Overlaps(DateRange other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Start <= other.End && other.Start <= End;
    }

    public override string ToString() =>
        $"{Start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}/{End.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
}
