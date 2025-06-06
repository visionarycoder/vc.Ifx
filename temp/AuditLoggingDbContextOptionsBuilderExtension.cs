using Microsoft.EntityFrameworkCore;

namespace Utility.AuditLogging;

public abstract class AuditLoggingDbContextOptionsBuilderExtension<TContext>(DbContextOptions<TContext> optionsBuilder) 
    : DbContextOptionsBuilder<TContext>(optionsBuilder)
    where TContext : DbContext;