namespace VisionaryCoder.Framework.Pipeline.Routing;

public sealed class ServiceEntry(string serviceName, Uri endpointUri, bool isLocal = false)
{
    public string ServiceName { get; } = serviceName;
    public Uri EndpointUri { get; } = endpointUri;
    public bool IsLocal { get; } = isLocal;
}