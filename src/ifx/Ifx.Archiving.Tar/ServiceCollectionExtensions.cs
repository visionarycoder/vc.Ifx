using Microsoft.Extensions.DependencyInjection;

namespace vc.Ifx.Archiving.Tar;

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
