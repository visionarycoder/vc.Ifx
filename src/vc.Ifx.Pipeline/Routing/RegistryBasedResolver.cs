using System.Collections.Concurrent;
using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Routing;

public sealed class RegistryBasedResolver(IServiceRegistry registry) : IEndpointResolver
{

    private readonly IServiceRegistry registry = registry ?? throw new ArgumentNullException(nameof(registry));
    private readonly ConcurrentDictionary<Type, EndpointResolution> cache = new();

    public EndpointResolution Resolve(Type requestType)
    {
        if (requestType == null)
            throw new ArgumentNullException(nameof(requestType));

        // Cache lookups for performance
        return cache.GetOrAdd(requestType, ResolveInternal);
    }

    private EndpointResolution ResolveInternal(Type requestType)
    {
        // Ask registry for service info
        ServiceEntry? entry = registry.Lookup(requestType);

        if (entry == null)
        {
            // Default: assume local if not registered
            return new EndpointResolution(IsLocal: true);
        }

        if (entry.IsLocal)
        {
            return new EndpointResolution(IsLocal: true);
        }

        // Remote resolution
        return new EndpointResolution(
            IsLocal: false,
            ServiceName: entry.ServiceName,
            Uri: entry.EndpointUri
        );
    }
}
