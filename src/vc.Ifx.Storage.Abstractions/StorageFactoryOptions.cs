using System.Collections.ObjectModel;

namespace VisionaryCoder.Framework.Storage;

/// <summary>
/// Configuration options for the storage factory.
/// </summary>
public sealed class StorageFactoryOptions
{
    private readonly Dictionary<string, StorageImplementation> implementations = new(StringComparer.Ordinal);

    /// <summary>Initializes an empty registration collection.</summary>
    public StorageFactoryOptions()
    {
        Implementations = new ReadOnlyDictionary<string, StorageImplementation>(implementations);
    }

    /// <summary>
    /// Gets a live, read-only view of the registered storage implementations.
    /// </summary>
    public IReadOnlyDictionary<string, StorageImplementation> Implementations { get; }

    /// <summary>Validates registration names and implementation types before activation.</summary>
    /// <remarks>
    /// Validation is explicit for compatibility with existing registration builders.
    /// Provider-specific options and constructor dependencies are validated by their owner.
    /// Queue and table registrations need not implement <see cref="IStorageProvider"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">A name is blank or a type is not a closed, concrete class.</exception>
    public void Validate()
    {
        foreach ((string name, StorageImplementation implementation) in implementations)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Storage registration names must not be blank.");
            }

            Type type = implementation.ImplementationType;
            if (!type.IsClass || type.IsAbstract || type.ContainsGenericParameters)
            {
                throw new InvalidOperationException($"Storage registration '{name}' requires a closed, concrete class.");
            }
        }
    }

    /// <summary>
    /// Registers a storage implementation.
    /// </summary>
    /// <param name="name">The unique name for this implementation.</param>
    /// <param name="implementationType">The implementation type.</param>
    /// <param name="options">Optional configuration options for the implementation.</param>
    internal void RegisterImplementation(string name, Type implementationType, object? options = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(implementationType);
        implementations[name] = new StorageImplementation(implementationType, options);
    }

}
