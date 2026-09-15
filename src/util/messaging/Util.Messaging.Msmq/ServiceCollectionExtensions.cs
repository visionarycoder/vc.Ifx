using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Util.Messaging.Msmq;

/// <summary>
/// Registers MSMQ-backed messaging services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the MSMQ message bus, publisher, queue client, and validated options.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Optional MSMQ options configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddMsmqMessaging(
        this IServiceCollection services,
        Action<MsmqOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<MsmqOptions>()
            .Configure(options => configure?.Invoke(options))
            .Validate(options =>
            {
                options.Validate();
                return true;
            })
            .ValidateOnStart();

        services.AddSingleton<IMsmqQueueClient, MsmqQueueClient>();
        services.AddSingleton<MsmqMessageBus>(provider =>
            new MsmqMessageBus(
                provider.GetRequiredService<IMsmqQueueClient>(),
                provider.GetRequiredService<IOptions<MsmqOptions>>()));
        services.AddSingleton<IMessageBus>(provider => provider.GetRequiredService<MsmqMessageBus>());
        services.AddSingleton<IMessagePublisher>(provider => provider.GetRequiredService<MsmqMessageBus>());

        return services;
    }

    /// <summary>
    /// Registers a message handler that starts with the host lifecycle.
    /// </summary>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="subscriptionName">The logical subscription name.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddMsmqMessageHandler<THandler, TMessage>(
        this IServiceCollection services,
        string subscriptionName = "default")
        where THandler : class, IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);

        services.AddSingleton<THandler>();
        services.AddSingleton<IHostedService>(provider =>
            new MsmqMessageHandlerRegistration<THandler, TMessage>(
                provider.GetRequiredService<MsmqMessageBus>(),
                provider.GetRequiredService<THandler>(),
                subscriptionName));

        return services;
    }
}
