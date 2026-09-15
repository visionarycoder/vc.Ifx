using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.Options;

namespace Util.Messaging.Azure.ServiceBus;

/// <summary>
/// Publishes messages to Azure Service Bus and creates consumers for configured entities.
/// </summary>
public sealed class AzureServiceBusMessageBus : IMessageBus, IMessagePublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IAzureServiceBusClient client;
    private readonly AzureServiceBusOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureServiceBusMessageBus"/> class.
    /// </summary>
    /// <param name="client">The Azure Service Bus client.</param>
    /// <param name="options">The messaging provider options.</param>
    public AzureServiceBusMessageBus(ServiceBusClient client, IOptions<AzureServiceBusOptions> options)
        : this(CreateClientAdapter(client), options)
    {
    }

    internal AzureServiceBusMessageBus(IAzureServiceBusClient client, IOptions<AzureServiceBusOptions> options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        this.client = client;
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
    /// Creates a consumer for a specific message type and handler.
    /// </summary>
    /// <typeparam name="TMessage">The message type to consume.</typeparam>
    /// <param name="subscriptionName">The subscription name when consuming from a topic.</param>
    /// <param name="handler">The message handler to invoke.</param>
    /// <returns>A configured message consumer.</returns>
    public IMessageConsumer CreateConsumer<TMessage>(string subscriptionName, IMessageHandler<TMessage> handler)
        where TMessage : IMessage
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);
        ArgumentNullException.ThrowIfNull(handler);

        IAzureServiceBusProcessor processor = options.UsesQueue()
            ? client.CreateQueueProcessor(options.QueueName!, options.CreateProcessorOptions())
            : client.CreateTopicProcessor(options.TopicName!, subscriptionName, options.CreateProcessorOptions());

        return new AzureServiceBusMessageConsumer<TMessage>(processor, handler, options);
    }

    /// <inheritdoc />
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        return RetryPolicy.ExecuteAsync(
            token => SendWithScopedSenderAsync(message, token),
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
    public Task ScheduleAsync<TMessage>(
        TMessage message,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        return RetryPolicy.ExecuteAsync(
            token => ScheduleWithScopedSenderAsync(message, scheduledTime, token),
            options.MaxRetryAttempts,
            options.InitialRetryDelay,
            cancellationToken);
    }

    internal static TMessage DeserializeMessage<TMessage>(BinaryData body)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(body);

        ServiceBusEnvelopePayload? envelope = JsonSerializer.Deserialize<ServiceBusEnvelopePayload>(
            body.ToString(),
            SerializerOptions);

        if (envelope is null)
        {
            throw new JsonException("The Service Bus message body did not contain an envelope.");
        }

        TMessage? message = envelope.Message.Deserialize<TMessage>(SerializerOptions);
        if (message is null)
        {
            throw new JsonException($"The Service Bus message body did not contain a {typeof(TMessage).FullName} payload.");
        }

        return message;
    }

    private static Guid? TryParseCorrelationId(string? correlationId)
    {
        return Guid.TryParse(correlationId, out Guid parsedCorrelationId)
            ? parsedCorrelationId
            : null;
    }

    private static IAzureServiceBusClient CreateClientAdapter(ServiceBusClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        return new AzureServiceBusClientAdapter(client);
    }

    private static ServiceBusMessage CreateTransportMessage<TMessage>(TMessage message)
        where TMessage : IMessage
    {
        MessageEnvelope envelope = MessageEnvelope.Create(message, TryParseCorrelationId(message.CorrelationId));
        ServiceBusEnvelopePayload payload = ServiceBusEnvelopePayload.Create(envelope, message.GetType(), SerializerOptions);
        string serializedEnvelope = JsonSerializer.Serialize(payload, SerializerOptions);

        ServiceBusMessage transportMessage = new(BinaryData.FromString(serializedEnvelope))
        {
            ContentType = "application/json",
            CorrelationId = envelope.CorrelationId?.ToString() ?? message.CorrelationId,
            MessageId = envelope.MessageId.ToString("D"),
            Subject = message.GetType().FullName
        };

        transportMessage.ApplicationProperties["messageType"] = payload.MessageType;

        return transportMessage;
    }

    private async Task SendWithScopedSenderAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IMessage
    {
        IAzureServiceBusSender sender = client.CreateSender(options.GetEntityPath());

        try
        {
            await sender.SendMessageAsync(CreateTransportMessage(message), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await sender.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task ScheduleWithScopedSenderAsync<TMessage>(TMessage message, DateTimeOffset scheduledTime, CancellationToken cancellationToken)
        where TMessage : IMessage
    {
        IAzureServiceBusSender sender = client.CreateSender(options.GetEntityPath());

        try
        {
            await sender.ScheduleMessageAsync(CreateTransportMessage(message), scheduledTime, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await sender.DisposeAsync().ConfigureAwait(false);
        }
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
