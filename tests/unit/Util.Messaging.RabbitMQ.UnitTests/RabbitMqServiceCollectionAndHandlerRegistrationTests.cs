using FluentAssertions;
using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Util.Messaging.RabbitMQ.UnitTests;

[TestClass]
public sealed class RabbitMqServiceCollectionAndHandlerRegistrationTests
{
    #region ServiceCollection Tests

    [TestMethod]
    public void AddRabbitMqMessaging_WhenConfigurationSectionIsProvided_ShouldBindOptions()
    {
        Dictionary<string, string?> values = new()
        {
            ["Rabbit:HostName"] = "rabbit",
            ["Rabbit:Port"] = "5679",
            ["Rabbit:VirtualHost"] = "/sales",
            ["Rabbit:UserName"] = "guest",
            ["Rabbit:Password"] = "guest",
            ["Rabbit:ExchangeName"] = "sales.exchange",
            ["Rabbit:ExchangeType"] = "topic",
            ["Rabbit:QueueName"] = "sales.queue",
            ["Rabbit:ClientProvidedName"] = "sales-client",
            ["Rabbit:InitialRetryDelay"] = "00:00:00"
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        ServiceCollection services = new();

        services.AddRabbitMqMessaging(configuration, "Rabbit");

        using ServiceProvider provider = services.BuildServiceProvider();
        RabbitMqOptions options = provider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

        options.HostName.Should().Be("rabbit");
        options.Port.Should().Be(5679);
        options.VirtualHost.Should().Be("/sales");
        options.ExchangeName.Should().Be("sales.exchange");
        options.ExchangeType.Should().Be("topic");
        options.QueueName.Should().Be("sales.queue");
        provider.GetRequiredService<IMessageBus>().Should().BeOfType<RabbitMqMessageBus>();
    }

    [TestMethod]
    public async Task AddRabbitMqMessageHandler_WhenResolved_ShouldCreateHostedRegistrationThatStartsAndStopsConsumer()
    {
        FakeRabbitMqChannelFactory factory = new();
        FakeRabbitMqChannel channel = new();
        factory.Enqueue(channel);
        RabbitMqMessageBus bus = new(factory, RabbitMqTestServices.CreateOptions());
        ServiceCollection services = new();
        services.AddSingleton(bus);

        services.AddRabbitMqMessageHandler<TestMessageHandler, TestMessage>("billing");

        using ServiceProvider provider = services.BuildServiceProvider();
        IHostedService hostedService = provider.GetServices<IHostedService>().Should().ContainSingle().Subject;

        await hostedService.StartAsync(CancellationToken.None);
        await hostedService.StopAsync(CancellationToken.None);

        hostedService.Should().BeOfType<RabbitMqMessageHandlerRegistration<TestMessageHandler, TestMessage>>();
        channel.DeclareConsumerTopologyCalls.Should().Be(1);
        channel.CancelCalls.Should().Be(1);
        channel.CancelledConsumerTag.Should().Be("consumer-1");
        channel.Disposed.Should().BeTrue();
    }

    #endregion

    #region Registration Lifecycle Tests

    [TestMethod]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        Action action = () => _ = new RabbitMqMessageHandlerRegistration<TestMessageHandler, TestMessage>(
            null!,
            new TestMessageHandler(),
            "billing");

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("bus");
    }

    [TestMethod]
    public void Constructor_WhenHandlerIsNull_ShouldThrowArgumentNullException()
    {
        RabbitMqMessageBus bus = CreateBus(new FakeRabbitMqChannelFactory());
        Action action = () => _ = new RabbitMqMessageHandlerRegistration<TestMessageHandler, TestMessage>(
            bus,
            null!,
            "billing");

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("handler");
    }

    [TestMethod]
    public void Constructor_WhenSubscriptionNameIsWhitespace_ShouldThrowArgumentException()
    {
        RabbitMqMessageBus bus = CreateBus(new FakeRabbitMqChannelFactory());
        Action action = () => _ = new RabbitMqMessageHandlerRegistration<TestMessageHandler, TestMessage>(
            bus,
            new TestMessageHandler(),
            " ");

        action.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("subscriptionName");
    }

    [TestMethod]
    public async Task StopAsync_WhenConsumerWasNotStarted_ShouldSucceed()
    {
        RabbitMqMessageBus bus = CreateBus(new FakeRabbitMqChannelFactory());
        RabbitMqMessageHandlerRegistration<TestMessageHandler, TestMessage> registration = new(
            bus,
            new TestMessageHandler(),
            "billing");

        await registration.Awaiting(service => service.StopAsync(CancellationToken.None)).Should().NotThrowAsync();
    }

    #endregion

    #region Topology Tests

    [TestMethod]
    public void Create_WhenDeadLetterNamesAreMissing_ShouldComposeDefaultTopologyNames()
    {
        RabbitMqTopology topology = RabbitMqTopology.Create<TestMessage>(
            RabbitMqTestServices.CreateOptions().Value,
            "billing");

        topology.QueueName.Should().Be("ifx.messages.billing.TestMessage");
        topology.RoutingKey.Should().Be(typeof(TestMessage).FullName);
        topology.DeadLetterExchangeName.Should().Be("ifx.messages.dead-letter");
        topology.DeadLetterQueueName.Should().Be("ifx.messages.dead-letter.billing.TestMessage");
        topology.DeadLetterRoutingKey.Should().Be($"{typeof(TestMessage).FullName}.dead-letter");
    }

    [TestMethod]
    public void Create_WhenDeadLetterNamesAreConfigured_ShouldUseConfiguredNames()
    {
        RabbitMqTopology topology = RabbitMqTopology.Create<TestMessage>(
            RabbitMqTestServices.CreateOptions(options =>
            {
                options.DeadLetterExchangeName = "billing.dlx";
                options.DeadLetterQueueName = "billing.dlq";
            }).Value,
            "billing");

        topology.DeadLetterExchangeName.Should().Be("billing.dlx");
        topology.DeadLetterQueueName.Should().Be("billing.dlq.billing.TestMessage");
    }

    #endregion

    private static RabbitMqMessageBus CreateBus(IRabbitMqChannelFactory factory)
    {
        return new RabbitMqMessageBus(factory, RabbitMqTestServices.CreateOptions());
    }

    public sealed class TestMessageHandler : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public sealed record TestMessage : IMessage
    {
        public string MessageId { get; init; } = Guid.NewGuid().ToString("D");

        public string? CorrelationId { get; init; }

        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    }
}
