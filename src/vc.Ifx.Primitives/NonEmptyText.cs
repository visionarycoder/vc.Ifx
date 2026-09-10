namespace VisionaryCoder.Framework.Primitives;

/// <summary>
/// Represents trimmed text that cannot be null, empty, or whitespace.
/// </summary>
/// <typeparam name="TOwner">The owner type that gives the text domain meaning.</typeparam>
public readonly struct NonEmptyText<TOwner> : IEquatable<NonEmptyText<TOwner>>, IPrimitiveValue<string>
    where TOwner : class
{
    private readonly string? value;

    /// <summary>
    /// Initializes a new instance of the <see cref="NonEmptyText{TOwner}"/> struct.
    /// </summary>
    /// <param name="value">The text value.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null, empty, or whitespace.</exception>
    public NonEmptyText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Text value cannot be empty or whitespace.", nameof(value));
        }

        this.value = value.Trim();
    }

    /// <inheritdoc />
    public string Value => value ?? string.Empty;

    /// <inheritdoc />
    public Type ValueType => typeof(string);

    /// <inheritdoc />
    public object BoxedValue => Value;

    /// <summary>
    /// Creates a non-empty text value.
    /// </summary>
    /// <param name="value">The text value.</param>
    /// <returns>The non-empty text value.</returns>
    public static NonEmptyText<TOwner> Create(string? value) => new(value);

    /// <summary>
    /// Parses a non-empty text value.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <returns>The parsed text value.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="text"/> is null, empty, or whitespace.</exception>
    public static NonEmptyText<TOwner> Parse(string? text)
    {
        if (TryParse(text, out NonEmptyText<TOwner> value))
        {
            return value;
        }

        throw new FormatException("Invalid non-empty text value.");
    }

    /// <summary>
    /// Attempts to parse a non-empty text value.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="value">The parsed value when parsing succeeds.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? text, out NonEmptyText<TOwner> value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        value = new NonEmptyText<TOwner>(text);
        return true;
    }

    /// <summary>
    /// Converts the value object to its string value.
    /// </summary>
    /// <param name="text">The non-empty text value.</param>
    public static explicit operator string(NonEmptyText<TOwner> text) => text.Value;

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    public bool Equals(NonEmptyText<TOwner> other) => StringComparer.Ordinal.Equals(Value, other.Value);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is NonEmptyText<TOwner> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>
    /// Compares two text values for equality.
    /// </summary>
    /// <param name="left">The left text value.</param>
    /// <param name="right">The right text value.</param>
    /// <returns><see langword="true"/> when both values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(NonEmptyText<TOwner> left, NonEmptyText<TOwner> right) => left.Equals(right);

    /// <summary>
    /// Compares two text values for inequality.
    /// </summary>
    /// <param name="left">The left text value.</param>
    /// <param name="right">The right text value.</param>
    /// <returns><see langword="true"/> when both values differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(NonEmptyText<TOwner> left, NonEmptyText<TOwner> right) => !left.Equals(right);
}
