---
name: performance-antipatterns
title: Performance Anti-Patterns Detection
description: Detect Azure Architecture Center performance anti-patterns in cloud applications with remediation guidance.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1200
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - antipattern-detection-bundle
  - analyzing-dotnet-performance
  - optimizing-ef-core-queries
  - caching-patterns-dotnet
related_docs:
  - https://learn.microsoft.com/en-us/azure/architecture/antipatterns/
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - antipatterns
  - performance
  - scalability
  - azure
---

# Performance Anti-Patterns Detection

Agent detects and remediates Azure Architecture Center performance anti-patterns that cause scalability and performance issues under load.

## When to Use

| Condition | Use |
|---|---|
| Performance degradation under load | Use this skill |
| Scalability concerns in cloud application | Use this skill |
| Code review for performance issues | Use this skill |
| Pre-production performance validation | Use this skill |

## When Not to Use

| Condition | Alternative |
|---|---|
| Code-level quality issues only | Use `code-smell-detection` |
| Architecture-wide design issues | Use `antipattern-detection-bundle` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Code or architecture description | Yes | Context for detection |
| Performance symptoms | No | Helps narrow detection scope |

## Azure Anti-Pattern Catalog

Agent detects these performance anti-patterns:

| Anti-Pattern | Detection Signal | Primary Impact | Reference |
|---|---|---|---|
| Busy Database | Business logic in stored procedures, triggers, or database | Database CPU contention | [Reference](references/busy-database.md) |
| Busy Front End | CPU-intensive work on request thread | Thread pool exhaustion | [Reference](references/busy-front-end.md) |
| Chatty I/O | Many small network requests in loops | Network latency accumulation | [Reference](references/chatty-io.md) |
| Extraneous Fetching | Retrieving more data than needed | Unnecessary I/O and memory | [Reference](references/extraneous-fetching.md) |
| Improper Instantiation | Creating/destroying shared objects per request | Memory pressure, GC overhead | [Reference](references/improper-instantiation.md) |
| Monolithic Persistence | Single data store for OLTP and analytics | Contention and poor optimization | [Reference](references/monolithic-persistence.md) |
| No Caching | Missing cache for frequently read data | Repeated expensive operations | [Reference](references/no-caching.md) |
| Noisy Neighbor | Single tenant consumes disproportionate resources | Resource starvation for others | [Reference](references/noisy-neighbor.md) |
| Retry Storm | Excessive retries overwhelming failing service | Cascading failure | [Reference](references/retry-storm.md) |
| Synchronous I/O | Blocking threads during I/O | Thread starvation | [Reference](references/synchronous-io.md) |

## Workflow

1. Agent scans code or architecture description for anti-pattern signals.
2. Agent classifies detected anti-patterns by severity and impact.
3. Agent references detailed detection and remediation guidance.
4. Agent proposes specific code changes or architectural adjustments.
5. Agent validates remediation eliminates anti-pattern.

Test: Run performance baseline comparison.
Pass: Anti-pattern eliminated. Performance metrics improve or remain stable.

## Detection Rules

| Rule | Pattern | Fix |
|---|---|---|
| PERF-001 | Stored procedure contains business calculations | Move logic to application service layer |
| PERF-002 | Request method performs CPU-intensive computation | Move to background job or queue |
| PERF-003 | Loop contains HTTP call or database query | Batch operations or use bulk API |
| PERF-004 | Query returns all columns when subset needed | Add projection with `Select()` |
| PERF-005 | `new DbContext()` or `new HttpClient()` per operation | Use DI with appropriate lifetime |
| PERF-006 | Analytics queries hit OLTP database | Add read replica or separate analytics store |
| PERF-007 | Frequently accessed static data bypasses cache | Add caching with expiration |
| PERF-008 | `.Result`, `.Wait()`, or `Task.Run` wrapping async | Use `await` throughout call chain |
| PERF-009 | Retry policy has no exponential backoff or circuit breaker | Add Polly with backoff and circuit breaker |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Performance baseline | Benchmark before/after remediation | Latency improves or stays within 5% |
| Load test | Simulate production load | Throughput scales linearly with resources |
| Resource monitoring | Monitor CPU, memory, thread pool | Resource usage decreases or remains stable |
| Build verification | `dotnet build` | Zero compile errors |

## Integration with Tooling

**Static Analysis:**
- Roslyn analyzers detect synchronous I/O in async methods
- Architectural analyzers detect layer violations

**Performance Testing:**
- Azure Load Testing for cloud workloads
- BenchmarkDotNet for microbenchmarks
- Application Insights for production telemetry

## Verification Checklist

Agent verifies:
- [ ] Anti-pattern detection scanned all catalog items
- [ ] Severity assigned based on measured or estimated impact
- [ ] Reference documentation consulted for detailed guidance
- [ ] Remediation validated with performance test
- [ ] No new anti-patterns introduced
