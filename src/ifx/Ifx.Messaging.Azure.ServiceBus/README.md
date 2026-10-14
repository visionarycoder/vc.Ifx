---
title: Ifx.Messaging.Azure.ServiceBus
doc_type: readme
status: active
last_updated: 2026-09-13
---

# Ifx.Messaging.Azure.ServiceBus

`Ifx.Messaging.Azure.ServiceBus` provides an `IMessageBus` and `IMessagePublisher` implementation backed by `Azure.Messaging.ServiceBus`.

## Options

Configure `AzureServiceBusOptions` through `IOptions<AzureServiceBusOptions>`.

- `ConnectionString` or `FullyQualifiedNamespace`: configure exactly one authentication path.
- `QueueName` or `TopicName`: configure exactly one entity path.
- `MaxRetryAttempts` and `InitialRetryDelay`: control retry behavior for send, receive settlement, and scheduling operations.
- `MaxConcurrentCalls`, `PrefetchCount`, and `MaxAutoLockRenewalDuration`: control `ServiceBusProcessor` behavior.
- `MaxDeliveryCountBeforeDeadLetter`: dead-letters handler failures after the configured delivery threshold.

## Usage

```csharp
using vc.Ifx.Messaging.Abstractions;
using vc.Ifx.Messaging.Azure.ServiceBus;

builder.Services.AddAzureServiceBusMessaging(options =>
{
    options.FullyQualifiedNamespace = builder.Configuration["Messaging:ServiceBus:FullyQualifiedNamespace"];
    options.TopicName = builder.Configuration["Messaging:ServiceBus:TopicName"];
    options.MaxRetryAttempts = 3;
    options.InitialRetryDelay = TimeSpan.FromMilliseconds(250);
});

builder.Services.AddAzureServiceBusMessageHandler<OrderPlacedHandler, OrderPlacedMessage>("orders");

public sealed record OrderPlacedMessage(string OrderNumber) : MessageBase;

public sealed class OrderPlacedHandler : IMessageHandler<OrderPlacedMessage>
{
    public Task HandleAsync(OrderPlacedMessage message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
```

Published messages are wrapped in `MessageEnvelope`, serialized into the Service Bus message body, and stamped with Service Bus `MessageId` and `CorrelationId` values.
