---
title: Util.Messaging.Msmq
doc_type: readme
status: active
last_updated: 2026-09-13
---

# Util.Messaging.Msmq

`Util.Messaging.Msmq` provides an MSMQ-backed implementation of `Ifx.Messaging.Abstractions`.

## Windows-only constraint

- The package targets `net10.0-windows`.
- MSMQ requires the Windows Message Queuing feature to be installed on the host.
- The package uses the `System.Messaging` API surface through `Experimental.System.Messaging` because modern `.NET` does not ship MSMQ support in-box.
- Runtime message operations fail when MSMQ is not installed or the configured queue does not exist.

## Options

`MsmqOptions` supports:

- `QueuePath` (required): MSMQ queue path such as `.\private$\orders`
- `MaxRetryAttempts` (default `3`): bounded retry count for send and receive operations
- `InitialRetryDelay` (default `00:00:00.250`): exponential-backoff starting delay
- `ReceiveTimeout` (default `00:00:01`): receive poll timeout used to honor cancellation

`QueuePath` may contain `{subscriptionName}` when each subscription should map to a distinct queue.

## Usage

```csharp
using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Util.Messaging.Msmq;

IServiceCollection services = new ServiceCollection();

services.AddMsmqMessaging(options =>
{
    options.QueuePath = @".\private$\orders-{subscriptionName}";
});

services.AddMsmqMessageHandler<OrderPlacedHandler, OrderPlacedMessage>("accounting");

ServiceProvider provider = services.BuildServiceProvider();
IMessagePublisher publisher = provider.GetRequiredService<IMessagePublisher>();

await publisher.PublishAsync(new OrderPlacedMessage
{
    CorrelationId = Guid.NewGuid().ToString()
});

public sealed record OrderPlacedMessage : MessageBase;

public sealed class OrderPlacedHandler : IMessageHandler<OrderPlacedMessage>
{
    public Task HandleAsync(OrderPlacedMessage message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
```
