using Azure.Messaging.ServiceBus;

namespace Util.Messaging.Azure.ServiceBus.UnitTests;

internal sealed class FakeServiceBusClient : IAzureServiceBusClient
{
    public FakeServiceBusSender Sender { get; } = new();

    public FakeServiceBusProcessor QueueProcessor { get; } = new();

    public FakeServiceBusProcessor TopicProcessor { get; } = new();

    public string? LastSenderEntityPath { get; private set; }

    public (string QueueName, ServiceBusProcessorOptions Options)? QueueProcessorRequest { get; private set; }

    public (string TopicName, string SubscriptionName, ServiceBusProcessorOptions Options)? TopicProcessorRequest { get; private set; }

    public IAzureServiceBusSender CreateSender(string entityPath)
    {
        LastSenderEntityPath = entityPath;
        return Sender;
    }

    public IAzureServiceBusProcessor CreateQueueProcessor(string queueName, ServiceBusProcessorOptions options)
    {
        QueueProcessorRequest = (queueName, options);
        return QueueProcessor;
    }

    public IAzureServiceBusProcessor CreateTopicProcessor(string topicName, string subscriptionName, ServiceBusProcessorOptions options)
    {
        TopicProcessorRequest = (topicName, subscriptionName, options);
        return TopicProcessor;
    }
}

internal sealed class FakeServiceBusSender : IAzureServiceBusSender
{
    public List<ServiceBusMessage> SentMessages { get; } = [];

    public List<DateTimeOffset> ScheduledTimes { get; } = [];

    public Func<ServiceBusMessage, CancellationToken, Task>? SendBehavior { get; set; }

    public Func<ServiceBusMessage, DateTimeOffset, CancellationToken, Task>? ScheduleBehavior { get; set; }

    public int SendAttempts { get; private set; }

    public int ScheduleAttempts { get; private set; }

    public async Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken)
    {
        SendAttempts++;
        SentMessages.Add(message);

        if (SendBehavior is not null)
        {
            await SendBehavior(message, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task ScheduleMessageAsync(ServiceBusMessage message, DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken)
    {
        ScheduleAttempts++;
        SentMessages.Add(message);
        ScheduledTimes.Add(scheduledEnqueueTime);

        if (ScheduleBehavior is not null)
        {
            await ScheduleBehavior(message, scheduledEnqueueTime, cancellationToken).ConfigureAwait(false);
        }
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

internal sealed class FakeServiceBusProcessor : IAzureServiceBusProcessor
{
    public Func<IAzureServiceBusReceivedMessageContext, CancellationToken, Task>? MessageHandler { get; set; }

    public Func<Exception, CancellationToken, Task>? ErrorHandler { get; set; }

    public List<FakeReceivedMessageContext> Contexts { get; } = [];

    public int StartCalls { get; private set; }

    public int StopCalls { get; private set; }

    public int DisposeCalls { get; private set; }

    public Task StartProcessingAsync(CancellationToken cancellationToken)
    {
        StartCalls++;
        return Task.CompletedTask;
    }

    public Task StopProcessingAsync(CancellationToken cancellationToken)
    {
        StopCalls++;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        DisposeCalls++;
        return ValueTask.CompletedTask;
    }

    public async Task RaiseMessageAsync(FakeReceivedMessageContext context, CancellationToken cancellationToken = default)
    {
        Contexts.Add(context);

        if (MessageHandler is not null)
        {
            await MessageHandler(context, cancellationToken).ConfigureAwait(false);
        }
    }
}

internal sealed class FakeReceivedMessageContext(BinaryData body) : IAzureServiceBusReceivedMessageContext
{
    public BinaryData Body { get; } = body;

    public string MessageId { get; init; } = Guid.NewGuid().ToString("D");

    public string? CorrelationId { get; init; }

    public int DeliveryCount { get; set; } = 1;

    public int CompleteCalls { get; private set; }

    public int AbandonCalls { get; private set; }

    public int DeadLetterCalls { get; private set; }

    public string? LastDeadLetterReason { get; private set; }

    public string? LastDeadLetterDescription { get; private set; }

    public Task CompleteMessageAsync(CancellationToken cancellationToken)
    {
        CompleteCalls++;
        return Task.CompletedTask;
    }

    public Task AbandonMessageAsync(CancellationToken cancellationToken)
    {
        AbandonCalls++;
        return Task.CompletedTask;
    }

    public Task DeadLetterMessageAsync(string reason, string? description, CancellationToken cancellationToken)
    {
        DeadLetterCalls++;
        LastDeadLetterReason = reason;
        LastDeadLetterDescription = description;
        return Task.CompletedTask;
    }
}
