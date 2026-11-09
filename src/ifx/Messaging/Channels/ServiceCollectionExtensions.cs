using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using vc.Ifx.Messaging;

namespace vc.Ifx.Messaging.Channels;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChannelMessaging(this IServiceCollection services)
    {
        services.AddSingleton<ChannelMessageBus>();
        services.AddSingleton<IMessageBus>(provider => provider.GetRequiredService<ChannelMessageBus>());
        services.AddSingleton<IMessagePublisher>(provider => provider.GetRequiredService<ChannelMessageBus>());

        return services;
    }

    public static IServiceCollection AddChannelMessageHandler<THandler, TMessage>(
        this IServiceCollection services,
        string subscriptionName = "default")
        where THandler : class, IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);

        services.TryAddScoped<THandler>();
        services.AddSingleton<IHostedService>(provider =>
            new ChannelMessageHandlerRegistration<THandler, TMessage>(
                provider.GetRequiredService<ChannelMessageBus>(),
                provider.GetRequiredService<IServiceScopeFactory>(),
                subscriptionName));

        return services;
    }
}
