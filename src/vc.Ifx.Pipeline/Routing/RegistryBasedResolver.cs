using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Routing;

/// <summary>Resolves the latest route, defaulting missing registrations to local dispatch.</summary>
public sealed class RegistryBasedResolver(IServiceRegistry registry) : IEndpointResolver
{
    private readonly IServiceRegistry registry = registry ?? throw new ArgumentNullException(nameof(registry));

    /// <inheritdoc />
    public EndpointResolution Resolve(Type requestType)
    {
        ArgumentNullException.ThrowIfNull(requestType);
        var entry = registry.Lookup(requestType);
        return entry is null || entry.IsLocal
            ? new EndpointResolution(true)
            : new EndpointResolution(false, entry.ServiceName, entry.EndpointUri);
    }
}
