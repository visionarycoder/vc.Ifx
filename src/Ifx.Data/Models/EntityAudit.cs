namespace Eyefinity.Utilities.AuditLogging.Models;

/// <summary>
/// High-level audit object without the field level details
/// </summary>
public sealed class EntityAudit
{
    /// <summary>
    /// Unique id for each audit event
    /// </summary>
    public Guid EventId { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Start time for the audit
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// End time for the audit
    /// </summary>
    public DateTime EndTime { get; set; }
    /// <summary>
    /// Count of entities affected during the update
    /// </summary>
    public int EntitiesAffectedCount { get; set; }
    /// <summary>
    /// Count of the properties affected by the audit
    /// </summary>
    public int PropertiesAffectedCount { get; set; }
    /// <summary>
    /// was the audit successful or not
    /// </summary>
    public bool Succeeded { get; set; }
    /// <summary>
    /// Collection of each fields that have changed
    /// </summary>
    public ICollection<EntityFieldAudit> Fields { get; set; } = new List<EntityFieldAudit>();
}