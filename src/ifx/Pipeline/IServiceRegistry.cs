using Ifx.Pipeline.Routing;

namespace Ifx.Pipeline;

public interface IServiceRegistry
{
    ServiceEntry? Lookup(Type requestType);
}