namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>
/// A range of date-times. <see cref="End"/> is inclusive for <see cref="Contains"/>.
/// </summary>
public sealed record DateTimeRange
{
    /// <summary>Initializes a new instance of the <see cref="DateTimeRange"/> record.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="end"/> is earlier than <paramref name="start"/>.</exception>
    public DateTimeRange(DateTime start, DateTime end)
    {
        if (end < start)
        {
            throw new ArgumentOutOfRangeException(nameof(end), end, "End must not be earlier than Start.");
        }

        Start = start;
        End = end;
    }

    /// <summary>Gets the start of the range.</summary>
    public DateTime Start { get; }

    /// <summary>Gets the end of the range.</summary>
    public DateTime End { get; }

    /// <summary>Gets the length of the range.</summary>
    public TimeSpan Duration => End - Start;

    /// <summary>Deconstructs the range into its start and end.</summary>
    public void Deconstruct(out DateTime start, out DateTime end) => (start, end) = (Start, End);

    /// <summary>Returns true when <paramref name="value"/> falls within the range, inclusive of both ends.</summary>
    public bool Contains(DateTime value) => value >= Start && value <= End;

    /// <summary>Returns true when the range ends before <paramref name="value"/>.</summary>
    public bool IsPast(DateTime value) => value > End;

    /// <summary>Returns true when the range starts after <paramref name="value"/>.</summary>
    public bool IsFuture(DateTime value) => value < Start;

    /// <summary>Returns true when the range has not yet ended as of <paramref name="value"/> (active or future).</summary>
    public bool IsCurrent(DateTime value) => !IsPast(value);

    /// <summary>Returns true when the two ranges share any time; ranges that only touch at an endpoint do not overlap.</summary>
    public bool Overlaps(DateTimeRange other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Start < other.End && End > other.Start;
    }
}
