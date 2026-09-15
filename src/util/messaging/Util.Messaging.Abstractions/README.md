---
title: Util.Messaging
doc_type: readme
status: active
last_updated: 2026-09-13
---

# Util.Messaging

`Util.Messaging` provides shared helper types for reusable message-bus provider implementations that build on `Ifx.Messaging.Abstractions`.

## Included helpers

### `MessageEnvelope`

Wraps an `IMessage` payload with a transport-level `MessageId`, optional `CorrelationId`, and `EnqueuedAtUtc` timestamp.

```csharp
using Ifx.Messaging.Abstractions;
using Util.Messaging;

public sealed record OrderPlacedMessage(string OrderNumber) : MessageBase;

Guid correlationId = Guid.NewGuid();
MessageEnvelope envelope = MessageEnvelope.Create(new OrderPlacedMessage("SO-1001"), correlationId);
```

### `RetryPolicy`

Runs async work with bounded exponential backoff using the repository's existing Polly-based resilience stack.

```csharp
using Util.Messaging;

await RetryPolicy.ExecuteAsync(
    async cancellationToken => await sender.SendAsync(envelope, cancellationToken),
    maxAttempts: 3,
    initialDelay: TimeSpan.FromMilliseconds(250),
    cancellationToken);
```

### `MessagingOptionsValidationExtensions`

Provides a guard-clause helper for validating required option values inside configuration validators.

```csharp
using Util.Messaging;

public sealed class QueueOptions
{
    public string? QueueName { get; init; }
}

public sealed class QueueOptionsValidator
{
    public void Validate(QueueOptions options)
    {
        options.QueueName.ValidateNotNullOrWhiteSpace(nameof(options.QueueName));
    }
}
```
