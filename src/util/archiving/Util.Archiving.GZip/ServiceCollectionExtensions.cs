using Microsoft.Extensions.DependencyInjection;

namespace Util.Archiving.GZip;

/// <summary>
/// Adds GZip archive services to a dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the GZip archive provider.
    /// </summary>
    public static IServiceCollection AddGZipArchiveProvider(this IServiceCollection services)
    {
        services.AddSingleton<GZipArchive>();
        services.AddSingleton<IArchiveProvider>(provider => provider.GetRequiredService<GZipArchive>());

        return services;
    }
}
