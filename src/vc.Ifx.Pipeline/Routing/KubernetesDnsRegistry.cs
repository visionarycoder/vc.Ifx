using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Routing;

public sealed class KubernetesDnsRegistry : IServiceRegistry
{
    public ServiceEntry? Lookup(Type requestType)
    {
        string serviceName = requestType.Name.Replace("Request", "").ToLowerInvariant();
        var uri = new Uri($"http://{serviceName}.default.svc.cluster.local/api/dispatch");
        return new ServiceEntry(serviceName, uri, isLocal: false);
    }
}