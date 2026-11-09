using Azure.Messaging.ServiceBus;

namespace vc.Ifx.Messaging.Azure.ServiceBus;

internal sealed class AzureServiceBusSenderAdapter : IAzureServiceBusSender, IAsyncDisposable
{
    private readonly ServiceBusSender sender;
    private bool disposed;

    internal AzureServiceBusSenderAdapter(ServiceBusSender sender)
    {
        this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public async Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        await sender.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
    }

    public async Task ScheduleMessageAsync(ServiceBusMessage message, DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        await sender.ScheduleMessageAsync(message, scheduledEnqueueTime, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (!disposed)
        {
            await sender.DisposeAsync().ConfigureAwait(false);
            disposed = true;
        }
    }
}
