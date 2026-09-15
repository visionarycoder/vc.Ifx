using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.Options;

namespace Util.Messaging.RabbitMQ;

/// <summary>
/// Publishes messages through RabbitMQ and creates typed RabbitMQ consumers.
/// </summary>
public sealed class RabbitMqMessageBus : IMessageBus, IMessagePublisher
{
    private readonly IRabbitMqChannelFactory channelFactory;
    private readonly RabbitMqOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqMessageBus"/> class.
    /// </summary>
    /// <param name="channelFactory">The channel factory.</param>
    /// <param name="options">The configured RabbitMQ transport options.</param>
    public RabbitMqMessageBus(
        RabbitMqChannelFactory channelFactory,
        IOptions<RabbitMqOptions> options)
        : this((IRabbitMqChannelFactory)channelFactory, options)
    {
    }

    internal RabbitMqMessageBus(
        IRabbitMqChannelFactory channelFactory,
        IOptions<RabbitMqOptions> options)
    {
        ArgumentNullException.ThrowIfNull(channelFactory);
        ArgumentNullException.ThrowIfNull(options);

        this.channelFactory = channelFactory;
        this.options = options.Value.Validate();
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
    /// Creates a consumer that delegates typed deliveries to the supplied handler.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="subscriptionName">The logical subscription name.</param>
    /// <param name="handler">The handler that processes typed messages.</param>
    /// <returns>A consumer bound to RabbitMQ for the supplied message type.</returns>
    public IMessageConsumer CreateConsumer<TMessage>(string subscriptionName, IMessageHandler<TMessage> handler)
        where TMessage : IMessage
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);
        ArgumentNullException.ThrowIfNull(handler);

        return new RabbitMqMessageConsumer<TMessage>(
            channelFactory,
            options,
            handler,
            subscriptionName);
    }

    /// <inheritdoc />
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        MessageEnvelope envelope = MessageEnvelope.Create(message, ResolveCorrelationId(message));
        byte[] body = RabbitMqEnvelopeSerializer.Serialize<TMessage>(envelope);
        string routingKey = typeof(TMessage).FullName!;
        string messageType = typeof(TMessage).FullName!;
        string? correlationId = envelope.CorrelationId?.ToString("D");

        return RetryPolicy.ExecuteAsync(
            async token =>
            {
                IRabbitMqChannel channel = await channelFactory.CreateChannelAsync(token).ConfigureAwait(false);

                try
                {
                    await channel.DeclareExchangeAsync(options.ExchangeName!, options.ExchangeType!, token).ConfigureAwait(false);
                    await channel.PublishAsync(
                        options.ExchangeName!,
                        routingKey,
                        messageType,
                        envelope.MessageId.ToString("D"),
                        correlationId,
                        envelope.EnqueuedAtUtc,
                        body,
                        token).ConfigureAwait(false);
                }
                finally
                {
                    channel.Dispose();
                }
            },
            options.MaxRetryAttempts,
            options.InitialRetryDelay,
            cancellationToken);
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

    private static Guid? ResolveCorrelationId(IMessage message)
    {
        return Guid.TryParse(message.CorrelationId, out Guid correlationId)
            ? correlationId
            : null;
    }

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
