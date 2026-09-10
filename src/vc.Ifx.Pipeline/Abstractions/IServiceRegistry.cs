using VisionaryCoder.Framework.Pipeline.Routing;

namespace VisionaryCoder.Framework.Pipeline.Abstractions;

public interface IServiceRegistry
{
    ServiceEntry? Lookup(Type requestType);
}