using Eyefinity.Utilities.AuditLogging.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Globalization;

namespace Eyefinity.Utilities.AuditLogging.Helpers;

internal static class PropertyEntryExtension
{
    /// <summary>
    /// Use simple string comparison to determine if this value has changed
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    internal static bool ValueChanged(this PropertyEntry source)
    {
        if ((source.OriginalValue == null && source.CurrentValue != null) ||
            (source.CurrentValue == null && source.OriginalValue != null))
        {
            return true;
        }
        else if (source.CurrentValue == null && source.OriginalValue == null)
        {
            return false;
        }
        
        return source.CurrentValue!.ToString() != source.OriginalValue!.ToString();
    }

    internal static EntityFieldAudit Convert(this PropertyEntry source, 
        bool updated, 
        string entityName, 
        string entityFullName,
        string tableName,
        string primaryKey, 
        DateTime timeStamp, 
        string operationType,
        Guid eventId)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return new EntityFieldAudit
        {
            OriginalValue = updated ? source.OriginalValue?.ToString() : null,
            CurrentValue = source.CurrentValue?.ToString(),
            PropertyName = source.Metadata.Name,
            EntityName = entityName,
            EntityTypeName = entityFullName,
            EntityKeyValue = primaryKey,
            TimeStamp = timeStamp,
            Operation = operationType,
            TableName = tableName,
            EventId = eventId
        };
    }

    /// <summary>
    /// Extracts the primary key or composite key from the list of property Entries
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    internal static string PrimaryKeyValue(this IEnumerable<PropertyEntry> source)
    {
        var keys = source.Where(property => property.Metadata.IsPrimaryKey() && !property.Metadata.IsForeignKey())
                        .ToList();

        if (keys.Count == 1)
        {
            return string.Format(CultureInfo.CurrentCulture,"{0}", keys.First().CurrentValue);
        }

        return string.Join(", ", keys.Select(n => string.Format(CultureInfo.CurrentCulture, "{0}:{1}",
            n.Metadata.Name, n.CurrentValue)).ToList());
    }
}
