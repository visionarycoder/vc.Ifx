using FluentAssertions;
using Ifx.Messaging.Abstractions;

namespace Util.Messaging.RabbitMQ.UnitTests;

[TestClass]
public sealed class RabbitMqMessageBusTests
{
    #region Construction Tests

    [TestMethod]
    public void Publisher_WhenAccessed_ShouldReturnCurrentInstance()
    {
        FakeRabbitMqChannelFactory factory = new();
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        bus.Publisher.Should().BeSameAs(bus);
    }

    [TestMethod]
    public void Constructor_WhenConcreteFactoryAndOptionsAreProvided_ShouldCreateBus()
    {
        using RabbitMqChannelFactory factory = new(RabbitMqTestServices.CreateOptions());

        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        bus.Should().NotBeNull();
        bus.Publisher.Should().BeSameAs(bus);
    }

    [TestMethod]
    public void CreateConsumer_WhenHandlerIsNotProvided_ShouldUseNullHandlerConsumer()
    {
        FakeRabbitMqChannelFactory factory = new();
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        IMessageConsumer consumer = bus.CreateConsumer<TestMessage>("billing");

        consumer.Should().BeOfType<RabbitMqMessageConsumer<TestMessage>>();
    }

    [TestMethod]
    public async Task CreateConsumer_WhenDefaultHandlerConsumesMessage_ShouldAcknowledgeWithoutCustomHandler()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new();
        factory.Enqueue(channel);
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());
        IMessageConsumer consumer = bus.CreateConsumer<TestMessage>("billing");

        await consumer.StartAsync();
        await channel.DeliverAsync(new TestMessage());

        channel.AckCalls.Should().Be(1);
        channel.RejectCalls.Should().Be(0);
        ((IDisposable)consumer).Dispose();
    }

    [TestMethod]
    public void CreateConsumer_WhenSubscriptionNameIsWhitespace_ShouldThrowArgumentException()
    {
        FakeRabbitMqChannelFactory factory = new();
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        Action action = () => _ = bus.CreateConsumer<TestMessage>(" ");

        action.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("subscriptionName");
    }

    [TestMethod]
    public void CreateConsumer_WhenHandlerIsNull_ShouldThrowArgumentNullException()
    {
        FakeRabbitMqChannelFactory factory = new();
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        Action action = () => _ = bus.CreateConsumer<TestMessage>("billing", null!);

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("handler");
    }

    #endregion

    #region PublishAsync Tests

    [TestMethod]
    public async Task PublishAsync_WhenMessageIsValid_ShouldStampEnvelopeAndSetCorrelationId()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new();
        factory.Enqueue(channel);
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());
        Guid correlationId = Guid.NewGuid();
        TestMessage message = new() { CorrelationId = correlationId.ToString("D") };

        await bus.PublishAsync(message);

        RabbitMqEnvelope<TestMessage>? envelope = RabbitMqEnvelopeSerializer.Deserialize<TestMessage>(channel.PublishedBody!);

        channel.DeclareExchangeCalls.Should().Be(1);
        channel.PublishCalls.Should().Be(1);
        channel.PublishedExchangeName.Should().Be("ifx.messages");
        channel.PublishedRoutingKey.Should().Be(typeof(TestMessage).FullName);
        channel.PublishedCorrelationId.Should().Be(correlationId.ToString("D"));
        envelope.Should().NotBeNull();
        envelope!.Message.Should().BeEquivalentTo(message);
        envelope.CorrelationId.Should().Be(correlationId);
        envelope.MessageId.Should().NotBe(Guid.Empty);
    }

    [TestMethod]
    public async Task PublishAsync_WhenCorrelationIdIsInvalid_ShouldPublishWithoutCorrelationId()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new();
        factory.Enqueue(channel);
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        await bus.PublishAsync(new TestMessage { CorrelationId = "not-a-guid" });

        RabbitMqEnvelope<TestMessage>? envelope = RabbitMqEnvelopeSerializer.Deserialize<TestMessage>(channel.PublishedBody!);

        channel.PublishedCorrelationId.Should().BeNull();
        envelope!.CorrelationId.Should().BeNull();
    }

    [TestMethod]
    public async Task PublishAsync_WhenPublishFailsTransiently_ShouldRetryUntilSuccess()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel firstAttempt = new() { PublishFailuresRemaining = 1 };
        FakeRabbitMqChannel secondAttempt = new();
        factory.Enqueue(firstAttempt);
        factory.Enqueue(secondAttempt);
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        await bus.PublishAsync(new TestMessage());

        firstAttempt.PublishCalls.Should().Be(1);
        secondAttempt.PublishCalls.Should().Be(1);
        factory.CreateCalls.Should().Be(2);
    }

    [TestMethod]
    public async Task PublishAsync_WhenRetriesAreExhausted_ShouldRethrowLastFailure()
    {
        FakeRabbitMqChannelFactory factory = new();
        factory.Enqueue(new FakeRabbitMqChannel { PublishFailuresRemaining = 1 });
        factory.Enqueue(new FakeRabbitMqChannel { PublishFailuresRemaining = 1 });
        factory.Enqueue(new FakeRabbitMqChannel { PublishFailuresRemaining = 1 });
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        Func<Task> action = () => bus.PublishAsync(new TestMessage());

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Publish failed.");
        factory.CreateCalls.Should().Be(3);
    }

    [TestMethod]
    public async Task PublishAsync_WhenTransportReturnsFaultedTask_ShouldRetryAndRethrowLastFailure()
    {
        AsyncFaultingRabbitMqChannelFactory factory = new();
        factory.Enqueue(new AsyncFaultingRabbitMqChannel());
        factory.Enqueue(new AsyncFaultingRabbitMqChannel());
        factory.Enqueue(new AsyncFaultingRabbitMqChannel());
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        Func<Task> action = () => bus.PublishAsync(new TestMessage());

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Async publish failed.");
        factory.CreateCalls.Should().Be(3);
    }

    [TestMethod]
    public async Task PublishAsync_WhenTransportCompletesAsynchronously_ShouldAwaitPublishOperation()
    {
        AsyncFaultingRabbitMqChannelFactory factory = new();
        factory.Enqueue(new AsyncCompletingRabbitMqChannel());
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        await bus.PublishAsync(new TestMessage());

        factory.CreateCalls.Should().Be(1);
    }

    public async Task PublishAsync_WhenMessageIsNull_ShouldThrowArgumentNullException()
    {
        FakeRabbitMqChannelFactory factory = new();
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        Func<Task> action = () => bus.PublishAsync<TestMessage>(null!);

        var assertions = await action.Should().ThrowAsync<ArgumentNullException>();
        assertions.Which.ParamName.Should().Be("message");
    }

    [TestMethod]
    public async Task PublishBatchAsync_WhenMessagesAreProvided_ShouldPublishEachMessage()
    {
        FakeRabbitMqChannelFactory factory = new();
        factory.Enqueue(new FakeRabbitMqChannel());
        factory.Enqueue(new FakeRabbitMqChannel());
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        await bus.PublishBatchAsync(
        [
            new TestMessage(),
            new TestMessage()
        ]);

        factory.CreateCalls.Should().Be(2);
    }

    [TestMethod]
    public async Task PublishBatchAsync_WhenMessagesAreEmpty_ShouldNotPublishAnyMessage()
    {
        FakeRabbitMqChannelFactory factory = new();
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        await bus.PublishBatchAsync(Array.Empty<TestMessage>());

        factory.CreateCalls.Should().Be(0);
    }

    [TestMethod]
    public async Task ScheduleAsync_WhenScheduledTimeHasElapsed_ShouldPublishImmediately()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new();
        factory.Enqueue(channel);
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        await bus.ScheduleAsync(new TestMessage(), DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMilliseconds(1)));

        channel.PublishCalls.Should().Be(1);
    }

    [TestMethod]
    public async Task ScheduleAsync_WhenScheduledTimeIsInTheFuture_ShouldDelayThenPublishMessage()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new();
        factory.Enqueue(channel);
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());

        await bus.ScheduleAsync(new TestMessage(), DateTimeOffset.UtcNow.AddMilliseconds(25));

        channel.PublishCalls.Should().Be(1);
    }

    #endregion

    private sealed record TestMessage : IMessage
    {
        public string MessageId { get; init; } = Guid.NewGuid().ToString("D");

        public string? CorrelationId { get; init; }

        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    }

    private sealed class AsyncFaultingRabbitMqChannelFactory : IRabbitMqChannelFactory
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

    private sealed class AsyncFaultingRabbitMqChannel : IRabbitMqChannel
    {
        public Task DeclareExchangeAsync(
            string exchangeName,
            string exchangeType,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeclareConsumerTopologyAsync(
            RabbitMqTopology topology,
            RabbitMqOptions options,
            CancellationToken cancellationToken = default)
        {
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
            return Task.FromException(new InvalidOperationException("Async publish failed."));
        }

        public Task<string> StartConsumerAsync(
            string queueName,
            Func<RabbitMqDelivery, CancellationToken, Task> onReceived,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task CancelConsumerAsync(string consumerTag, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task AcknowledgeAsync(ulong deliveryTag, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task RejectAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task NegativeAcknowledgeAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    private sealed class AsyncCompletingRabbitMqChannel : IRabbitMqChannel
    {
        public Task DeclareExchangeAsync(
            string exchangeName,
            string exchangeType,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeclareConsumerTopologyAsync(
            RabbitMqTopology topology,
            RabbitMqOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public async Task PublishAsync(
            string exchangeName,
            string routingKey,
            string messageType,
            string messageId,
            string? correlationId,
            DateTimeOffset enqueuedAtUtc,
            ReadOnlyMemory<byte> body,
            CancellationToken cancellationToken = default)
        {
            await CreateAsynchronousCompletionTask().ConfigureAwait(false);
        }

        public Task<string> StartConsumerAsync(
            string queueName,
            Func<RabbitMqDelivery, CancellationToken, Task> onReceived,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task CancelConsumerAsync(string consumerTag, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task AcknowledgeAsync(ulong deliveryTag, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task RejectAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task NegativeAcknowledgeAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask(CreateAsynchronousCompletionTask());
        }

        private static Task CreateAsynchronousCompletionTask()
        {
            TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = Task.Run(() => taskCompletionSource.SetResult());
            return taskCompletionSource.Task;
        }
    }

}
