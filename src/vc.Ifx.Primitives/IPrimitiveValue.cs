namespace VisionaryCoder.Framework.Primitives;

/// <summary>
/// Exposes boxed primitive value metadata for infrastructure adapters.
/// </summary>
public interface IPrimitiveValue
{
    /// <summary>
    /// Gets the underlying primitive value type.
    /// </summary>
    Type ValueType { get; }

    /// <summary>
    /// Gets the boxed primitive value.
    /// </summary>
    object BoxedValue { get; }
}

