---
name: message-patterns-dotnet
title: Message Patterns for .NET
description: Apply pub/sub, request/reply, idempotency, outbox, and saga orchestration patterns in .NET without MediatR.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1790
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - azure-service-bus-patterns
  - rabbitmq-patterns
  - background-services-channels
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - dotnet
  - messaging
  - outbox
  - saga
  - idempotency
---
# Message Patterns for .NET

Implement transport-agnostic messaging patterns with explicit contracts, durable dispatch, and replay-safe business handling.

## When to Use

| Condition | Use |
|---|---|
| Task needs pub/sub, request/reply, or event-driven boundaries | Use this skill |
| Duplicate delivery or replay safety matters | Use this skill |
| Persistence and message dispatch need one transaction boundary | Use this skill |
| Long-running business workflow spans multiple steps or services | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Need is broker-specific topology or settlement | Use `azure-service-bus-patterns` or `rabbitmq-patterns` |
| Work is in-process request orchestration only | Use direct method calls or channels |
| Task asks for MediatR mediator pipeline | Use explicit application services instead |
| Requirement is distributed cache or Redis pub/sub details | Use `redis-patterns-dotnet` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Message contract set | Yes | Commands, events, replies, and ids |
| Consistency boundary | Yes | Database transaction or equivalent |
| Idempotency key source | Yes | Message id, business key, or request token |
| Workflow state | Yes | Saga state fields and terminal conditions |
| Timeout and retry path | Yes | Expiry, retry budget, compensation path |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Classify message flow as pub/sub, request/reply, outbox, or saga. | Review chosen pattern. | Pattern matches business boundary. |
| 2 | Define stable message contracts with ids and correlation fields. | Inspect DTOs. | Contracts expose message id and correlation data. |
| 3 | Implement idempotent handler boundary. | Review persistence path. | Duplicate delivery does not repeat side effects. |
| 4 | Add outbox persistence where state change and publish share one unit of work. | Inspect transaction and outbox write. | State change and outbox record commit together. |
| 5 | Add saga state and transition rules for long-running flow. | Review saga state transitions. | Every step has next, timeout, and terminal path. |
| 6 | Verify duplicate, timeout, and compensation behavior. | Run targeted tests. | Success and failure flows pass. |

## Implementation Patterns

| Pattern | Use | Avoid |
|---|---|---|
| Pub/sub event | One producer notifies many subscribers | Synchronous fan-out in the request thread |
| Request/reply | Caller needs one correlated response | Event flow for strict single reply |
| Idempotent handler | At-least-once delivery exists | Side effects keyed by transient transport state |
| Outbox | Database write and publish share one boundary | Publish-after-commit with no durable dispatch record |
| Saga orchestration | Multi-step workflow needs state and compensation | Distributed transaction across services |

```csharp
public interface IMessage
{
    string MessageId { get; }
    string CorrelationId { get; }
}

public sealed record OrderSubmitted(string MessageId, string CorrelationId, string OrderId, decimal Total) : IMessage;

public sealed class OrderService(AppDbContext dbContext)
{
    public async Task SubmitAsync(Order order, CancellationToken cancellationToken)
    {
        dbContext.Orders.Add(order);
        dbContext.OutboxMessages.Add(OutboxMessage.From("orders.submitted", JsonSerializer.Serialize(new OrderSubmitted(order.Id, order.Id, order.Id, order.Total))));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class OrderSaga
{
    public string OrderId { get; init; } = string.Empty;
    public string State { get; private set; } = "Submitted";
    public void Apply(CreditReserved _) => State = "CreditReserved";
    public void Apply(InventoryRejected _) => State = "Compensating";
}
```


## Rules

| Rule | Agent Verifies | Fix |
|---|---|---|
| MSG-001 | Contracts expose message id and correlation id | Add stable identifiers |
| MSG-002 | Duplicate delivery does not repeat external side effects | Add idempotency store keyed by message id |
| MSG-003 | Publish-after-write uses outbox inside same persistence boundary | Add outbox table and dispatcher |
| MSG-004 | Saga state stays explicit and persistent | Add state record with timestamps and terminal state |
| MSG-005 | Request/reply uses reply contract and timeout path | Add reply correlation and timeout handling |
| MSG-006 | Event handlers stay transport-agnostic | Move broker logic behind publisher or consumer adapter |
| MSG-007 | MediatR stays out of new design | Use explicit interfaces and handlers |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Contract and correlation shape | Review DTOs and publisher or consumer boundaries. | Contracts expose message and correlation ids. |
| Idempotency and outbox | Process the same message twice and simulate publish-after-write outage. | Side effects occur once and outbox rows persist for later dispatch. |
| Request, event, and saga behavior | Exercise pub/sub, request-reply, and compensation paths that changed. | Replies correlate, subscribers react once, and saga state reaches the expected terminal value. |
| Build and tests | Run targeted build and tests. | Build succeeds and changed tests pass. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Transport adapter contains business orchestration | Agent moves orchestration into application service or saga |
| Publish occurs after database commit with no outbox record | Add outbox persistence |
| Idempotency key uses random per-attempt value | Agent keys off stable message or business id |
| Saga state lives in memory only | Agent persists saga state |
| MediatR enters transport boundary design | Use explicit contracts and handlers |

## MCP Hooks

| Need | GitHub MCP hook | Agent action |
|---|---|---|
| Find existing contract and outbox patterns | `search_code` | Search for message ids, correlation ids, outbox tables, and saga state before authoring new flows. |
| Review messaging PR changes | `pull_request_read` | Read changed publishers, handlers, and persistence boundaries before extending message workflows. |
| Verify idempotency coverage | `search_code` | Search for duplicate guards, retry logic, and compensation tests so replay safety stays consistent. |

## Outputs

- Stable command, event, and reply contracts
- Idempotent handler boundary
- Outbox persistence and dispatcher flow
- Saga state and transition model
- Targeted verification steps for duplicate, timeout, and compensation paths
