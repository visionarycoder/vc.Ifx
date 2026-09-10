using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VisionaryCoder.Framework.Secrets.Local;

namespace VisionaryCoder.Framework.Secrets.Azure.KeyVault;

/// <summary>
/// Extension methods for configuring Azure Key Vault secret services.
/// </summary>
public static class KeyVaultExtensions
{
    /// <summary>
    /// Adds Azure Key Vault secret provider to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration to read settings from.</param>
    /// <param name="configure">Optional configuration action for KeyVaultOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureKeyVaultSecrets(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<KeyVaultOptions>? configure = null)
    {
        services.AddMemoryCache();
        var options = new KeyVaultOptions();
        configuration.GetSection("KeyVault").Bind(options);
        configure?.Invoke(options);
        services.Configure<KeyVaultOptions>(opts =>
        {
            opts.VaultUri = options.VaultUri;
            opts.CacheTtl = options.CacheTtl;
            opts.UseLocalSecrets = options.UseLocalSecrets;
            opts.LocalSecretsPrefix = options.LocalSecretsPrefix;
            opts.MaxRetries = options.MaxRetries;
            opts.RetryDelay = options.RetryDelay;
        });

        // Local-first toggle (explicit) OR missing vault URI => local
        bool useLocal = options.UseLocalSecrets || options.VaultUri is null;
        if (useLocal)
        {
            services.AddSingleton<ISecretProvider>(provider =>
            {
                IConfiguration config = provider.GetRequiredService<IConfiguration>();
                KeyVaultOptions keyVaultOptions = provider.GetRequiredService<IOptions<KeyVaultOptions>>().Value;
                return new LocalSecretProvider(config, keyVaultOptions);
            });
            return services;
        }

        // Configure Azure Key Vault client with managed identity
        services.AddSingleton(provider =>
        {
            IOptions<KeyVaultOptions> opts = provider.GetRequiredService<IOptions<KeyVaultOptions>>();
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeInteractiveBrowserCredential = true // Better for production scenarios
            });

            // Create client options then configure the existing Retry instance (Retry is read-only)
            var clientOptions = new SecretClientOptions();
            clientOptions.Retry.MaxRetries = opts.Value.MaxRetries;
            clientOptions.Retry.Delay = opts.Value.RetryDelay;
            clientOptions.Retry.Mode = RetryMode.Exponential;

            return new SecretClient(opts.Value.VaultUri!, credential, clientOptions);
        });
        services.AddSingleton<ISecretProvider, KeyVaultSecretProvider>();
        return services;
    }

    /// <summary>
    /// Adds a null secret provider (useful for testing or when secrets are not needed).
    /// </summary>
    public static IServiceCollection AddNullSecrets(this IServiceCollection services)
    {
        services.AddSingleton<ISecretProvider>(NullSecretProvider.Instance);
        return services;
    }
}
