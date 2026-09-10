using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Routing;

public sealed class InMemoryServiceRegistry : IServiceRegistry
{
    private readonly Dictionary<Type, ServiceEntry> map = new();

    public void Register<TRequest>(ServiceEntry entry)
    {
        map[typeof(TRequest)] = entry;
    }

    public ServiceEntry? Lookup(Type requestType)
    {
        map.TryGetValue(requestType, out ServiceEntry? entry);
        return entry;
    }
}
