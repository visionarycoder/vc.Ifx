using Microsoft.EntityFrameworkCore;

namespace Utility.AuditLogging;

public class EntityAudit
{

    public int Id { get; set; }
    public EntityState State { get; set; }
    public string AuditMessage { get; set; } = string.Empty;

    public SaveChangesAudit SaveChangesAudit { get; set; } = null!;

}