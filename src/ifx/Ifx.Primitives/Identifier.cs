using System.Globalization;
using Ifx.Abstractions;

namespace Ifx.Primitives;

/// <summary>
/// Represents a strongly typed non-empty GUID identity for the specified owner type.
/// </summary>
/// <typeparam name="TOwner">The owner type that gives the identity domain meaning.</typeparam>
public readonly struct Identifier<TOwner> : IEquatable<Identifier<TOwner>>, IPrimitiveValue<Guid>
    where TOwner : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Identifier{TOwner}"/> struct.
    /// </summary>
    /// <param name="value">The non-empty GUID value.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public Identifier(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identifier value cannot be empty.", nameof(value));
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
    public static Identifier<TOwner> Create(Guid value) => new(value);

    /// <summary>
    /// Creates a new identity from a randomly generated GUID (UUID v4).
    /// </summary>
    /// <remarks>
    /// Random values are unsuitable for SQL clustered primary keys because insert order has no
    /// relationship to key order, causing index fragmentation. Prefer <see cref="NewSequential"/>
    /// when the identity backs, or is stored in, a clustered index.
    /// </remarks>
    /// <returns>The generated strongly typed identity.</returns>
    public static Identifier<TOwner> New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a new identity from a time-ordered GUID (UUID v7, RFC 9562).
    /// </summary>
    /// <remarks>
    /// Sequential values append to the end of a clustered index in insert order, avoiding the
    /// page-split fragmentation caused by random (v4) GUIDs. Prefer this method when the identity
    /// backs, or is stored in, a SQL clustered primary key.
    /// </remarks>
    /// <returns>The generated strongly typed identity.</returns>
    public static Identifier<TOwner> NewSequential() => new(Guid.CreateVersion7());

    /// <summary>
    /// Parses a strongly typed identity from text.
    /// </summary>
    /// <param name="text">The identity text.</param>
    /// <returns>The parsed identity.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="text"/> is not a non-empty GUID.</exception>
    public static Identifier<TOwner> Parse(string? text)
    {
        if (TryParse(text, out Identifier<TOwner> identity))
        {
            return identity;
        }

        throw new FormatException("Invalid identity value.");
    }

    /// <summary>
    /// Attempts to parse a strongly typed identifier from text.
    /// </summary>
    /// <param name="text">The identifier text.</param>
    /// <param name="identifier">The parsed identifier when parsing succeeds.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? text, out Identifier<TOwner> identifier)
    {
        identifier = default;
        if (!Guid.TryParse(text, out Guid value) || value == Guid.Empty)
        {
            return false;
        }

        identifier = new Identifier<TOwner>(value);
        return true;
    }

    /// <summary>
    /// Converts an identifier to its GUID value.
    /// </summary>
    /// <param name="identifier">The strongly typed identifier.</param>
    public static explicit operator Guid(Identifier<TOwner> identifier) => identifier.Value;

    /// <inheritdoc />
    public override string ToString() => Value.ToString("D", CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public bool Equals(Identifier<TOwner> other) => Value.Equals(other.Value);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Identifier<TOwner> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Compares two identities for equality.
    /// </summary>
    /// <param name="left">The left identity.</param>
    /// <param name="right">The right identity.</param>
    /// <returns><see langword="true"/> when both values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Identifier<TOwner> left, Identifier<TOwner> right) => left.Equals(right);

    /// <summary>
    /// Compares two identities for inequality.
    /// </summary>
    /// <param name="left">The left identity.</param>
    /// <param name="right">The right identity.</param>
    /// <returns><see langword="true"/> when both values differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Identifier<TOwner> left, Identifier<TOwner> right) => !left.Equals(right);
    
}

