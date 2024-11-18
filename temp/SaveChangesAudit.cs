namespace Eyefinity.Utility.AuditLogging;

public class SaveChangesAudit
{

    public int Id { get; set; }
    public Guid AuditId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool Succeeded { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public ICollection<EntityAudit> Entities { get; } = null!;

}