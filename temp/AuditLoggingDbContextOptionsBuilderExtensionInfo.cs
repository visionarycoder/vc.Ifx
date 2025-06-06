using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Utility.AuditLogging;

public abstract class AuditLoggingDbContextOptionsBuilderExtensionInfo<TContext>(IDbContextOptionsExtension extension) 
    : DbContextOptionsExtensionInfo(extension) where TContext 
    : DbContext;