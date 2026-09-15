using FluentAssertions;
using Ifx.Messaging.Abstractions;

namespace Util.Messaging.RabbitMQ.UnitTests;

[TestClass]
public sealed class RabbitMqMessageConsumerTests
{
    #region Construction Tests

    [TestMethod]
    public void Constructor_WhenConcreteFactoryAndOptionsAreProvided_ShouldCreateConsumer()
    {
        using RabbitMqChannelFactory factory = new(RabbitMqTestServices.CreateOptions());
        RecordingHandler<TestMessage> handler = new();

        RabbitMqMessageConsumer<TestMessage> consumer = new(
            factory,
            RabbitMqTestServices.CreateOptions(),
            handler,
            "orders");

        consumer.Should().NotBeNull();
        consumer.Dispose();
    }

    [TestMethod]
    public void Constructor_WhenOptionsAreNull_ShouldThrowArgumentNullException()
    {
        using RabbitMqChannelFactory factory = new(RabbitMqTestServices.CreateOptions());
        RecordingHandler<TestMessage> handler = new();

        Action action = () => _ = new RabbitMqMessageConsumer<TestMessage>(
            factory,
            null!,
            handler,
            "orders");

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("options");
    }

    #endregion

    #region Lifecycle Tests

    [TestMethod]
    public async Task StartAsync_WhenDeliveryIsValid_ShouldHandleAndAcknowledgeMessage()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new();
        factory.Enqueue(channel);
        RecordingHandler<TestMessage> handler = new();
        RabbitMqMessageConsumer<TestMessage> consumer = new(
            factory,
            RabbitMqTestServices.CreateOptions().Value,
            handler,
            "orders");

        await consumer.StartAsync();
        await channel.DeliverAsync(new TestMessage { Value = "accepted" });

        handler.Messages.Should().ContainSingle()
            .Which.Value.Should().Be("accepted");
        channel.AckCalls.Should().Be(1);
        channel.RejectCalls.Should().Be(0);
        channel.DeclaredTopology!.QueueName.Should().Be("ifx.messages.orders.TestMessage");
    }

    [TestMethod]
    public async Task StartAsync_WhenPayloadCannotBeDeserialized_ShouldRejectMessage()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new();
        factory.Enqueue(channel);
        RecordingHandler<TestMessage> handler = new();
        RabbitMqMessageConsumer<TestMessage> consumer = new(
            factory,
            RabbitMqTestServices.CreateOptions().Value,
            handler,
            "orders");

        await consumer.StartAsync();
        await channel.DeliverRawAsync("not-json"u8.ToArray());

        handler.Messages.Should().BeEmpty();
        channel.RejectCalls.Should().Be(1);
        channel.AckCalls.Should().Be(0);
    }

    [TestMethod]
    public async Task StartAsync_WhenPayloadDeserializesToNullEnvelope_ShouldRejectMessage()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new();
        factory.Enqueue(channel);
        RecordingHandler<TestMessage> handler = new();
        RabbitMqMessageConsumer<TestMessage> consumer = new(
            factory,
            RabbitMqTestServices.CreateOptions().Value,
            handler,
            "orders");

        await consumer.StartAsync();
        await channel.DeliverRawAsync("null"u8.ToArray());

        handler.Messages.Should().BeEmpty();
        channel.RejectCalls.Should().Be(1);
        channel.LastRejectRequeue.Should().BeFalse();
    }

    [TestMethod]
    public async Task StartAsync_WhenHandlerFailsForAllRetries_ShouldRejectMessage()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new();
        factory.Enqueue(channel);
        RecordingHandler<TestMessage> handler = new(_ => throw new InvalidOperationException("Handler failed."));
        RabbitMqMessageConsumer<TestMessage> consumer = new(
            factory,
            RabbitMqTestServices.CreateOptions().Value,
            handler,
            "orders");

        await consumer.StartAsync();
        await channel.DeliverAsync(new TestMessage { Value = "retry" });

        handler.Attempts.Should().Be(3);
        channel.RejectCalls.Should().Be(1);
        channel.AckCalls.Should().Be(0);
    }

    [TestMethod]
    public async Task StartAsync_WhenHandlerCancelsForRequestedCancellation_ShouldNegativeAcknowledgeMessage()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new();
        factory.Enqueue(channel);
        CancellationTokenSource cancellationTokenSource = new();
        RecordingHandler<TestMessage> handler = new(_ =>
        {
            cancellationTokenSource.Cancel();
            throw new OperationCanceledException(cancellationTokenSource.Token);
        });
        RabbitMqMessageConsumer<TestMessage> consumer = new(
            factory,
            RabbitMqTestServices.CreateOptions().Value,
            handler,
            "orders");

        await consumer.StartAsync();
        await channel.DeliverAsync(new TestMessage { Value = "cancelled" }, cancellationToken: cancellationTokenSource.Token);

        handler.Attempts.Should().Be(1);
        channel.NackCalls.Should().Be(1);
        channel.LastNegativeAcknowledgeRequeue.Should().BeTrue();
        channel.RejectCalls.Should().Be(0);
    }

    [TestMethod]
    public async Task StartAsync_WhenCalledMoreThanOnce_ShouldReuseExistingChannel()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new();
        factory.Enqueue(channel);
        RabbitMqMessageConsumer<TestMessage> consumer = new(
            factory,
            RabbitMqTestServices.CreateOptions().Value,
            new RecordingHandler<TestMessage>(),
            "orders");

        await consumer.StartAsync();
        await consumer.StartAsync();

        factory.CreateCalls.Should().Be(1);
        channel.DeclareConsumerTopologyCalls.Should().Be(1);
    }

    [TestMethod]
    public async Task StartAsync_WhenTopologyDeclarationFails_ShouldDisposeChannelAndRethrow()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new() { DeclareConsumerTopologyFailuresRemaining = 3 };
        factory.Enqueue(channel);
        RabbitMqMessageConsumer<TestMessage> consumer = new(
            factory,
            RabbitMqTestServices.CreateOptions().Value,
            new RecordingHandler<TestMessage>(),
            "orders");

        Func<Task> action = () => consumer.StartAsync();

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Topology declaration failed.");
        channel.DeclareConsumerTopologyCalls.Should().Be(3);
        channel.Disposed.Should().BeTrue();
    }

    [TestMethod]
    public async Task StartAsync_WhenConsumerStartFails_ShouldDisposeChannelAndRethrow()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new() { StartConsumerFailuresRemaining = 3 };
        factory.Enqueue(channel);
        RabbitMqMessageConsumer<TestMessage> consumer = new(
            factory,
            RabbitMqTestServices.CreateOptions().Value,
            new RecordingHandler<TestMessage>(),
            "orders");

        Func<Task> action = () => consumer.StartAsync();

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Consumer start failed.");
        channel.DeclareConsumerTopologyCalls.Should().Be(1);
        channel.Disposed.Should().BeTrue();
    }

    [TestMethod]
    public async Task StopAsync_WhenConsumerHasTag_ShouldCancelAndDisposeChannel()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new();
        factory.Enqueue(channel);
        RabbitMqMessageConsumer<TestMessage> consumer = new(
            factory,
            RabbitMqTestServices.CreateOptions().Value,
            new RecordingHandler<TestMessage>(),
            "orders");

        await consumer.StartAsync();
        await consumer.StopAsync();

        channel.CancelCalls.Should().Be(1);
        channel.CancelledConsumerTag.Should().Be("consumer-1");
        channel.Disposed.Should().BeTrue();
    }

    [TestMethod]
    public async Task StopAsync_WhenConsumerTagIsWhitespace_ShouldSkipCancelAndDisposeChannel()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new() { ConsumerTagToReturn = " " };
        factory.Enqueue(channel);
        RabbitMqMessageConsumer<TestMessage> consumer = new(
            factory,
            RabbitMqTestServices.CreateOptions().Value,
            new RecordingHandler<TestMessage>(),
            "orders");

        await consumer.StartAsync();
        await consumer.StopAsync();

        channel.CancelCalls.Should().Be(0);
        channel.Disposed.Should().BeTrue();
    }

    [TestMethod]
    public async Task StopAsync_WhenConsumerWasNotStarted_ShouldReturnWithoutError()
    {
        FakeRabbitMqChannelFactory factory = new();
        RabbitMqMessageConsumer<TestMessage> consumer = new(
            factory,
            RabbitMqTestServices.CreateOptions().Value,
            new RecordingHandler<TestMessage>(),
            "orders");

        await consumer.Awaiting(value => value.StopAsync()).Should().NotThrowAsync();
    }

    [TestMethod]
    public async Task Dispose_WhenConsumerWasStarted_ShouldDisposeActiveChannel()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new();
        factory.Enqueue(channel);
        RabbitMqMessageConsumer<TestMessage> consumer = new(
            factory,
            RabbitMqTestServices.CreateOptions().Value,
            new RecordingHandler<TestMessage>(),
            "orders");

        await consumer.StartAsync();
        consumer.Dispose();

        channel.Disposed.Should().BeTrue();
    }

    [TestMethod]
    public void Dispose_WhenConsumerWasNotStarted_ShouldSucceed()
    {
        FakeRabbitMqChannelFactory factory = new();
        RabbitMqMessageConsumer<TestMessage> consumer = new(
            factory,
            RabbitMqTestServices.CreateOptions().Value,
            new RecordingHandler<TestMessage>(),
            "orders");

        consumer.Invoking(value => value.Dispose()).Should().NotThrow();
    }

    #endregion

    private sealed record TestMessage : IMessage
    {
        public string MessageId { get; init; } = Guid.NewGuid().ToString("D");

        public string? CorrelationId { get; init; }

        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

        public string? Value { get; init; }
    }
}
