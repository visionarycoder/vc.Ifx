using System.Collections.Concurrent;
using System.Threading.Channels;
using Ifx.Messaging.Abstractions;

namespace Util.Messaging.Channels;

public sealed class ChannelMessageBus : IMessageBus, IMessagePublisher
{
    private readonly ConcurrentDictionary<MessageSubscriptionKey, ConcurrentDictionary<Guid, Channel<IMessage>>> subscriptions = [];

    public IMessagePublisher Publisher => this;

    public IMessageConsumer CreateConsumer<TMessage>(string subscriptionName)
        where TMessage : IMessage
    {
        return CreateConsumer(subscriptionName, NullMessageHandler<TMessage>.Instance);
    }

    public IMessageConsumer CreateConsumer<TMessage>(string subscriptionName, IMessageHandler<TMessage> handler)
        where TMessage : IMessage
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);
        ArgumentNullException.ThrowIfNull(handler);

        var key = new MessageSubscriptionKey(typeof(TMessage), subscriptionName);
        var subscriptionId = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<IMessage>();
        var subscribers = subscriptions.GetOrAdd(key, _ => []);

        subscribers[subscriptionId] = channel;

        return new ChannelMessageConsumer<TMessage>(
            channel,
            handler,
            () => RemoveSubscription(key, subscriptionId));
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        MessageSubscriptionKey[] keys = subscriptions.Keys
            .Where(key => key.MessageType == typeof(TMessage))
            .ToArray();

        foreach (MessageSubscriptionKey key in keys)
        {
            if (!subscriptions.TryGetValue(key, out ConcurrentDictionary<Guid, Channel<IMessage>>? subscribers))
            {
                continue;
            }

            foreach (Channel<IMessage> channel in subscribers.Values)
            {
                await channel.Writer.WriteAsync(message, cancellationToken);
            }
        }
    }

    public async Task PublishBatchAsync<TMessage>(IEnumerable<TMessage> messages, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(messages);

        foreach (TMessage message in messages)
        {
            await PublishAsync(message, cancellationToken);
        }
    }

    public async Task ScheduleAsync<TMessage>(
        TMessage message,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        TimeSpan delay = scheduledTime - DateTimeOffset.UtcNow;

        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, cancellationToken);
        }

        await PublishAsync(message, cancellationToken);
    }

    private void RemoveSubscription(MessageSubscriptionKey key, Guid subscriptionId)
    {
        if (!subscriptions.TryGetValue(key, out ConcurrentDictionary<Guid, Channel<IMessage>>? subscribers))
        {
            return;
        }

        if (subscribers.TryRemove(subscriptionId, out Channel<IMessage>? channel))
        {
            channel.Writer.TryComplete();
        }

        if (subscribers.IsEmpty)
        {
            subscriptions.TryRemove(key, out _);
        }
    }

    private sealed record MessageSubscriptionKey(Type MessageType, string SubscriptionName);

    private sealed class NullMessageHandler<TMessage> : IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        public static readonly NullMessageHandler<TMessage> Instance = new();

        public Task HandleAsync(TMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
