using Microsoft.EntityFrameworkCore;

namespace VisionaryCoder.Framework.Querying.Sample;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    protected override void OnConfiguring(DbContextOptionsBuilder opts) =>
        opts.UseInMemoryDatabase("QueryFilterDemoDb");
}