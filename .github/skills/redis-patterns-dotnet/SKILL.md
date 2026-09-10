---
name: redis-patterns-dotnet
title: Redis Patterns for .NET
description: Implement StackExchange.Redis connection multiplexing, distributed cache, pub/sub, streams, and Lua-scripted atomic flows in .NET.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1990
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - caching-patterns-dotnet
  - message-patterns-dotnet
  - dotnet-api-client-resilience
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - redis
  - stackexchange-redis
  - cache
  - pubsub
  - streams
---
# Redis Patterns for .NET

Agent implements Redis access with singleton connection multiplexing, explicit cache policy, stream consumption, and atomic scripting where single commands are insufficient.

## When to Use

| Condition | Use |
|---|---|
| Application needs shared distributed cache | Use this skill |
| Application uses StackExchange.Redis directly | Use this skill |
| Requirement includes Redis pub/sub or streams | Use this skill |
| Multi-key atomic update needs Lua script | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Requirement is durable enterprise broker semantics | Use `azure-service-bus-patterns` or `rabbitmq-patterns` |
| Need is process-local cache only | Use `IMemoryCache` guidance |
| Need is transport-agnostic outbox or saga design | Use `message-patterns-dotnet` |
| Query pattern depends on relational filtering | Use database-native storage |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Redis role | Yes | Cache, pub/sub, stream, or script |
| Key model | Yes | Prefix, tenancy, and expiry rules |
| Consistency budget | Yes | Staleness, replay, or loss tolerance |
| Connection endpoint | Yes | TLS, auth, database index |
| Recovery path | Yes | Reconnect, replay, or fallback behavior |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent classifies Redis usage as cache, pub/sub, stream, or script. | Review chosen pattern. | Pattern matches latency and durability needs. |
| 2 | Agent registers singleton `ConnectionMultiplexer`. | Inspect DI setup. | One shared multiplexer serves application scope. |
| 3 | Agent centralizes key format and expiry logic. | Review key helpers. | Keys and TTL rules are explicit. |
| 4 | Agent implements cache, publish, stream, or script access with async APIs. | Inspect code paths. | Calls are async and bounded. |
| 5 | Agent adds reconnect-safe error handling and telemetry. | Review logs and retries. | Failures surface clearly without secret leakage. |
| 6 | Agent verifies cache behavior or message flow. | Run targeted tests. | Success and failure paths pass. |

## Implementation Patterns

| Pattern | Use | Avoid |
|---|---|---|
| Singleton multiplexer | Shared connection management across app | Per-request multiplexer |
| Distributed cache | Shared read optimization with TTL | Hidden correctness fix |
| Pub/sub | Low-latency notifications with loss tolerance | Durable business queue |
| Streams | Ordered append log with consumer groups | Pub/sub for replay-required flow |
| Lua script | Multi-step atomic update across keys | Client-side read-modify-write race |

### Singleton Multiplexer

```csharp
using StackExchange.Redis;

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var options = ConfigurationOptions.Parse(builder.Configuration["Redis:Configuration"]);
    options.AbortOnConnectFail = false;
    options.ConnectRetry = 3;
    options.Ssl = true;

    return ConnectionMultiplexer.Connect(options);
});
```

### Distributed Cache Access

```csharp
public sealed class ProductCache(IConnectionMultiplexer multiplexer)
{
    private readonly IDatabase database = multiplexer.GetDatabase();

    public async Task<ProductDto?> GetAsync(string productId)
    {
        var value = await database.StringGetAsync($"product:{productId}");
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<ProductDto>(value!);
    }

    public Task SetAsync(ProductDto product)
    {
        return database.StringSetAsync(
            $"product:{product.Id}",
            JsonSerializer.Serialize(product),
            expiry: TimeSpan.FromMinutes(10));
    }
}
```

### Pub/Sub

```csharp
var subscriber = multiplexer.GetSubscriber();

await subscriber.SubscribeAsync("inventory.changed", async (_, value) =>
{
    var message = JsonSerializer.Deserialize<InventoryChanged>(value!)!;
    await handler.HandleAsync(message, CancellationToken.None);
});

await subscriber.PublishAsync("inventory.changed", JsonSerializer.Serialize(new InventoryChanged("sku-42")));
```

### Streams and Consumer Group

```csharp
var database = multiplexer.GetDatabase();

await database.StreamAddAsync(
    "orders-stream",
    [new NameValueEntry("orderId", orderId), new NameValueEntry("status", "Submitted")]);

var entries = await database.StreamReadGroupAsync(
    "orders-stream",
    "order-workers",
    "worker-01",
    ">",
    count: 20);

foreach (var entry in entries)
{
    await handler.HandleAsync(entry, cancellationToken);
    await database.StreamAcknowledgeAsync("orders-stream", "order-workers", entry.Id);
}
```

### Lua Script

```csharp
var script = LuaScript.Prepare("""
local current = redis.call('GET', @key)
if current then
    return 0
end
redis.call('SET', @key, @value, 'EX', @ttl)
return 1
""");

var result = (int)await database.ScriptEvaluateAsync(
    script,
    new { key = (RedisKey)$"idem:{messageId}", value = "1", ttl = 3600 });
```

## Rules

| Rule | Agent Verifies | Fix |
|---|---|---|
| REDIS-001 | `ConnectionMultiplexer` is singleton-scoped | Register one shared instance |
| REDIS-002 | Keys use centralized prefix and TTL conventions | Add key helper and config |
| REDIS-003 | Pub/sub carries notification semantics only | Move durable workflows to stream or broker |
| REDIS-004 | Streams use consumer groups and ack path for work distribution | Add group creation and ack logic |
| REDIS-005 | Multi-step atomic update uses Lua or single-command primitive | Replace client-side race path |
| REDIS-006 | Commands avoid blocking scans in hot paths | Replace `KEYS` with `SCAN` or indexed sets |
| REDIS-007 | Cache outage does not become data-loss bug | Add fallback path and telemetry |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Build | `dotnet build [project].csproj` | Zero compile errors |
| Cache path | Set, get, expire one key | Value reads correctly then expires |
| Pub/sub path | Publish one notification | Subscriber handles one notification |
| Stream path | Add entries and read through consumer group | Entries process and acknowledge |
| Script path | Execute atomic idempotency script twice | First result succeeds and second result returns duplicate outcome |
| Connection path | Recycle Redis connection during test | Multiplexer reconnects and logs transition |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| New multiplexer per request or repository | Agent shares singleton multiplexer |
| Redis pub/sub used for durable command processing | Agent switches to streams or broker |
| Cache key format differs across features | Agent centralizes key helper |
| `KEYS` appears in request path | Agent replaces with indexed access or `SCAN` |
| Stream consumer skips acknowledgment | Agent adds `StreamAcknowledgeAsync` |

## MCP Hooks

| Need | GitHub MCP hook | Agent action |
|---|---|---|
| Find existing Redis integration points | `search_code` | Agent searches for `ConnectionMultiplexer`, key prefixes, streams, and Lua usage before editing. |
| Review Redis PR changes | `pull_request_read` | Agent reads changed cache services, subscribers, and stream consumers before extending Redis behavior. |
| Verify consistency surfaces | `search_code` | Agent searches for TTL rules, reconnect handling, and consumer acknowledgments so updates stay coherent. |
