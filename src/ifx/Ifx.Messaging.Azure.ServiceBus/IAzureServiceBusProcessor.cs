using Azure.Messaging.ServiceBus;

namespace vc.Ifx.Messaging.Azure.ServiceBus;

internal interface IAzureServiceBusProcessor : IAsyncDisposable
{
    Func<IAzureServiceBusReceivedMessageContext, CancellationToken, Task>? MessageHandler { get; set; }

    Func<Exception, CancellationToken, Task>? ErrorHandler { get; set; }

    Task StartProcessingAsync(CancellationToken cancellationToken);

    Task StopProcessingAsync(CancellationToken cancellationToken);
}
