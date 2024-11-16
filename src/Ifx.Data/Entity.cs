namespace vc.Ifx.Data;

public abstract class Entity
{
    public int Id { get; set; } = -1;
    public Guid Uuid { get; set; } = Guid.NewGuid();
}