using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Utility.AuditLogging;

public class AuditLoggingDbContextOptionsExtension<TContext> 
    : IDbContextOptionsExtension where TContext 
    : DbContext
{
    private DbContextOptionsExtensionInfo? info;

    public virtual DbContextOptionsExtensionInfo Info => info ??= new AuditLoggingDbContextOptionsExtensionInfo(this);

    public virtual void AddOrUpdateExtraState(IDictionary<string, object> state)
    {
        state["AuditLogging:" + typeof(TContext).Name] = true;
    }

    public virtual void ApplyServices(IServiceCollection services)
    {
        services.AddScoped<IAuditLogger<TContext>, AuditLogger<TContext>>();
        services.AddScoped<IInterceptor, AuditSaveChangesInterceptor<TContext>>();
    }

    public virtual void Validate(IDbContextOptions options)
    {
        // Validation logic here
    }

    public virtual bool ShouldLog(EventId eventId, LogLevel logLevel)
    {
        // Default implementation - override in derived classes
        return logLevel >= LogLevel.Information;
    }

    public virtual void Log(EventData eventData)
    {
        // Default implementation - override in derived classes
    }

    private sealed class AuditLoggingDbContextOptionsExtensionInfo(AuditLoggingDbContextOptionsExtension<TContext> extension) 
        : DbContextOptionsExtensionInfo(extension)
    {
        private readonly AuditLoggingDbContextOptionsExtension<TContext> extension = extension;

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "Using Audit Logging";

        public override int GetServiceProviderHashCode() => 0;
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return false;
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo[$"{nameof(AuditLoggingDbContextOptionsExtension<TContext>)}:Enabled"] = "1";
        }
    }
}

// Interface for audit logging
public interface IAuditLogger<in TContext> where TContext 
    : DbContext
{
    void LogChanges(TContext context, IReadOnlyList<EntityEntry> entries);
}

// Default implementation
public class AuditLogger<TContext>(ILogger<AuditLogger<TContext>> logger) 
    : IAuditLogger<TContext> where TContext 
    : DbContext
{
    public void LogChanges(TContext context, IReadOnlyList<EntityEntry> entries)
    {
        // Implement your audit logging logic here
        logger.LogInformation("Audited {Count} changes for {Context}", entries.Count, typeof(TContext).Name);
    }
}

// Interceptor to capture SaveChanges events
public class AuditSaveChangesInterceptor<TContext>(IAuditLogger<TContext> auditLogger) 
    : SaveChangesInterceptor where TContext 
    : DbContext
{

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not TContext context)
        {
            return base.SavingChanges(eventData, result);
        }
        var entries = context.ChangeTracker.Entries().ToList();
        auditLogger.LogChanges(context, entries);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not TContext context)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var entries = context.ChangeTracker.Entries().ToList();
        auditLogger.LogChanges(context, entries);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

}