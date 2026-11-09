using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Ifx.Storage;

using Ifx.Storage.Azure.Tables;

public static class AzureTableStorageExtensions
{
    /// <summary>
    /// Registers the Azure Table storage provider implementation.
    /// </summary>
    public static IServiceCollection AddAzureTableStorage(this IServiceCollection services, AzureTableStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        services.TryAddSingleton(options);
        services.TryAddTransient<ITableStorageProvider, AzureTableStorageProvider>();
        return services;
    }

    /// <summary>
    /// Registers the Azure Table storage provider implementation.
    /// </summary>
    public static IServiceCollection AddNamedAzureTableStorage(this IServiceCollection services, string name, AzureTableStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(options);
        services.TryAddKeyedTransient<ITableStorageProvider>(name,
            (provider, key) => new AzureTableStorageProvider(options, provider.GetRequiredService<ILogger<AzureTableStorageProvider>>()));
        return services;
    }
}
