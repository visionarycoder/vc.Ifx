using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VisionaryCoder.Framework.Secrets;

namespace VisionaryCoder.Framework.Extensions;
/// <summary>
/// Extension methods for configuring database connections and connection strings.
/// </summary>
public static class DataConfigurationExtensions
{

    /// <summary>
    /// Adds a connection string from configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the connection string to.</param>
    /// <param name="configuration">The configuration containing the connection string.</param>
    /// <param name="connectionName">The name of the connection string in configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConnectionString(this IServiceCollection services, IConfiguration configuration, string connectionName)
    {
        string? connectionStringValue = configuration.GetConnectionString(connectionName);

        if (string.IsNullOrWhiteSpace(connectionStringValue))
        {
            throw new InvalidOperationException($"Connection string '{connectionName}' is not configured.");
        }
        services.AddSingleton(connectionStringValue);
        return services;
    }

    /// <summary>
    /// Adds a named connection string from configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the connection string to.</param>
    /// <param name="configuration">The configuration containing the connection string.</param>
    /// <param name="connectionName">The name of the connection string in configuration.</param>
    /// <param name="serviceName">The service name to register the connection string under.</param>
    public static IServiceCollection AddNamedConnectionString(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionName,
        string serviceName)
    {
        string? connectionStringValue = configuration.GetConnectionString(connectionName);
        if (string.IsNullOrWhiteSpace(connectionStringValue))
        {
            throw new InvalidOperationException($"Connection string '{connectionName}' is not configured.");
        }
        services.AddKeyedSingleton(serviceName, connectionStringValue);
        return services;
    }
    /// Adds a connection string from a secret provider to the service collection.
    /// <param name="services">The service collection to add the connection string to.</param>
    /// <param name="secretName">The name of the secret containing the connection string.</param>
    public static IServiceCollection AddConnectionStringFromSecret(this IServiceCollection services, string secretName)
    {
        services.AddSingleton<string>(provider =>
        {
            ISecretProvider secretProvider = provider.GetRequiredService<ISecretProvider>();
            string? connectionStringValue = secretProvider.GetAsync(secretName).GetAwaiter().GetResult();
            if (string.IsNullOrWhiteSpace(connectionStringValue))
            {
                throw new InvalidOperationException($"Connection string secret '{secretName}' is not available or empty.");
            }
            return connectionStringValue;
        });
        return services;
    }
}
