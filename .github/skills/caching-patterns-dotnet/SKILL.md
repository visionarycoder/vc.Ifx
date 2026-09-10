---
name: caching-patterns-dotnet
title: Caching Patterns for .NET
description: Apply in-memory, distributed, and hybrid caching in .NET with explicit expiration, invalidation, L1+L2 layering, and stampede-control patterns.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 3500
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - configuration-options-pattern
  - dotnet-api-client-resilience
  - azure-app-configuration-dotnet
  - redis-patterns-dotnet
related_docs:
  - https://learn.microsoft.com/en-us/azure/architecture/best-practices/caching
  - https://learn.microsoft.com/en-us/azure/architecture/databases/architecture/write-through-caching-azure-sql-managed-redis
  - https://learn.microsoft.com/en-us/azure/architecture/patterns/cache-aside
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - caching
  - dotnet
  - redis
  - performance
  - expiration
  - hybridcache
  - l1-l2
  - azure
---

# Caching Patterns for .NET

Agent applies caching only when the cache improves latency or load without hiding correctness defects.

## When to Use

| Condition | Use |
|---|---|
| Agent reduces repeated read cost | Use this skill |
| Agent caches reference data, feature data, or external responses | Use this skill |
| Agent chooses `IMemoryCache`, `IDistributedCache`, or `HybridCache` | Use this skill |
| Agent adds expiration or invalidation strategy | Use this skill |
| Agent implements L1+L2 cache layering | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Source of truth changes every request | Use direct read path |
| Defect is N+1 query or poor SQL shape | Fix query first |
| Write path or transaction consistency is main bottleneck | Use non-cache optimization |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Cached data | Yes | Value, projection, or expensive computation |
| Staleness budget | Yes | Minutes, seconds, or zero |
| Cache scope | Yes | Process-local, shared across instances, or automatic L1+L2 |
| Invalidation trigger | No | Expiration, write success, or event |
| Stampede protection | No | Required for hot keys or expensive misses |

## Caching Solution Decision Matrix

Agent selects the caching implementation by deployment scope, .NET version, and complexity tolerance.

| Scenario | .NET Version | Deployment | Recommended Solution | Rationale |
|---|---|---|---|---|
| Single-instance API | Any | Single process | `IMemoryCache` | Simplest, lowest latency, no network cost |
| Multi-instance service | .NET 9+ | Scaled-out | `HybridCache` | Automatic L1+L2, built-in stampede protection, tag-based invalidation |
| Multi-instance service | .NET 8 or earlier | Scaled-out | `IDistributedCache` + manual L1 | Manual L1+L2 layering, explicit stampede control |
| Read-heavy distributed | .NET 9+ | Scaled-out | `HybridCache` with Redis backend | Automatic L1 in-process + L2 Redis, transparent operation |
| Read-heavy distributed | .NET 8 or earlier | Scaled-out | Custom L1+L2 wrapper over `IMemoryCache` + Redis | Explicit two-tier implementation |
| High-consistency writes | Any | Any | `IDistributedCache` only (no L1) | Avoids L1 staleness on write-heavy workloads |
| Hot key with expensive source | .NET 9+ | Any | `HybridCache` | Built-in per-key stampede protection |
| Hot key with expensive source | .NET 8 or earlier | Any | `IDistributedCache` + `SemaphoreSlim` per key | Manual single-flight pattern |
| Reference data (static) | Any | Any | `IMemoryCache` or `HybridCache` with long TTL | Data rarely changes; cache at startup (seeding) |
| Rapidly changing data | Any | Any | Short TTL cache or cache-bypass | Stale data risk outweighs cache benefit unless non-critical |
| Session state | Any | Scaled-out | `IDistributedCache` (Redis) | Per-user data; survives instance failure |

## L1 + L2 Caching Strategies

Agent applies L1 (in-process) and L2 (distributed) cache layering when read latency matters and data is shared across instances.

### HybridCache (Recommended for .NET 9+)

`HybridCache` provides automatic L1+L2 layering with no manual coordination.

**Characteristics:**
- L1: In-process memory cache (lowest latency)
- L2: Distributed backend (Redis, SQL Server, etc.)
- Stampede protection: Built-in per-key locking
- Serialization: Automatic with `System.Text.Json` or custom serializers
- Invalidation: Tag-based removal across L1 and L2

**When to use:**
- Multi-instance .NET 9+ services
- Read-heavy workloads with moderate write rates
- Hot keys requiring stampede protection
- Teams preferring framework-managed complexity

**Pattern:**
```csharp
// Agent registers HybridCache with Redis L2 backend
services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(1)
    };
})
.AddDistributedMemoryCache(); // or .AddStackExchangeRedisCache()

// Agent uses HybridCache with automatic L1+L2 coordination
var product = await hybridCache.GetOrCreateAsync(
    $"product:{productId}",
    async cancel => await LoadProductAsync(productId, cancel),
    tags: ["products"],
    cancellationToken: cancellationToken);
```

### Manual L1 + L2 (.NET 8 and earlier)

Agent implements explicit L1+L2 layering when `HybridCache` is unavailable.

**Pattern:**
```csharp
// Agent checks L1 first (fastest)
if (!memoryCache.TryGetValue(key, out TValue value))
{
    // Agent checks L2 second
    var serialized = await distributedCache.GetStringAsync(key);
    if (serialized != null)
    {
        value = JsonSerializer.Deserialize<TValue>(serialized);
        // Agent backfills L1
        memoryCache.Set(key, value, TimeSpan.FromMinutes(1));
    }
    else
    {
        // Agent fetches from source
        value = await LoadFromSourceAsync();
        // Agent writes to both layers
        await distributedCache.SetStringAsync(key, JsonSerializer.Serialize(value), 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
        memoryCache.Set(key, value, TimeSpan.FromMinutes(1));
    }
}
```

**L1 vs L2 Expiration Guidelines:**
- L1 expiration: 10-20% of L2 expiration (reduces cross-instance staleness)
- L2 expiration: Matches source data staleness tolerance
- Write invalidation: Removes both L1 and L2 entries

## Cache Invalidation and Write Consistency Strategies

Agent selects invalidation strategy based on read-after-write freshness requirements and write frequency.

### Invalidate-on-Write (Cache-Aside)

Agent removes cache entry after writing to source. Next read repopulates from source.

**When to use:**
- Read-heavy workloads with infrequent writes
- Acceptable for readers to see stale data briefly after write
- Simpler write path without cache coordination

**Pattern:**
```csharp
// Agent writes to source
await database.UpdateProductAsync(product);

// Agent invalidates cache
await cache.RemoveAsync($"product:{product.Id}");
// or with HybridCache tags
await hybridCache.RemoveByTagAsync("products");
```

**Trade-offs:**
- ✅ Simple write path
- ✅ Cache and source cannot diverge permanently
- ⚠️ Cache miss on next read after write
- ⚠️ Brief window where readers see stale data

### Write-Through

Agent updates cache and source together. Returns only after both succeed.

**When to use:**
- Read-after-write freshness is required
- Read-heavy paths where cache miss cost is high
- Write frequency is low to moderate

**Pattern:**
```csharp
// Agent updates source and cache atomically
await database.UpdateProductAsync(product);
await cache.SetAsync($"product:{product.Id}", product, 
    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
```

**Trade-offs:**
- ✅ Immediate read-after-write consistency
- ✅ No cache miss after write
- ⚠️ Higher write latency
- ⚠️ More complex write coordination

**Reference:** [Write-through caching with Azure Managed Redis and Azure SQL Database](https://learn.microsoft.com/en-us/azure/architecture/databases/architecture/write-through-caching-azure-sql-managed-redis)

### Cache Seeding

Agent pre-populates cache with reference data at startup.

**When to use:**
- Static or rarely changing reference data (product catalogs, pricing, configuration)
- Predictable working set
- Reducing startup latency matters less than runtime performance

**Pattern:**
```csharp
// Agent seeds cache during application startup
public async Task SeedCacheAsync(IDistributedCache cache)
{
    var products = await database.GetAllProductsAsync();
    foreach (var product in products)
    {
        await cache.SetAsync($"product:{product.Id}", 
            JsonSerializer.SerializeToUtf8Bytes(product),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) });
    }
}
```

**Trade-offs:**
- ✅ Eliminates cold-start cache misses
- ✅ Predictable cache hit rate from startup
- ⚠️ High load on source at startup
- ⚠️ Not suitable for large or dynamic datasets

**Reference:** [Caching Guidance - Determine how to cache data effectively](https://learn.microsoft.com/en-us/azure/architecture/best-practices/caching#determine-how-to-cache-data-effectively)

## Microsoft Learn Integration

Agent uses Microsoft Learn MCP and Azure Architecture Center for current documentation and API reference when implementing caching patterns.

**Azure Architecture Center:**
- [Caching Guidance](https://learn.microsoft.com/en-us/azure/architecture/best-practices/caching) — Comprehensive best practices covering:
  - Private vs shared caching strategies
  - Cache-aside and write-through patterns
  - Data expiration and eviction policies
  - Concurrency management
  - Dynamic data caching considerations
  - Cache seeding strategies

**Recommended MCP queries:**
- `HybridCache .NET 9` — Official HybridCache guidance
- `IDistributedCache Redis` — Redis distributed cache setup
- `IMemoryCache expiration` — Memory cache lifetime management
- `cache stampede protection` — Concurrency control patterns
- `Azure Cache for Redis` — Azure managed Redis configuration
- `cache-aside pattern` — Cache population and invalidation patterns

## Workflow

1. Agent identifies the cached data and stale-data budget.
2. Agent selects caching solution using the Decision Matrix (§ Caching Solution Decision Matrix).
3. Agent implements L1+L2 layering when deployment is multi-instance (§ L1 + L2 Caching Strategies).
4. Agent adds explicit expiration, invalidation, and stampede control.
5. Agent verifies hit, miss, expiry, and cache-outage behavior.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero cache entry without explicit freshness rule in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| CACHE-001 | Cache scope matches deployment scope | Process-local cache used for shared multi-instance data | Use `HybridCache` (.NET 9+) or `IDistributedCache` |
| CACHE-002 | Every entry has explicit freshness rule | Entry added with no absolute or sliding expiration | Add expiration policy |
| CACHE-003 | Mutable data has invalidation plan | Writes succeed and stale entry remains | Remove or update cache on write |
| CACHE-004 | Key format stays centralized | Inline string keys appear across files | Move key generation to helper |
| CACHE-005 | Cache-aside path is explicit | Cache read and fill logic is duplicated | Centralize miss handling or use `HybridCache.GetOrCreateAsync` |
| CACHE-006 | Expensive misses have stampede control | Hot-key miss fans out to many source calls | Use `HybridCache` or add per-key lock |
| CACHE-007 | Cache outage does not become service outage | Cache exception aborts source fallback | Fall back to source path |
| CACHE-008 | Telemetry exposes hit and miss behavior | No counters or logs for cache results | Add structured hit, miss, and eviction telemetry |
| CACHE-009 | L1 expiration is shorter than L2 | L1 and L2 use same expiration in manual layering | Set L1 to 10-20% of L2 duration |
| CACHE-010 | HybridCache used for .NET 9+ multi-instance | Manual L1+L2 implementation in .NET 9+ project | Replace with `HybridCache` |
| CACHE-011 | Tag-based invalidation used when available | Manual key enumeration for bulk invalidation | Use `HybridCache` tags or key prefix patterns |

## Pattern Matrix

| Scenario | Use | Avoid |
|---|---|---|
| Single instance API | `IMemoryCache` with explicit expiration | Distributed cache without need |
| Multi-instance .NET 9+ | `HybridCache` with Redis or SQL backend | Manual L1+L2 implementation |
| Multi-instance .NET 8 or earlier | `IDistributedCache` + manual L1 wrapper | Process-local cache for shared data |
| Mutable reference data | Expiration plus write invalidation | No invalidation path |
| Expensive hot key | `HybridCache` (.NET 9+) or cache-aside with `SemaphoreSlim` | Unbounded concurrent misses |
| Write-heavy shared data | `IDistributedCache` only (no L1) | L1 cache with high staleness risk |
| Tag-based bulk invalidation | `HybridCache` with tags | Manual key iteration |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Scope verification | Search changed files for inline cache keys or entries with no expiration | Zero unauthorized matches |
| Decision matrix alignment | Compare selected cache type against Decision Matrix | Selection matches deployment and .NET version |
| L1+L2 expiration ratio | Check L1 and L2 expiration settings in manual implementations | L1 expiration ≤ 20% of L2 expiration |
| Behavior verification | Run `dotnet test [test-project].csproj --filter Cache` when tests exist | Zero failing tests |

## Verification Checklist

Agent verifies:
- [ ] Cache scope matches deployment scope
- [ ] Caching solution follows Decision Matrix recommendation
- [ ] .NET 9+ multi-instance services use `HybridCache` when applicable
- [ ] Every entry has explicit expiration or invalidation
- [ ] Mutable data has cache update or removal path
- [ ] L1 expiration is 10-20% of L2 expiration in manual L1+L2 implementations
- [ ] Expensive misses use stampede control when load warrants it
- [ ] Telemetry exposes hit, miss, and cache-outage behavior
- [ ] Tag-based invalidation used when supported

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Shared data uses process-local cache | Use `HybridCache` (.NET 9+) or `IDistributedCache` |
| Manual L1+L2 in .NET 9+ projects | Replace with `HybridCache` |
| Cache key strings are copied inline | Centralize key helper |
| Write path skips invalidation | Remove or update cache entry, use tags when available |
| Cache outage aborts source fallback | Use source fallback |
| L1 and L2 have same expiration | Set L1 to 10-20% of L2 duration |
| No stampede protection on hot keys | Use `HybridCache` or add per-key `SemaphoreSlim` |
