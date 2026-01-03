// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VisionaryCoder.Framework.Messaging.Azure;

namespace VisionaryCoder.Framework.Messaging;

/// <summary>
/// Extension methods for configuring messaging services.
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Service Bus messaging with connection string.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">Azure Service Bus connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddServiceBusMessaging(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        // Register Service Bus client
        services.TryAddSingleton(_ => new ServiceBusClient(connectionString));

        // Register publisher
        services.TryAddSingleton<IMessagePublisher, ServiceBusMessagePublisher>();

        return services;
    }

    /// <summary>
    /// Adds Azure Service Bus messaging with options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for Service Bus client options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddServiceBusMessaging(
        this IServiceCollection services,
        Action<ServiceBusClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new ServiceBusClientOptions();
        configure(options);

        // Register publisher
        services.TryAddSingleton<IMessagePublisher, ServiceBusMessagePublisher>();

        return services;
    }

    /// <summary>
    /// Registers a message handler for a specific message type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to handle.</typeparam>
    /// <typeparam name="THandler">The type of handler.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMessageHandler<TMessage, THandler>(this IServiceCollection services)
        where TMessage : IMessage
        where THandler : class, IMessageHandler<TMessage>
    {
        services.AddScoped<IMessageHandler<TMessage>, THandler>();
        return services;
    }

    /// <summary>
    /// Registers a consumer for a specific message type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to consume.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="subscriptionName">The name of the subscription.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMessageConsumer<TMessage>(
        this IServiceCollection services,
        string subscriptionName)
        where TMessage : IMessage
    {
        services.AddSingleton<IMessageConsumer>(sp =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ServiceBusMessageConsumer<TMessage>>>();
            return new ServiceBusMessageConsumer<TMessage>(client, sp, logger, subscriptionName);
        });

        return services;
    }
}
