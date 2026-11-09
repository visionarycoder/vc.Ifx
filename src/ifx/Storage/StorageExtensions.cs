using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Ifx.Storage;

using Ifx.Storage.Local;

public static class StorageExtensions
{
    /// <summary>
    /// Registers the local storage implementation.
    /// </summary>
    public static IServiceCollection AddLocalStorage(this IServiceCollection services, LocalStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        services.TryAddSingleton(options);
        services.TryAddTransient<IStorageProvider, LocalStorageProvider>();
        return services;
    }

    /// <summary>
    /// Registers the local storage implementation.
    /// </summary>
    public static IServiceCollection AddNamedLocalStorage(this IServiceCollection services, string name, LocalStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(options);
        services.TryAddKeyedTransient<IStorageProvider>(name,
            (provider, key) => new LocalStorageProvider(options, provider.GetRequiredService<ILogger<LocalStorageProvider>>()));
        return services;
    }
}
