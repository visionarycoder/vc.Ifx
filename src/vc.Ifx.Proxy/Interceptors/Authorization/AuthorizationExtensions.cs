// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Policies;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authorization;

/// <summary>
/// Extension methods for adding comprehensive authorization services to the dependency injection container.
/// Provides fluent configuration for authorization policies following SOLID principles.
/// Supports explicit policy registration with null object fallbacks for safe operation.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Adds authorization infrastructure with null object fallbacks (SOLID principle).
    /// Following SOLID principles: requires explicit intent for policies, uses Null Object pattern for fallbacks.
    /// </summary>
    /// <param name="services">The service collection to add authorization services to.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddAuthorization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register NULL OBJECT implementation as fallback (SOLID principle)
        // This will be used if no explicit authorization policies are registered
        services.TryAddSingleton<IAuthorizationPolicy, NullAuthorizationPolicy>();

        return services;
    }

    // SOLID Principle - Explicit Policy Registration Methods

    /// <summary>
    /// Explicitly registers an authorization policy implementation (SOLID principle).
    /// Replaces any existing null policy with explicit intent, no automatic defaults.
    /// </summary>
    /// <typeparam name="T">The authorization policy implementation type.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddAuthorizationPolicy<T>(this IServiceCollection services)
        where T : class, IAuthorizationPolicy
    {
        ArgumentNullException.ThrowIfNull(services);

        // Add authorization policy (multiple policies can coexist)
        services.AddSingleton<IAuthorizationPolicy, T>();

        return services;
    }

    /// <summary>
    /// Explicitly registers a role-based authorization policy with specific roles.
    /// Provides a convenient way to create role-based authorization without manual configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="requiredRoles">The roles required for authorization.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or requiredRoles is null.</exception>
    public static IServiceCollection AddRoleBasedAuthorization(this IServiceCollection services, params string[] requiredRoles)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(requiredRoles);

        // Add role-based authorization policy with explicit configuration
        services.AddSingleton<IAuthorizationPolicy>(provider =>
            new RoleBasedAuthorizationPolicy(requiredRoles));

        return services;
    }

    /// <summary>
    /// Explicitly registers a custom authorization policy instance.
    /// Allows for pre-configured policy instances with specific settings.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="policy">The authorization policy instance to register.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or policy is null.</exception>
    public static IServiceCollection AddAuthorizationPolicy(this IServiceCollection services, IAuthorizationPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(policy);

        // Add specific policy instance
        services.AddSingleton(policy);

        return services;
    }

    /// <summary>
    /// Replaces the null authorization policy with a functional implementation (SOLID principle).
    /// This method removes the null object and replaces it with an explicit policy type.
    /// </summary>
    /// <typeparam name="T">The authorization policy implementation type.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection ReplaceAuthorizationPolicy<T>(this IServiceCollection services)
        where T : class, IAuthorizationPolicy
    {
        ArgumentNullException.ThrowIfNull(services);

        // Remove existing authorization policies and replace with explicit type
        services.RemoveAll<IAuthorizationPolicy>();
        services.AddSingleton<IAuthorizationPolicy, T>();

        return services;
    }

    /// <summary>
    /// Convenience method to register default role-based authorization explicitly.
    /// This method demonstrates how to replace null object policies with functional implementations.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="defaultRoles">The default roles required for authorization.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or defaultRoles is null.</exception>
    public static IServiceCollection UseDefaultRoleBasedAuthorization(this IServiceCollection services, params string[] defaultRoles)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(defaultRoles);

        // Remove null policy and replace with role-based authorization
        services.RemoveAll<IAuthorizationPolicy>();
        services.AddSingleton<IAuthorizationPolicy>(provider =>
            new RoleBasedAuthorizationPolicy(defaultRoles));

        return services;
    }
}
