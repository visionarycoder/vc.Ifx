using System.Collections.Concurrent;
using Ifx.Pipeline.Abstractions;

namespace Ifx.Pipeline.Routing;

/// <summary>Concurrent exact-type registration with replacement semantics.</summary>
public sealed class InMemoryServiceRegistry : IServiceRegistry
{
    private readonly ConcurrentDictionary<Type, ServiceEntry> map = new();

    /// <summary>Registers or replaces one request route.</summary>
    public void Register<TRequest>(ServiceEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        map[typeof(TRequest)] = entry;
    }

    /// <inheritdoc />
    public ServiceEntry? Lookup(Type requestType)
    {
        ArgumentNullException.ThrowIfNull(requestType);
        map.TryGetValue(requestType, out var entry);
        return entry;
    }
}
