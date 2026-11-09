using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Ifx.Storage;

using Ifx.Storage.Azure.Blobs;

public static class AzureBlobStorageRegistrationExtensions
{
    /// <summary>
    /// Adds an Azure Blob storage implementation to the factory.
    /// </summary>
    /// <param name="builder">The storage registration builder.</param>
    /// <param name="name">The unique logical name for this storage implementation. Must not be null or whitespace.</param>
    /// <param name="options">Azure Blob storage options (connection string, container name, etc.). Options should be validated before calling.</param>
    /// <returns>The builder instance to allow fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public static StorageRegistrationBuilder AddBlob(this StorageRegistrationBuilder builder, string name, AzureBlobStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IServiceCollection services = builder.Services;
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(options);
        services.Configure<StorageFactoryOptions>(factoryOptions => factoryOptions.RegisterImplementation(name, typeof(AzureBlobStorageProvider), options));
        services.TryAddTransient(provider => new AzureBlobStorageProvider(options, provider.GetRequiredService<ILogger<AzureBlobStorageProvider>>()));
        services.AddKeyedTransient<AzureBlobStorageProvider>(name,
            (provider, key) => new AzureBlobStorageProvider(options, provider.GetRequiredService<ILogger<AzureBlobStorageProvider>>()));
        return builder;
    }
}
