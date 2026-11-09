using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Ifx.Storage;

using Ifx.Storage.Azure.Tables;

public static class AzureTableStorageRegistrationExtensions
{
    /// <summary>
    /// Adds an Azure Table storage implementation to the factory.
    /// </summary>
    /// <param name="builder">The storage registration builder.</param>
    /// <param name="name">The unique logical name for this storage implementation. Must not be null or whitespace.</param>
    /// <param name="options">Azure Table storage options (connection string, table name, retry settings, etc.). Options should be validated before calling.</param>
    /// <returns>The builder instance to allow fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public static StorageRegistrationBuilder AddTable(this StorageRegistrationBuilder builder, string name, AzureTableStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IServiceCollection services = builder.Services;
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(options);
        services.Configure<StorageFactoryOptions>(factoryOptions => factoryOptions.RegisterImplementation(name, typeof(AzureTableStorageProvider), options));
        services.TryAddTransient(provider => new AzureTableStorageProvider(options, provider.GetRequiredService<ILogger<AzureTableStorageProvider>>()));
        services.TryAddTransient<ITableStorageProvider>(provider => provider.GetRequiredService<AzureTableStorageProvider>());
        services.AddKeyedTransient<AzureTableStorageProvider>(name,
            (provider, key) => new AzureTableStorageProvider(options, provider.GetRequiredService<ILogger<AzureTableStorageProvider>>()));
        return builder;
    }
}
