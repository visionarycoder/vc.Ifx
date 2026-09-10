using System.Text.RegularExpressions;

namespace VisionaryCoder.Framework.Primitives;

/// <summary>
/// Represents an uppercase domain code with safe identifier characters.
/// </summary>
/// <typeparam name="TOwner">The owner type that gives the code domain meaning.</typeparam>
public readonly struct Code<TOwner> : IEquatable<Code<TOwner>>, IPrimitiveValue<string>
    where TOwner : class
{
    private static readonly Regex CodePattern = new("^[A-Z0-9][A-Z0-9._-]*$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private readonly string? value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Code{TOwner}"/> struct.
    /// </summary>
    /// <param name="value">The code value.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not a valid code.</exception>
    public Code(string? value)
    {
        string normalized = Normalize(value);
        if (!CodePattern.IsMatch(normalized))
        {
            throw new ArgumentException("Code value must start with an uppercase letter or digit and contain only uppercase letters, digits, '.', '_', or '-'.", nameof(value));
        }

        this.value = normalized;
    }

    /// <inheritdoc />
    public string Value => value ?? string.Empty;

    /// <inheritdoc />
    public Type ValueType => typeof(string);

    /// <inheritdoc />
    public object BoxedValue => Value;

    /// <summary>
    /// Creates a normalized code value.
    /// </summary>
    /// <param name="value">The code value.</param>
    /// <returns>The normalized code value.</returns>
    public static Code<TOwner> Create(string? value) => new(value);

    /// <summary>
    /// Parses a normalized code value.
    /// </summary>
    /// <param name="text">The code text.</param>
    /// <returns>The parsed code value.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="text"/> is not a valid code.</exception>
    public static Code<TOwner> Parse(string? text)
    {
        if (TryParse(text, out Code<TOwner> value))
        {
            return value;
        }

        throw new FormatException("Invalid code value.");
    }

    /// <summary>
    /// Attempts to parse a normalized code value.
    /// </summary>
    /// <param name="text">The code text.</param>
    /// <param name="value">The parsed value when parsing succeeds.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? text, out Code<TOwner> value)
    {
        value = default;
        string normalized = Normalize(text);
        if (!CodePattern.IsMatch(normalized))
        {
            return false;
        }

        value = new Code<TOwner>(normalized);
        return true;
    }

    /// <summary>
    /// Converts the value object to its string value.
    /// </summary>
    /// <param name="code">The code value.</param>
    public static explicit operator string(Code<TOwner> code) => code.Value;

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    public bool Equals(Code<TOwner> other) => StringComparer.Ordinal.Equals(Value, other.Value);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Code<TOwner> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>
    /// Compares two code values for equality.
    /// </summary>
    /// <param name="left">The left code value.</param>
    /// <param name="right">The right code value.</param>
    /// <returns><see langword="true"/> when both values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Code<TOwner> left, Code<TOwner> right) => left.Equals(right);

    /// <summary>
    /// Compares two code values for inequality.
    /// </summary>
    /// <param name="left">The left code value.</param>
    /// <param name="right">The right code value.</param>
    /// <returns><see langword="true"/> when both values differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Code<TOwner> left, Code<TOwner> right) => !left.Equals(right);

    private static string Normalize(string? value) => value?.Trim().ToUpperInvariant() ?? string.Empty;
}
