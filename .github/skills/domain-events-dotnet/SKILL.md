---
name: domain-events-dotnet
title: Domain Events for .NET
description: Model and publish domain events in .NET with hand-rolled contracts, consistency boundaries, and VBD-aligned event ownership.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1802
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vbd-system-design
  - repository-unitofwork-efcore
  - result-types-dotnet
  - quartz-scheduling-patterns
appliesTo: '**/*.{cs,csproj,md}'
tags:
  - dotnet
  - ddd
  - domain-events
  - vbd
  - consistency
---
# Domain Events for .NET

Model domain events as explicit business facts with owned boundaries and hand-rolled publication flow.

## When to Use

| Prompt or Code Shape | Use |
|---|---|
| Aggregate state change triggers follow-up work inside one domain | Use this skill |
| Business facts need durable names for later policy evolution | Use this skill |
| VBD boundaries need decoupled reactions across owned scopes | Use this skill |
| Team avoids framework event buses and reflection-heavy pipelines | Use this skill |

## When Not to Use

| Prompt or Code Shape | Route |
|---|---|
| One method call inside one aggregate completes all work | Keep direct method calls |
| Integration event publication to a broker is the only scope | Use the matching messaging skill |
| UI notification without domain meaning is in scope | Keep that concern outside the domain layer |
| Work item asks only for repository transaction control | Use `repository-unitofwork-efcore` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Aggregate boundaries | Yes | Tie events to aggregate-owned facts. |
| Consistency requirements | Yes | Agent decides in-transaction or post-commit handling. |
| Downstream reactions | Yes | Map handlers to owned scopes. |
| Event naming vocabulary | Yes | Use business language, not technical verbs only. |
| Failure policy | No | Record retry, idempotency, or compensating action rules. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Name events as past-tense business facts owned by one aggregate boundary. | Read event types. | Each event states one completed fact with one owner. |
| 2 | Record events inside the aggregate during mutation. | Read aggregate methods. | State change and event capture occur together. |
| 3 | Agent publishes events at the selected consistency boundary. | Compare unit-of-work save flow to publisher flow. | Publication timing matches the documented consistency rule. |
| 4 | Keep handlers small and single-purpose inside owned scopes. | Read handler classes. | Each handler reacts to one event with one clear responsibility. |
| 5 | Prevent duplicate side effects with idempotent handler logic when replay risk exists. | Read handler persistence and guards. | Repeated delivery keeps state stable. |
| 6 | Agent runs targeted build and tests. | Run `dotnet build <project>` and targeted tests. | Build succeeds and event flows pass tests. |

## VBD Event Modeling

| Concern | Agent Action |
|---|---|
| Event ownership | Agent places each event beside the aggregate or engine that owns the fact. |
| Boundary crossing | Use events where downstream policy changes independently from the source aggregate. |
| Stable vocabulary | Name events from domain language that remains stable across caller churn. |
| Consistency boundary | Keep in-transaction handlers local and post-commit handlers explicit. |
| Event payload | Keep payloads minimal, version-tolerant, and owned by the source boundary. |

## Implementation Patterns

```csharp
public interface IDomainEvent
{
    DateTime OccurredUtc { get; }
}

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => domainEvents;
    protected void Raise(IDomainEvent domainEvent) => domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => domainEvents.Clear();
}

public sealed record InvoiceApproved(Guid InvoiceId, DateOnly ApprovalDate, DateTime OccurredUtc) : IDomainEvent;

public sealed class Invoice : AggregateRoot
{
    public void Approve(DateOnly approvalDate, IClock clock)
    {
        Raise(new InvoiceApproved(Id, approvalDate, clock.UtcNow));
    }
}

public interface IDomainEventPublisher
{
    Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
```

```csharp
var aggregates = dbContext.ChangeTracker.Entries<AggregateRoot>()
    .Select(x => x.Entity)
    .Where(x => x.DomainEvents.Count > 0)
    .ToArray();
```


## Rules

| Rule | Agent Action |
|---|---|
| Event name | Use business fact names such as `InvoiceApproved` and `PayrollScheduled`. |
| MediatR | Agent avoids notification abstractions from MediatR. |
| Event payload | Keep only data required for downstream reaction. |
| Consistency | Agent documents whether handlers run before commit, after commit, or through an outbox. |
| Handler scope | Keep one reason to change per handler. |
| Mapping | Use hand-written transformations only. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Event vocabulary and capture | Review event names and aggregate mutation methods. | Events read as completed business facts and are raised in the same mutation flow. |
| Publication timing | Review save pipeline, outbox bridge, and tests. | Publish timing matches the documented consistency boundary. |
| MediatR absence and replay safety | Search for MediatR types and run duplicate-handler tests where replay risk exists. | The changed scope stays hand-rolled and repeated delivery preserves correct state. |
| Build and tests | Run targeted build and tests. | Build succeeds and changed tests pass. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Event name mirrors a technical method such as `StatusUpdated` | Agent renames the event to the business fact. |
| Handler reaches back into the source aggregate for extra mutation | Agent moves that mutation into the original command flow or source aggregate. |
| Event payload carries an entire entity graph | Agent trims payload to identifiers and stable business values. |
| Publication occurs before failed persistence | Agent moves publication to the correct consistency boundary. |
| One handler owns unrelated side effects | Agent splits the handler by owned reaction. |

## MCP Hooks

| Need | GitHub MCP hook | Agent action |
|---|---|---|
| Find existing domain event vocabulary | `search_code` | Search for `IDomainEvent`, aggregate event lists, and publisher registrations before editing. |
| Review domain-event PR changes | `pull_request_read` | Read changed aggregates, handlers, and save pipelines before extending event publication flow. |
| Verify consistency-boundary coverage | `search_code` | Search for outbox bridges, idempotent handlers, and event tests so publication timing stays explicit. |

## Outputs

- Domain event contracts
- Aggregate event capture pattern
- Publisher and handler registrations
- Consistency-boundary guidance
- Idempotent reaction guidance
- Verification steps for event flows
