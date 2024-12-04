using Microsoft.Extensions.Logging;

namespace vc.Ifx.Base;

public abstract class ServiceBase<T>(ILogger<T> logger)
{
    internal ILogger<T> logger = logger;
    public Guid Instance { get; } = Guid.NewGuid();

}