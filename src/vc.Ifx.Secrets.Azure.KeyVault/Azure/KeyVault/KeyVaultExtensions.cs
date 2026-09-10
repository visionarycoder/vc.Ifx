using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VisionaryCoder.Framework.Secrets.Local;

namespace VisionaryCoder.Framework.Secrets.Azure.KeyVault;

/// <summary>Registers explicit local or remote secret retrieval.</summary>
public static class KeyVaultExtensions
{
    /// <summary>Registers a secret provider using validated configuration and an optional override.</summary>
    /// <param name="services">The application's service collection.</param>
    /// <param name="configuration">Configuration containing the KeyVault section.</param>
    /// <param name="configure">An optional options override applied after configuration binding.</param>
    /// <returns>The supplied service collection.</returns>
    /// <remarks>
    /// Local mode requires UseLocalSecrets=true; a missing vault URI is a configuration error.
    /// Remote mode preserves pre-registered SecretClient, TokenCredential, and TimeProvider
    /// instances. Otherwise it uses noninteractive DefaultAzureCredential and SDK-only retries.
    /// Options are snapshotted at registration; configuration reload still applies to local values.
    /// </remarks>
    public static IServiceCollection AddAzureKeyVaultSecrets(this IServiceCollection services,
        IConfiguration configuration, Action<KeyVaultOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        var configured = new KeyVaultOptions();
        configuration.GetSection("KeyVault").Bind(configured);
        configure?.Invoke(configured);
        KeyVaultOptions settings = KeyVaultSettings.Snapshot(configured);

        if (settings.UseLocalSecrets)
        {
            var local = new LocalSecretProvider(configuration, settings);
            services.AddSingleton<ISecretProvider>(local);
        }
        else
        {
            KeyVaultSettings.ValidateRemote(settings, requireUri: true);
            services.AddMemoryCache();
            services.TryAddSingleton(TimeProvider.System);
            services.TryAddSingleton<TokenCredential>(provider =>
                new DefaultAzureCredential(new DefaultAzureCredentialOptions { ExcludeInteractiveBrowserCredential = true }));
            services.TryAddSingleton(provider => KeyVaultSettings.CreateClientOptions(settings));
            services.TryAddSingleton(provider => new SecretClient(settings.VaultUri!,
                provider.GetRequiredService<TokenCredential>(), provider.GetRequiredService<SecretClientOptions>()));
            services.AddSingleton<ISecretProvider>(provider => new KeyVaultSecretProvider(
                provider.GetRequiredService<SecretClient>(), Options.Create(settings),
                provider.GetRequiredService<IMemoryCache>(),
                provider.GetService<ILogger<KeyVaultSecretProvider>>() ?? NullLogger<KeyVaultSecretProvider>.Instance,
                provider.GetRequiredService<TimeProvider>()));
        }

        services.AddSingleton<IOptions<KeyVaultOptions>>(Options.Create(KeyVaultSettings.Snapshot(settings)));
        return services;
    }

    /// <summary>Registers the null provider when secret retrieval is intentionally disabled.</summary>
    /// <param name="services">The application's service collection.</param>
    /// <returns>The supplied service collection.</returns>
    public static IServiceCollection AddNullSecrets(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<ISecretProvider>(NullSecretProvider.Instance);
        return services;
    }
}
