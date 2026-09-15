using System.Collections.Concurrent;
using FluentAssertions;
using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Util.Messaging.Msmq;

namespace Util.Messaging.Msmq.UnitTests;

[TestClass]
public sealed class MsmqMessageConsumerTests
{
    #region Consumer Lifecycle Tests

    [TestMethod]
    public async Task StartAsync_WhenMessageArrives_ShouldDispatchToHandler()
    {
        TestMessage expectedMessage = new() { CorrelationId = Guid.NewGuid().ToString() };
        Queue<TestMessage> messages = new([expectedMessage]);
        RecordingHandler handler = new();
        TestQueueClient queueClient = new(messages);
        MsmqMessageConsumer<TestMessage> consumer = new(
            queueClient,
            CreateOptions(),
            handler,
            @".\private$\orders",
            () => { });

        await consumer.StartAsync();
        TestMessage handledMessage = await handler.WaitAsync();
        await consumer.StopAsync();

        handledMessage.Should().BeEquivalentTo(expectedMessage);
        queueClient.ReceiveAttempts.Should().BeGreaterThan(0);
    }

    [TestMethod]
    public async Task StopAsync_WhenConsumerNeverStarted_ShouldSucceed()
    {
        MsmqMessageConsumer<TestMessage> consumer = new(
            new TestQueueClient([]),
            CreateOptions(),
            new RecordingHandler(),
            @".\private$\orders",
            () => { });

        Func<Task> action = () => consumer.StopAsync();

        await action.Should().NotThrowAsync();
    }

    [TestMethod]
    public async Task StartAsync_WhenCalledMoreThanOnce_ShouldReuseExistingReceiveLoop()
    {
        BlockingQueueClient queueClient = new();
        MsmqMessageConsumer<TestMessage> consumer = new(
            queueClient,
            CreateOptions(),
            new RecordingHandler(),
            @".\private$\orders",
            () => { });

        await consumer.StartAsync();
        await queueClient.WaitForReceiveAttemptAsync();
        await consumer.StartAsync();
        await consumer.StopAsync(new CancellationToken(canceled: true));
        queueClient.Release();

        queueClient.ReceiveAttempts.Should().Be(1);
    }

    [TestMethod]
    public async Task StartAsync_WhenCancellationIsRequested_ShouldThrowOperationCanceledException()
    {
        MsmqMessageConsumer<TestMessage> consumer = new(
            new TestQueueClient([]),
            CreateOptions(),
            new RecordingHandler(),
            @".\private$\orders",
            () => { });
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        Func<Task> action = () => consumer.StartAsync(cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task StopAsync_WhenWaitIsCanceled_ShouldSwallowOperationCanceledExceptionAndResetState()
    {
        BlockingQueueClient queueClient = new();
        MsmqMessageConsumer<TestMessage> consumer = new(
            queueClient,
            CreateOptions(),
            new RecordingHandler(),
            @".\private$\orders",
            () => { });

        await consumer.StartAsync();
        await queueClient.WaitForReceiveAttemptAsync();
        await consumer.StopAsync(new CancellationToken(canceled: true));
        queueClient.Release();
        await consumer.StopAsync();

        queueClient.ReceiveAttempts.Should().Be(1);
    }

    [TestMethod]
    public async Task StopAsync_WhenReceivedMessageTypeDoesNotMatchConsumerType_ShouldRethrowInvalidOperationException()
    {
        WrongTypeQueueClient queueClient = new();
        MsmqMessageConsumer<TestMessage> consumer = new(
            queueClient,
            CreateOptions(),
            new RecordingHandler(),
            @".\private$\orders",
            () => { });

        await consumer.StartAsync();
        await queueClient.WaitForReceiveAttemptAsync();
        Func<Task> action = () => consumer.StopAsync();

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Received message type '*AnotherMessage' does not match consumer type '*TestMessage'.");
    }

    [TestMethod]
    public void Dispose_WhenCalled_ShouldUnsubscribe()
    {
        var unsubscribed = false;
        MsmqMessageConsumer<TestMessage> consumer = new(
            new TestQueueClient([]),
            CreateOptions(),
            new RecordingHandler(),
            @".\private$\orders",
            () => unsubscribed = true);

        consumer.Dispose();

        unsubscribed.Should().BeTrue();
    }

    #endregion

    private static IOptions<MsmqOptions> CreateOptions()
    {
        return Options.Create(new MsmqOptions
        {
            QueuePath = @".\private$\orders",
            MaxRetryAttempts = 3,
            InitialRetryDelay = TimeSpan.Zero,
            ReceiveTimeout = TimeSpan.FromMilliseconds(10)
        });
    }

    private sealed class RecordingHandler : IMessageHandler<TestMessage>
    {
        private readonly TaskCompletionSource<TestMessage> messageSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            messageSource.TrySetResult(message);
            return Task.CompletedTask;
        }

        public Task<TestMessage> WaitAsync()
        {
            return messageSource.Task.WaitAsync(TimeSpan.FromSeconds(2));
        }
    }

    private sealed class BlockingQueueClient : IMsmqQueueClient
    {
        private readonly TaskCompletionSource completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource startedSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int ReceiveAttempts { get; private set; }

        public Task SendAsync(string queuePath, SerializedMsmqMessage message, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public async Task<ReceivedMsmqMessage?> ReceiveAsync(string queuePath, TimeSpan receiveTimeout, CancellationToken cancellationToken)
        {
            ReceiveAttempts++;
            startedSource.TrySetResult();
            await completionSource.Task.WaitAsync(cancellationToken);
            return null;
        }

        public void Release()
        {
            completionSource.TrySetResult();
        }

        public Task WaitForReceiveAttemptAsync()
        {
            return startedSource.Task.WaitAsync(TimeSpan.FromSeconds(2));
        }
    }

    private sealed class TestQueueClient(Queue<TestMessage> messages) : IMsmqQueueClient
    {
        private readonly ConcurrentQueue<TestMessage> messages = new(messages);

        public int ReceiveAttempts { get; private set; }

        public Task SendAsync(string queuePath, SerializedMsmqMessage message, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public async Task<ReceivedMsmqMessage?> ReceiveAsync(string queuePath, TimeSpan receiveTimeout, CancellationToken cancellationToken)
        {
            ReceiveAttempts++;

            if (messages.TryDequeue(out TestMessage? message))
            {
                SerializedMsmqMessage serializedMessage = MsmqEnvelopeSerializer.Serialize(message);
                return new ReceivedMsmqMessage(serializedMessage.Body, serializedMessage.CorrelationId, serializedMessage.Label);
            }

            await Task.Delay(receiveTimeout, cancellationToken);
            return null;
        }
    }

    private sealed class WrongTypeQueueClient : IMsmqQueueClient
    {
        private readonly TaskCompletionSource startedSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task SendAsync(string queuePath, SerializedMsmqMessage message, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<ReceivedMsmqMessage?> ReceiveAsync(string queuePath, TimeSpan receiveTimeout, CancellationToken cancellationToken)
        {
            SerializedMsmqMessage serializedMessage = MsmqEnvelopeSerializer.Serialize(new AnotherMessage());
            startedSource.TrySetResult();
            return Task.FromResult<ReceivedMsmqMessage?>(
                new ReceivedMsmqMessage(serializedMessage.Body, serializedMessage.CorrelationId, serializedMessage.Label));
        }

        public Task WaitForReceiveAttemptAsync()
        {
            return startedSource.Task.WaitAsync(TimeSpan.FromSeconds(2));
        }
    }

    private sealed record AnotherMessage : MessageBase;

    private sealed record TestMessage : MessageBase;
}
