using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VisionaryCoder.Framework.Data.Azure.Table;
using VisionaryCoder.Framework.Messaging.Azure.Queue;
using VisionaryCoder.Framework.Storage.Azure.Blob;
using VisionaryCoder.Framework.Storage.Ftp;
using VisionaryCoder.Framework.Storage.Local;

namespace VisionaryCoder.Framework.Storage;

/// <summary>
/// Builder used to register multiple storage implementations with the DI container and
/// to configure factory options for each named implementation.
/// </summary>
/// <remarks>
/// This builder records registration information onto <see cref="StorageFactoryOptions"/>
/// and registers provider implementations into the provided <see cref="IServiceCollection"/>.
/// Use this class from extension methods that expose a fluent registration API.
/// </remarks>
public sealed class StorageRegistrationBuilder(IServiceCollection services)
{

    /// <summary>
    /// Adds a local file system storage implementation to the factory.
    /// </summary>
    /// <param name="name">The unique logical name for this storage implementation. Must not be null or whitespace.</param>
    /// <returns>The builder instance to allow fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public StorageRegistrationBuilder AddLocal(string name = "local")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        services.Configure<StorageFactoryOptions>(options => options.RegisterImplementation(name, typeof(LocalStorageProvider)));
        services.TryAddTransient<LocalStorageProvider>();
        return this;
    }

    /// <summary>
    /// Adds an FTP/FTPS storage implementation to the factory using FluentFTP-based provider.
    /// </summary>
    /// <param name="name">The unique logical name for this storage implementation. Must not be null or whitespace.</param>
    /// <param name="options">Configuration options for the FTP provider. Caller is responsible for validating options.</param>
    /// <returns>The builder instance to allow fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public StorageRegistrationBuilder AddFtp(string name, FtpStorageOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        services.Configure<StorageFactoryOptions>(factoryOptions => factoryOptions.RegisterImplementation(name, typeof(FtpStorageProvider), options));
        services.TryAddTransient<FtpStorageProvider>();
        return this;
    }

    /// <summary>
    /// Adds an Azure Blob storage implementation to the factory.
    /// </summary>
    /// <param name="name">The unique logical name for this storage implementation. Must not be null or whitespace.</param>
    /// <param name="options">Azure Blob storage options (connection string, container name, etc.). Options should be validated before calling.</param>
    /// <returns>The builder instance to allow fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public StorageRegistrationBuilder AddBlob(string name, AzureBlobStorageOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        services.Configure<StorageFactoryOptions>(factoryOptions => factoryOptions.RegisterImplementation(name, typeof(AzureBlobStorageProvider), options));
        services.TryAddTransient<AzureBlobStorageProvider>();
        return this;
    }

    /// <summary>
    /// Adds an Azure Queue storage implementation to the factory.
    /// </summary>
    /// <param name="name">The unique logical name for this storage implementation. Must not be null or whitespace.</param>
    /// <param name="options">Azure Queue storage options (connection string, queue name, TTL, etc.). Options should be validated before calling.</param>
    /// <returns>The builder instance to allow fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public StorageRegistrationBuilder AddQueue(string name, AzureQueueStorageOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        services.Configure<StorageFactoryOptions>(factoryOptions => factoryOptions.RegisterImplementation(name, typeof(AzureQueueStorageProvider), options));
        services.TryAddTransient<AzureQueueStorageProvider>();
        services.TryAddTransient<IQueueStorageProvider, AzureQueueStorageProvider>();
        return this;
    }

    /// <summary>
    /// Adds an Azure Table storage implementation to the factory.
    /// </summary>
    /// <param name="name">The unique logical name for this storage implementation. Must not be null or whitespace.</param>
    /// <param name="options">Azure Table storage options (connection string, table name, retry settings, etc.). Options should be validated before calling.</param>
    /// <returns>The builder instance to allow fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public StorageRegistrationBuilder AddTable(string name, AzureTableStorageOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        services.Configure<StorageFactoryOptions>(factoryOptions => factoryOptions.RegisterImplementation(name, typeof(AzureTableStorageProvider), options));
        services.TryAddTransient<AzureTableStorageProvider>();
        services.TryAddTransient<ITableStorageProvider, AzureTableStorageProvider>();
        return this;
    }
}
