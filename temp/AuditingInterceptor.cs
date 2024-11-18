using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Eyefinity.Utility.AuditLogging;

public abstract class AuditingInterceptor : IInterceptor, IDbContextOptionsExtension
{

    private readonly Dictionary<string, object> stateCollection = new ();
    public DbContextOptionsExtensionInfo Info { get; } = new AuditLoggingDbContextOptionsExtensionInfo(new AuditLoggingDbContextOptionsExtension());

    public void AddOrUpdateExtraState(IDictionary<string, object> state)
    {
        
        foreach (var entry in state)
        {
            stateCollection[entry.Key] = entry.Value;
        }

    }

    public abstract void ApplyServices(IServiceCollection services);
    public abstract void Validate(IDbContextOptions options);
    public abstract void Log(EventData eventData);
    public abstract bool ShouldLog(EventId eventId, LogLevel logLevel);
}