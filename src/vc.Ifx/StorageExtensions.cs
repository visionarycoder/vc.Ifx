using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VisionaryCoder.Framework.Data.Azure.Table;
using VisionaryCoder.Framework.Messaging.Azure.Queue;
using VisionaryCoder.Framework.Storage.Azure.Blob;
using VisionaryCoder.Framework.Storage.Ftp;
using VisionaryCoder.Framework.Storage.Local;

namespace VisionaryCoder.Framework.Storage;

/// <summary>
/// Extension methods for registering storage services with dependency injection.
/// </summary>
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
        services.TryAddSingleton(options);
        services.TryAddKeyedTransient<IStorageProvider, LocalStorageProvider>(name);
        return services;
    }

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
        services.TryAddSingleton(options);
        services.TryAddKeyedTransient<IStorageProvider, FtpStorageProvider>(name);
        return services;
    }

    /// <summary>
    /// Registers the Azure Blob storage provider implementation.
    /// </summary>
    public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, AzureBlobStorageOptions options)
    {
        services.TryAddSingleton(options);
        services.TryAddTransient<IStorageProvider, AzureBlobStorageProvider>();
        return services;
    }

    /// <summary>
    /// Registers the Azure Blob storage provider implementation.
    /// </summary>
    public static IServiceCollection AddNamedAzureBlobStorage(this IServiceCollection services, string name, AzureBlobStorageOptions options)
    {
        services.TryAddSingleton(options);
        services.TryAddKeyedTransient<IStorageProvider, AzureBlobStorageProvider>(name);
        return services;
    }

    /// <summary>
    /// Registers the Azure Queue storage provider implementation.
    /// </summary>
    public static IServiceCollection AddAzureQueueStorage(this IServiceCollection services, AzureQueueStorageOptions options)
    {
        services.TryAddSingleton(options);
        services.TryAddTransient<IQueueStorageProvider, AzureQueueStorageProvider>();
        return services;
    }

    /// <summary>
    /// Registers the Azure Queue storage provider implementation.
    /// </summary>
    public static IServiceCollection AddNamedAzureQueueStorage(this IServiceCollection services, string name, AzureQueueStorageOptions options)
    {
        services.TryAddSingleton(options);
        services.TryAddKeyedTransient<IQueueStorageProvider, AzureQueueStorageProvider>(name);
        return services;
    }

    /// <summary>
    /// Registers the Azure Table storage provider implementation.
    /// </summary>
    public static IServiceCollection AddAzureTableStorage(this IServiceCollection services, AzureTableStorageOptions options)
    {
        services.TryAddSingleton(options);
        services.TryAddTransient<ITableStorageProvider, AzureTableStorageProvider>();
        return services;
    }

    /// <summary>
    /// Registers the Azure Table storage provider implementation.
    /// </summary>
    public static IServiceCollection AddNamedAzureTableStorage(this IServiceCollection services, string name, AzureTableStorageOptions options)
    {
        services.TryAddSingleton(options);
        services.TryAddKeyedTransient<ITableStorageProvider, AzureTableStorageProvider>(name);
        return services;
    }

}
