using Microsoft.EntityFrameworkCore;
using vc.Ifx.Data;

namespace Example.Filtering.Gamma.Orm.Models;

[PrimaryKey(nameof(Id))]
public class Assembly : Entity
{
    public required int Id { get; set; }

    public int? ParentAssembly { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public decimal Cost { get; set; }

    public int Quantity { get; set; }

    public virtual ICollection<Assembly> SubAssemblies { get; } = new List<Assembly>();
    public virtual ICollection<Part> Parts { get; } = new List<Part>();

}