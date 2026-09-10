---
name: orleans-virtual-actors
title: Microsoft Orleans Virtual Actors
description: Build Orleans grains, clustering, persistence, and silo configuration when agent work involves virtual actor architecture or grain lifecycle behavior.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1958
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - azure-storage-dotnet
  - configuration-options-pattern
  - dependency-injection-patterns
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - orleans
  - grains
  - virtual-actors
  - persistence
  - clustering
---
# Microsoft Orleans Virtual Actors

Implement Orleans virtual actor systems with explicit grain contracts, silo configuration, persistence choices, and lifecycle validation.

## When to Use

| Condition | Use |
|---|---|
| Agent authors grain interfaces, grain classes, or silo registration | Use this skill |
| Add stateful grains with Orleans persistence providers | Use this skill |
| Configure clustering, membership, or storage for an Orleans host | Use this skill |
| Agent reviews activation, deactivation, or stateless worker behavior | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work targets message brokers without actor identity or placement concerns | Use broker or queue guidance |
| Work targets local in-memory objects with no distributed runtime | Use ordinary .NET service patterns |
| Work targets Quartz recurrence semantics | Use Quartz skills |
| Work targets ASP.NET request handlers only | Use Web API or web app guidance |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Grain responsibility | Yes | One business capability per grain type |
| Grain identity model | Yes | Key type, tenancy boundary, or aggregate boundary |
| State model | Yes | Stateless, persisted state, or externalized storage |
| Hosting topology | Yes | Local silo, clustered silo, or hosted client plus silo |
| Validation scope | No | Build, tests, dashboard checks, or lifecycle inspection |

## Workflow

| Step | Action | Pass |
|---|---|---|
| 1 | Map one business capability to one grain interface and implementation. | Grain names and contracts keep one responsibility. |
| 2 | Pick regular, persistent, or stateless-worker grain behavior before editing. | Grain type matches execution and state needs. |
| 3 | Register clustering, storage, and serialization services in host startup. | Host configuration resolves every changed grain dependency. |
| 4 | Keep grain methods async, idempotent where replay matters, and free of blocking waits. | Changed Orleans code has no sync-over-async calls. |
| 5 | Verify activation, persistence, and deactivation. | Build passes and lifecycle behavior matches the selected pattern. |


## Grain Type Matrix

| Need | Preferred Pattern | Avoid |
|---|---|---|
| CPU-bound fan-out with no durable state | `StatelessWorker` grain | Persistent state on a stateless worker |
| Entity or aggregate with isolated mutable state | Stateful grain with `IPersistentState<TState>` | Shared mutable singleton services |
| Lightweight request coordination with no stored state | Regular stateless grain | External lock orchestration around grain calls |
| Cross-grain orchestration boundary | Dedicated coordinator grain | Large multi-purpose grain classes |

## Hosting and Clustering Matrix

| Concern | Preferred Pattern | Notes |
|---|---|---|
| Local development | Localhost clustering | Fast validation path |
| Azure-hosted cluster membership | Azure Storage clustering | Fits Azure-hosted Orleans deployments |
| SQL-backed membership | ADO.NET clustering | Fits relational hosting environments |
| Grain state in Azure | Azure Table, Blob, or Cosmos-backed provider used by the repo pattern | Keep provider choice explicit per grain state |
| Grain state in SQL | ADO.NET grain storage | Align schema and connection management with existing data standards |

## Grain Implementation Patterns

```csharp
public interface ICartGrain : IGrainWithStringKey
{
    Task<CartSnapshot> GetAsync();
    Task AddItemAsync(string sku, int quantity);
}

public sealed class CartGrain([PersistentState("cart", "cartStore")] IPersistentState<CartState> state) : Grain, ICartGrain
{
    public Task<CartSnapshot> GetAsync() => Task.FromResult(new CartSnapshot(state.State.Items));
    public async Task AddItemAsync(string sku, int quantity)
    {
        state.State.AddOrUpdate(sku, quantity);
        await state.WriteStateAsync();
    }
}

[StatelessWorker]
public sealed class PricingGrain : Grain, IPricingGrain
{
    public Task<decimal> QuoteAsync(PriceRequest request) => Task.FromResult(request.Quantity * request.UnitPrice);
}
```

| Pattern | Fit |
|---|---|
| Grain contract | Stable actor-facing contract |
| Stateful grain | Durable aggregate state |
| Stateless worker | High-concurrency stateless work |
| Activation hook | Initialization or metadata refresh that stays idempotent |


## State Management and Persistence Patterns

| Concern | Preferred Pattern | Example |
|---|---|---|
| Strongly typed grain state | `IPersistentState<TState>` injection | `public sealed class ProfileState { public string DisplayName { get; set; } = string.Empty; }` |
| Azure persistence | Named storage provider | `silo.AddAzureTableGrainStorage("profileStore", options => ...);` |
| ADO.NET persistence | Membership and storage providers with invariant plus connection string | `silo.AddAdoNetGrainStorage("profileStore", options => ...);` |
| Terminal cleanup | `ClearStateAsync()` plus `DeactivateOnIdle()` | `await state.ClearStateAsync(); DeactivateOnIdle();` |


## Activation and Deactivation Matrix

| Concern | Preferred Pattern | Avoid |
|---|---|---|
| Cold start state load | Read persisted state during activation path | Manual caching that bypasses provider state |
| Idle cleanup | `DeactivateOnIdle()` after terminal work | Long-lived in-grain timers for completed entities |
| Rehydration safety | Keep activation logic idempotent | Side effects on every activation without guards |
| External resources | Resolve through DI and dispose outside grain lifecycle when possible | Direct socket or client creation inside every grain call |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Grain contract and host registration | Build interfaces with implementations and review provider names. | Signatures match and every persistent grain resolves its storage provider. |
| Persistence and lifecycle | Run existing Orleans tests or local host verification for read, write, clear, activate, and deactivate paths. | State and lifecycle behavior match the selected grain pattern. |
| Async and concurrency safety | Inspect changed methods for blocking waits or shared mutable static state. | Orleans code stays async and isolated. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Grain interface exposes transport or controller types | Replace with grain-specific contracts or DTOs |
| Stateless worker stores mutable request-specific fields | Move state to method scope or convert to a regular grain |
| Persistent grain writes state on every trivial read | Write only on state mutation paths |
| Host registers grain storage with one name and grain requests another name | Align provider names in both locations |
| Activation path emits duplicate side effects | Make initialization idempotent and state-aware |

## Outputs List

| Output | Description |
|---|---|
| Grain contract plan | Identities, responsibilities, and contract boundaries |
| Pattern selection | Chosen regular, stateless, or persistent grain mapping |
| Silo delta | Clustering, storage, and serialization registrations |
| Verification evidence | Build, test, and lifecycle results |

