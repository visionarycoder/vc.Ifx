using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Ifx.Storage;

using Ifx.Messaging.Azure.Queues;

public static class AzureQueueStorageExtensions
{
    /// <summary>
    /// Registers the Azure Queue storage provider implementation.
    /// </summary>
    public static IServiceCollection AddAzureQueueStorage(this IServiceCollection services, AzureQueueStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        services.TryAddSingleton(options);
        services.TryAddTransient<IQueueStorageProvider, AzureQueueStorageProvider>();
        return services;
    }

    /// <summary>
    /// Registers the Azure Queue storage provider implementation.
    /// </summary>
    public static IServiceCollection AddNamedAzureQueueStorage(this IServiceCollection services, string name, AzureQueueStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(options);
        services.TryAddKeyedTransient<IQueueStorageProvider>(name,
            (provider, key) => new AzureQueueStorageProvider(options, provider.GetRequiredService<ILogger<AzureQueueStorageProvider>>()));
        return services;
    }
}
