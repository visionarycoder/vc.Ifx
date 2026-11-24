using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using vc.Ifx.Messaging;
using vc.Ifx.Messaging.Kafka;
using vc.Ifx.Messaging.ProviderSupport;

namespace Ifx.Tests.Messaging.Kafka;

[TestClass]
public sealed class KafkaMessageBusTests
{
    [TestMethod]
    public async Task PublishAsync_UsesStableTopicAndSerializesEnvelope()
    {
        Message<Null, byte[]>? published = null;
        string? topic = null;
        Mock<IProducer<Null, byte[]>> producer = CreateProducer((actualTopic, message) =>
        {
            topic = actualTopic;
            published = message;
        });
        using var bus = new KafkaMessageBus(Options(), producer.Object, _ => throw new InvalidOperationException());
        var message = new TestMessage { Value = "payload", CorrelationId = Guid.NewGuid().ToString("D") };

        await bus.PublishAsync(message);

        Assert.IsNotNull(topic);
        Assert.IsTrue(topic.StartsWith("orders-", StringComparison.Ordinal));
        Assert.IsNotNull(published);
        KafkaEnvelope<TestMessage> envelope = KafkaEnvelope<TestMessage>.Deserialize(published.Value);
        Assert.AreEqual(message.Value, envelope.Message?.Value);
        Assert.AreEqual(message.CorrelationId, envelope.CorrelationId?.ToString("D"));
    }

    [TestMethod]
    public async Task PublishBatchAsync_PublishesEachMessageInOrder()
    {
        int calls = 0;
        Mock<IProducer<Null, byte[]>> producer = CreateProducer((_, _) => calls++);
        using var bus = new KafkaMessageBus(Options(), producer.Object, _ => throw new InvalidOperationException());

        await bus.PublishBatchAsync([new TestMessage(), new TestMessage()]);

        Assert.AreEqual(2, calls);
    }

    [TestMethod]
    public async Task PublishAsync_RejectsNullAndCanceledOperations()
    {
        using var bus = new KafkaMessageBus(Options(), CreateProducer((_, _) => { }).Object, _ => throw new InvalidOperationException());
        using var canceled = new CancellationTokenSource();
        await canceled.CancelAsync();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => bus.PublishAsync<TestMessage>(null!));
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
            bus.PublishAsync(new TestMessage(), canceled.Token));
    }

    [TestMethod]
    public void OptionsValidateTopicAndCreateConsumerConfiguration()
    {
        KafkaOptions options = Options();
        ConsumerConfig config = options.CreateConsumerConfig("billing");

        Assert.AreEqual("group.billing", config.GroupId);
        Assert.AreEqual(AutoOffsetReset.Earliest, config.AutoOffsetReset);
        Assert.IsFalse(config.EnableAutoCommit);
        Assert.IsTrue(options.GetTopicName<TestMessage>().StartsWith("orders-", StringComparison.Ordinal));
        Assert.AreEqual(39, options.GetTopicName<TestMessage>().Length);
        Assert.ThrowsExactly<ArgumentException>(() => new KafkaOptions
        {
            BootstrapServers = "broker:9092",
            GroupId = "group",
            TopicPrefix = "_private"
        }.Validate());
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new KafkaOptions
        {
            BootstrapServers = "broker:9092",
            GroupId = "group",
            AutoOffsetReset = (AutoOffsetReset)int.MaxValue
        }.Validate());
    }

    [TestMethod]
    public void AddKafkaMessaging_RegistersBusPublisherAndHostedHandler()
    {
        var services = new ServiceCollection();
        services.AddKafkaMessaging(options =>
        {
            options.BootstrapServers = "localhost:9092";
            options.GroupId = "group";
        });
        services.AddKafkaMessageHandler<TestHandler, TestMessage>("billing");

        Assert.IsTrue(services.Any(descriptor => descriptor.ServiceType == typeof(IMessageBus)));
        Assert.IsTrue(services.Any(descriptor => descriptor.ServiceType == typeof(IMessagePublisher)));
        Assert.IsTrue(services.Any(descriptor => descriptor.ServiceType == typeof(IHostedService)));
        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.IsInstanceOfType<KafkaMessageBus>(provider.GetRequiredService<IMessageBus>());
    }

    [TestMethod]
    public async Task Consumer_CommitsOffsetAfterHandlerSucceeds()
    {
        byte[] payload = KafkaEnvelope<TestMessage>.Serialize(
            MessageEnvelope.Create(new TestMessage { Value = "received" }));
        var result = new ConsumeResult<Ignore, byte[]>
        {
            TopicPartitionOffset = new TopicPartitionOffset("orders-topic", new Partition(0), new Offset(1)),
            Message = new Message<Ignore, byte[]> { Value = payload }
        };
        var received = new TaskCompletionSource<TestMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var consumer = new Mock<IConsumer<Ignore, byte[]>>(MockBehavior.Strict);
        consumer.Setup(value => value.Subscribe(It.IsAny<string>()));
        consumer.SetupSequence(value => value.Consume(It.IsAny<TimeSpan>()))
            .Returns(result)
            .Returns((ConsumeResult<Ignore, byte[]>)null!);
        consumer.Setup(value => value.Commit(result));
        consumer.Setup(value => value.Close());
        consumer.Setup(value => value.Dispose());

        using var bus = new KafkaMessageBus(Options(), CreateProducer((_, _) => { }).Object, _ => consumer.Object);
        IMessageConsumer subscription = bus.CreateConsumer("billing", new CapturingHandler(received));
        await subscription.StartAsync();
        TestMessage message = await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await subscription.StopAsync();

        Assert.AreEqual("received", message.Value);
        consumer.Verify(value => value.Commit(result), Times.Once);
    }

    [TestMethod]
    public async Task Consumer_LeavesOffsetUncommittedWhenHandlerFails()
    {
        byte[] payload = KafkaEnvelope<TestMessage>.Serialize(
            MessageEnvelope.Create(new TestMessage { Value = "failed" }));
        var result = new ConsumeResult<Ignore, byte[]>
        {
            TopicPartitionOffset = new TopicPartitionOffset("orders-topic", new Partition(0), new Offset(1)),
            Message = new Message<Ignore, byte[]> { Value = payload }
        };
        var received = new TaskCompletionSource<TestMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var consumer = new Mock<IConsumer<Ignore, byte[]>>(MockBehavior.Strict);
        consumer.Setup(value => value.Subscribe(It.IsAny<string>()));
        consumer.SetupSequence(value => value.Consume(It.IsAny<TimeSpan>()))
            .Returns(result)
            .Returns((ConsumeResult<Ignore, byte[]>)null!);
        consumer.Setup(value => value.Commit(result));
        consumer.Setup(value => value.Close());
        consumer.Setup(value => value.Dispose());

        using var bus = new KafkaMessageBus(Options(), CreateProducer((_, _) => { }).Object, _ => consumer.Object);
        IMessageConsumer subscription = bus.CreateConsumer("billing", new FailingHandler(received));
        await subscription.StartAsync();
        await received.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => subscription.StopAsync());
        consumer.Verify(value => value.Commit(result), Times.Never);
    }

    private static KafkaOptions Options() => new()
    {
        BootstrapServers = "localhost:9092",
        GroupId = "group",
        TopicPrefix = "orders"
    };

    private static Mock<IProducer<Null, byte[]>> CreateProducer(Action<string, Message<Null, byte[]>> onProduce)
    {
        var producer = new Mock<IProducer<Null, byte[]>>(MockBehavior.Strict);
        producer.Setup(value => value.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<Null, byte[]>>(),
                It.IsAny<CancellationToken>()))
            .Returns((string topic, Message<Null, byte[]> message, CancellationToken _) =>
            {
                onProduce(topic, message);
                return Task.FromResult<DeliveryResult<Null, byte[]>>(null!);
            });
        producer.Setup(value => value.Flush(It.IsAny<TimeSpan>())).Returns(0);
        producer.Setup(value => value.Dispose());
        return producer;
    }

    private sealed record TestMessage : MessageBase
    {
        public string Value { get; init; } = "";
    }

    private sealed class TestHandler : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class CapturingHandler(TaskCompletionSource<TestMessage> received) : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            received.SetResult(message);
            return Task.CompletedTask;
        }
    }

    private sealed class FailingHandler(TaskCompletionSource<TestMessage> received) : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            received.SetResult(message);
            return Task.FromException(new InvalidOperationException("handler failed"));
        }
    }
}
