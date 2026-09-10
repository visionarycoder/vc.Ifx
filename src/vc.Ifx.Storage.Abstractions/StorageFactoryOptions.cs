namespace VisionaryCoder.Framework.Storage;

/// <summary>
/// Configuration options for the storage factory.
/// </summary>
public sealed class StorageFactoryOptions
{
    
    private readonly Dictionary<string, StorageImplementation> implementations = new();

    /// <summary>
    /// Gets the registered storage implementations.
    /// </summary>
    public IReadOnlyDictionary<string, StorageImplementation> Implementations => implementations;

    /// <summary>
    /// Registers a storage implementation.
    /// </summary>
    /// <param name="name">The unique name for this implementation.</param>
    /// <param name="implementationType">The implementation type.</param>
    /// <param name="options">Optional configuration options for the implementation.</param>
    internal void RegisterImplementation(string name, Type implementationType, object? options = null)
    {
        implementations[name] = new StorageImplementation(implementationType, options);
    }

}
