using Eyefinity.Utilities.AuditLogging.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Eyefinity.Utilities.AuditLogging.Helpers;

internal static class EntityEntryExtension
{
    /// <summary>
    /// Returns the entity type for the EntityEntry
    /// this will then be used to get the table name
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    internal static IEntityType? EntityType(this EntityEntry source)
    {
        return source.Context.Model.FindEntityType(source.Metadata.Name);
    }

    /// <summary>
    /// Generates the list of EntityFieldAudit objects. one per property changed
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<EntityFieldAudit> EntityFieldAuditList(this EntityEntry source, Guid eventId, IEntityType? entityType = null)
    {
        var added = source.State == EntityState.Added;
        var updated = source.State == EntityState.Modified;
        var deleted = source.State == EntityState.Deleted;
        
        if (!added && !updated && !deleted) 
        {
            return new List<EntityFieldAudit>();
        }

        var propertiesToInclude = source
            .Properties
            .Where(property => !updated || (property.IsModified && property.ValueChanged()))
            .Where(property => !added || !property.IsTemporary)
            .Where(property => !deleted)
            .ToList();

        var primaryKey = source.Properties.PrimaryKeyValue();
        var entityName = source.Metadata.DisplayName();
        var entityFullName = source.Metadata.Name;
        var tableName = entityType?.GetTableName() ?? entityName;
        var operationType = updated ? "Update" : (added ? "Insert" : "Delete");
        DateTime timeStamp = DateTime.UtcNow;

        return propertiesToInclude
                .Select(n => n.Convert(updated, entityName, entityFullName, tableName, primaryKey, timeStamp, operationType, eventId)).ToList();
    }

}
