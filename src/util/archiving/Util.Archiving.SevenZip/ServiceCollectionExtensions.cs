using Microsoft.Extensions.DependencyInjection;

namespace Util.Archiving.SevenZip;

/// <summary>
/// Adds 7z archive services to a dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the 7z archive provider.
    /// </summary>
    public static IServiceCollection AddSevenZipArchiving(this IServiceCollection services)
    {
        services.AddSingleton<SevenZipArchive>();
        services.AddSingleton<IArchiveProvider>(provider => provider.GetRequiredService<SevenZipArchive>());

        return services;
    }
}
