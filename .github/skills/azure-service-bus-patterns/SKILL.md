---
name: azure-service-bus-patterns
title: Azure Service Bus Patterns
description: Implement Azure Service Bus queues, topics, sessions, dead-letter handling, deferral, and reliable processor flows in .NET.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1886
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - azure-ad-authentication-dotnet
  - message-patterns-dotnet
  - background-services-channels
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - azure
  - service-bus
  - messaging
  - queues
  - topics
---
# Azure Service Bus Patterns

Implement Azure Service Bus messaging with explicit settlement, session-aware processing, dead-letter routing, and replay-safe handlers.

## When to Use

| Condition | Use |
|---|---|
| Workload uses Azure-managed brokered messaging | Use this skill |
| Flow needs queues, topics, subscriptions, or sessions | Use this skill |
| Handler needs dead-letter, deferral, or poison-message control | Use this skill |
| Service uses `ServiceBusProcessor` or `ServiceBusSessionProcessor` | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work stays inside one process | Use `background-services-channels` |
| Flow needs event log retention and partition replay | Use Event Hubs |
| Deployment uses self-hosted broker | Use `rabbitmq-patterns` |
| Requirement is cross-cutting idempotency or outbox only | Use `message-patterns-dotnet` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Namespace and entity names | Yes | Queue, topic, subscription, and session scope |
| Auth path | Yes | Use `DefaultAzureCredential` with RBAC |
| Message contract | Yes | Stable payload and correlation fields |
| Settlement path | Yes | Complete, abandon, dead-letter, or defer |
| Retry budget | Yes | Handler retry and lock-renew strategy |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Map workload to queue, topic, or session entity. | Review entity choice. | Entity shape matches delivery semantics. |
| 2 | Agent registers `ServiceBusClient` and processor options. | Inspect DI setup. | Client and processor use explicit options. |
| 3 | Implement handler with async settlement path. | Inspect message handler. | Handler completes every success and failure branch. |
| 4 | Add dead-letter, deferral, and poison-message rules. | Review failure path. | Unrecoverable messages move to explicit path. |
| 5 | Add idempotency and correlation fields where duplicates matter. | Inspect message contract and storage. | Duplicate delivery does not create duplicate business effects. |
| 6 | Verify receive, retry, session, and dead-letter behavior. | Run targeted tests. | Main success and failure flows pass. |

## Implementation Patterns

| Pattern | Use | Avoid |
|---|---|---|
| Queue | Single consumer group owns one work stream | Topic when fan-out is absent |
| Topic + subscription | Many downstream handlers need independent delivery | One queue with in-handler branching |
| Sessions | Per-key ordering and exclusive processing matter | Global ordering across unrelated keys |
| Dead-letter | Validation or business rejection is final | Infinite abandon loop |
| Deferral | Dependency arrives later or sequence gap exists | Dead-lettering recoverable ordering cases |

```csharp
builder.Services.AddSingleton(_ => new ServiceBusClient(
    builder.Configuration["Messaging:ServiceBus:Namespace"],
    new DefaultAzureCredential()));

builder.Services.AddSingleton(_ => new ServiceBusProcessorOptions
{
    AutoCompleteMessages = false,
    MaxConcurrentCalls = 8,
    PrefetchCount = 32,
    MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
});

public sealed class InvoicePostedProcessor(ServiceBusClient client, ServiceBusProcessorOptions options, InvoicePostedHandler handler) : IHostedService, IAsyncDisposable
{
    private readonly ServiceBusProcessor processor = client.CreateProcessor("invoice-posted", options);
    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            await handler.HandleAsync(args.Message.Body.ToObjectFromJson<InvoicePostedMessage>()!, args.CancellationToken);
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
        catch (WaitingOnDependencyException ex)
        {
            await args.DeferMessageAsync(args.Message, new Dictionary<string, object> { ["DeferredReason"] = ex.Message }, args.CancellationToken);
        }
        catch (InvalidMessageException ex)
        {
            await args.DeadLetterMessageAsync(args.Message, "ValidationFailed", ex.Message, args.CancellationToken);
        }
    }
}
```


## Rules

| Rule | Agent Verifies | Fix |
|---|---|---|
| ASB-001 | `AutoCompleteMessages` stays `false` for custom reliability logic | Use explicit settlement |
| ASB-002 | Handlers are idempotent when duplicate delivery matters | Add idempotency store keyed by `MessageId` |
| ASB-003 | Session entities use stable `SessionId` from domain key | Set `SessionId` at publish time |
| ASB-004 | Poison messages move to dead-letter with reason metadata | Add `DeadLetterMessageAsync` branch |
| ASB-005 | Recoverable ordering gaps use deferral | Store sequence data and defer |
| ASB-006 | Processor concurrency matches handler throughput and lock duration | Tune `MaxConcurrentCalls`, prefetch, and lock renewal |
| ASB-007 | Logs include entity path, message id, and correlation id only | Remove payload or secret logging |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Registration and entity fit | Review DI setup and queue, topic, or session selection. | Client, processor, and entity shape match the delivery semantics. |
| Success and replay safety | Send a valid message and then replay the same `MessageId`. | The handler completes successfully and duplicate delivery does not repeat business effects. |
| Dead-letter, deferral, and session behavior | Send invalid, out-of-order, and same-session sequences. | Poison messages dead-letter, recoverable gaps defer, and session messages keep order. |
| Build and tests | Run targeted build and tests. | Build succeeds and changed tests pass. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| `AutoCompleteMessages = true` with business logic side effects | Switch to explicit settlement |
| No dead-letter reason data | Agent writes stable reason and description |
| Session entity with random `SessionId` | Map `SessionId` to business key |
| Blind abandon loop for poison payload | Agent dead-letters unrecoverable cases |
| Deferral with no retrieval index | Store `SequenceNumber` and retrieval key |

## MCP Hooks

| Need | GitHub MCP hook | Agent action |
|---|---|---|
| Find existing Service Bus processors | `search_code` | Search for `ServiceBusProcessor`, `ServiceBusSessionProcessor`, settlement calls, and entity names before editing. |
| Review Service Bus PR changes | `pull_request_read` | Read changed processors, handlers, and dead-letter or deferral flow before extending messaging behavior. |
| Verify session and retry coverage | `search_code` | Search for `SessionId`, duplicate guards, and dead-letter tests so reliability work stays aligned. |

## Outputs

- Service Bus client and processor registration
- Queue, topic, or session handler implementation
- Dead-letter and deferral policy
- Idempotent settlement flow
- Targeted verification steps for delivery and failure paths
