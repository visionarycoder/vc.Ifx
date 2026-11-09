namespace Ifx.Primitives;

/// <summary>
/// Represents an immutable connection string value.
/// </summary>
public sealed class ConnectionString : IEquatable<ConnectionString>
{
    /// <summary>
    /// Gets the connection string value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionString"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string value.</param>
    /// <exception cref="ArgumentNullException">Thrown if the value is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the value is empty or whitespace.</exception>
    public ConnectionString(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        Value = connectionString;
    }

    /// <summary>
    /// Returns the string representation of the connection string.
    /// </summary>
    /// <returns>The connection string value.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Determines whether this instance is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>true if the objects are equal; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is ConnectionString other && Equals(other);

    /// <summary>
    /// Determines whether this instance is equal to another <see cref="ConnectionString"/>.
    /// </summary>
    /// <param name="other">The connection string to compare with.</param>
    /// <returns>true if the connection strings are equal; otherwise, false.</returns>
    public bool Equals(ConnectionString? other) => other is not null && Value.Equals(other.Value, StringComparison.Ordinal);

    /// <summary>
    /// Returns the hash code for this connection string.
    /// </summary>
    /// <returns>The hash code for this connection string.</returns>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two connection strings are equal.
    /// </summary>
    /// <param name="left">The first connection string.</param>
    /// <param name="right">The second connection string.</param>
    /// <returns>true if the connection strings are equal; otherwise, false.</returns>
    public static bool operator ==(ConnectionString? left, ConnectionString? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two connection strings are not equal.
    /// </summary>
    /// <param name="left">The first connection string.</param>
    /// <param name="right">The second connection string.</param>
    /// <returns>true if the connection strings are not equal; otherwise, false.</returns>
    public static bool operator !=(ConnectionString? left, ConnectionString? right) => !(left == right);

    /// <summary>
    /// Creates a string from a connection string value.
    /// </summary>
    /// <param name="connectionString">The connection string value.</param>
    public static implicit operator string(ConnectionString connectionString) => connectionString.Value;

    /// <summary>
    /// Creates a connection string from a string value.
    /// </summary>
    /// <param name="connectionString">The connection string value.</param>
    /// <returns>A new connection string.</returns>
    public static explicit operator ConnectionString(string connectionString) => new(connectionString);
}
