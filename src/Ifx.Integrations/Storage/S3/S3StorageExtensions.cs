using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ifx.Storage.S3;

/// <summary>Registers an S3 object storage provider.</summary>
public static class S3StorageExtensions
{
    /// <summary>Registers object storage using the AWS SDK's default credential and region chain.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The bucket and key-prefix options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddS3ObjectStorage(this IServiceCollection services, S3StorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        services.TryAddSingleton(options);
        services.TryAddSingleton<IAmazonS3>(_ => new AmazonS3Client());
        services.TryAddTransient<IObjectStorageProvider, S3StorageProvider>();
        return services;
    }

    /// <summary>Registers a named S3 object storage provider.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The provider key.</param>
    /// <param name="options">The bucket and key-prefix options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNamedS3ObjectStorage(
        this IServiceCollection services,
        string name,
        S3StorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        services.TryAddSingleton(options);
        services.TryAddSingleton<IAmazonS3>(_ => new AmazonS3Client());
        services.AddKeyedTransient<IObjectStorageProvider>(name, (provider, _) =>
            new S3StorageProvider(
                provider.GetRequiredService<IAmazonS3>(),
                options));
        return services;
    }
}
