using System.Text.RegularExpressions;
using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Routing;

/// <summary>Conventional DNS discovery in the default Kubernetes namespace.</summary>
public sealed class KubernetesDnsRegistry : IServiceRegistry
{
    /// <inheritdoc />
    public ServiceEntry? Lookup(Type requestType)
    {
        ArgumentNullException.ThrowIfNull(requestType);
        string name = requestType.Name;
        string serviceName = (name.EndsWith("Request", StringComparison.Ordinal) ? name[..^7] : name).ToLowerInvariant();
        if (!Regex.IsMatch(serviceName, "^[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?$", RegexOptions.CultureInvariant))
            throw new ArgumentException("The request type does not produce a valid DNS service label.", nameof(requestType));
        return new ServiceEntry(serviceName, new Uri($"http://{serviceName}.default.svc.cluster.local/api/dispatch"));
    }
}
