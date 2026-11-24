using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using vc.Ifx.Messaging;
using vc.Ifx.Messaging.ProviderSupport;

namespace vc.Ifx.Messaging.Kafka;

/// <summary>Publishes messages to Kafka and creates typed Kafka consumers.</summary>
public sealed class KafkaMessageBus : IMessageBus, IMessagePublisher, IDisposable
{
    private readonly KafkaOptions options;
    private readonly IProducer<Null, byte[]> producer;
    private readonly Func<ConsumerConfig, IConsumer<Ignore, byte[]>> consumerFactory;
    private readonly ILogger<KafkaMessageBus> logger;
    private bool disposed;

    /// <summary>Creates a Kafka bus using the supplied options and default Confluent clients.</summary>
    public KafkaMessageBus(IOptions<KafkaOptions> options, ILogger<KafkaMessageBus>? logger = null)
        : this(GetOptions(options), logger ?? NullLogger<KafkaMessageBus>.Instance)
    {
    }

    internal KafkaMessageBus(KafkaOptions options)
        : this(options, NullLogger<KafkaMessageBus>.Instance)
    {
    }

    internal KafkaMessageBus(KafkaOptions options, ILogger<KafkaMessageBus> logger)
    {
        this.options = ValidateOptions(options);
        ArgumentNullException.ThrowIfNull(logger);
        producer = new ProducerBuilder<Null, byte[]>(this.options.CreateProducerConfig()).Build();
        consumerFactory = config => new ConsumerBuilder<Ignore, byte[]>(config).Build();
        this.logger = logger;
    }

    internal KafkaMessageBus(
        KafkaOptions options,
        IProducer<Null, byte[]> producer,
        Func<ConsumerConfig, IConsumer<Ignore, byte[]>> consumerFactory,
        ILogger<KafkaMessageBus>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(producer);
        ArgumentNullException.ThrowIfNull(consumerFactory);

        this.options = options.Validate();
        this.producer = producer;
        this.consumerFactory = consumerFactory;
        this.logger = logger ?? NullLogger<KafkaMessageBus>.Instance;
    }

    /// <inheritdoc />
    public IMessagePublisher Publisher => this;

    /// <inheritdoc />
    public IMessageConsumer CreateConsumer<TMessage>(string subscriptionName)
        where TMessage : IMessage =>
        CreateConsumer(subscriptionName, NullMessageHandler<TMessage>.Instance);

    /// <summary>Creates a typed Kafka consumer using the supplied message handler.</summary>
    public IMessageConsumer CreateConsumer<TMessage>(string subscriptionName, IMessageHandler<TMessage> handler)
        where TMessage : IMessage
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);
        ArgumentNullException.ThrowIfNull(handler);
        return new KafkaMessageConsumer<TMessage>(options, consumerFactory, handler, subscriptionName, logger);
    }

    /// <inheritdoc />
    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();

        Guid? correlationId = Guid.TryParse(message.CorrelationId, out Guid parsedCorrelationId)
            ? parsedCorrelationId
            : null;
        byte[] value = KafkaEnvelope<TMessage>.Serialize(MessageEnvelope.Create(message, correlationId));
        var kafkaMessage = new Message<Null, byte[]> { Value = value };
        _ = await producer.ProduceAsync(options.GetTopicName<TMessage>(), kafkaMessage, cancellationToken).ConfigureAwait(false);
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

    /// <summary>Flushes and releases the owned Kafka producer.</summary>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        try
        {
            _ = producer.Flush(TimeSpan.FromSeconds(10));
        }
        finally
        {
            producer.Dispose();
        }
    }

    private static KafkaOptions GetOptions(IOptions<KafkaOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Value.Validate();
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
    }

    private sealed class NullMessageHandler<TMessage> : IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        public static readonly NullMessageHandler<TMessage> Instance = new();

        public Task HandleAsync(TMessage message, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private static KafkaOptions ValidateOptions(KafkaOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Validate();
    }
}
