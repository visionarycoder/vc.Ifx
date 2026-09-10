---
name: orleans-patterns
title: Microsoft Orleans Design Patterns
description: Apply Orleans communication, streaming, timer, reminder, and transaction patterns when agent work involves distributed actor collaboration or runtime operations.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1942
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - azure-storage-dotnet
  - long-running-job-patterns
  - structured-logging-serilog
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - orleans
  - streams
  - observers
  - reminders
  - transactions
  - dashboard
---
# Microsoft Orleans Design Patterns

Apply Orleans collaboration patterns with explicit grain messaging, stream topology, reminder selection, transaction boundaries, reentrancy limits, and runtime observability.

## When to Use

| Condition | Use |
|---|---|
| Agent designs grain-to-grain communication paths | Use this skill |
| Add Orleans observers or streams for fan-out delivery | Use this skill |
| Select timers, reminders, transactions, reentrancy, or request context behavior | Use this skill |
| Configure Orleans dashboard or operational visibility | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work focuses on base grain persistence or silo membership only | Use `orleans-virtual-actors` |
| Work uses broker consumers outside Orleans runtime collaboration | Use broker or messaging guidance |
| Work uses ASP.NET request scoping with no Orleans actor interaction | Use web app or Web API guidance |
| Work targets workflow schedulers driven by cron semantics | Use Quartz skills |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Collaboration style | Yes | Direct call, observer, stream, timer, reminder, or transaction |
| Delivery semantics | Yes | Request-response, fan-out, at-least-once, or durable recurrence |
| Throughput shape | Yes | Burst, steady state, or sparse periodic work |
| Operational visibility need | Yes | Dashboard, logs, counters, or trace correlation |
| Failure boundary | No | Retry, duplicate handling, transactional consistency, or backpressure concern |

## Workflow

| Step | Action | Pass |
|---|---|---|
| 1 | Pick direct calls, observers, streams, or transactions from the decision matrix. | Each collaboration path maps to one Orleans pattern. |
| 2 | Pick timers or reminders from durability and activation needs. | Periodic work uses the correct recurrence primitive. |
| 3 | Apply reentrancy, request context, and transactions only where throughput or consistency needs justify them. | Advanced coordination stays explicit and narrow. |
| 4 | Register stream providers, transaction services, and dashboard or telemetry wiring. | Host startup contains every required runtime service. |
| 5 | Verify communication flow, recurrence, transaction safety, and observability. | Build passes and runtime evidence matches the selected patterns. |


## Communication Pattern Matrix

| Need | Preferred Pattern | Avoid |
|---|---|---|
| One grain requests one other grain result | Direct grain call | Stream or observer overuse for request-response |
| One publisher notifies many transient listeners | Observer | Persistent stream when listener lifetime is short and local to active users |
| Durable fan-out or integration flow | Orleans stream | Manual subscriber lists inside grain state |
| Cross-cutting workflow coordinator | Coordinator grain that calls worker grains | Chatty peer mesh with no ownership boundary |
| Multi-grain state change with atomic intent | Orleans transaction across transactional grains | Ad hoc compensating logic for simple atomic updates |

## Grain Communication Patterns

```csharp
public sealed class OrderGrain : Grain, IOrderGrain
{
    public async Task SubmitAsync(Guid customerId) =>
        await GrainFactory.GetGrain<ICustomerGrain>(customerId).RecordOrderAsync(this.GetPrimaryKey());
}

public sealed class OrderFeedGrain(ILogger<OrderFeedGrain> logger) : Grain, IOrderFeedGrain
{
    private readonly ObserverManager<IOrderObserver> observers = new(TimeSpan.FromMinutes(5), logger);
    public Task SubscribeAsync(IOrderObserver observer)
    {
        observers.Subscribe(observer, observer);
        return Task.CompletedTask;
    }
}

public sealed class InvoiceGrain : Grain, IInvoiceGrain
{
    public Task PublishAsync(InvoiceIssued evt) =>
        GetStreamProvider("finance").GetStream<InvoiceIssued>(this.GetPrimaryKey(), "invoice-issued").OnNextAsync(evt);
}
```


## Streams and Transactions Matrix

| Concern | Preferred pattern |
|---|---|
| In-cluster fan-out | In-memory or persistent Orleans stream provider |
| Event Hubs integration | Stream provider backed by Azure Event Hubs |
| Service Bus integration | Stream provider or adapter backed by Azure Service Bus |
| Transactional balance or inventory change | Transactional state on the owning grains |
| Long-running external side effects | Event-driven workflow with idempotent compensation |


## Recurrence, Context, and Transaction Matrix

| Concern | Preferred Pattern | Avoid |
|---|---|---|
| Work repeats only while grain stays active | Grain timer | Reminder for short-lived active-session polling |
| Work repeats across deactivation and silo restarts | Grain reminder | Timer for durable recurring obligations |
| High-latency interleaving on carefully bounded grain methods | Targeted `[Reentrant]` or method interleaving configuration | Broad reentrancy on mutable workflows with ordering dependencies |
| Correlation metadata across grain calls | `RequestContext` for lightweight propagation | Packing correlation data into every method signature with no shared rule |
| Transactional update | Bounded transactional grain method that touches owned state only | External HTTP or broker work inside the transaction scope |

## Transaction Pattern

```csharp
public sealed class TransferGrain([TransactionalState("ledger", "ledgerStore")] ITransactionalState<LedgerState> ledger)
    : Grain, ITransferGrain
{
    public Task ApplyAsync(decimal amount) => ledger.PerformUpdate(state => state.Balance += amount);
}
```


## Dashboard and Operations Matrix

| Concern | Preferred pattern |
|---|---|
| Local runtime visibility | Orleans dashboard and host registration used by the repo pattern |
| Production visibility | Structured logs, metrics, traces, and dashboard only where policy allows |
| Failure diagnosis | Correlation IDs plus request-context propagation |


## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Communication selection | Review grain calls, observers, streams, and transaction use. | Each path maps to the intended Orleans pattern. |
| Wiring and recurrence | Inspect host startup and run timer or reminder verification. | Providers, transactions, and recurrence behavior match the selected pattern. |
| Reentrancy and observability | Review interleaving-sensitive code and inspect dashboard or telemetry signals. | Reentrant paths preserve correctness and runtime visibility covers activations, failures, and stream activity. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Direct grain calls form deep cyclic chains | Introduce a coordinator grain or event-driven fan-out boundary. |
| Observer subscriptions persist forever | Add unsubscribe or expiry management through `ObserverManager`. |
| Timer is selected for durable business recurrence | Replace it with a reminder. |
| Reentrant grain mutates shared state across awaited calls with no ordering guard | Remove reentrancy or split mutable work into isolated methods. |
| Transaction scope includes external I/O | Move external calls outside the atomic grain update. |
