using System.Globalization;

namespace VisionaryCoder.Framework.Primitives;

/// <summary>
/// Represents a decimal amount that cannot be negative.
/// </summary>
/// <typeparam name="TOwner">The owner type that gives the amount domain meaning.</typeparam>
public readonly struct NonNegativeAmount<TOwner> : IEquatable<NonNegativeAmount<TOwner>>, IPrimitiveValue<decimal>
    where TOwner : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NonNegativeAmount{TOwner}"/> struct.
    /// </summary>
    /// <param name="value">The non-negative amount value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is negative.</exception>
    public NonNegativeAmount(decimal value)
    {
        if (value < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Amount value cannot be negative.");
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
    /// Creates a non-negative amount.
    /// </summary>
    /// <param name="value">The non-negative amount value.</param>
    /// <returns>The non-negative amount.</returns>
    public static NonNegativeAmount<TOwner> Create(decimal value) => new(value);

    /// <summary>
    /// Parses a non-negative amount.
    /// </summary>
    /// <param name="text">The amount text.</param>
    /// <returns>The parsed amount.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="text"/> is not a non-negative decimal.</exception>
    public static NonNegativeAmount<TOwner> Parse(string? text)
    {
        if (TryParse(text, out NonNegativeAmount<TOwner> value))
        {
            return value;
        }

        throw new FormatException("Invalid non-negative amount value.");
    }

    /// <summary>
    /// Attempts to parse a non-negative amount.
    /// </summary>
    /// <param name="text">The amount text.</param>
    /// <param name="value">The parsed value when parsing succeeds.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? text, out NonNegativeAmount<TOwner> value)
    {
        value = default;
        if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsed) || parsed < 0m)
        {
            return false;
        }

        value = new NonNegativeAmount<TOwner>(parsed);
        return true;
    }

    /// <summary>
    /// Converts the value object to its decimal value.
    /// </summary>
    /// <param name="amount">The non-negative amount.</param>
    public static explicit operator decimal(NonNegativeAmount<TOwner> amount) => amount.Value;

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public bool Equals(NonNegativeAmount<TOwner> other) => Value == other.Value;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is NonNegativeAmount<TOwner> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Compares two amounts for equality.
    /// </summary>
    /// <param name="left">The left amount.</param>
    /// <param name="right">The right amount.</param>
    /// <returns><see langword="true"/> when both values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(NonNegativeAmount<TOwner> left, NonNegativeAmount<TOwner> right) => left.Equals(right);

    /// <summary>
    /// Compares two amounts for inequality.
    /// </summary>
    /// <param name="left">The left amount.</param>
    /// <param name="right">The right amount.</param>
    /// <returns><see langword="true"/> when both values differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(NonNegativeAmount<TOwner> left, NonNegativeAmount<TOwner> right) => !left.Equals(right);
}

