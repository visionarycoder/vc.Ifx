using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Util.Messaging.Azure.ServiceBus.UnitTests;

[TestClass]
public sealed class AzureServiceBusSdkAbstractionsTests
{
    #region Client Adapter Tests

    [TestMethod]
    public void CreateMembers_WhenCalled_ShouldDelegateToUnderlyingClient()
    {
        RecordingSdkServiceBusClient client = new();
        AzureServiceBusClientAdapter adapter = new(client);
        ServiceBusProcessorOptions options = new() { PrefetchCount = 5 };

        IAzureServiceBusSender sender = adapter.CreateSender("orders");
        IAzureServiceBusProcessor queueProcessor = adapter.CreateQueueProcessor("orders", options);
        IAzureServiceBusProcessor topicProcessor = adapter.CreateTopicProcessor("orders", "billing", options);

        sender.Should().BeOfType<AzureServiceBusSenderAdapter>();
        queueProcessor.Should().BeOfType<AzureServiceBusProcessorAdapter>();
        topicProcessor.Should().BeOfType<AzureServiceBusProcessorAdapter>();
        client.LastSenderEntityPath.Should().Be("orders");
        client.QueueProcessorRequest.Should().Be(("orders", options));
        client.TopicProcessorRequest.Should().Be(("orders", "billing", options));
    }

    #endregion

    #region Sender Adapter Tests

    [TestMethod]
    public async Task SenderAdapter_WhenOperationsAreInvoked_ShouldDelegateToUnderlyingSender()
    {
        RecordingServiceBusSender sender = new();
        AzureServiceBusSenderAdapter adapter = new(sender);
        ServiceBusMessage message = new(BinaryData.FromString("{}"));
        DateTimeOffset scheduledTime = DateTimeOffset.UtcNow.AddMinutes(1);

        await adapter.SendMessageAsync(message, CancellationToken.None);
        await adapter.ScheduleMessageAsync(message, scheduledTime, CancellationToken.None);
        await adapter.DisposeAsync();

        (ServiceBusMessage Message, DateTimeOffset ScheduledTime) scheduledMessage =
            sender.ScheduledMessages.Should().ContainSingle().Subject;
        sender.SentMessages.Should().ContainSingle().Which.Should().BeSameAs(message);
        scheduledMessage.Message.Should().BeSameAs(message);
        scheduledMessage.ScheduledTime.Should().Be(scheduledTime);
        sender.DisposeCalls.Should().Be(1);
    }

    #endregion

    #region Processor Adapter Tests

    [TestMethod]
    public async Task ProcessorAdapter_WhenHandlersAreNull_ShouldIgnoreIncomingEvents()
    {
        RecordingServiceBusProcessor processor = new();
        AzureServiceBusProcessorAdapter adapter = new(processor);

        await adapter.StartProcessingAsync(CancellationToken.None);
        await InvokeProcessMessageAsync(adapter, CreateProcessMessageEventArgs());
        await InvokeProcessErrorAsync(adapter, new ProcessErrorEventArgs(
            new InvalidOperationException("boom"),
            ServiceBusErrorSource.Receive,
            "fully-qualified-namespace",
            "orders",
            CancellationToken.None));

        processor.StartCalls.Should().Be(1);
        await adapter.StopProcessingAsync(CancellationToken.None);
        await adapter.DisposeAsync();
        processor.StopCalls.Should().Be(1);
    }

    [TestMethod]
    public async Task ProcessorAdapter_WhenHandlersAreAssigned_ShouldDelegateEventsAndUnsubscribeOnDispose()
    {
        RecordingServiceBusProcessor processor = new();
        AzureServiceBusProcessorAdapter adapter = new(processor);
        RecordingProcessMessageEventArgs arguments = CreateProcessMessageEventArgs(
            body: BinaryData.FromString("{\"value\":1}"),
            messageId: "message-1",
            correlationId: "corr-1",
            deliveryCount: 4);
        int messageCalls = 0;
        int errorCalls = 0;

        adapter.MessageHandler = async (context, cancellationToken) =>
        {
            messageCalls++;
            context.Body.ToString().Should().NotBeNullOrWhiteSpace();
            context.MessageId.Should().Be("message-1");
            context.CorrelationId.Should().Be("corr-1");
            context.DeliveryCount.Should().BePositive();

            await context.CompleteMessageAsync(cancellationToken);
            await context.AbandonMessageAsync(cancellationToken);
            await context.DeadLetterMessageAsync("invalid", "bad payload", cancellationToken);
        };
        adapter.ErrorHandler = (exception, _) =>
        {
            errorCalls++;
            exception.Message.Should().Be("boom");
            return Task.CompletedTask;
        };

        await adapter.StartProcessingAsync(CancellationToken.None);
        await InvokeProcessMessageAsync(adapter, arguments);
        await InvokeProcessErrorAsync(adapter, new ProcessErrorEventArgs(
            new InvalidOperationException("boom"),
            ServiceBusErrorSource.Receive,
            "fully-qualified-namespace",
            "orders",
            CancellationToken.None));
        await adapter.DisposeAsync();

        messageCalls.Should().Be(1);
        errorCalls.Should().Be(1);
        arguments.CompleteCalls.Should().Be(1);
        arguments.AbandonCalls.Should().Be(1);
        arguments.DeadLetterCalls.Should().Be(1);
    }

    #endregion

    private static RecordingProcessMessageEventArgs CreateProcessMessageEventArgs(
        BinaryData? body = null,
        string messageId = "message-1",
        string? correlationId = "corr-1",
        int deliveryCount = 2)
    {
        ServiceBusReceivedMessage message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: body ?? BinaryData.FromString("{}"),
            messageId: messageId,
            correlationId: correlationId,
            deliveryCount: deliveryCount);

        return new RecordingProcessMessageEventArgs(message, new RecordingServiceBusReceiver());
    }

    private static Task InvokeProcessMessageAsync(
        AzureServiceBusProcessorAdapter adapter,
        ProcessMessageEventArgs arguments)
    {
        MethodInfo method = typeof(AzureServiceBusProcessorAdapter)
            .GetMethod("HandleProcessMessageAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

        return (Task)method.Invoke(adapter, [arguments])!;
    }

    private static Task InvokeProcessErrorAsync(
        AzureServiceBusProcessorAdapter adapter,
        ProcessErrorEventArgs arguments)
    {
        MethodInfo method = typeof(AzureServiceBusProcessorAdapter)
            .GetMethod("HandleProcessErrorAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

        return (Task)method.Invoke(adapter, [arguments])!;
    }
}

internal sealed class RecordingSdkServiceBusClient : ServiceBusClient
{
    public RecordingServiceBusSender Sender { get; } = new();

    public RecordingServiceBusProcessor QueueProcessor { get; } = new();

    public RecordingServiceBusProcessor TopicProcessor { get; } = new();

    public string? LastSenderEntityPath { get; private set; }

    public (string QueueName, ServiceBusProcessorOptions Options)? QueueProcessorRequest { get; private set; }

    public (string TopicName, string SubscriptionName, ServiceBusProcessorOptions Options)? TopicProcessorRequest { get; private set; }

    public override ServiceBusSender CreateSender(string queueOrTopicName)
    {
        LastSenderEntityPath = queueOrTopicName;
        return Sender;
    }

    public override ServiceBusProcessor CreateProcessor(string queueName, ServiceBusProcessorOptions options)
    {
        QueueProcessorRequest = (queueName, options);
        return QueueProcessor;
    }

    public override ServiceBusProcessor CreateProcessor(string topicName, string subscriptionName, ServiceBusProcessorOptions options)
    {
        TopicProcessorRequest = (topicName, subscriptionName, options);
        return TopicProcessor;
    }
}

internal sealed class RecordingServiceBusSender : ServiceBusSender
{
    public List<ServiceBusMessage> SentMessages { get; } = [];

    public List<(ServiceBusMessage Message, DateTimeOffset ScheduledTime)> ScheduledMessages { get; } = [];

    public int DisposeCalls { get; private set; }

    public override Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken = default)
    {
        SentMessages.Add(message);
        return Task.CompletedTask;
    }

    public override Task<long> ScheduleMessageAsync(
        ServiceBusMessage message,
        DateTimeOffset scheduledEnqueueTime,
        CancellationToken cancellationToken = default)
    {
        ScheduledMessages.Add((message, scheduledEnqueueTime));
        return Task.FromResult(42L);
    }

    public override ValueTask DisposeAsync()
    {
        DisposeCalls++;
        return ValueTask.CompletedTask;
    }
}

internal sealed class RecordingServiceBusProcessor : ServiceBusProcessor
{
    public List<RecordingProcessMessageEventArgs> MessageArgs { get; } = [];

    public int StartCalls { get; private set; }

    public int StopCalls { get; private set; }

    public override Task StartProcessingAsync(CancellationToken cancellationToken = default)
    {
        StartCalls++;
        return Task.CompletedTask;
    }

    public override Task StopProcessingAsync(CancellationToken cancellationToken = default)
    {
        StopCalls++;
        return Task.CompletedTask;
    }

}

internal sealed class RecordingServiceBusReceiver : ServiceBusReceiver;

internal sealed class RecordingProcessMessageEventArgs : ProcessMessageEventArgs
{
    public RecordingProcessMessageEventArgs(
        ServiceBusReceivedMessage message,
        ServiceBusReceiver receiver,
        CancellationToken cancellationToken = default)
        : base(message, receiver, cancellationToken)
    {
    }

    public int CompleteCalls { get; private set; }

    public int AbandonCalls { get; private set; }

    public int DeadLetterCalls { get; private set; }

    public override Task CompleteMessageAsync(
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken = default)
    {
        CompleteCalls++;
        return Task.CompletedTask;
    }

    public override Task AbandonMessageAsync(
        ServiceBusReceivedMessage message,
        IDictionary<string, object>? propertiesToModify = null,
        CancellationToken cancellationToken = default)
    {
        AbandonCalls++;
        return Task.CompletedTask;
    }

    public override Task DeadLetterMessageAsync(
        ServiceBusReceivedMessage message,
        string deadLetterReason,
        string deadLetterErrorDescription = null!,
        CancellationToken cancellationToken = default)
    {
        DeadLetterCalls++;
        return Task.CompletedTask;
    }
}
