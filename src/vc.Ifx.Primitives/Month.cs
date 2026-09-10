using System.Globalization;

namespace VisionaryCoder.Framework.Primitives;

/// <summary>
/// Represents a calendar month in a specific year.
/// </summary>
public sealed class Month : IEquatable<Month>, IPrimitiveValue<int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Month"/> class.
    /// </summary>
    /// <param name="year">The calendar year.</param>
    /// <param name="number">The one-based month number.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="year"/> or <paramref name="number"/> is outside the supported range.</exception>
    public Month(int year, int number)
    {
        if (year < 1 || year > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year), year, "Year value must be between 1 and 9999.");
        }

        if (number < 1 || number > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(number), number, "Month value must be between 1 and 12.");
        }

        Year = year;
        Number = number;
    }

    /// <summary>
    /// Gets the calendar year.
    /// </summary>
    public int Year { get; }

    /// <summary>
    /// Gets the one-based month number.
    /// </summary>
    public int Number { get; }

    /// <inheritdoc />
    public int Value => (Year * 100) + Number;

    /// <inheritdoc />
    public Type ValueType => typeof(int);

    /// <inheritdoc />
    public object BoxedValue => Value;

    /// <summary>
    /// Gets the first date in the month.
    /// </summary>
    public DateOnly FirstDay => new(Year, Number, 1);

    /// <summary>
    /// Gets the last date in the month.
    /// </summary>
    public DateOnly LastDay => new(Year, Number, DateTime.DaysInMonth(Year, Number));

    /// <summary>
    /// Creates a month from a year and month number.
    /// </summary>
    /// <param name="year">The calendar year.</param>
    /// <param name="number">The one-based month number.</param>
    /// <returns>The calendar month.</returns>
    public static Month Create(int year, int number) => new(year, number);

    /// <summary>
    /// Creates a month from a date.
    /// </summary>
    /// <param name="date">The source date.</param>
    /// <returns>The calendar month that contains the date.</returns>
    public static Month FromDate(DateOnly date) => new(date.Year, date.Month);

    /// <summary>
    /// Parses a month from text formatted as <c>yyyy-MM</c>.
    /// </summary>
    /// <param name="text">The month text.</param>
    /// <returns>The parsed calendar month.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="text"/> is not formatted as a calendar month.</exception>
    public static Month Parse(string? text)
    {
        if (TryParse(text, out Month? month) && month is not null)
        {
            return month;
        }

        throw new FormatException("Invalid month value.");
    }

    /// <summary>
    /// Attempts to parse a month from text formatted as <c>yyyy-MM</c>.
    /// </summary>
    /// <param name="text">The month text.</param>
    /// <param name="month">The parsed month when parsing succeeds.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? text, out Month? month)
    {
        month = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string[] parts = text.Trim().Split('-', StringSplitOptions.None);
        if (parts.Length != 2 || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out int year) || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int number))
        {
            return false;
        }

        if (year < 1 || year > 9999 || number < 1 || number > 12)
        {
            return false;
        }

        month = new Month(year, number);
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Year:0000}-{Number:00}");

    /// <inheritdoc />
    public bool Equals(Month? other) => other is not null && Year == other.Year && Number == other.Number;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Month other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Year, Number);

    /// <summary>
    /// Compares two month values for equality.
    /// </summary>
    /// <param name="left">The left month value.</param>
    /// <param name="right">The right month value.</param>
    /// <returns><see langword="true"/> when both values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Month? left, Month? right) => left is null ? right is null : left.Equals(right);

    /// <summary>
    /// Compares two month values for inequality.
    /// </summary>
    /// <param name="left">The left month value.</param>
    /// <param name="right">The right month value.</param>
    /// <returns><see langword="true"/> when both values differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Month? left, Month? right) => !(left == right);
}
