using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Util.Messaging.Azure.ServiceBus;

/// <summary>
/// Registers Azure Service Bus messaging services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Service Bus messaging services with strongly typed options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">The options configuration delegate.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddAzureServiceBusMessaging(
        this IServiceCollection services,
        Action<AzureServiceBusOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<AzureServiceBusOptions>()
            .Configure(options => configureOptions?.Invoke(options))
            .Validate(options =>
            {
                options.Validate();
                return true;
            })
            .ValidateOnStart();

        services.AddSingleton(provider =>
        {
            AzureServiceBusOptions options = provider.GetRequiredService<IOptions<AzureServiceBusOptions>>().Value.Validate();

            return !string.IsNullOrWhiteSpace(options.ConnectionString)
                ? new ServiceBusClient(options.ConnectionString)
                : new ServiceBusClient(options.FullyQualifiedNamespace!, new DefaultAzureCredential());
        });

        services.AddSingleton<AzureServiceBusMessageBus>();
        services.AddSingleton<IMessageBus>(provider => provider.GetRequiredService<AzureServiceBusMessageBus>());
        services.AddSingleton<IMessagePublisher>(provider => provider.GetRequiredService<AzureServiceBusMessageBus>());

        return services;
    }

    /// <summary>
    /// Registers a singleton message handler hosted service for Azure Service Bus processing.
    /// </summary>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="subscriptionName">The topic subscription name.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddAzureServiceBusMessageHandler<THandler, TMessage>(
        this IServiceCollection services,
        string subscriptionName = "default")
        where THandler : class, IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);

        services.AddSingleton<THandler>();
        services.AddSingleton<IHostedService>(provider =>
            new AzureServiceBusMessageHandlerRegistration<THandler, TMessage>(
                provider.GetRequiredService<AzureServiceBusMessageBus>(),
                provider.GetRequiredService<THandler>(),
                subscriptionName));

        return services;
    }
}
