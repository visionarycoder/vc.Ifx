// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VisionaryCoder.Framework.Patterns.Events.Abstractions;

namespace VisionaryCoder.Framework.Patterns.Events;

/// <summary>
/// Extension methods for configuring domain event services.
/// </summary>
public static class DomainEventServiceCollectionExtensions
{
    /// <summary>
    /// Adds domain event dispatcher and scans assemblies for event handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan for event handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDomainEvents(this IServiceCollection services, params System.Reflection.Assembly[] assemblies)
    {
        // Register the dispatcher
        services.TryAddScoped<DomainEventDispatcher>();

        // If no assemblies specified, use calling assembly
        if (assemblies.Length == 0)
        {
            assemblies = [System.Reflection.Assembly.GetCallingAssembly()];
        }

        // Scan and register all event handlers
        RegisterEventHandlers(services, assemblies);

        return services;
    }

    /// <summary>
    /// Adds domain event dispatcher and scans the assembly containing the specified type for event handlers.
    /// </summary>
    /// <typeparam name="TAssemblyMarker">Type in the assembly to scan.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDomainEvents<TAssemblyMarker>(this IServiceCollection services)
    {
        return services.AddDomainEvents(typeof(TAssemblyMarker).Assembly);
    }

    /// <summary>
    /// Registers a specific domain event handler.
    /// </summary>
    /// <typeparam name="TEvent">The type of domain event.</typeparam>
    /// <typeparam name="THandler">The type of handler.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDomainEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : IDomainEvent
        where THandler : class, IDomainEventHandler<TEvent>
    {
        services.AddScoped<IDomainEventHandler<TEvent>, THandler>();
        return services;
    }

    private static void RegisterEventHandlers(IServiceCollection services, System.Reflection.Assembly[] assemblies)
    {
        // Find all handler types in the assemblies
        var handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition);

        foreach (var handlerType in handlerTypes)
        {
            // Register domain event handlers
            foreach (var interfaceType in handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
            {
                services.AddScoped(interfaceType, handlerType);
            }
        }
    }
}
