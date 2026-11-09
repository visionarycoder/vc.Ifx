using Microsoft.Extensions.Logging.Abstractions;

namespace Ifx;

public class ServiceRequest : ComponentRequest
{
    public ServiceRequest() : base(NullLogger<ComponentRequest>.Instance)
    {
    }
}
