using System.Collections.Concurrent;
using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.Options;

namespace Util.Messaging.Msmq;

/// <summary>
/// Publishes messages to MSMQ queues and creates MSMQ-backed consumers.
/// </summary>
public sealed class MsmqMessageBus : IMessageBus, IMessagePublisher
{
    private const string DefaultSubscriptionName = "default";

    private readonly ConcurrentDictionary<MessageSubscriptionRegistration, byte> subscriptions = [];
    private readonly IMsmqQueueClient queueClient;
    private readonly IOptions<MsmqOptions> options;

    internal MsmqMessageBus(IMsmqQueueClient queueClient, IOptions<MsmqOptions> options)
    {
        this.queueClient = queueClient;
        this.options = options;
    }

    /// <inheritdoc />
    public IMessagePublisher Publisher => this;

    /// <inheritdoc />
    public IMessageConsumer CreateConsumer<TMessage>(string subscriptionName)
        where TMessage : IMessage
    {
        return CreateConsumer(subscriptionName, NullMessageHandler<TMessage>.Instance);
    }

    /// <summary>
    /// Creates a consumer for a specific handler and subscription.
    /// </summary>
    /// <typeparam name="TMessage">The message type to consume.</typeparam>
    /// <param name="subscriptionName">The logical subscription name.</param>
    /// <param name="handler">The message handler to invoke.</param>
    /// <returns>An MSMQ-backed consumer.</returns>
    public IMessageConsumer CreateConsumer<TMessage>(string subscriptionName, IMessageHandler<TMessage> handler)
        where TMessage : IMessage
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);
        ArgumentNullException.ThrowIfNull(handler);

        MessageSubscriptionRegistration registration = new(
            typeof(TMessage),
            subscriptionName,
            Guid.NewGuid(),
            options.Value.ResolveQueuePath(subscriptionName));

        subscriptions.TryAdd(registration, 0);

        return new MsmqMessageConsumer<TMessage>(
            queueClient,
            options,
            handler,
            registration.QueuePath,
            () => subscriptions.TryRemove(registration, out _));
    }

    /// <inheritdoc />
    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        SerializedMsmqMessage serializedMessage = MsmqEnvelopeSerializer.Serialize(message);
        string[] queuePaths = GetQueuePaths(typeof(TMessage));

        foreach (string queuePath in queuePaths)
        {
            await RetryPolicy.ExecuteAsync(
                token => queueClient.SendAsync(queuePath, serializedMessage, token),
                options.Value.MaxRetryAttempts,
                options.Value.InitialRetryDelay,
                cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task PublishBatchAsync<TMessage>(IEnumerable<TMessage> messages, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(messages);

        foreach (TMessage message in messages)
        {
            await PublishAsync(message, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task ScheduleAsync<TMessage>(
        TMessage message,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        TimeSpan delay = scheduledTime - DateTimeOffset.UtcNow;

        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }

        await PublishAsync(message, cancellationToken).ConfigureAwait(false);
    }

    private string[] GetQueuePaths(Type messageType)
    {
        string[] queuePaths = subscriptions.Keys
            .Where(subscription => subscription.MessageType == messageType)
            .Select(subscription => subscription.QueuePath)
            .ToArray();

        return queuePaths.Length > 0
            ? queuePaths
            : [options.Value.ResolveQueuePath(DefaultSubscriptionName)];
    }

    private sealed record MessageSubscriptionRegistration(
        Type MessageType,
        string SubscriptionName,
        Guid SubscriptionId,
        string QueuePath);

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
