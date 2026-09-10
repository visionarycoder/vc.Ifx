---
mode: agent
title: FEMS IET Builder Migration
description: Refactor FemsTransformer to use FemsIETXMLBuilder, extract crosswalk cache helper, and add INTERFACE_2057 and INTERFACE_2058 handling.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1230
invokes_skills:
  - dotnet-unit-testing
  - refactor
  - post-implementation-cleanup
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - technology-stack-dictionary
related_skills: []
appliesTo: '**/*'
tags:
  - prompts
  - prompt
  - refactor
  - ste
---
# FEMS IET Builder Migration
Agent refactors `FemsTransformer` to use `FemsIETXMLBuilder`, extracts the crosswalk cache helper, and preserves emitted XML behavior.
## Inputs
| Path | Role |
|---|---|
| `AzureAPI/src/Engine.Transforming.Service/Transformers/FemsTransformer.cs` | Source transformer |
| `AzureAPI/src/Engine.Transforming.Service/Helpers/FemsIETXMLBuilder.cs` | Source builder |
| `Tests/UnitTests/Engine.Transforming.UnitTests/` | Unit test scope |
| `Tests/IntegrationTests/Solution.IntegrationTests/` | Integration test scope |
## Workflow
| Phase | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent writes `CrosswalkSessionCacheHelper` with prewarm, lookup, clear, and statistics methods. | Run `dotnet build Wa.Wsdot.Fin.Idl.slnx`. | Build passes. |
| 2 | Agent uses `ConcurrentDictionary`, preserves key format, preserves fallback logic, and injects `ICostAccountingAccess` plus `ILogger`. | Read helper implementation. | Helper contains required dependencies and methods. |
| 3 | Agent routes `FemsTransformer` through the helper and deletes duplicate cache state. | Read transformer diff. | Transformer contains helper usage and no duplicate cache store. |
| 4 | Agent adds `FemsIETXMLBuilder(TDocument existingDocument, string ietVersion)` and crosswalk segment helpers. | Run `dotnet build Wa.Wsdot.Fin.Idl.slnx`. | Build passes. |
| 5 | Agent routes header writes by `ietVersion` and writes 2058-only fields only for `INTERFACE_2058`. | Run targeted unit tests. | 2057 and 2058 header tests pass. |
| 6 | Agent rewrites transformer methods to accept builder and interface version. | Read transformer diff. | Builder owns XML segment writes. |
| 7 | Agent updates `ConvertToIETXML`, `Transform2057FileAsync`, and `Transform2058FileAsync`. | Run targeted transformer tests. | Both entry points pass interface version. |
| 8 | Agent writes unit tests for cache hit, miss, fallback, clear, statistics, null crosswalk, and version behavior. | Run targeted unit tests. | All targeted unit tests pass. |
| 9 | Agent writes integration tests for 2057, 2058, mixed credit and debit, and split groups above 200 records. | Run targeted integration tests. | All targeted integration tests pass. |
| 10 | Agent writes XML docs, deletes dead code, and deletes unused usings. | Run `dotnet build Wa.Wsdot.Fin.Idl.slnx`. | Build passes with no new compile defects. |
## Required Members
| Type | Member |
|---|---|
| `CrosswalkSessionCacheHelper` | `PrewarmCacheAsync(List<JournalVoucherDetail>)` |
| `CrosswalkSessionCacheHelper` | `PrewarmCacheWithBatchesAsync(List<(string,string,string?)>)` |
| `CrosswalkSessionCacheHelper` | `GetCachedCrosswalkAsync(string workOrder, string group, string? controlSection)` |
| `CrosswalkSessionCacheHelper` | `ClearCache()` and `GetCacheStatistics()` |
| `FemsIETXMLBuilder` | `FemsIETXMLBuilder(TDocument existingDocument, string ietVersion)` |
| `FemsIETXMLBuilder` | `AddVendorSegmentFromCrosswalk(...)` |
| `FemsIETXMLBuilder` | `AddAccountingSegmentFromCrosswalk(...)` |
## Rules
| Rule | Test | Pass |
|---|---|---|
| Agent preserves observable XML behavior except intentional 2058 additions. | Run XML comparison tests. | Only approved 2058 deltas appear. |
| Agent avoids underscore-prefixed identifiers. | Search changed files for `_[A-Za-z]`. | Zero matches. |
| Agent keeps existing tests and adds new tests only. | Read git diff. | No existing test file deletion appears. |
| Agent follows repository naming and XML doc patterns. | Read changed files. | Public members contain XML docs. |
## Test Matrix
| Scope | Command | Pass |
|---|---|---|
| Build | `dotnet build Wa.Wsdot.Fin.Idl.slnx` | Exit code 0 |
| Unit tests | `dotnet test Tests/UnitTests/Engine.Transforming.UnitTests/Engine.Transforming.UnitTests.csproj` | Exit code 0 |
| Integration tests | `dotnet test Tests/IntegrationTests/Solution.IntegrationTests/Solution.IntegrationTests.csproj --filter FemsTransformer2057And2058Tests` | Exit code 0 |
| Token budget | Count estimated tokens. | Estimated tokens stay below 1500. |
## Invocation
Agent applies this prompt to refactor the transformer, write tests, run build, and return results plus remaining defects.
