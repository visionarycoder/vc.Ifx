using FluentAssertions;
using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Util.Messaging.RabbitMQ.UnitTests;

[TestClass]
public sealed class RabbitMqOptionsAndRegistrationTests
{
    #region Validate Tests

    [TestMethod]
    public void Validate_WhenRequiredValuesAreMissing_ShouldThrowArgumentException()
    {
        RabbitMqOptions options = new()
        {
            ExchangeName = "ifx.messages",
            ExchangeType = null,
            QueueName = "ifx.messages"
        };

        Action action = () => _ = options.Validate();

        action.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be(nameof(RabbitMqOptions.ExchangeType));
    }

    [TestMethod]
    public void Validate_WhenConnectionStringIsProvided_ShouldAllowMissingDiscreteConnectionSettings()
    {
        RabbitMqOptions options = new()
        {
            ConnectionString = "amqp://guest:guest@localhost:5672/orders",
            ExchangeName = "ifx.messages",
            ExchangeType = "topic",
            QueueName = "ifx.messages",
            ClientProvidedName = "client",
            InitialRetryDelay = TimeSpan.Zero
        };

        RabbitMqOptions validated = options.Validate();

        validated.Should().BeSameAs(options);
    }

    [TestMethod]
    public void Validate_WhenInitialRetryDelayIsNegative_ShouldThrowArgumentOutOfRangeException()
    {
        RabbitMqOptions options = RabbitMqTestServices.CreateOptions(configure =>
        {
            configure.InitialRetryDelay = TimeSpan.FromMilliseconds(-1);
        }).Value;

        Action action = () => _ = options.Validate();

        action.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be(nameof(RabbitMqOptions.InitialRetryDelay));
    }

    [TestMethod]
    public void Validate_WhenConsumerDispatchConcurrencyIsZero_ShouldThrowArgumentOutOfRangeException()
    {
        RabbitMqOptions options = RabbitMqTestServices.CreateOptions(configure =>
        {
            configure.ConsumerDispatchConcurrency = 0;
        }).Value;

        Action action = () => _ = options.Validate();

        action.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be(nameof(RabbitMqOptions.ConsumerDispatchConcurrency));
    }

    [TestMethod]
    public void AddRabbitMqMessaging_WhenRegistered_ShouldExposeMessageBusAndPublisher()
    {
        using ServiceProvider provider = RabbitMqTestServices.BuildProvider();

        IMessageBus bus = provider.GetRequiredService<IMessageBus>();
        IMessagePublisher publisher = provider.GetRequiredService<IMessagePublisher>();

        bus.Should().BeOfType<RabbitMqMessageBus>();
        publisher.Should().BeSameAs(bus);
    }

    [TestMethod]
    public void AddRabbitMqMessaging_WhenOptionsAreInvalid_ShouldFailDuringResolution()
    {
        ServiceCollection services = new();
        services.AddRabbitMqMessaging(options =>
        {
            options.ExchangeName = "ifx.messages";
            options.ExchangeType = "direct";
            options.QueueName = "ifx.messages";
            options.HostName = "localhost";
            options.UserName = "guest";
            options.Password = "guest";
            options.Port = 0;
            options.VirtualHost = "/";
        });

        using ServiceProvider provider = services.BuildServiceProvider();
        Action action = () => provider.GetRequiredService<IMessageBus>();

        action.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be(nameof(RabbitMqOptions.Port));
    }

    [TestMethod]
    public void AddRabbitMqMessaging_WhenUsingConfigurationOverload_ShouldBindSection()
    {
        Dictionary<string, string?> settings = new()
        {
            ["Messaging:RabbitMq:HostName"] = "rabbit",
            ["Messaging:RabbitMq:Port"] = "5672",
            ["Messaging:RabbitMq:VirtualHost"] = "/",
            ["Messaging:RabbitMq:UserName"] = "guest",
            ["Messaging:RabbitMq:Password"] = "guest",
            ["Messaging:RabbitMq:ExchangeName"] = "ifx.messages",
            ["Messaging:RabbitMq:ExchangeType"] = "direct",
            ["Messaging:RabbitMq:QueueName"] = "ifx.messages",
            ["Messaging:RabbitMq:ClientProvidedName"] = "bound-client",
            ["Messaging:RabbitMq:InitialRetryDelay"] = "00:00:00"
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        ServiceCollection services = new();

        services.AddRabbitMqMessaging(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<IMessageBus>().Should().BeOfType<RabbitMqMessageBus>();
        provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqOptions>>().Value.ClientProvidedName
            .Should().Be("bound-client");
    }

    #endregion
}
