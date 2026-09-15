using Ifx.Pipeline.Routing;

namespace Ifx.Pipeline.Abstractions;

public interface IServiceRegistry
{
    ServiceEntry? Lookup(Type requestType);
}