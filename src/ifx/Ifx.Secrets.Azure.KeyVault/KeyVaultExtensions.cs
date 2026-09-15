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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Ifx.Secrets.Azure.KeyVault;

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
            services.AddSingleton(CreateLocalProvider(configuration, settings));
        }
        else
        {
            KeyVaultSettings.ValidateRemote(settings, requireUri: true);
            services.AddMemoryCache();
            services.TryAddSingleton<TimeProvider>(TimeProvider.System);
            services.TryAddSingleton<TokenCredential>(_ =>
                new DefaultAzureCredential(new DefaultAzureCredentialOptions { ExcludeInteractiveBrowserCredential = true }));
            services.TryAddSingleton(KeyVaultSettings.CreateClientOptions(settings));
            services.TryAddSingleton(provider => new SecretClient(settings.VaultUri!,
                provider.GetRequiredService<TokenCredential>(), provider.GetRequiredService<SecretClientOptions>()));
            services.AddSingleton<ISecretProvider>(provider => new KeyVaultSecretProvider(
                provider.GetRequiredService<SecretClient>(), Options.Create(KeyVaultSettings.Snapshot(settings)),
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

    private static ISecretProvider CreateLocalProvider(IConfiguration configuration, KeyVaultOptions options) =>
        CreateLocalProvider(Type.GetType("Ifx.Secrets.Local.LocalSecretProvider, Ifx.Secrets.Local", throwOnError: false), configuration, options);

    private static ISecretProvider CreateLocalProvider(Type? providerType, IConfiguration configuration, KeyVaultOptions options)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(options);
        if (providerType is null)
        {
            throw new InvalidOperationException("Local secrets support requires the Ifx.Secrets.Local assembly.");
        }

        try
        {
            object? provider = Activator.CreateInstance(providerType, configuration, KeyVaultSettings.Snapshot(options));
            return provider as ISecretProvider
                ?? throw new InvalidOperationException("Local secrets support could not create an ISecretProvider instance.");
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            return Rethrow<ISecretProvider>(exception.InnerException);
        }
    }

    [ExcludeFromCodeCoverage]
    [DoesNotReturn]
    private static T Rethrow<T>(Exception exception)
    {
        ExceptionDispatchInfo.Capture(exception).Throw();
        throw new UnreachableException();
    }
}
