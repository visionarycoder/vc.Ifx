using System.Globalization;

namespace VisionaryCoder.Framework.Primitives;

/// <summary>
/// Represents a percentage value between zero and one hundred.
/// </summary>
/// <typeparam name="TOwner">The owner type that gives the percentage domain meaning.</typeparam>
public readonly struct Percentage<TOwner> : IEquatable<Percentage<TOwner>>, IPrimitiveValue<decimal>
    where TOwner : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Percentage{TOwner}"/> struct.
    /// </summary>
    /// <param name="value">The percentage value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is outside zero to one hundred.</exception>
    public Percentage(decimal value)
    {
        if (value is < 0m or > 100m)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Percentage value must be between 0 and 100.");
        }

        Value = value;
    }

    /// <inheritdoc />
    public decimal Value { get; }

    /// <inheritdoc />
    public Type ValueType => typeof(decimal);

    /// <inheritdoc />
    public object BoxedValue => Value;

    /// <summary>
    /// Creates a bounded percentage.
    /// </summary>
    /// <param name="value">The percentage value.</param>
    /// <returns>The bounded percentage.</returns>
    public static Percentage<TOwner> Create(decimal value) => new(value);

    /// <summary>
    /// Parses a bounded percentage.
    /// </summary>
    /// <param name="text">The percentage text.</param>
    /// <returns>The parsed percentage.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="text"/> is not between zero and one hundred.</exception>
    public static Percentage<TOwner> Parse(string? text)
    {
        if (TryParse(text, out Percentage<TOwner> value))
        {
            return value;
        }

        throw new FormatException("Invalid percentage value.");
    }

    /// <summary>
    /// Attempts to parse a bounded percentage.
    /// </summary>
    /// <param name="text">The percentage text.</param>
    /// <param name="value">The parsed value when parsing succeeds.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? text, out Percentage<TOwner> value)
    {
        value = default;
        if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsed) || parsed is < 0m or > 100m)
        {
            return false;
        }

        value = new Percentage<TOwner>(parsed);
        return true;
    }

    /// <summary>
    /// Converts the value object to its decimal value.
    /// </summary>
    /// <param name="percentage">The percentage value.</param>
    public static explicit operator decimal(Percentage<TOwner> percentage) => percentage.Value;

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public bool Equals(Percentage<TOwner> other) => Value == other.Value;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Percentage<TOwner> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Compares two percentages for equality.
    /// </summary>
    /// <param name="left">The left percentage.</param>
    /// <param name="right">The right percentage.</param>
    /// <returns><see langword="true"/> when both values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Percentage<TOwner> left, Percentage<TOwner> right) => left.Equals(right);

    /// <summary>
    /// Compares two percentages for inequality.
    /// </summary>
    /// <param name="left">The left percentage.</param>
    /// <param name="right">The right percentage.</param>
    /// <returns><see langword="true"/> when both values differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Percentage<TOwner> left, Percentage<TOwner> right) => !left.Equals(right);
}
