using System.Globalization;

namespace VisionaryCoder.Framework.Primitives;

/// <summary>
/// Represents a strongly typed non-empty GUID identity for the specified owner type.
/// </summary>
/// <typeparam name="TOwner">The owner type that gives the identity domain meaning.</typeparam>
public readonly struct Identity<TOwner> : IEquatable<Identity<TOwner>>, IPrimitiveValue<Guid>
    where TOwner : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Identity{TOwner}"/> struct.
    /// </summary>
    /// <param name="value">The non-empty GUID value.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public Identity(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identity value cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <inheritdoc />
    public Guid Value { get; }

    /// <inheritdoc />
    public Type ValueType => typeof(Guid);

    /// <inheritdoc />
    public object BoxedValue => Value;

    /// <summary>
    /// Creates a new identity from a GUID.
    /// </summary>
    /// <param name="value">The non-empty GUID value.</param>
    /// <returns>The strongly typed identity.</returns>
    public static Identity<TOwner> Create(Guid value) => new(value);

    /// <summary>
    /// Creates a new identity from a generated GUID.
    /// </summary>
    /// <returns>The generated strongly typed identity.</returns>
    public static Identity<TOwner> New() => new(Guid.NewGuid());

    /// <summary>
    /// Parses a strongly typed identity from text.
    /// </summary>
    /// <param name="text">The identity text.</param>
    /// <returns>The parsed identity.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="text"/> is not a non-empty GUID.</exception>
    public static Identity<TOwner> Parse(string? text)
    {
        if (TryParse(text, out Identity<TOwner> identity))
        {
            return identity;
        }

        throw new FormatException("Invalid identity value.");
    }

    /// <summary>
    /// Attempts to parse a strongly typed identity from text.
    /// </summary>
    /// <param name="text">The identity text.</param>
    /// <param name="identity">The parsed identity when parsing succeeds.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? text, out Identity<TOwner> identity)
    {
        identity = default;
        if (!Guid.TryParse(text, out Guid value) || value == Guid.Empty)
        {
            return false;
        }

        identity = new Identity<TOwner>(value);
        return true;
    }

    /// <summary>
    /// Converts an identity to its GUID value.
    /// </summary>
    /// <param name="identity">The strongly typed identity.</param>
    public static explicit operator Guid(Identity<TOwner> identity) => identity.Value;

    /// <inheritdoc />
    public override string ToString() => Value.ToString("D", CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public bool Equals(Identity<TOwner> other) => Value.Equals(other.Value);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Identity<TOwner> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Compares two identities for equality.
    /// </summary>
    /// <param name="left">The left identity.</param>
    /// <param name="right">The right identity.</param>
    /// <returns><see langword="true"/> when both values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Identity<TOwner> left, Identity<TOwner> right) => left.Equals(right);

    /// <summary>
    /// Compares two identities for inequality.
    /// </summary>
    /// <param name="left">The left identity.</param>
    /// <param name="right">The right identity.</param>
    /// <returns><see langword="true"/> when both values differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Identity<TOwner> left, Identity<TOwner> right) => !left.Equals(right);
}

