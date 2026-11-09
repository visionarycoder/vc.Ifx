using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Ifx.Storage;

using Ifx.Storage.Local;

public sealed class StorageRegistrationBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));
    /// <summary>
    /// Adds a local file system storage implementation to the factory.
    /// </summary>
    /// <param name="name">The unique logical name for this storage implementation. Must not be null or whitespace.</param>
    /// <returns>The builder instance to allow fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public StorageRegistrationBuilder AddLocal(string name = "local")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Services.Configure<StorageFactoryOptions>(options => options.RegisterImplementation(name, typeof(LocalStorageProvider)));
        Services.TryAddTransient<LocalStorageProvider>();
        Services.AddKeyedTransient<LocalStorageProvider>(name,
            (provider, key) => ActivatorUtilities.CreateInstance<LocalStorageProvider>(provider));
        return this;
    }
}
