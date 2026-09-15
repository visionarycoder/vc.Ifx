using System.Text.Json;
using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Util.Messaging.RabbitMQ.UnitTests;

internal sealed class FakeRabbitMqChannelFactory : IRabbitMqChannelFactory
{
    private readonly Queue<IRabbitMqChannel> channels = new();

    public int CreateCalls { get; private set; }

    public void Enqueue(IRabbitMqChannel channel)
    {
        channels.Enqueue(channel);
    }

    public Task<IRabbitMqChannel> CreateChannelAsync(CancellationToken cancellationToken = default)
    {
        CreateCalls++;
        return Task.FromResult(channels.Dequeue());
    }
}

internal sealed class FakeRabbitMqChannel : IRabbitMqChannel
{
    public int DeclareExchangeCalls { get; private set; }

    public int DeclareConsumerTopologyCalls { get; private set; }

    public int PublishCalls { get; private set; }

    public int CancelCalls { get; private set; }

    public int AckCalls { get; private set; }

    public int RejectCalls { get; private set; }

    public int NackCalls { get; private set; }

    public int PublishFailuresRemaining { get; set; }

    public int DeclareConsumerTopologyFailuresRemaining { get; set; }

    public int StartConsumerFailuresRemaining { get; set; }

    public int CancelFailuresRemaining { get; set; }

    public string ConsumerTagToReturn { get; set; } = "consumer-1";

    public string? DeclaredExchangeName { get; private set; }

    public string? DeclaredExchangeType { get; private set; }

    public RabbitMqTopology? DeclaredTopology { get; private set; }

    public string? PublishedExchangeName { get; private set; }

    public string? PublishedRoutingKey { get; private set; }

    public string? PublishedMessageType { get; private set; }

    public string? PublishedMessageId { get; private set; }

    public string? PublishedCorrelationId { get; private set; }

    public DateTimeOffset PublishedEnqueuedAtUtc { get; private set; }

    public byte[]? PublishedBody { get; private set; }

    public Func<RabbitMqDelivery, CancellationToken, Task>? OnReceived { get; private set; }

    public string? CancelledConsumerTag { get; private set; }

    public bool? LastRejectRequeue { get; private set; }

    public bool? LastNegativeAcknowledgeRequeue { get; private set; }

    public bool Disposed { get; private set; }

    public Task DeclareExchangeAsync(string exchangeName, string exchangeType, CancellationToken cancellationToken = default)
    {
        DeclareExchangeCalls++;
        DeclaredExchangeName = exchangeName;
        DeclaredExchangeType = exchangeType;
        return Task.CompletedTask;
    }

    public Task DeclareConsumerTopologyAsync(
        RabbitMqTopology topology,
        RabbitMqOptions options,
        CancellationToken cancellationToken = default)
    {
        DeclareConsumerTopologyCalls++;

        if (DeclareConsumerTopologyFailuresRemaining-- > 0)
        {
            throw new InvalidOperationException("Topology declaration failed.");
        }

        DeclaredTopology = topology;
        return Task.CompletedTask;
    }

    public Task PublishAsync(
        string exchangeName,
        string routingKey,
        string messageType,
        string messageId,
        string? correlationId,
        DateTimeOffset enqueuedAtUtc,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken = default)
    {
        PublishCalls++;

        if (PublishFailuresRemaining-- > 0)
        {
            throw new InvalidOperationException("Publish failed.");
        }

        PublishedExchangeName = exchangeName;
        PublishedRoutingKey = routingKey;
        PublishedMessageType = messageType;
        PublishedMessageId = messageId;
        PublishedCorrelationId = correlationId;
        PublishedEnqueuedAtUtc = enqueuedAtUtc;
        PublishedBody = body.ToArray();
        return Task.CompletedTask;
    }

    public Task<string> StartConsumerAsync(
        string queueName,
        Func<RabbitMqDelivery, CancellationToken, Task> onReceived,
        CancellationToken cancellationToken = default)
    {
        if (StartConsumerFailuresRemaining-- > 0)
        {
            throw new InvalidOperationException("Consumer start failed.");
        }

        OnReceived = onReceived;
        return Task.FromResult(ConsumerTagToReturn);
    }

    public Task CancelConsumerAsync(string consumerTag, CancellationToken cancellationToken = default)
    {
        CancelCalls++;
        CancelledConsumerTag = consumerTag;

        if (CancelFailuresRemaining-- > 0)
        {
            throw new InvalidOperationException("Consumer cancel failed.");
        }

        return Task.CompletedTask;
    }

    public Task AcknowledgeAsync(ulong deliveryTag, CancellationToken cancellationToken = default)
    {
        AckCalls++;
        return Task.CompletedTask;
    }

    public Task RejectAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken = default)
    {
        RejectCalls++;
        LastRejectRequeue = requeue;
        return Task.CompletedTask;
    }

    public Task NegativeAcknowledgeAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken = default)
    {
        NackCalls++;
        LastNegativeAcknowledgeRequeue = requeue;
        return Task.CompletedTask;
    }

    public Task DeliverAsync<TMessage>(
        TMessage message,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        MessageEnvelope envelope = MessageEnvelope.Create(
            message,
            correlationId is null ? null : Guid.Parse(correlationId));

        byte[] body = RabbitMqEnvelopeSerializer.Serialize<TMessage>(envelope);
        return DeliverRawAsync(body, cancellationToken);
    }

    public Task DeliverEnvelopeAsync<TMessage>(RabbitMqEnvelope<TMessage> envelope, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        byte[] body = JsonSerializer.SerializeToUtf8Bytes(envelope);
        return DeliverRawAsync(body, cancellationToken);
    }

    public Task DeliverRawAsync(byte[] body, CancellationToken cancellationToken = default)
    {
        return OnReceived!(
            new RabbitMqDelivery(11UL, "message-11", null, "exchange", "routing", body),
            cancellationToken);
    }

    public void Dispose()
    {
        Disposed = true;
    }

    public ValueTask DisposeAsync()
    {
        Disposed = true;
        return ValueTask.CompletedTask;
    }
}

internal sealed class RecordingHandler<TMessage> : IMessageHandler<TMessage>
    where TMessage : IMessage
{
    private readonly Func<TMessage, Task>? callback;

    public RecordingHandler(Func<TMessage, Task>? callback = null)
    {
        this.callback = callback;
    }

    public List<TMessage> Messages { get; } = [];

    public int Attempts { get; private set; }

    public async Task HandleAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        Attempts++;
        Messages.Add(message);

        if (callback is not null)
        {
            await callback(message).ConfigureAwait(false);
        }
    }
}

internal static class RabbitMqTestServices
{
    public static ServiceProvider BuildProvider(Action<RabbitMqOptions>? configure = null)
    {
        ServiceCollection services = new();
        services.AddRabbitMqMessaging(options =>
        {
            options.HostName = "localhost";
            options.Port = 5672;
            options.VirtualHost = "/";
            options.UserName = "guest";
            options.Password = "guest";
            options.ExchangeName = "ifx.messages";
            options.ExchangeType = "direct";
            options.QueueName = "ifx.messages";
            options.InitialRetryDelay = TimeSpan.Zero;
            configure?.Invoke(options);
        });

        return services.BuildServiceProvider();
    }

    public static IOptions<RabbitMqOptions> CreateOptions(Action<RabbitMqOptions>? configure = null)
    {
        RabbitMqOptions options = new()
        {
            HostName = "localhost",
            Port = 5672,
            VirtualHost = "/",
            UserName = "guest",
            Password = "guest",
            ExchangeName = "ifx.messages",
            ExchangeType = "direct",
            QueueName = "ifx.messages",
            InitialRetryDelay = TimeSpan.Zero
        };

        configure?.Invoke(options);
        return Options.Create(options);
    }
}
