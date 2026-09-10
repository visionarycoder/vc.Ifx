namespace VisionaryCoder.Framework.Storage;

/// <summary>
/// Represents a registered storage implementation.
/// </summary>
/// <param name="ImplementationType">The type of the storage implementation.</param>
/// <param name="Options">Optional configuration options for the implementation.</param>
public sealed record StorageImplementation(Type ImplementationType, object? Options = null);