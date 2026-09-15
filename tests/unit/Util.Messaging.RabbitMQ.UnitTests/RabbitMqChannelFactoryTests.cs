using System.Reflection;
using FluentAssertions;
using Moq;
using RabbitMQ.Client;

namespace Util.Messaging.RabbitMQ.UnitTests;

[TestClass]
public sealed class RabbitMqChannelFactoryTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_WhenOptionsAreNull_ShouldThrowArgumentNullException()
    {
        Action action = () => _ = new RabbitMqChannelFactory(null!);

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("options");
    }

    #endregion

    #region CreateFactory Tests

    [TestMethod]
    public void CreateFactory_WhenConnectionStringIsNotConfigured_ShouldMapDiscreteConnectionSettings()
    {
        using RabbitMqChannelFactory factory = new(RabbitMqTestServices.CreateOptions(options =>
        {
            options.HostName = "rabbit";
            options.Port = 5678;
            options.VirtualHost = "/billing";
            options.UserName = "app-user";
            options.Password = "app-password";
            options.ClientProvidedName = "billing-client";
            options.ConsumerDispatchConcurrency = 2;
        }));

        ConnectionFactory connectionFactory = InvokeCreateFactory(factory);

        connectionFactory.AutomaticRecoveryEnabled.Should().BeTrue();
        connectionFactory.TopologyRecoveryEnabled.Should().BeTrue();
        connectionFactory.ConsumerDispatchConcurrency.Should().Be(2);
        connectionFactory.ClientProvidedName.Should().Be("billing-client");
        connectionFactory.VirtualHost.Should().Be("/billing");
        connectionFactory.HostName.Should().Be("rabbit");
        connectionFactory.Port.Should().Be(5678);
        connectionFactory.UserName.Should().Be("app-user");
        connectionFactory.Password.Should().Be("app-password");
    }

    [TestMethod]
    public void CreateFactory_WhenConnectionStringIsConfigured_ShouldUseUri()
    {
        using RabbitMqChannelFactory factory = new(RabbitMqTestServices.CreateOptions(options =>
        {
            options.ConnectionString = "amqp://guest:guest@localhost:5672/custom-vhost";
            options.HostName = null;
            options.UserName = null;
            options.Password = null;
        }));

        ConnectionFactory connectionFactory = InvokeCreateFactory(factory);

        connectionFactory.Uri.Should().Be(new Uri("amqp://guest:guest@localhost:5672/custom-vhost"));
    }

    #endregion

    #region CreateChannelAsync Tests

    [TestMethod]
    public async Task CreateChannelAsync_WhenOpenConnectionAlreadyExists_ShouldReuseExistingConnection()
    {
        using RabbitMqChannelFactory factory = new(RabbitMqTestServices.CreateOptions());
        Mock<IConnection> connectionMock = CreateOpenConnectionMock();
        Mock<IChannel> channelMock = new(MockBehavior.Strict);

        channelMock.Setup(channel => channel.Dispose());
        connectionMock
            .Setup(connection => connection.CreateChannelAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(channelMock.Object);
        SetConnection(factory, connectionMock.Object);

        IRabbitMqChannel channel = await ((IRabbitMqChannelFactory)factory).CreateChannelAsync();
        channel.Dispose();

        channel.Should().BeOfType<RabbitMqChannel>();
        connectionMock.Verify(connection => connection.CreateChannelAsync(null, It.IsAny<CancellationToken>()), Times.Once);
        connectionMock.Verify(connection => connection.Dispose(), Times.Never);
        channelMock.Verify(channel => channel.Dispose(), Times.Once);
    }

    [TestMethod]
    public async Task CreateChannelAsync_WhenConnectionBecomesAvailableAfterWait_ShouldReuseOpenedConnection()
    {
        using RabbitMqChannelFactory factory = new(RabbitMqTestServices.CreateOptions());
        SemaphoreSlim gate = GetGate(factory);
        await gate.WaitAsync();

        Mock<IConnection> connectionMock = CreateOpenConnectionMock();
        Mock<IChannel> channelMock = new(MockBehavior.Strict);
        channelMock.Setup(channel => channel.Dispose());
        connectionMock
            .Setup(connection => connection.CreateChannelAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(channelMock.Object);

        Task<IRabbitMqChannel> createTask = ((IRabbitMqChannelFactory)factory).CreateChannelAsync();
        await Task.Delay(20);
        SetConnection(factory, connectionMock.Object);
        gate.Release();

        IRabbitMqChannel channel = await createTask;
        channel.Dispose();

        channel.Should().BeOfType<RabbitMqChannel>();
        connectionMock.Verify(connection => connection.CreateChannelAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateChannelAsync_WhenNoConnectionExists_ShouldCreateAndCacheConnection()
    {
        Mock<IConnection> connectionMock = CreateOpenConnectionMock();
        Mock<IChannel> channelMock = new(MockBehavior.Strict);
        channelMock.Setup(channel => channel.Dispose());
        connectionMock
            .Setup(connection => connection.CreateChannelAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(channelMock.Object);
        using RabbitMqChannelFactory factory = new(
            RabbitMqTestServices.CreateOptions().Value,
            _ => Task.FromResult(connectionMock.Object));

        IRabbitMqChannel channel = await ((IRabbitMqChannelFactory)factory).CreateChannelAsync();
        channel.Dispose();

        channel.Should().BeOfType<RabbitMqChannel>();
        connectionMock.Verify(connection => connection.CreateChannelAsync(null, It.IsAny<CancellationToken>()), Times.Once);
        GetConnection(factory).Should().BeSameAs(connectionMock.Object);
    }

    [TestMethod]
    public async Task CreateChannelAsync_WhenClosedConnectionExists_ShouldDisposeItBeforeCreatingReplacement()
    {
        Mock<IConnection> closedConnectionMock = new(MockBehavior.Strict);
        closedConnectionMock.SetupGet(connection => connection.IsOpen).Returns(false);
        closedConnectionMock.Setup(connection => connection.Dispose());

        Mock<IConnection> replacementConnectionMock = CreateOpenConnectionMock();
        Mock<IChannel> channelMock = new(MockBehavior.Strict);
        channelMock.Setup(channel => channel.Dispose());
        replacementConnectionMock
            .Setup(connection => connection.CreateChannelAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(channelMock.Object);

        using RabbitMqChannelFactory factory = new(
            RabbitMqTestServices.CreateOptions().Value,
            _ => Task.FromResult(replacementConnectionMock.Object));
        SetConnection(factory, closedConnectionMock.Object);

        IRabbitMqChannel channel = await ((IRabbitMqChannelFactory)factory).CreateChannelAsync();
        channel.Dispose();

        closedConnectionMock.Verify(connection => connection.Dispose(), Times.Once);
        replacementConnectionMock.Verify(connection => connection.CreateChannelAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateChannelAsync_WhenUsingDefaultConnectionFactory_ShouldAttemptBrokerConnection()
    {
        using RabbitMqChannelFactory factory = new(RabbitMqTestServices.CreateOptions(options =>
        {
            options.HostName = "127.0.0.1";
            options.Port = 1;
        }));

        Func<Task> action = async () => _ = await ((IRabbitMqChannelFactory)factory).CreateChannelAsync();

        await action.Should().ThrowAsync<Exception>();
    }

    #endregion

    #region Dispose Tests

    [TestMethod]
    public void Dispose_WhenConnectionIsMissing_ShouldSucceed()
    {
        using RabbitMqChannelFactory factory = new(RabbitMqTestServices.CreateOptions());

        factory.Invoking(channelFactory => channelFactory.Dispose()).Should().NotThrow();
    }

    [TestMethod]
    public void Dispose_WhenConnectionExists_ShouldDisposeConnection()
    {
        RabbitMqChannelFactory factory = new(RabbitMqTestServices.CreateOptions());
        Mock<IConnection> connectionMock = CreateOpenConnectionMock();
        connectionMock.Setup(connection => connection.Dispose());
        SetConnection(factory, connectionMock.Object);

        factory.Dispose();

        connectionMock.Verify(connection => connection.Dispose(), Times.Once);
    }

    [TestMethod]
    public async Task DisposeAsync_WhenConnectionIsMissing_ShouldSucceed()
    {
        await using RabbitMqChannelFactory factory = new(RabbitMqTestServices.CreateOptions());

        await factory.Awaiting(channelFactory => channelFactory.DisposeAsync().AsTask()).Should().NotThrowAsync();
    }

    [TestMethod]
    public async Task DisposeAsync_WhenConnectionExists_ShouldDisposeConnectionAsynchronously()
    {
        RabbitMqChannelFactory factory = new(RabbitMqTestServices.CreateOptions());
        Mock<IConnection> connectionMock = CreateOpenConnectionMock();
        connectionMock
            .Setup(connection => connection.DisposeAsync())
            .Returns(ValueTask.CompletedTask);
        SetConnection(factory, connectionMock.Object);

        await factory.DisposeAsync();

        connectionMock.Verify(connection => connection.DisposeAsync(), Times.Once);
    }

    #endregion

    private static Mock<IConnection> CreateOpenConnectionMock()
    {
        Mock<IConnection> connectionMock = new(MockBehavior.Strict);
        connectionMock.SetupGet(connection => connection.IsOpen).Returns(true);
        connectionMock.Setup(connection => connection.Dispose());
        connectionMock
            .Setup(connection => connection.DisposeAsync())
            .Returns(ValueTask.CompletedTask);
        return connectionMock;
    }

    private static ConnectionFactory InvokeCreateFactory(RabbitMqChannelFactory factory)
    {
        MethodInfo method = typeof(RabbitMqChannelFactory).GetMethod("CreateFactory", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (ConnectionFactory)method.Invoke(factory, null)!;
    }

    private static SemaphoreSlim GetGate(RabbitMqChannelFactory factory)
    {
        FieldInfo field = typeof(RabbitMqChannelFactory).GetField("gate", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (SemaphoreSlim)field.GetValue(factory)!;
    }

    private static void SetConnection(RabbitMqChannelFactory factory, IConnection? connection)
    {
        FieldInfo field = typeof(RabbitMqChannelFactory).GetField("connection", BindingFlags.Instance | BindingFlags.NonPublic)!;
        field.SetValue(factory, connection);
    }

    private static IConnection? GetConnection(RabbitMqChannelFactory factory)
    {
        FieldInfo field = typeof(RabbitMqChannelFactory).GetField("connection", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (IConnection?)field.GetValue(factory);
    }
}
