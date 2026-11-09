using Azure.Messaging.ServiceBus;

namespace vc.Ifx.Messaging.Azure.ServiceBus;

internal interface IAzureServiceBusSender : IAsyncDisposable
{
    Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken);

    Task ScheduleMessageAsync(ServiceBusMessage message, DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken);
}
