using Azure.Messaging.ServiceBus;

namespace vc.Ifx.Messaging.Azure.ServiceBus;

internal sealed class AzureServiceBusReceivedMessageContextAdapter : IAzureServiceBusReceivedMessageContext
{
    private readonly ProcessMessageEventArgs args;

    internal AzureServiceBusReceivedMessageContextAdapter(ProcessMessageEventArgs args)
    {
        this.args = args ?? throw new ArgumentNullException(nameof(args));
    }

    public BinaryData Body => args.Message.Body;

    public string MessageId => args.Message.MessageId;

    public string? CorrelationId => args.Message.CorrelationId;

    public int DeliveryCount => args.Message.DeliveryCount;

    public Task CompleteMessageAsync(CancellationToken cancellationToken)
    {
        args.CompleteMessageAsync(args.Message);
        return Task.CompletedTask;
    }

    public Task AbandonMessageAsync(CancellationToken cancellationToken)
    {
        args.AbandonMessageAsync(args.Message);
        return Task.CompletedTask;
    }

    public Task DeadLetterMessageAsync(string reason, string? description, CancellationToken cancellationToken)
    {
        args.DeadLetterMessageAsync(args.Message, reason, description);
        return Task.CompletedTask;
    }
}
