using System.Text.Json;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Util.Messaging.Azure.ServiceBus.UnitTests;

[TestClass]
public sealed class AzureServiceBusMessageBusTests
{
    #region PublishAsync Tests

    [TestMethod]
    public async Task PublishAsync_WhenMessageIsProvided_ShouldStampEnvelopeAndHeaders()
    {
        FakeServiceBusClient client = new();
        AzureServiceBusMessageBus bus = CreateBus(client);
        TestMessage message = new()
        {
            CorrelationId = Guid.NewGuid().ToString("D"),
            OrderNumber = "SO-1001"
        };

        await bus.PublishAsync(message);

        client.LastSenderEntityPath.Should().Be("orders");
        client.Sender.SendAttempts.Should().Be(1);
        ServiceBusMessage transportMessage = client.Sender.SentMessages.Should().ContainSingle().Subject;
        transportMessage.MessageId.Should().NotBeNullOrWhiteSpace();
        transportMessage.CorrelationId.Should().Be(message.CorrelationId);
        transportMessage.ContentType.Should().Be("application/json");
        transportMessage.ApplicationProperties["messageType"].Should().Be(typeof(TestMessage).AssemblyQualifiedName);

        using JsonDocument document = JsonDocument.Parse(transportMessage.Body.ToString());
        document.RootElement.GetProperty("messageId").GetString().Should().NotBeNullOrWhiteSpace();
        document.RootElement.GetProperty("message").GetProperty("orderNumber").GetString().Should().Be("SO-1001");
    }

    [TestMethod]
    public async Task PublishAsync_WhenSendFails_ShouldRetryUntilBudgetIsExhausted()
    {
        FakeServiceBusClient client = new();
        client.Sender.SendBehavior = static (_, _) => throw new InvalidOperationException("Transient failure.");
        AzureServiceBusMessageBus bus = CreateBus(client);

        Func<Task> action = () => bus.PublishAsync(new TestMessage());

        var assertions = await action.Should().ThrowAsync<InvalidOperationException>();

        assertions.Which.Message.Should().Be("Transient failure.");
        client.Sender.SendAttempts.Should().Be(3);
    }

    [TestMethod]
    public async Task PublishBatchAsync_WhenMessagesAreProvided_ShouldPublishEachMessage()
    {
        FakeServiceBusClient client = new();
        AzureServiceBusMessageBus bus = CreateBus(client);
        TestMessage[] messages =
        [
            new TestMessage { OrderNumber = "SO-1001" },
            new TestMessage { OrderNumber = "SO-1002" }
        ];

        await bus.PublishBatchAsync(messages);

        client.Sender.SendAttempts.Should().Be(2);
    }

    [TestMethod]
    public async Task ScheduleAsync_WhenMessageIsProvided_ShouldScheduleTransportMessage()
    {
        FakeServiceBusClient client = new();
        AzureServiceBusMessageBus bus = CreateBus(client);
        DateTimeOffset scheduledTime = DateTimeOffset.UtcNow.AddMinutes(5);

        await bus.ScheduleAsync(new TestMessage { OrderNumber = "SO-2001" }, scheduledTime);

        client.Sender.ScheduleAttempts.Should().Be(1);
        client.Sender.ScheduledTimes.Should().ContainSingle().Which.Should().Be(scheduledTime);
    }

    [TestMethod]
    public void CreateConsumer_WhenTopicIsConfigured_ShouldCreateTopicProcessorForSubscription()
    {
        FakeServiceBusClient client = new();
        AzureServiceBusMessageBus bus = CreateBus(
            client,
            new AzureServiceBusOptions
            {
                FullyQualifiedNamespace = "contoso.servicebus.windows.net",
                TopicName = "orders",
                InitialRetryDelay = TimeSpan.Zero
            });

        IMessageConsumer consumer = bus.CreateConsumer<TestMessage>("billing");

        consumer.Should().BeOfType<AzureServiceBusMessageConsumer<TestMessage>>();
        client.TopicProcessorRequest.Should().NotBeNull();
        client.TopicProcessorRequest!.Value.SubscriptionName.Should().Be("billing");
    }

    #endregion

    private static AzureServiceBusMessageBus CreateBus(FakeServiceBusClient client, AzureServiceBusOptions? options = null)
    {
        return new AzureServiceBusMessageBus(
            client,
            Options.Create(options ?? new AzureServiceBusOptions
            {
                ConnectionString = TestData.ConnectionString,
                QueueName = "orders",
                InitialRetryDelay = TimeSpan.Zero
            }));
    }

    private sealed record TestMessage : MessageBase
    {
        public string OrderNumber { get; init; } = "SO-0001";
    }
}
