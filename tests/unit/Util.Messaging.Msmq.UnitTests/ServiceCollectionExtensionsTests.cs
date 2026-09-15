using FluentAssertions;
using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Util.Messaging.Msmq;

namespace Util.Messaging.Msmq.UnitTests;

[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
    #region AddMsmqMessaging Tests

    [TestMethod]
    public async Task AddMsmqMessaging_WhenOptionsValid_ShouldRegisterServices()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Services.AddMsmqMessaging(options =>
        {
            options.QueuePath = @".\private$\orders-{subscriptionName}";
            options.InitialRetryDelay = TimeSpan.Zero;
            options.ReceiveTimeout = TimeSpan.FromMilliseconds(10);
        });
        builder.Services.AddSingleton<IMsmqQueueClient, NoOpQueueClient>();
        builder.Services.AddMsmqMessageHandler<TestMessageHandler, TestMessage>("billing");

        using IHost host = builder.Build();
        await host.StartAsync();

        host.Services.GetRequiredService<IMessageBus>().Should().BeOfType<MsmqMessageBus>();
        host.Services.GetRequiredService<IMessagePublisher>().Should().BeOfType<MsmqMessageBus>();
        host.Services.GetRequiredService<IOptions<MsmqOptions>>().Value.ResolveQueuePath("billing")
            .Should().Be(@".\private$\orders-billing");

        await host.StopAsync();
    }

    [TestMethod]
    public async Task AddMsmqMessaging_WhenQueuePathMissing_ShouldFailAtHostStartup()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Services.AddMsmqMessaging(options => options.QueuePath = "");

        using IHost host = builder.Build();
        Func<Task> action = () => host.StartAsync();

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [TestMethod]
    public async Task AddMsmqMessaging_WhenConfigurationDelegateIsNull_ShouldDeferValidationUntilOptionsResolution()
    {
        ServiceCollection services = [];
        services.AddMsmqMessaging();

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        Action action = () => _ = serviceProvider.GetRequiredService<IOptions<MsmqOptions>>().Value;

        action.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be(nameof(MsmqOptions.QueuePath));
    }

    [TestMethod]
    public void AddMsmqMessaging_WhenServicesNull_ShouldThrow()
    {
        Action action = () => ServiceCollectionExtensions.AddMsmqMessaging(null!);

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("services");
    }

    [TestMethod]
    public void AddMsmqMessageHandler_WhenSubscriptionMissing_ShouldThrow()
    {
        IServiceCollection services = new ServiceCollection();

        Action action = () => services.AddMsmqMessageHandler<TestMessageHandler, TestMessage>(" ");

        action.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("subscriptionName");
    }

    [TestMethod]
    public void AddMsmqMessageHandler_WhenServicesNull_ShouldThrow()
    {
        Action action = () => ServiceCollectionExtensions.AddMsmqMessageHandler<TestMessageHandler, TestMessage>(null!);

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("services");
    }

    #endregion

    private sealed class TestMessageHandler : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpQueueClient : IMsmqQueueClient
    {
        public Task SendAsync(string queuePath, SerializedMsmqMessage message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task<ReceivedMsmqMessage?> ReceiveAsync(string queuePath, TimeSpan receiveTimeout, CancellationToken cancellationToken)
        {
            await Task.Delay(receiveTimeout, cancellationToken);
            return null;
        }
    }

    private sealed record TestMessage : MessageBase;
}
