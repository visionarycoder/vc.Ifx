namespace Eyefinity.Utilities.AuditLogging.Models
{
    /// <summary>
    /// Audit details for a single field in a table
    /// </summary>
    public class EntityFieldAudit
    {
        /// <summary>
        /// Name of the property that changed.
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;
        /// <summary>
        /// Current/New value for this property
        /// </summary>
        public string? CurrentValue { get; set; }
        /// <summary>
        /// The value before the change
        /// </summary>
        public string? OriginalValue { get; set; }
        /// <summary>
        /// The primary key value for the entity row
        /// </summary>
        /// <remarks>This could be more than 1 property for the key, which wil be Property:Value,Property:Value format</remarks>
        public string EntityKeyValue { get; set; } = string.Empty;
        /// <summary>
        /// Name of the entity as defined in the DbContext code
        /// </summary>
        public string EntityName { get; set; } = string.Empty;
        /// <summary>
        /// Full EntityName with namespace
        /// </summary>
        public string EntityTypeName { get; set; } = string.Empty;
        /// <summary>
        /// The table name that the Entity maps to
        /// </summary>
        public string TableName { get; set; } = string.Empty;
        /// <summary>
        /// Operation can be Insert,Update,Delete
        /// </summary>
        public string Operation { get; set; } = string.Empty;
        /// <summary>
        /// Date time of the audit event
        /// </summary>
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Ties back to the EntityAudit event id
        /// </summary>
        public Guid EventId { get; set; } = Guid.NewGuid();

        public EntityFieldAudit() { 
        }
    }
}
