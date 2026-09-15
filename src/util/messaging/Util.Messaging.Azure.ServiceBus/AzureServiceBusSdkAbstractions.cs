using Azure.Messaging.ServiceBus;

namespace Util.Messaging.Azure.ServiceBus;

internal interface IAzureServiceBusClient
{
    IAzureServiceBusSender CreateSender(string entityPath);

    IAzureServiceBusProcessor CreateQueueProcessor(string queueName, ServiceBusProcessorOptions options);

    IAzureServiceBusProcessor CreateTopicProcessor(string topicName, string subscriptionName, ServiceBusProcessorOptions options);
}

internal interface IAzureServiceBusSender : IAsyncDisposable
{
    Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken);

    Task ScheduleMessageAsync(ServiceBusMessage message, DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken);
}

internal interface IAzureServiceBusProcessor : IAsyncDisposable
{
    Func<IAzureServiceBusReceivedMessageContext, CancellationToken, Task>? MessageHandler { get; set; }

    Func<Exception, CancellationToken, Task>? ErrorHandler { get; set; }

    Task StartProcessingAsync(CancellationToken cancellationToken);

    Task StopProcessingAsync(CancellationToken cancellationToken);
}

internal interface IAzureServiceBusReceivedMessageContext
{
    BinaryData Body { get; }

    string MessageId { get; }

    string? CorrelationId { get; }

    int DeliveryCount { get; }

    Task CompleteMessageAsync(CancellationToken cancellationToken);

    Task AbandonMessageAsync(CancellationToken cancellationToken);

    Task DeadLetterMessageAsync(string reason, string? description, CancellationToken cancellationToken);
}

internal sealed class AzureServiceBusClientAdapter(ServiceBusClient client) : IAzureServiceBusClient
{
    public IAzureServiceBusSender CreateSender(string entityPath)
    {
        return new AzureServiceBusSenderAdapter(client.CreateSender(entityPath));
    }

    public IAzureServiceBusProcessor CreateQueueProcessor(string queueName, ServiceBusProcessorOptions options)
    {
        return new AzureServiceBusProcessorAdapter(client.CreateProcessor(queueName, options));
    }

    public IAzureServiceBusProcessor CreateTopicProcessor(string topicName, string subscriptionName, ServiceBusProcessorOptions options)
    {
        return new AzureServiceBusProcessorAdapter(client.CreateProcessor(topicName, subscriptionName, options));
    }
}

internal sealed class AzureServiceBusSenderAdapter(ServiceBusSender sender) : IAzureServiceBusSender
{
    public Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken)
    {
        return sender.SendMessageAsync(message, cancellationToken);
    }

    public async Task ScheduleMessageAsync(ServiceBusMessage message, DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken)
    {
        _ = await sender.ScheduleMessageAsync(message, scheduledEnqueueTime, cancellationToken).ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        return sender.DisposeAsync();
    }
}

internal sealed class AzureServiceBusProcessorAdapter(ServiceBusProcessor processor) : IAzureServiceBusProcessor
{
    public Func<IAzureServiceBusReceivedMessageContext, CancellationToken, Task>? MessageHandler { get; set; }

    public Func<Exception, CancellationToken, Task>? ErrorHandler { get; set; }

    public Task StartProcessingAsync(CancellationToken cancellationToken)
    {
        processor.ProcessMessageAsync += HandleProcessMessageAsync;
        processor.ProcessErrorAsync += HandleProcessErrorAsync;

        return processor.StartProcessingAsync(cancellationToken);
    }

    public Task StopProcessingAsync(CancellationToken cancellationToken)
    {
        return processor.StopProcessingAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        processor.ProcessMessageAsync -= HandleProcessMessageAsync;
        processor.ProcessErrorAsync -= HandleProcessErrorAsync;
        await processor.DisposeAsync().ConfigureAwait(false);
    }

    private Task HandleProcessMessageAsync(ProcessMessageEventArgs arguments)
    {
        if (MessageHandler is null)
        {
            return Task.CompletedTask;
        }

        return MessageHandler(new AzureServiceBusReceivedMessageContextAdapter(arguments), arguments.CancellationToken);
    }

    private Task HandleProcessErrorAsync(ProcessErrorEventArgs arguments)
    {
        if (ErrorHandler is null)
        {
            return Task.CompletedTask;
        }

        return ErrorHandler(arguments.Exception, arguments.CancellationToken);
    }
}

internal sealed class AzureServiceBusReceivedMessageContextAdapter(ProcessMessageEventArgs arguments) : IAzureServiceBusReceivedMessageContext
{
    public BinaryData Body => arguments.Message.Body;

    public string MessageId => arguments.Message.MessageId;

    public string? CorrelationId => arguments.Message.CorrelationId;

    public int DeliveryCount => arguments.Message.DeliveryCount;

    public Task CompleteMessageAsync(CancellationToken cancellationToken)
    {
        return arguments.CompleteMessageAsync(arguments.Message, cancellationToken);
    }

    public Task AbandonMessageAsync(CancellationToken cancellationToken)
    {
        return arguments.AbandonMessageAsync(arguments.Message, cancellationToken: cancellationToken);
    }

    public Task DeadLetterMessageAsync(string reason, string? description, CancellationToken cancellationToken)
    {
        return arguments.DeadLetterMessageAsync(arguments.Message, reason, description, cancellationToken);
    }
}
