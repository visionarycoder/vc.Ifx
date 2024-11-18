using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Eyefinity.Utility.AuditLogging;

public abstract class AuditLoggingDbContextOptionsExtension<TContext> : IDbContextOptionsExtension where TContext : DbContext
{

    public abstract DbContextOptionsExtensionInfo Info { get; }
    public abstract void AddOrUpdateExtraState(IDictionary<string, object> state);
    public abstract void ApplyServices(IServiceCollection services);
    public abstract void Validate(IDbContextOptions options);
    public abstract void Log(EventData eventData);
    public abstract bool ShouldLog(EventId eventId, LogLevel logLevel);

}

public abstract class AuditLoggingDbContextOptionsExtensionInfo<TContext> : DbContextOptionsExtensionInfo<TContext>
{

    protected AuditLoggingDbContextOptionsExtensionInfo(IDbContextOptionsExtension extension)
        : base(extension)
    {
    }

    public override bool IsDatabaseProvider => false;

    public override string LogFragment => "Using Audit Logging";

    public override int GetServiceProviderHashCode() => base.GetHashCode();

    public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
    {
        debugInfo[$"{GetType().FullName}:" + nameof(AuditLoggingDbContextOptionsExtensionInfo<TContext>)] = "1";
    }

}