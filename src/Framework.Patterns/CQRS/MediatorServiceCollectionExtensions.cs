// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VisionaryCoder.Framework.Patterns.CQRS.Abstractions;

namespace VisionaryCoder.Framework.Patterns.CQRS;

/// <summary>
/// Extension methods for configuring CQRS services in dependency injection.
/// </summary>
public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Adds the mediator and scans the specified assemblies for command/query handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Register the mediator
        services.TryAddScoped<IMediator, Mediator>();

        // If no assemblies specified, use calling assembly
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        // Scan and register all handlers
        RegisterHandlers(services, assemblies);

        return services;
    }

    /// <summary>
    /// Adds the mediator and scans the assembly containing the specified type for handlers.
    /// </summary>
    /// <typeparam name="TAssemblyMarker">Type in the assembly to scan.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediator<TAssemblyMarker>(this IServiceCollection services)
    {
        return services.AddMediator(typeof(TAssemblyMarker).Assembly);
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        // Find all handler types in the assemblies
        IEnumerable<Type> handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition);

        foreach (Type handlerType in handlerTypes)
        {
            // Register command handlers (void)
            foreach (Type interfaceType in handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
            {
                services.AddScoped(interfaceType, handlerType);
            }

            // Register command handlers (with response)
            foreach (Type interfaceType in handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
            {
                services.AddScoped(interfaceType, handlerType);
            }

            // Register query handlers
            foreach (Type interfaceType in handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
            {
                services.AddScoped(interfaceType, handlerType);
            }
        }
    }
}
