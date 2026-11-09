using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Ifx.Storage;

using Ifx.Storage.Ftp;

public static class FtpStorageExtensions
{
    /// <summary>
    /// Registers the FluentFTP-based storage implementation.
    /// </summary>
    public static IServiceCollection AddFtpStorage(this IServiceCollection services, FtpStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        services.TryAddSingleton(options);
        services.TryAddTransient<IStorageProvider, FtpStorageProvider>();
        return services;
    }

    /// <summary>
    /// Registers a named FluentFTP-based storage implementation (requires .NET 8 keyed services).
    /// </summary>
    public static IServiceCollection AddNamedFtpStorage(this IServiceCollection services, string name, FtpStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(options);
        services.TryAddKeyedTransient<IStorageProvider>(name,
            (provider, key) => new FtpStorageProvider(options, provider.GetRequiredService<ILogger<FtpStorageProvider>>()));
        return services;
    }
}
