using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Ifx.Storage;

using Ifx.Storage.Azure.Blobs;

public static class AzureBlobStorageExtensions
{
    /// <summary>
    /// Registers the Azure Blob storage provider implementation.
    /// </summary>
    public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, AzureBlobStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        services.TryAddSingleton(options);
        services.TryAddTransient<IStorageProvider, AzureBlobStorageProvider>();
        return services;
    }

    /// <summary>
    /// Registers the Azure Blob storage provider implementation.
    /// </summary>
    public static IServiceCollection AddNamedAzureBlobStorage(this IServiceCollection services, string name, AzureBlobStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(options);
        services.TryAddKeyedTransient<IStorageProvider>(name,
            (provider, key) => new AzureBlobStorageProvider(options, provider.GetRequiredService<ILogger<AzureBlobStorageProvider>>()));
        return services;
    }
}
