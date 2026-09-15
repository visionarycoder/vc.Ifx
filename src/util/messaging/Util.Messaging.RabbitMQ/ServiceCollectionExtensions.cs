using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Util.Messaging.RabbitMQ;

/// <summary>
/// Registers RabbitMQ messaging services and typed message handlers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the RabbitMQ message bus by using the supplied option configuration callback.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configure">The option configuration callback.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRabbitMqMessaging(
        this IServiceCollection services,
        Action<RabbitMqOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<RabbitMqOptions>()
            .Configure(configure)
            .Validate(options =>
            {
                options.Validate();
                return true;
            })
            .ValidateOnStart();
        services.AddSingleton<RabbitMqChannelFactory>();
        services.AddSingleton<IRabbitMqChannelFactory>(provider => provider.GetRequiredService<RabbitMqChannelFactory>());
        services.AddSingleton<RabbitMqMessageBus>();
        services.AddSingleton<IMessageBus>(provider => provider.GetRequiredService<RabbitMqMessageBus>());
        services.AddSingleton<IMessagePublisher>(provider => provider.GetRequiredService<RabbitMqMessageBus>());

        return services;
    }

    /// <summary>
    /// Registers the RabbitMQ message bus by binding a configuration section to <see cref="RabbitMqOptions"/>.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The source configuration.</param>
    /// <param name="sectionPath">The section path to bind.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRabbitMqMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionPath = RabbitMqOptions.DefaultSectionName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        sectionPath.ValidateNotNullOrWhiteSpace(nameof(sectionPath));

        return services.AddRabbitMqMessaging(options => configuration.GetSection(sectionPath).Bind(options));
    }

    /// <summary>
    /// Registers a hosted RabbitMQ message handler for a subscription.
    /// </summary>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="services">The service collection to update.</param>
    /// <param name="subscriptionName">The logical subscription name.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRabbitMqMessageHandler<THandler, TMessage>(
        this IServiceCollection services,
        string subscriptionName = "default")
        where THandler : class, IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);

        services.AddSingleton<THandler>();
        services.AddSingleton<IHostedService>(provider =>
            new RabbitMqMessageHandlerRegistration<THandler, TMessage>(
                provider.GetRequiredService<RabbitMqMessageBus>(),
                provider.GetRequiredService<THandler>(),
                subscriptionName));

        return services;
    }
}
