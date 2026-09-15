using Microsoft.Extensions.DependencyInjection;

namespace Util.Archiving.Tar;

/// <summary>
/// Adds TAR archive services to a dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the TAR archive provider.
    /// </summary>
    public static IServiceCollection AddTarArchiveProvider(this IServiceCollection services)
    {
        services.AddSingleton<TarArchive>();
        services.AddSingleton<IArchiveProvider>(provider => provider.GetRequiredService<TarArchive>());

        return services;
    }
}
