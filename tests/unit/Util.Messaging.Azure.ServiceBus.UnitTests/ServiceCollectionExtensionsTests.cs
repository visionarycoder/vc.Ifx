using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Util.Messaging.Azure.ServiceBus.UnitTests;

[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
    #region AddAzureServiceBusMessaging Tests

    [TestMethod]
    public async Task AddAzureServiceBusMessaging_WhenCalled_ShouldRegisterBusPublisherAndOptions()
    {
        ServiceCollection services = [];
        services.AddAzureServiceBusMessaging(options =>
        {
            options.ConnectionString = TestData.ConnectionString;
            options.QueueName = "orders";
            options.InitialRetryDelay = TimeSpan.Zero;
        });

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IMessageBus messageBus = serviceProvider.GetRequiredService<IMessageBus>();
        IMessagePublisher publisher = serviceProvider.GetRequiredService<IMessagePublisher>();
        AzureServiceBusOptions options = serviceProvider.GetRequiredService<IOptions<AzureServiceBusOptions>>().Value;
        ServiceBusClient client = serviceProvider.GetRequiredService<ServiceBusClient>();

        messageBus.Should().BeSameAs(publisher);
        messageBus.Should().BeOfType<AzureServiceBusMessageBus>();
        options.QueueName.Should().Be("orders");
        client.Should().NotBeNull();
    }

    [TestMethod]
    public async Task AddAzureServiceBusMessageHandler_WhenCalled_ShouldRegisterHostedService()
    {
        ServiceCollection services = [];
        services.AddAzureServiceBusMessaging(options =>
        {
            options.ConnectionString = TestData.ConnectionString;
            options.QueueName = "orders";
        });

        services.AddAzureServiceBusMessageHandler<TestHandler, TestMessage>();

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IHostedService[] hostedServices = serviceProvider.GetServices<IHostedService>().ToArray();

        hostedServices.Should().ContainSingle();
        hostedServices[0].Should().BeOfType<AzureServiceBusMessageHandlerRegistration<TestHandler, TestMessage>>();
    }

    #endregion

    private sealed class TestHandler : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed record TestMessage : MessageBase;
}
