using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Ifx.Storage;

using Ifx.Messaging.Azure.Queues;

public static class AzureQueueStorageRegistrationExtensions
{
    /// <summary>
    /// Adds an Azure Queue storage implementation to the factory.
    /// </summary>
    /// <param name="builder">The storage registration builder.</param>
    /// <param name="name">The unique logical name for this storage implementation. Must not be null or whitespace.</param>
    /// <param name="options">Azure Queue storage options (connection string, queue name, TTL, etc.). Options should be validated before calling.</param>
    /// <returns>The builder instance to allow fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public static StorageRegistrationBuilder AddQueue(this StorageRegistrationBuilder builder, string name, AzureQueueStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IServiceCollection services = builder.Services;
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(options);
        services.Configure<StorageFactoryOptions>(factoryOptions => factoryOptions.RegisterImplementation(name, typeof(AzureQueueStorageProvider), options));
        services.TryAddTransient(provider => new AzureQueueStorageProvider(options, provider.GetRequiredService<ILogger<AzureQueueStorageProvider>>()));
        services.TryAddTransient<IQueueStorageProvider>(provider => provider.GetRequiredService<AzureQueueStorageProvider>());
        services.AddKeyedTransient<AzureQueueStorageProvider>(name,
            (provider, key) => new AzureQueueStorageProvider(options, provider.GetRequiredService<ILogger<AzureQueueStorageProvider>>()));
        return builder;
    }
}
