using System.Globalization;

namespace VisionaryCoder.Framework.Primitives;

/// <summary>
/// Represents a positive whole-number quantity.
/// </summary>
/// <typeparam name="TOwner">The owner type that gives the quantity domain meaning.</typeparam>
public readonly struct PositiveQuantity<TOwner> : IEquatable<PositiveQuantity<TOwner>>, IPrimitiveValue<int>
    where TOwner : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PositiveQuantity{TOwner}"/> struct.
    /// </summary>
    /// <param name="value">The positive quantity value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than one.</exception>
    public PositiveQuantity(int value)
    {
        if (value < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Quantity value must be greater than zero.");
        }

        Value = value;
    }

    /// <inheritdoc />
    public int Value { get; }

    /// <inheritdoc />
    public Type ValueType => typeof(int);

    /// <inheritdoc />
    public object BoxedValue => Value;

    /// <summary>
    /// Creates a positive quantity.
    /// </summary>
    /// <param name="value">The positive quantity value.</param>
    /// <returns>The positive quantity.</returns>
    public static PositiveQuantity<TOwner> Create(int value) => new(value);

    /// <summary>
    /// Parses a positive quantity.
    /// </summary>
    /// <param name="text">The quantity text.</param>
    /// <returns>The parsed quantity.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="text"/> is not a positive integer.</exception>
    public static PositiveQuantity<TOwner> Parse(string? text)
    {
        if (TryParse(text, out PositiveQuantity<TOwner> value))
        {
            return value;
        }

        throw new FormatException("Invalid positive quantity value.");
    }

    /// <summary>
    /// Attempts to parse a positive quantity.
    /// </summary>
    /// <param name="text">The quantity text.</param>
    /// <param name="value">The parsed value when parsing succeeds.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? text, out PositiveQuantity<TOwner> value)
    {
        value = default;
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) || parsed < 1)
        {
            return false;
        }

        value = new PositiveQuantity<TOwner>(parsed);
        return true;
    }

    /// <summary>
    /// Converts the value object to its integer value.
    /// </summary>
    /// <param name="quantity">The positive quantity.</param>
    public static explicit operator int(PositiveQuantity<TOwner> quantity) => quantity.Value;

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public bool Equals(PositiveQuantity<TOwner> other) => Value == other.Value;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PositiveQuantity<TOwner> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Compares two quantities for equality.
    /// </summary>
    /// <param name="left">The left quantity.</param>
    /// <param name="right">The right quantity.</param>
    /// <returns><see langword="true"/> when both values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(PositiveQuantity<TOwner> left, PositiveQuantity<TOwner> right) => left.Equals(right);

    /// <summary>
    /// Compares two quantities for inequality.
    /// </summary>
    /// <param name="left">The left quantity.</param>
    /// <param name="right">The right quantity.</param>
    /// <returns><see langword="true"/> when both values differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(PositiveQuantity<TOwner> left, PositiveQuantity<TOwner> right) => !left.Equals(right);
}

