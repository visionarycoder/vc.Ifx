using Azure.Messaging.ServiceBus;

namespace vc.Ifx.Messaging.Azure.ServiceBus;

internal interface IAzureServiceBusReceivedMessageContext
{
    BinaryData Body { get; }

    string MessageId { get; }

    string? CorrelationId { get; }

    int DeliveryCount { get; }

    Task CompleteMessageAsync(CancellationToken cancellationToken);

    Task AbandonMessageAsync(CancellationToken cancellationToken);

    Task DeadLetterMessageAsync(string reason, string? description, CancellationToken cancellationToken);
}
