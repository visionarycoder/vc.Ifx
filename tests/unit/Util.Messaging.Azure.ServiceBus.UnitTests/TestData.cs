using System.Text.Json;
using Ifx.Messaging.Abstractions;

namespace Util.Messaging.Azure.ServiceBus.UnitTests;

internal static class TestData
{
    public const string ConnectionString = "Endpoint=sb://contoso.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=fakeKey=";

    public static AzureServiceBusOptions CreateOptions(int maxDeliveryCountBeforeDeadLetter = 5)
    {
        return new AzureServiceBusOptions
        {
            ConnectionString = ConnectionString,
            QueueName = "orders",
            InitialRetryDelay = TimeSpan.Zero,
            MaxDeliveryCountBeforeDeadLetter = maxDeliveryCountBeforeDeadLetter
        };
    }

    public static FakeReceivedMessageContext CreateContext<TMessage>(TMessage message, int deliveryCount = 1)
        where TMessage : IMessage
    {
        string payload = JsonSerializer.Serialize(new
        {
            messageId = Guid.NewGuid(),
            correlationId = Guid.TryParse(message.CorrelationId, out Guid correlationId) ? correlationId : (Guid?)null,
            enqueuedAtUtc = DateTimeOffset.UtcNow,
            messageType = message.GetType().AssemblyQualifiedName,
            message
        });

        return new FakeReceivedMessageContext(BinaryData.FromString(payload))
        {
            DeliveryCount = deliveryCount
        };
    }
}
