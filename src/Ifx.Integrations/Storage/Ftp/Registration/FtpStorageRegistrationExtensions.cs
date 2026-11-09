using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Ifx.Storage;

using Ifx.Storage.Ftp;

public static class FtpStorageRegistrationExtensions
{
    /// <summary>
    /// Adds an FTP/FTPS storage implementation to the factory using FluentFTP-based provider.
    /// </summary>
    /// <param name="builder">The storage registration builder.</param>
    /// <param name="name">The unique logical name for this storage implementation. Must not be null or whitespace.</param>
    /// <param name="options">Configuration options for the FTP provider. Caller is responsible for validating options.</param>
    /// <returns>The builder instance to allow fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public static StorageRegistrationBuilder AddFtp(this StorageRegistrationBuilder builder, string name, FtpStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IServiceCollection services = builder.Services;
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(options);
        services.Configure<StorageFactoryOptions>(factoryOptions => factoryOptions.RegisterImplementation(name, typeof(FtpStorageProvider), options));
        services.TryAddTransient(provider => new FtpStorageProvider(options, provider.GetRequiredService<ILogger<FtpStorageProvider>>()));
        services.AddKeyedTransient<FtpStorageProvider>(name,
            (provider, key) => new FtpStorageProvider(options, provider.GetRequiredService<ILogger<FtpStorageProvider>>()));
        return builder;
    }
}
