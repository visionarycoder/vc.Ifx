using System.Text.Json;
using Ifx.Messaging.Abstractions;

namespace Util.Messaging.Azure.ServiceBus;

/// <summary>
/// Consumes Azure Service Bus messages for a specific payload type.
/// </summary>
/// <typeparam name="TMessage">The message type to consume.</typeparam>
public sealed class AzureServiceBusMessageConsumer<TMessage> : IMessageConsumer, IAsyncDisposable
    where TMessage : IMessage
{
    private readonly IAzureServiceBusProcessor processor;
    private readonly IMessageHandler<TMessage> handler;
    private readonly AzureServiceBusOptions options;
    private bool started;
    private bool disposed;

    internal AzureServiceBusMessageConsumer(
        IAzureServiceBusProcessor processor,
        IMessageHandler<TMessage> handler,
        AzureServiceBusOptions options)
    {
        ArgumentNullException.ThrowIfNull(processor);
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(options);

        this.processor = processor;
        this.handler = handler;
        this.options = options.Validate();

        this.processor.MessageHandler = ProcessMessageAsync;
        this.processor.ErrorHandler = HandleProcessorErrorAsync;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        if (started)
        {
            return Task.CompletedTask;
        }

        started = true;
        return processor.StartProcessingAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!started)
        {
            return;
        }

        await processor.StopProcessingAsync(cancellationToken).ConfigureAwait(false);
        started = false;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        if (started)
        {
            await StopAsync().ConfigureAwait(false);
        }

        await processor.DisposeAsync().ConfigureAwait(false);
        disposed = true;
    }

    private async Task ProcessMessageAsync(IAzureServiceBusReceivedMessageContext context, CancellationToken cancellationToken)
    {
        try
        {
            TMessage message = AzureServiceBusMessageBus.DeserializeMessage<TMessage>(context.Body);

            await RetryPolicy.ExecuteAsync(
                token => handler.HandleAsync(message, token),
                options.MaxRetryAttempts,
                options.InitialRetryDelay,
                cancellationToken).ConfigureAwait(false);

            await RetryPolicy.ExecuteAsync(
                token => context.CompleteMessageAsync(token),
                options.MaxRetryAttempts,
                options.InitialRetryDelay,
                cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException exception)
        {
            await DeadLetterAsync(context, "InvalidMessagePayload", exception.Message, cancellationToken).ConfigureAwait(false);
        }
        catch (NotSupportedException exception)
        {
            await DeadLetterAsync(context, "UnsupportedMessagePayload", exception.Message, cancellationToken).ConfigureAwait(false);
        }
        catch (ArgumentException exception)
        {
            await DeadLetterAsync(context, "InvalidMessagePayload", exception.Message, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            if (context.DeliveryCount >= options.MaxDeliveryCountBeforeDeadLetter)
            {
                await DeadLetterAsync(context, "HandlerFailure", exception.Message, cancellationToken).ConfigureAwait(false);
                return;
            }

            await RetryPolicy.ExecuteAsync(
                token => context.AbandonMessageAsync(token),
                options.MaxRetryAttempts,
                options.InitialRetryDelay,
                cancellationToken).ConfigureAwait(false);
        }
    }

    private Task HandleProcessorErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(exception);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    private Task DeadLetterAsync(
        IAzureServiceBusReceivedMessageContext context,
        string reason,
        string? description,
        CancellationToken cancellationToken)
    {
        return RetryPolicy.ExecuteAsync(
            token => context.DeadLetterMessageAsync(reason, description, token),
            options.MaxRetryAttempts,
            options.InitialRetryDelay,
            cancellationToken);
    }
}
