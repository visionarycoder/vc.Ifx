namespace VisionaryCoder.Framework.Pipeline.Routing;

/// <summary>Immutable service routing metadata.</summary>
public sealed class ServiceEntry
{
    /// <summary>Creates a named route with an absolute HTTP(S) endpoint.</summary>
    public ServiceEntry(string serviceName, Uri endpointUri, bool isLocal = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        ServiceName = serviceName;
        EndpointUri = PipelineGuard.HttpUri(endpointUri);
        IsLocal = isLocal;
    }

    /// <summary>Gets the service name.</summary>
    public string ServiceName { get; }
    /// <summary>Gets the endpoint URI.</summary>
    public Uri EndpointUri { get; }
    /// <summary>Gets whether dispatch remains in-process.</summary>
    public bool IsLocal { get; }
}
