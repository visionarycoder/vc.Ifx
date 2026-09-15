using Microsoft.Extensions.DependencyInjection;

namespace Util.Archiving.Zip;

/// <summary>
/// Adds ZIP archive services to a dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the ZIP archive provider.
    /// </summary>
    public static IServiceCollection AddZipArchiveProvider(this IServiceCollection services)
    {
        services.AddSingleton<ZipArchive>();
        services.AddSingleton<IArchiveProvider>(provider => provider.GetRequiredService<ZipArchive>());

        return services;
    }
}
