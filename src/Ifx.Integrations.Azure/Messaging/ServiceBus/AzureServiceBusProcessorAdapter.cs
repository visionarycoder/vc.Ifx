using Azure.Messaging.ServiceBus;

namespace vc.Ifx.Messaging.Azure.ServiceBus;

internal sealed class AzureServiceBusProcessorAdapter : IAzureServiceBusProcessor, IAsyncDisposable
{
    private readonly ServiceBusProcessor processor;
    private bool disposed;
    private bool started;

    internal AzureServiceBusProcessorAdapter(ServiceBusProcessor processor)
    {
        this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
    }

    public Func<IAzureServiceBusReceivedMessageContext, CancellationToken, Task>? MessageHandler { get; set; }

    public Func<Exception, CancellationToken, Task>? ErrorHandler { get; set; }

    public Task StartProcessingAsync(CancellationToken cancellationToken)
    {
        if (started)
        {
            return Task.CompletedTask;
        }

        started = true;
        return processor.StartProcessingAsync(cancellationToken);
    }

    public Task StopProcessingAsync(CancellationToken cancellationToken)
    {
        if (!started)
        {
            return Task.CompletedTask;
        }

        return processor.StopProcessingAsync(cancellationToken);
    }

    internal async Task HandleProcessMessageAsync(ProcessMessageEventArgs args)
    {
        if (MessageHandler != null)
        {
            var context = new AzureServiceBusReceivedMessageContextAdapter(args);
            await MessageHandler(context, args.CancellationToken);
        }
    }

    internal async Task HandleProcessErrorAsync(ProcessErrorEventArgs args)
    {
        if (ErrorHandler != null)
        {
            await ErrorHandler(args.Exception, args.CancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!disposed)
        {
            await processor.DisposeAsync().ConfigureAwait(false);
            disposed = true;
        }
    }
}
