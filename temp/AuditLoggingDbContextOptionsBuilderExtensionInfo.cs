using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Eyefinity.Utility.AuditLogging;

public abstract class AuditLoggingDbContextOptionsBuilderExtensionInfo<TContext> : DbContextOptionsExtensionInfo where TContext : DbContext
{

    protected AuditLoggingDbContextOptionsBuilderExtensionInfo(IDbContextOptionsExtension extension) 
        : base(extension)
    {
        
    }

}