namespace VisionaryCoder.Framework.Primitives;

/// <summary>
/// Exposes a strongly typed primitive value.
/// </summary>
/// <typeparam name="TValue">The underlying primitive value type.</typeparam>
public interface IPrimitiveValue<out TValue> : IPrimitiveValue
    where TValue : notnull
{
    /// <summary>
    /// Gets the underlying primitive value.
    /// </summary>
    TValue Value { get; }
}

