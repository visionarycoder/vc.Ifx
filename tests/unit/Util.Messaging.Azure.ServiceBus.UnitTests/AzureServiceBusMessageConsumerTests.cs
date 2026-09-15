using System.Text.Json;
using FluentAssertions;
using Ifx.Messaging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Util.Messaging.Azure.ServiceBus.UnitTests;

[TestClass]
public sealed class AzureServiceBusMessageConsumerTests
{
    #region Processing Tests

    [TestMethod]
    public async Task StartAsync_WhenValidMessageArrives_ShouldInvokeHandlerAndCompleteMessage()
    {
        FakeServiceBusProcessor processor = new();
        RecordingMessageHandler handler = new();
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            processor,
            handler,
            TestData.CreateOptions());

        await consumer.StartAsync();
        await processor.RaiseMessageAsync(TestData.CreateContext(new TestMessage { OrderNumber = "SO-1001" }));

        processor.StartCalls.Should().Be(1);
        handler.Messages.Should().ContainSingle().Which.OrderNumber.Should().Be("SO-1001");
        processor.Contexts.Should().ContainSingle().Which.CompleteCalls.Should().Be(1);
    }

    [TestMethod]
    public async Task StartAsync_WhenHandlerFailsAtDeliveryLimit_ShouldRetryAndDeadLetterMessage()
    {
        FakeServiceBusProcessor processor = new();
        ThrowingMessageHandler handler = new();
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            processor,
            handler,
            TestData.CreateOptions(maxDeliveryCountBeforeDeadLetter: 1));

        await consumer.StartAsync();
        await processor.RaiseMessageAsync(TestData.CreateContext(new TestMessage(), deliveryCount: 1));

        handler.Attempts.Should().Be(3);
        processor.Contexts.Should().ContainSingle().Which.DeadLetterCalls.Should().Be(1);
    }

    [TestMethod]
    public async Task StartAsync_WhenPayloadIsInvalid_ShouldDeadLetterMessage()
    {
        FakeServiceBusProcessor processor = new();
        RecordingMessageHandler handler = new();
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            processor,
            handler,
            TestData.CreateOptions());

        await consumer.StartAsync();
        await processor.RaiseMessageAsync(new FakeReceivedMessageContext(BinaryData.FromString("not-json")));

        handler.Messages.Should().BeEmpty();
        processor.Contexts.Should().ContainSingle().Which.DeadLetterCalls.Should().Be(1);
    }

    [TestMethod]
    public async Task StopAsync_WhenConsumerWasNotStarted_ShouldReturnWithoutCallingProcessor()
    {
        FakeServiceBusProcessor processor = new();
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            processor,
            new RecordingMessageHandler(),
            TestData.CreateOptions());

        await consumer.StopAsync();

        processor.StopCalls.Should().Be(0);
    }

    [TestMethod]
    public async Task DisposeAsync_WhenConsumerWasStarted_ShouldStopAndDisposeProcessor()
    {
        FakeServiceBusProcessor processor = new();
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            processor,
            new RecordingMessageHandler(),
            TestData.CreateOptions());

        await consumer.StartAsync();
        await consumer.DisposeAsync();

        processor.StopCalls.Should().Be(1);
        processor.DisposeCalls.Should().Be(1);
    }

    #endregion

    private sealed class RecordingMessageHandler : IMessageHandler<TestMessage>
    {
        public List<TestMessage> Messages { get; } = [];

        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingMessageHandler : IMessageHandler<TestMessage>
    {
        public int Attempts { get; private set; }

        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            Attempts++;
            throw new InvalidOperationException("Handler failed.");
        }
    }

    private sealed record TestMessage : MessageBase
    {
        public string OrderNumber { get; init; } = "SO-0001";
    }
}
