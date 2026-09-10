# vc.Ifx.Messaging.Azure.Queues

.NET 10 / C# 14 Azure Queue Storage transport. The package owns storage communication; callers own processing, idempotency, scheduling, poison handling, and business retry decisions.

## Stable contract

The existing `IQueueStorageProvider` and `AzureQueueStorageProvider(options, logger)` constructor remain source-compatible. An additive `(options, logger, QueueClient)` constructor borrows an SDK virtual client for DI and offline testing, without another wrapper interface. Configure borrowed clients with `options.CreateClientOptions()`; the caller must match queue identity, encoding, retry and timeout settings. The provider never disposes a borrowed client.

Construction validates but makes no network requests. Except for existence checks, the first operation optionally creates the queue. A single initialization gate coordinates synchronous and asynchronous callers. Failed or canceled creation does not mark initialization successful; a later operation can try again. External queue deletion does not reset the gate. Quiesce active operations before disposal; subsequent calls fail with `ObjectDisposedException`.

## Configuration

- Queue names: 3-63 ASCII lowercase letters/digits, with single internal hyphens.
- Connection mode requires a connection string; the SDK parses its format when constructing a client. Managed-identity mode requires an absolute HTTPS account URI without credentials, query or fragment.
- Compatibility quirk: `UseManagedIdentity` retains the historic `DefaultAzureCredential` chain, including development credentials. For a strictly constrained identity, inject a client with the desired credential.
- `TimeoutMilliseconds` is a positive per-network-operation timeout, not a total deadline. Use cancellation for an overall deadline.
- The SDK is the only retry owner: exponential retry, 0-10 additional attempts (default 3), nonnegative initial delay (default 1000 ms), and the SDK's bounded maximum delay. No Polly or outer retry policy is stacked.
- `MaxMessagesToRetrieve` and per-call overrides are 1-32. Receive visibility is 1-604800 seconds (default 30).
- `MessageTimeToLiveSeconds` is positive (default 604800) or exactly -1 for no expiration. The old -1-to-null mapping incorrectly selected the SDK default lifetime and is corrected.

## Messages and acknowledgements

Text sends continue to reject null, empty and whitespace-only values. Object sends reject null and use `System.Text.Json` defaults; serialization failures occur before storage access. The provider passes text to the SDK once, without manual Base64 conversion. `EncodeMessages=true` now actually configures SDK Base64 encoding on sends/updates and decoding on receives/peeks; the legacy constructor previously discarded this setting.

Content is validated as strict UTF-8 and limited to 64 KiB after encoding. Base64 mode permits at most 49152 UTF-8 bytes; raw mode permits 65536 and additionally requires valid XML characters. Oversized content and invalid Unicode fail before creation or transport. Raw mode leaves XML escaping to the SDK. All producers and consumers must agree on encoding; migrate legacy queues that contain plain text before enabling Base64 decoding. Malformed encoded messages surface the SDK decoding failure, with no automatic deletion.

Receive returns the SDK messages, bodies, pop receipts, dequeue counts and timestamps unchanged. It hides messages temporarily but does not delete them. Peek does not acquire a receipt or alter visibility. Empty queues return empty arrays. Counts are approximate, not a consistency guarantee.

Delete requires a nonblank message ID and the latest pop receipt. A stale receipt, missing message, authorization error or service failure propagates unchanged, including 404; errors are never converted into successful acknowledgements.

`UpdateMessageWithReceipt` and `UpdateMessageWithReceiptAsync` return the SDK `UpdateReceipt` with the renewed pop receipt and next-visible timestamp. Use that receipt for subsequent update/delete. Null text preserves the existing body; empty text explicitly replaces it with an empty body. Update visibility defaults to zero and permits 0-7 days; the SDK represents it in whole seconds, and Azure validates it against remaining message lifetime. The legacy void/Task update methods delegate and discard the receipt for compatibility, so they are unsuitable when the caller needs to acknowledge immediately afterward.

Poison thresholds and routing belong to application policy. Inspect `DequeueCount`, durably record or route a failed message as appropriate, and explicitly acknowledge only after the chosen processing policy succeeds. There is no automatic poison removal or exactly-once guarantee. Retryable sends may be duplicated after an ambiguous response; cancellation after a mutation likewise does not imply rollback.

Async methods reject pre-cancellation, forward tokens through creation and SDK operations, and observe cancellation after SDK completion. Canceling an initialization waiter does not cancel another caller's creation. This provider does not log message bodies, receipts, credentials, or caught SDK exceptions.

## Example

```csharp
var options = new AzureQueueStorageOptions
{
    QueueName = "work-items",
    UseManagedIdentity = true,
    StorageAccountUri = "https://account.queue.core.windows.net/",
    EncodeMessages = true
};
using var queue = new AzureQueueStorageProvider(options, logger);
foreach (var message in await queue.ReceiveMessagesAsync(cancellationToken: token))
{
    await ProcessAsync(message.Body, token);
    await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt, token);
}
```

## Verification

Isolated unit tests use virtual `QueueClient` subclasses and an in-memory HTTP transport through the real SDK. No live Azure account, network calls or Azurite are required.

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Messaging.Azure.Queues -TestSourceScope Messaging/AzureQueues -TestPackage vc.Ifx.Messaging.Azure.Queues -Filter FullyQualifiedName~Messaging.AzureQueues -WarningsAsErrors
```

Strict scoped verification passed 17/17 tests, 248/248 lines and 72/72 branches (100% lines, branches and methods), with warnings treated as errors. Evidence: `TestResults/coverage/vc.Ifx.Messaging.Azure.Queues/0182bec1f2dc45b3acf54858ef43bea5/vc.Ifx.UnitTests/coverage.opencover.xml`. These scoped results are historical. [Final local acceptance](../../docs/planning/local-verification-20260910.md) subsequently passed the full solution, suite, strict coverage, reporting, and packaging. This is not live Azure service validation; hosted execution and publishing checks remain open.

Service semantics: [Put Message](https://learn.microsoft.com/en-us/rest/api/storageservices/put-message), [Get Messages](https://learn.microsoft.com/en-us/rest/api/storageservices/get-messages), [Update Message](https://learn.microsoft.com/en-us/rest/api/storageservices/update-message).
