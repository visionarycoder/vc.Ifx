using Eyefinity.Utilities.AuditLogging.Helpers;
using Eyefinity.Utilities.AuditLogging.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Eyefinity.Utilities.AuditLogging.UnitTest")]
namespace Eyefinity.Utilities.AuditLogging
{
    public class AuditLogger : IAuditLogger<DbContext>
    {
        private readonly Guid instanceId = Guid.NewGuid();
        private readonly ILogger logger;
        private EntityAudit audit = new();
        private List<EntityFieldAudit> fieldAuditEntries = new List<EntityFieldAudit>();
        private List<string> excludedTables = new List<string>();

        public AuditLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public Guid InstanceId => instanceId;

        public void Failed()
        {
            audit.Succeeded = false;
        }

        internal ICollection<string> ExcludedEntities => this.excludedTables;
        internal EntityAudit EntityAudit => this.audit;
        internal ICollection<EntityFieldAudit> FieldAuditEntries => this.fieldAuditEntries;

        public void SetExcludedEntities(ICollection<string> excludedEntities)
        {
            excludedTables = excludedEntities?.ToList() ?? new List<string>();
            logger.Message(instanceId, $"Excluding Tables {string.Join(",", excludedTables)}");
        }

        public void StartEntry(DbContext? context)
        {
            if (context is null)
            {
                logger.NullArgument(instanceId, "Start Entry called with null DbContext");
                return;
            }

            fieldAuditEntries = new List<EntityFieldAudit>();
            audit = new EntityAudit
            {
                EventId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow,
                Succeeded = true
            };

            context.ChangeTracker.DetectChanges();
            var entriesAffected = context.ChangeTracker.Entries();
            var entities = new HashSet<string>();

            foreach (var entry in entriesAffected)
            {
                var entityType = entry.EntityType();
                if (excludedTables.Contains(entityType.GetTableName()))
                {
                    continue;
                }

                var fieldAuditList = entry.State switch
                {
                    EntityState.Deleted => entry.EntityFieldAuditList(audit.EventId, entityType),
                    EntityState.Modified => entry.EntityFieldAuditList(audit.EventId, entityType),
                    EntityState.Added => entry.EntityFieldAuditList(audit.EventId, entityType),
                    _ => null
                };

                if (fieldAuditList is null)
                {
                    continue;
                }

                entities.Add(entry.Metadata.Name);
                fieldAuditEntries.AddRange(fieldAuditList);
            }

            audit.PropertiesAffectedCount = fieldAuditEntries.Count;
            audit.EntitiesAffectedCount = entities.Count;
           
        }
        
        public void EndEntry()
        {
            audit.EndTime = DateTime.UtcNow;
            LogAudit();
        }

        internal void LogAudit([CallerMemberName] string callerMemberName = "Unknown Caller")
        {
            var entries = FieldAuditEntries;
            audit.PropertiesAffectedCount = entries.Count;
            audit.Fields = entries;

            logger.LogAudit(instanceId, audit.EventId, audit, callerMemberName);

            foreach (var item in entries)
            {
                logger.LogFieldAudit(instanceId, audit.EventId, item, callerMemberName);
            }
        }
    }
}
