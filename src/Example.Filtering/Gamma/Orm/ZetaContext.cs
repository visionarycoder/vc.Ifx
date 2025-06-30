using Example.Filtering.Gamma.Orm.Models;
using Microsoft.EntityFrameworkCore;

namespace Example.Filtering.Gamma.Orm;

public class ZetaContext : DbContext
{

    public DbSet<Assembly> Assemblies { get; set; }
    public DbSet<Part> Parts { get; set; }

    public DbSet<AssemblyType> AssemblyTypes { get; set; }
    public DbSet<PartType> PartTypes { get; set; }

}