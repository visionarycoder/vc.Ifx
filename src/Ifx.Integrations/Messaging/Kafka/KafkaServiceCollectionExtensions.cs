using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using vc.Ifx.Messaging;

namespace vc.Ifx.Messaging.Kafka;

/// <summary>Registers Kafka messaging and typed hosted consumers.</summary>
public static class KafkaServiceCollectionExtensions
{
    /// <summary>Registers the Kafka bus using a typed options callback.</summary>
    public static IServiceCollection AddKafkaMessaging(
        this IServiceCollection services,
        Action<KafkaOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        services.AddOptions<KafkaOptions>()
            .Configure(configure)
            .Validate(options => { options.Validate(); return true; })
            .ValidateOnStart();
        services.AddSingleton<KafkaMessageBus>();
        services.AddSingleton<IMessageBus>(provider => provider.GetRequiredService<KafkaMessageBus>());
        services.AddSingleton<IMessagePublisher>(provider => provider.GetRequiredService<KafkaMessageBus>());
        return services;
    }

    /// <summary>Registers the Kafka bus by binding the named configuration section.</summary>
    public static IServiceCollection AddKafkaMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionPath = KafkaOptions.DefaultSectionName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionPath);
        return services.AddKafkaMessaging(options => configuration.GetSection(sectionPath).Bind(options));
    }

    /// <summary>Registers a typed Kafka message handler as a hosted consumer.</summary>
    public static IServiceCollection AddKafkaMessageHandler<THandler, TMessage>(
        this IServiceCollection services,
        string subscriptionName = "default")
        where THandler : class, IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);
        services.AddSingleton<THandler>();
        services.AddSingleton<IHostedService>(provider =>
            new KafkaMessageHandlerRegistration<THandler, TMessage>(
                provider.GetRequiredService<KafkaMessageBus>(),
                provider.GetRequiredService<THandler>(),
                subscriptionName));
        return services;
    }
}
