using Eyefinity.Utilities.AuditLogging.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Eyefinity.Utilities.AuditLogging;

public class AuditLoggingInterceptor : ISaveChangesInterceptor
{
    private readonly ILogger logger;
    private IAuditLogger<DbContext> auditLogger;

    public AuditLoggingInterceptor(ILogger<AuditLoggingInterceptor> logger, IAuditLogger<DbContext>? auditLogger = null)
    {
        this.logger = logger;

        if (auditLogger != null)
        {
            this.auditLogger = auditLogger;
        }
        else
        {
            this.auditLogger = new AuditLogger(logger);
        }
    }

    internal IAuditLogger<DbContext> AuditLogger => this.auditLogger;

    public void ExcludeTables(ICollection<string> tablesToExclude)
    {
        this.auditLogger.SetExcludedEntities(tablesToExclude);
    }

    #region SavingChanges
    public async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await Task.Run(() => { 
            try
            {
                auditLogger.StartEntry(eventData.Context);
            }
            catch (Exception ex)
            {
                auditLogger.Failed();
                this.logger.ExceptionOccurred(ex, auditLogger.InstanceId, "Error occurred gather audit details", nameof(auditLogger.StartEntry));
            }
        }, cancellationToken).ConfigureAwait(true);
        return result;
    }

    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        try
        {
            auditLogger.StartEntry(eventData.Context);
        }
        catch (Exception ex) { 
            auditLogger.Failed();
            this.logger.ExceptionOccurred(ex, auditLogger.InstanceId, "Error occurred gather audit details", nameof(auditLogger.StartEntry));
        }
        return result;
    }
    #endregion

    #region SavedChanges
    public int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        auditLogger.EndEntry();
        return result;
    }

    public async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await Task.Run(() => auditLogger.EndEntry(), cancellationToken).ConfigureAwait(true);
        return result;
    }
    #endregion
}