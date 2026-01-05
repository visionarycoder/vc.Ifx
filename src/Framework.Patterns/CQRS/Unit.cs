namespace VisionaryCoder.Framework.Patterns.CQRS;

/// <summary>
/// Represents a void response for commands that don't return a value.
/// Similar to MediatR's Unit type but framework-native.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// Default and only value of the Unit type.
    /// </summary>
    public static readonly Unit Value = new();

    /// <inheritdoc />
    public bool Equals(Unit other) => true;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Unit;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Unit left, Unit right) => false;

    /// <inheritdoc />
    public override string ToString() => "()";
}