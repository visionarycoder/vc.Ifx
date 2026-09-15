using FluentAssertions;
using Ifx.Messaging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Util.Messaging.Azure.ServiceBus.UnitTests;

[TestClass]
public sealed class AzureServiceBusMessageConsumerAdditionalTests
{
    #region Lifecycle Tests

    [TestMethod]
    public async Task StartAsync_WhenCalledMoreThanOnce_ShouldReuseExistingProcessor()
    {
        FakeServiceBusProcessor processor = new();
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            processor,
            new RecordingHandler(),
            TestData.CreateOptions());

        await consumer.StartAsync();
        await consumer.StartAsync();

        processor.StartCalls.Should().Be(1);
    }

    [TestMethod]
    public async Task StartAsync_WhenConsumerWasDisposed_ShouldThrowObjectDisposedException()
    {
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            new FakeServiceBusProcessor(),
            new RecordingHandler(),
            TestData.CreateOptions());
        await consumer.DisposeAsync();

        Func<Task> action = () => consumer.StartAsync();

        await action.Should().ThrowAsync<ObjectDisposedException>();
    }

    [TestMethod]
    public async Task DisposeAsync_WhenCalledTwice_ShouldIgnoreSecondInvocation()
    {
        FakeServiceBusProcessor processor = new();
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            processor,
            new RecordingHandler(),
            TestData.CreateOptions());

        await consumer.DisposeAsync();
        await consumer.DisposeAsync();

        processor.DisposeCalls.Should().Be(1);
    }

    #endregion

    #region Processing Tests

    [TestMethod]
    public async Task StartAsync_WhenHandlerThrowsNotSupportedException_ShouldDeadLetterMessage()
    {
        FakeServiceBusProcessor processor = new();
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            processor,
            new DelegateHandler(_ => throw new NotSupportedException("unsupported")),
            TestData.CreateOptions());

        await consumer.StartAsync();
        await processor.RaiseMessageAsync(TestData.CreateContext(new TestMessage()));

        processor.Contexts.Should().ContainSingle().Which.LastDeadLetterReason.Should().Be("UnsupportedMessagePayload");
    }

    [TestMethod]
    public async Task StartAsync_WhenHandlerThrowsArgumentException_ShouldDeadLetterMessage()
    {
        FakeServiceBusProcessor processor = new();
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            processor,
            new DelegateHandler(_ => throw new ArgumentException("bad", "message")),
            TestData.CreateOptions());

        await consumer.StartAsync();
        await processor.RaiseMessageAsync(TestData.CreateContext(new TestMessage()));

        FakeReceivedMessageContext context = processor.Contexts.Should().ContainSingle().Subject;
        context.DeadLetterCalls.Should().Be(1);
        context.LastDeadLetterReason.Should().Be("InvalidMessagePayload");
    }

    [TestMethod]
    public async Task StartAsync_WhenHandlerCancellationIsRequested_ShouldRethrowOperationCanceledException()
    {
        FakeServiceBusProcessor processor = new();
        CancellationTokenSource cancellationTokenSource = new();
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            processor,
            new DelegateHandler(token =>
            {
                cancellationTokenSource.Cancel();
                throw new OperationCanceledException(token);
            }),
            TestData.CreateOptions());
        await consumer.StartAsync();

        Func<Task> action = () => processor.RaiseMessageAsync(
            TestData.CreateContext(new TestMessage()),
            cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
        processor.Contexts.Should().ContainSingle().Which.AbandonCalls.Should().Be(0);
    }

    [TestMethod]
    public async Task StartAsync_WhenHandlerFailsBeforeDeliveryLimit_ShouldAbandonMessage()
    {
        FakeServiceBusProcessor processor = new();
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            processor,
            new DelegateHandler(_ => throw new InvalidOperationException("retry")),
            TestData.CreateOptions(maxDeliveryCountBeforeDeadLetter: 4));

        await consumer.StartAsync();
        await processor.RaiseMessageAsync(TestData.CreateContext(new TestMessage(), deliveryCount: 1));

        FakeReceivedMessageContext context = processor.Contexts.Should().ContainSingle().Subject;
        context.AbandonCalls.Should().Be(1);
        context.DeadLetterCalls.Should().Be(0);
    }

    [TestMethod]
    public async Task ErrorHandler_WhenExceptionIsNull_ShouldThrowArgumentNullException()
    {
        FakeServiceBusProcessor processor = new();
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            processor,
            new RecordingHandler(),
            TestData.CreateOptions());
        await consumer.StartAsync();

        Func<Task> action = () => processor.ErrorHandler!(null!, CancellationToken.None);

        var assertions = await action.Should().ThrowAsync<ArgumentNullException>();
        assertions.Which.ParamName.Should().Be("exception");
    }

    [TestMethod]
    public async Task ErrorHandler_WhenCancellationIsRequested_ShouldThrowOperationCanceledException()
    {
        FakeServiceBusProcessor processor = new();
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            processor,
            new RecordingHandler(),
            TestData.CreateOptions());
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        await consumer.StartAsync();

        Func<Task> action = () => processor.ErrorHandler!(new InvalidOperationException("boom"), cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task ErrorHandler_WhenExceptionIsProvidedAndCancellationIsNotRequested_ShouldComplete()
    {
        FakeServiceBusProcessor processor = new();
        AzureServiceBusMessageConsumer<TestMessage> consumer = new(
            processor,
            new RecordingHandler(),
            TestData.CreateOptions());
        await consumer.StartAsync();

        await processor.ErrorHandler!(new InvalidOperationException("boom"), CancellationToken.None);
    }

    #endregion

    private sealed class RecordingHandler : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class DelegateHandler(Func<CancellationToken, Task> callback) : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            return callback(cancellationToken);
        }
    }

    private sealed record TestMessage : MessageBase;
}
