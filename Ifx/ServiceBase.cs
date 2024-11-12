using Microsoft.Extensions.Logging;

#pragma warning disable CS9113 // Parameter is unread.

namespace vc;

public abstract class ServiceBase<T>(ILogger<T> logger)
{
    public Guid Instance { get; } = Guid.NewGuid();

}