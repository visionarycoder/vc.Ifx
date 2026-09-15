using FluentAssertions;
using Ifx.Messaging.Abstractions;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Util.Messaging.RabbitMQ.UnitTests;

[TestClass]
public sealed class RabbitMqChannelTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_WhenChannelIsNull_ShouldThrowArgumentNullException()
    {
        Action action = () => _ = new RabbitMqChannel(null!);

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("channel");
    }

    #endregion

    #region Delegation Tests

    [TestMethod]
    public async Task DeclareConsumerTopologyAsync_WhenInvoked_ShouldDeclareExchangeQueuesBindingsAndQos()
    {
        Mock<IChannel> channelMock = new(MockBehavior.Strict);
        RabbitMqOptions options = RabbitMqTestServices.CreateOptions().Value;
        RabbitMqTopology topology = RabbitMqTopology.Create<TestMessage>(options, "billing");

        channelMock
            .Setup(channel => channel.ExchangeDeclareAsync(
                options.ExchangeName!,
                options.ExchangeType!,
                true,
                false,
                It.IsAny<IDictionary<string, object?>?>(),
                false,
                false,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        channelMock
            .Setup(channel => channel.ExchangeDeclareAsync(
                topology.DeadLetterExchangeName,
                "direct",
                true,
                false,
                It.IsAny<IDictionary<string, object?>?>(),
                false,
                false,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        channelMock
            .Setup(channel => channel.QueueDeclareAsync(
                topology.QueueName,
                true,
                false,
                false,
                It.Is<IDictionary<string, object?>?>(arguments =>
                    arguments != null &&
                    (string)arguments["x-dead-letter-exchange"]! == topology.DeadLetterExchangeName &&
                    (string)arguments["x-dead-letter-routing-key"]! == topology.DeadLetterRoutingKey),
                false,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueueDeclareOk(topology.QueueName, 0, 0));
        channelMock
            .Setup(channel => channel.QueueDeclareAsync(
                topology.DeadLetterQueueName,
                true,
                false,
                false,
                It.IsAny<IDictionary<string, object?>?>(),
                false,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueueDeclareOk(topology.DeadLetterQueueName, 0, 0));
        channelMock
            .Setup(channel => channel.QueueBindAsync(
                topology.QueueName,
                options.ExchangeName!,
                topology.RoutingKey,
                It.IsAny<IDictionary<string, object?>?>(),
                false,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        channelMock
            .Setup(channel => channel.QueueBindAsync(
                topology.DeadLetterQueueName,
                topology.DeadLetterExchangeName,
                topology.DeadLetterRoutingKey,
                It.IsAny<IDictionary<string, object?>?>(),
                false,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        channelMock
            .Setup(channel => channel.BasicQosAsync(
                0,
                options.PrefetchCount,
                false,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        RabbitMqChannel channel = new(channelMock.Object);

        await channel.DeclareConsumerTopologyAsync(topology, options);

        channelMock.VerifyAll();
    }

    [TestMethod]
    public async Task PublishAsync_WhenInvoked_ShouldPublishPersistentJsonMessage()
    {
        Mock<IChannel> channelMock = new(MockBehavior.Strict);
        BasicProperties? publishedProperties = null;
        ReadOnlyMemory<byte> publishedBody = default;
        DateTimeOffset enqueuedAtUtc = DateTimeOffset.UtcNow;

        channelMock
            .Setup(channel => channel.BasicPublishAsync(
                "ifx.messages",
                "routing-key",
                false,
                It.IsAny<BasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, bool, BasicProperties, ReadOnlyMemory<byte>, CancellationToken>(
                (_, _, _, properties, body, _) =>
                {
                    publishedProperties = properties;
                    publishedBody = body;
                })
            .Returns(ValueTask.CompletedTask);

        RabbitMqChannel channel = new(channelMock.Object);
        byte[] body = [0x01, 0x02, 0x03];

        await channel.PublishAsync(
            "ifx.messages",
            "routing-key",
            "message-type",
            "message-id",
            "correlation-id",
            enqueuedAtUtc,
            body);

        publishedProperties.Should().NotBeNull();
        publishedProperties!.ContentType.Should().Be("application/json");
        publishedProperties.ContentEncoding.Should().Be("utf-8");
        publishedProperties.MessageId.Should().Be("message-id");
        publishedProperties.CorrelationId.Should().Be("correlation-id");
        publishedProperties.Type.Should().Be("message-type");
        publishedProperties.Persistent.Should().BeTrue();
        publishedProperties.Timestamp.UnixTime.Should().Be(enqueuedAtUtc.ToUnixTimeSeconds());
        publishedBody.ToArray().Should().Equal(body);
        channelMock.VerifyAll();
    }

    [TestMethod]
    public async Task StartConsumerAsync_WhenDeliveryArrives_ShouldMapAndForwardRabbitMqDelivery()
    {
        Mock<IChannel> channelMock = new(MockBehavior.Strict);
        IAsyncBasicConsumer? capturedConsumer = null;
        RabbitMqDelivery? receivedDelivery = null;
        CancellationToken receivedToken = default;
        byte[] body = [0x0A, 0x0B];

        channelMock
            .Setup(channel => channel.BasicConsumeAsync(
                "orders",
                false,
                string.Empty,
                false,
                false,
                It.IsAny<IDictionary<string, object?>?>(),
                It.IsAny<IAsyncBasicConsumer>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, bool, string, bool, bool, IDictionary<string, object?>?, IAsyncBasicConsumer, CancellationToken>(
                (_, _, _, _, _, _, consumer, _) => capturedConsumer = consumer)
            .ReturnsAsync("consumer-tag");

        RabbitMqChannel channel = new(channelMock.Object);

        string consumerTag = await channel.StartConsumerAsync(
            "orders",
            (delivery, cancellationToken) =>
            {
                receivedDelivery = delivery;
                receivedToken = cancellationToken;
                return Task.CompletedTask;
            });

        await ((AsyncEventingBasicConsumer)capturedConsumer!).HandleBasicDeliverAsync(
            "consumer-tag",
            11UL,
            false,
            "orders-exchange",
            "orders.created",
            new BasicProperties
            {
                MessageId = "message-11",
                CorrelationId = "correlation-11"
            },
            body,
            CancellationToken.None);

        consumerTag.Should().Be("consumer-tag");
        receivedDelivery.Should().NotBeNull();
        receivedDelivery!.DeliveryTag.Should().Be(11UL);
        receivedDelivery.MessageId.Should().Be("message-11");
        receivedDelivery.CorrelationId.Should().Be("correlation-11");
        receivedDelivery.Exchange.Should().Be("orders-exchange");
        receivedDelivery.RoutingKey.Should().Be("orders.created");
        receivedDelivery.Body.ToArray().Should().Equal(body);
        receivedToken.CanBeCanceled.Should().BeFalse();
        channelMock.VerifyAll();
    }

    [TestMethod]
    public async Task StartConsumerAsync_WhenCallbackIsNull_ShouldThrowArgumentNullException()
    {
        Mock<IChannel> channelMock = new(MockBehavior.Loose);
        RabbitMqChannel channel = new(channelMock.Object);

        Func<Task> action = () => channel.StartConsumerAsync("orders", null!);

        var assertions = await action.Should().ThrowAsync<ArgumentNullException>();
        assertions.Which.ParamName.Should().Be("onReceived");
    }

    [TestMethod]
    public async Task ControlOperations_WhenInvoked_ShouldDelegateToUnderlyingChannel()
    {
        Mock<IChannel> channelMock = new(MockBehavior.Strict);

        channelMock
            .Setup(channel => channel.BasicCancelAsync("consumer-tag", false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        channelMock
            .Setup(channel => channel.BasicAckAsync(7UL, false, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        channelMock
            .Setup(channel => channel.BasicRejectAsync(8UL, true, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        channelMock
            .Setup(channel => channel.BasicNackAsync(9UL, false, true, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        channelMock.Setup(channel => channel.Dispose());
        channelMock.Setup(channel => channel.DisposeAsync()).Returns(ValueTask.CompletedTask);

        RabbitMqChannel channel = new(channelMock.Object);

        await channel.CancelConsumerAsync("consumer-tag");
        await channel.AcknowledgeAsync(7UL);
        await channel.RejectAsync(8UL, requeue: true);
        await channel.NegativeAcknowledgeAsync(9UL, requeue: true);
        channel.Dispose();
        await channel.DisposeAsync();

        channelMock.VerifyAll();
    }

    #endregion

    private sealed record TestMessage : IMessage
    {
        public string MessageId { get; init; } = Guid.NewGuid().ToString("D");

        public string? CorrelationId { get; init; }

        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    }
}
