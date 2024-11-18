using Microsoft.EntityFrameworkCore;

namespace Eyefinity.Utility.AuditLogging;

public abstract class AuditLoggingDbContextOptionsBuilderExtension<TContext> : DbContextOptionsBuilder<TContext> where TContext : DbContext
{

    protected AuditLoggingDbContextOptionsBuilderExtension(DbContextOptions<TContext> optionsBuilder)
        : base(optionsBuilder)
    {

    }

}