using vc.Ifx.Data;

namespace Example.Filtering.Gamma.Orm.Models;

public class Part : Entity
{

    public int? ParentAssembly { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public decimal Cost { get; set; }

    public int Quantity { get; set; }

}