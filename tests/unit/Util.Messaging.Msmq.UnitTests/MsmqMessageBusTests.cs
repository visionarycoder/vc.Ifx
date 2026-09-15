using FluentAssertions;
using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using Util.Messaging.Msmq;

namespace Util.Messaging.Msmq.UnitTests;

[TestClass]
public sealed class MsmqMessageBusTests
{
    #region Construction Tests

    [TestMethod]
    public void Publisher_WhenAccessed_ShouldReturnCurrentInstance()
    {
        RecordingQueueClient queueClient = new();
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders"));

        bus.Publisher.Should().BeSameAs(bus);
    }

    [TestMethod]
    public void CreateConsumer_WhenSubscriptionNameIsWhitespace_ShouldThrowArgumentException()
    {
        RecordingQueueClient queueClient = new();
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders"));

        Action action = () => _ = bus.CreateConsumer<TestMessage>(" ");

        action.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("subscriptionName");
    }

    [TestMethod]
    public void CreateConsumer_WhenHandlerIsNull_ShouldThrowArgumentNullException()
    {
        RecordingQueueClient queueClient = new();
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders"));

        Action action = () => _ = bus.CreateConsumer<TestMessage>("billing", null!);

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("handler");
    }

    [TestMethod]
    public async Task CreateConsumer_WhenDefaultHandlerConsumesMessage_ShouldCompleteWithoutCustomHandler()
    {
        TestMessage expectedMessage = new();
        SingleMessageQueueClient queueClient = new(expectedMessage);
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders-{subscriptionName}"));
        IMessageConsumer consumer = bus.CreateConsumer<TestMessage>("billing");

        await consumer.StartAsync();
        await queueClient.WaitForReceiveAttemptAsync();
        await consumer.StopAsync();

        queueClient.ReceiveAttempts.Should().BeGreaterThan(0);
        queueClient.LastQueuePath.Should().Be(@".\private$\orders-billing");
        ((IDisposable)consumer).Dispose();
    }

    [TestMethod]
    public void CreateConsumer_WhenCreated_ShouldCaptureSubscriptionMetadata()
    {
        RecordingQueueClient queueClient = new();
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders-{subscriptionName}"));

        IMessageConsumer consumer = bus.CreateConsumer<TestMessage>("billing");
        object registration = GetSingleSubscriptionRegistration(bus);
        string subscriptionName = (string)(registration.GetType().GetProperty("SubscriptionName", BindingFlags.Instance | BindingFlags.Public) ?? throw new InvalidOperationException("SubscriptionName property was not found."))
            .GetValue(registration)!;
        Guid subscriptionId = (Guid)(registration.GetType().GetProperty("SubscriptionId", BindingFlags.Instance | BindingFlags.Public) ?? throw new InvalidOperationException("SubscriptionId property was not found."))
            .GetValue(registration)!;

        subscriptionName.Should().Be("billing");
        subscriptionId.Should().NotBe(Guid.Empty);
        ((IDisposable)consumer).Dispose();
    }

    #endregion

    #region PublishAsync Tests

    [TestMethod]
    public async Task PublishAsync_WhenSubscriptionExists_ShouldSerializeEnvelopeAndSendToResolvedQueue()
    {
        RecordingQueueClient queueClient = new();
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders-{subscriptionName}"));
        IMessageConsumer consumer = bus.CreateConsumer<TestMessage>("billing");
        TestMessage message = new() { CorrelationId = Guid.NewGuid().ToString() };

        await bus.PublishAsync(message);

        queueClient.SentMessages.Should().ContainSingle();
        SentQueueMessage sentMessage = queueClient.SentMessages.Single();
        sentMessage.QueuePath.Should().Be(@".\private$\orders-billing");
        sentMessage.Message.CorrelationId.Should().Be(message.CorrelationId);
        MessageEnvelope envelope = MsmqEnvelopeSerializer.Deserialize(sentMessage.Message.Body);
        envelope.Message.Should().BeEquivalentTo(message);
        envelope.CorrelationId.Should().Be(Guid.Parse(message.CorrelationId!));
        ((IDisposable)consumer).Dispose();
    }

    [TestMethod]
    public async Task PublishAsync_WhenNoSubscriptionsExist_ShouldUseDefaultQueuePath()
    {
        RecordingQueueClient queueClient = new();
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders-{subscriptionName}"));

        await bus.PublishAsync(new TestMessage());

        queueClient.SentMessages.Select(message => message.QueuePath).Should().Equal(@".\private$\orders-default");
    }

    [TestMethod]
    public async Task PublishAsync_WhenQueueClientFails_ShouldRetryUntilAttemptsExhausted()
    {
        FailingQueueClient queueClient = new("send failed");
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders"));
        Func<Task> action = () => bus.PublishAsync(new TestMessage());

        var assertions = await action.Should().ThrowAsync<InvalidOperationException>();

        assertions.Which.Message.Should().Be("send failed");
        queueClient.SendAttempts.Should().Be(3);
    }

    [TestMethod]
    public async Task PublishAsync_WhenMessageIsNull_ShouldThrowArgumentNullException()
    {
        RecordingQueueClient queueClient = new();
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders"));

        Func<Task> action = () => bus.PublishAsync<TestMessage>(null!);

        var assertions = await action.Should().ThrowAsync<ArgumentNullException>();
        assertions.Which.ParamName.Should().Be("message");
    }

    [TestMethod]
    public async Task PublishBatchAsync_WhenMessagesProvided_ShouldSendEachMessage()
    {
        RecordingQueueClient queueClient = new();
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders"));

        await bus.PublishBatchAsync([new TestMessage(), new TestMessage()]);

        queueClient.SentMessages.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task PublishBatchAsync_WhenMessagesAreNull_ShouldThrowArgumentNullException()
    {
        RecordingQueueClient queueClient = new();
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders"));

        Func<Task> action = () => bus.PublishBatchAsync<TestMessage>(null!);

        var assertions = await action.Should().ThrowAsync<ArgumentNullException>();
        assertions.Which.ParamName.Should().Be("messages");
    }

    [TestMethod]
    public async Task PublishBatchAsync_WhenMessagesAreEmpty_ShouldNotSendAnyMessage()
    {
        RecordingQueueClient queueClient = new();
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders"));

        await bus.PublishBatchAsync(Array.Empty<TestMessage>());

        queueClient.SentMessages.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ScheduleAsync_WhenScheduledTimeHasElapsed_ShouldPublishImmediately()
    {
        RecordingQueueClient queueClient = new();
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders"));

        await bus.ScheduleAsync(new TestMessage(), DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMilliseconds(1)));

        queueClient.SentMessages.Should().ContainSingle();
    }

    [TestMethod]
    public async Task ScheduleAsync_WhenScheduledTimeIsInTheFuture_ShouldDelayThenPublish()
    {
        RecordingQueueClient queueClient = new();
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders"));
        DateTimeOffset startedAt = DateTimeOffset.UtcNow;

        await bus.ScheduleAsync(new TestMessage(), startedAt.AddMilliseconds(25));

        DateTimeOffset completedAt = DateTimeOffset.UtcNow;
        queueClient.SentMessages.Should().ContainSingle();
        (completedAt - startedAt).Should().BeGreaterThan(TimeSpan.FromMilliseconds(10));
    }

    [TestMethod]
    public async Task ScheduleAsync_WhenMessageIsNull_ShouldThrowArgumentNullException()
    {
        RecordingQueueClient queueClient = new();
        MsmqMessageBus bus = new(queueClient, CreateOptions(@".\private$\orders"));

        Func<Task> action = () => bus.ScheduleAsync<TestMessage>(null!, DateTimeOffset.UtcNow);

        var assertions = await action.Should().ThrowAsync<ArgumentNullException>();
        assertions.Which.ParamName.Should().Be("message");
    }

    #endregion

    private static IOptions<MsmqOptions> CreateOptions(string queuePath)
    {
        return Options.Create(new MsmqOptions
        {
            QueuePath = queuePath,
            MaxRetryAttempts = 3,
            InitialRetryDelay = TimeSpan.Zero,
            ReceiveTimeout = TimeSpan.FromMilliseconds(10)
        });
    }

    private static object GetSingleSubscriptionRegistration(MsmqMessageBus bus)
    {
        FieldInfo subscriptionsField = typeof(MsmqMessageBus).GetField("subscriptions", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("subscriptions field was not found.");
        System.Collections.IDictionary subscriptions = (System.Collections.IDictionary)(subscriptionsField.GetValue(bus)
            ?? throw new InvalidOperationException("subscriptions were not available."));

        subscriptions.Count.Should().Be(1);
        return subscriptions.Keys.Cast<object>().Single();
    }

    private sealed class RecordingQueueClient : IMsmqQueueClient
    {
        public List<SentQueueMessage> SentMessages { get; } = [];

        public Task SendAsync(string queuePath, SerializedMsmqMessage message, CancellationToken cancellationToken)
        {
            SentMessages.Add(new SentQueueMessage(queuePath, message));
            return Task.CompletedTask;
        }

        public Task<ReceivedMsmqMessage?> ReceiveAsync(string queuePath, TimeSpan receiveTimeout, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class SingleMessageQueueClient(TestMessage message) : IMsmqQueueClient
    {
        private int delivered;
        private readonly TaskCompletionSource startedSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public string? LastQueuePath { get; private set; }

        public int ReceiveAttempts { get; private set; }

        public Task SendAsync(string queuePath, SerializedMsmqMessage message, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public async Task<ReceivedMsmqMessage?> ReceiveAsync(string queuePath, TimeSpan receiveTimeout, CancellationToken cancellationToken)
        {
            LastQueuePath = queuePath;
            ReceiveAttempts++;
            startedSource.TrySetResult();

            if (Interlocked.Exchange(ref delivered, 1) == 0)
            {
                SerializedMsmqMessage serializedMessage = MsmqEnvelopeSerializer.Serialize(message);
                return new ReceivedMsmqMessage(serializedMessage.Body, serializedMessage.CorrelationId, serializedMessage.Label);
            }

            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return null;
        }

        public Task WaitForReceiveAttemptAsync()
        {
            return startedSource.Task.WaitAsync(TimeSpan.FromSeconds(2));
        }
    }

    private sealed class FailingQueueClient(string errorMessage) : IMsmqQueueClient
    {
        public int SendAttempts { get; private set; }

        public Task SendAsync(string queuePath, SerializedMsmqMessage message, CancellationToken cancellationToken)
        {
            SendAttempts++;
            throw new InvalidOperationException(errorMessage);
        }

        public Task<ReceivedMsmqMessage?> ReceiveAsync(string queuePath, TimeSpan receiveTimeout, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed record SentQueueMessage(string QueuePath, SerializedMsmqMessage Message);

    private sealed record TestMessage : MessageBase;
}
