// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Interceptors;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication;

/// <summary>
/// Extension methods for configuring authentication services in the dependency injection container.
/// Provides comprehensive setup for JWT authentication, token providers, and authentication interceptors.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds JWT authentication services to the dependency injection container with explicit provider registration.
    /// Following SOLID principles: requires explicit intent for all providers, uses Null Object pattern for fallbacks.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureOptions">Action to configure JWT options.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    /// <exception cref="ArgumentException">Thrown when JWT options are invalid.</exception>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, Action<JwtOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = new JwtOptions();
        configureOptions(options);

        if (!options.IsValid())
        {
            throw new ArgumentException("JWT options configuration is invalid. Please check Authority, Audience, and other required settings.");
        }

        // Register JWT options
        services.TryAddSingleton(options);

        // Register NULL OBJECT implementations as fallbacks (SOLID principle)
        // These will be used if no explicit providers are registered
        services.TryAddScoped<IUserContextProvider, NullUserContextProvider>();
        services.TryAddScoped<ITenantContextProvider, NullTenantContextProvider>();
        services.TryAddScoped<ITokenProvider, NullTokenProvider>();

        // Register JWT-specific services
        services.TryAddScoped<JwtAuthenticationInterceptor>();

        return services;
    }

    /// <summary>
    /// Explicitly registers a user context provider implementation (SOLID principle).
    /// Replaces any existing provider with explicit intent, no automatic defaults.
    /// </summary>
    /// <typeparam name="T">The user context provider implementation type.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection ReplaceUserContextProvider<T>(this IServiceCollection services)
        where T : class, IUserContextProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        // Replace any existing implementation with explicit provider
        services.RemoveAll<IUserContextProvider>();
        services.AddScoped<IUserContextProvider, T>();

        return services;
    }

    /// <summary>
    /// Explicitly registers a tenant context provider implementation (SOLID principle).
    /// Replaces any existing provider with explicit intent, no automatic defaults.
    /// </summary>
    /// <typeparam name="T">The tenant context provider implementation type.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection ReplaceTenantContextProvider<T>(this IServiceCollection services)
        where T : class, ITenantContextProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        // Replace any existing implementation with explicit provider
        services.RemoveAll<ITenantContextProvider>();
        services.AddScoped<ITenantContextProvider, T>();

        return services;
    }

    /// <summary>
    /// Explicitly registers a token provider implementation (SOLID principle).
    /// Replaces any existing provider with explicit intent, no automatic defaults.
    /// </summary>
    /// <typeparam name="T">The token provider implementation type.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection ReplaceTokenProvider<T>(this IServiceCollection services)
        where T : class, ITokenProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        // Replace any existing implementation with explicit provider
        services.RemoveAll<ITokenProvider>();
        services.AddScoped<ITokenProvider, T>();

        return services;
    }

    /// <summary>
    /// Convenience method to register default authentication providers explicitly.
    /// This method demonstrates how to replace null object providers with functional implementations.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection UseDefaultAuthenticationProviders(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.ReplaceUserContextProvider<DefaultUserContextProvider>();
        services.ReplaceTenantContextProvider<DefaultTenantContextProvider>();
        services.ReplaceTokenProvider<DefaultTokenProvider>();

        return services;
    }

    /// <summary>
    /// Adds JWT authentication services with a typed configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddJwtAuthentication<TOptions>(this IServiceCollection services)
        where TOptions : class, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IUserContextProvider, DefaultUserContextProvider>();
        services.TryAddScoped<ITenantContextProvider, DefaultTenantContextProvider>();
        services.TryAddScoped<ITokenProvider, DefaultTokenProvider>();
        services.TryAddScoped<JwtAuthenticationInterceptor>();

        return services;
    }

    /// <summary>
    /// Adds Key Vault JWT authentication services to the dependency injection container.
    /// Configures authentication using JWT tokens stored in Azure Key Vault.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureOptions">Action to configure Key Vault JWT options.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddKeyVaultJwtAuthentication(
        this IServiceCollection services,
        Action<KeyVaultJwtOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = new KeyVaultJwtOptions();
        configureOptions(options);

        if (!options.IsValid())
        {
            throw new ArgumentException("Key Vault JWT options configuration is invalid. Please check SecretName and other required settings.");
        }

        // Register Key Vault JWT options
        services.TryAddSingleton(options);

        // Register core authentication services
        services.TryAddScoped<IUserContextProvider, DefaultUserContextProvider>();
        services.TryAddScoped<ITenantContextProvider, DefaultTenantContextProvider>();

        // Register Key Vault JWT interceptor
        services.TryAddScoped<KeyVaultJwtInterceptor>();

        return services;
    }

    /// <summary>
    /// Adds user context services for managing authenticated user information.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IUserContextProvider, DefaultUserContextProvider>();
        services.TryAddScoped<UserContext>();

        return services;
    }

    /// <summary>
    /// Adds user context services with a custom user context provider.
    /// </summary>
    /// <typeparam name="TProvider">The type of user context provider to register.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddUserContext<TProvider>(this IServiceCollection services)
        where TProvider : class, IUserContextProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IUserContextProvider, TProvider>();
        services.TryAddScoped<UserContext>();

        return services;
    }

    /// <summary>
    /// Adds tenant context services for managing multi-tenant authentication scenarios.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddTenantContext(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ITenantContextProvider, DefaultTenantContextProvider>();
        services.TryAddScoped<TenantContext>();

        return services;
    }

    /// <summary>
    /// Adds tenant context services with a custom tenant context provider.
    /// </summary>
    /// <typeparam name="TProvider">The type of tenant context provider to register.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddTenantContext<TProvider>(this IServiceCollection services)
        where TProvider : class, ITenantContextProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ITenantContextProvider, TProvider>();
        services.TryAddScoped<TenantContext>();

        return services;
    }

    /// <summary>
    /// Adds a custom token provider implementation to the dependency injection container.
    /// </summary>
    /// <typeparam name="TTokenProvider">The type of token provider to register.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddTokenProvider<TTokenProvider>(this IServiceCollection services)
        where TTokenProvider : class, ITokenProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ITokenProvider, TTokenProvider>();

        return services;
    }

    /// <summary>
    /// Adds authentication interceptors to the dependency injection container.
    /// Registers all available authentication interceptors for use in proxy scenarios.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddAuthenticationInterceptors(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<JwtAuthenticationInterceptor>();
        services.TryAddScoped<KeyVaultJwtInterceptor>();

        return services;
    }

    /// <summary>
    /// Adds comprehensive authentication services including JWT, user context, tenant context, and interceptors.
    /// This is a convenience method that sets up a complete authentication system.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureJwtOptions">Action to configure JWT options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCompleteAuthentication(
        this IServiceCollection services,
        Action<JwtOptions> configureJwtOptions)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddJwtAuthentication(configureJwtOptions)
            .AddUserContext()
            .AddTenantContext()
            .AddAuthenticationInterceptors();
    }

    /// <summary>
    /// Adds authentication services with validation to ensure proper configuration.
    /// Performs comprehensive validation of authentication setup during service registration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureOptions">Action to configure JWT options.</param>
    /// <param name="validateSetup">Whether to validate the authentication setup. Defaults to true.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddAuthenticationWithValidation(
        this IServiceCollection services,
        Action<JwtOptions> configureOptions,
        bool validateSetup = true)
    {
        ArgumentNullException.ThrowIfNull(services);

        IServiceCollection result = services.AddCompleteAuthentication(configureOptions);

        if (validateSetup)
        {
            // Validate that all required services are registered
            ValidateAuthenticationSetup(services);
        }

        return result;
    }

    /// <summary>
    /// Validates that all required authentication services are properly registered.
    /// </summary>
    /// <param name="services">The service collection to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when required services are missing.</exception>
    private static void ValidateAuthenticationSetup(IServiceCollection services)
    {
        Type[] requiredServices =
        [
            typeof(IUserContextProvider),
            typeof(ITenantContextProvider),
            typeof(ITokenProvider),
            typeof(JwtAuthenticationInterceptor)
        ];

        var missingServices = requiredServices
            .Where(serviceType => !services.Any(s => s.ServiceType == serviceType))
            .ToList();

        if (missingServices.Count != 0)
        {
            string missingServiceNames = string.Join(", ", missingServices.Select(t => t.Name));
            throw new InvalidOperationException($"Authentication setup is incomplete. Missing services: {missingServiceNames}");
        }
    }
}
