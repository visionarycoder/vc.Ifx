# vc.Ifx Parallel Upgrade Plan

## Goal

Upgrade and expand the `vc.Ifx` framework packages so each project is production-quality, independently testable, package-ready, and covered by unit tests at 100% line and branch coverage for its public and internal framework behavior.

This plan is optimized for multiple agents working in parallel. Agents should claim one project or one dependency-gate workstream at a time, update the status checkboxes before editing, and avoid changing another agent's project unless the dependency contract requires it.

## Status Protocol

Use exactly one active status per project or gate.

- [ ] Ready
- [ ] In-flight
- [ ] Blocked
- [ ] Complete

When claiming work, add an owner and date:

`Owner: <agent or initials>; Updated: yyyy-mm-dd; Notes: <short reason>`

## Completion Standard

Every project is complete only when all of these are true:

- [ ] Public API reviewed for minimal, framework-level contracts.
- [ ] Project references point only inward to stable dependencies.
- [ ] Package metadata is correct: `PackageId`, description, tags, readme, license, repository.
- [ ] Nullable warnings, analyzer warnings, and XML documentation warnings are resolved or deliberately documented.
- [ ] Unit tests cover all public behavior, edge cases, failures, cancellation paths, and DI registration paths.
- [ ] Coverage is 100% line and branch for the project, excluding generated code by explicit rule.
- [ ] `dotnet build vc.Ifx.slnx --no-restore -v:minimal` passes with 0 warnings.
- [ ] Targeted test run for the project passes.
- [ ] Full suite passes or any unrelated failure is recorded with evidence.

## Target Framework and Language Version Standard

- Every project targets `net10.0` except the Roslyn family.
- Roslyn family (`vc.Ifx.Roslyn`, `vc.Ifx.Analyzers`, `vc.Ifx.CodeFixes`, `vc.Ifx.Generators`) targets `netstandard2.0` for Roslyn SDK compatibility.
- Every project, including the Roslyn family, sets `LangVersion` to `preview` to leverage the latest available language features.
- `vc.Ifx.Generators.Abstractions` targets `net10.0` and follows the default (non-Roslyn) standard.
- Agents verify `TargetFramework` and `LangVersion` for any new or modified project against this standard before marking a project `Complete`.

## Dependency Order

Use these waves as coordination hints. The dispatch readiness rules below determine whether a specific agent can claim a section.

1. Foundation contracts:
   `vc.Ifx.Abstractions`, `vc.Ifx.Primitives`, `vc.Ifx.Storage.Abstractions`, `vc.Ifx.Secrets.Abstractions`, `vc.Ifx.Roslyn`, `vc.Ifx.Generators.Abstractions`.
2. Pure framework logic:
   `vc.Ifx.Filtering`, `vc.Ifx.Querying`, `vc.Ifx.Pipeline`.
3. Provider packages:
   `vc.Ifx.Storage.Local`, `vc.Ifx.Storage.Ftp`, `vc.Ifx.Storage.Azure.Blobs`, `vc.Ifx.Secrets.Local`, `vc.Ifx.Secrets.Azure.KeyVault`, `vc.Ifx.Data.Azure.Tables`, `vc.Ifx.Messaging.Azure.Queues`, `vc.Ifx.Proxy.Http`, `vc.Ifx.Proxy.AspNetCore`, `vc.Ifx.Filtering.EntityFrameworkCore`, `vc.Ifx.Pipeline.Grpc`.
4. Cross-cutting packages:
   `vc.Ifx.Proxy`, `vc.Ifx.WebApi`, `vc.Ifx.Observability`, `vc.Ifx.Analyzers`, `vc.Ifx.CodeFixes`, `vc.Ifx.Generators`.
5. Aggregator:
   `vc.Ifx`.

## Dispatch Readiness

The plan is ready for parallel execution when every section has exactly one active status and every agent can determine whether the work is independent, prerequisite, or blocked by a named dependency.

- [x] Gate workstreams are ready to claim immediately.
- [x] Foundation projects are ready to claim immediately.
- [x] Pure framework logic projects are ready to claim after any directly consumed foundation contract is either unchanged or explicitly marked stable in that project's Notes line.
- [x] Provider and cross-cutting projects are ready to claim after their direct abstractions are stable, or when the agent's scope is documentation, tests, packaging, or local refactoring that does not change shared contracts.
- [x] Aggregator work is ready only after the packages it aggregates expose stable public contracts.
- [x] Design artifacts named in this plan are ready to create immediately and should be created before implementation work that depends on them.

## Agent Claim Template

Agents claim work by editing the target section header block before implementation:

`Status: [ ] Ready [x] In-flight [ ] Blocked [ ] Complete`

`Owner: Agent <letter or name>; Updated: yyyy-mm-dd; Notes: <exact scope and expected verification command>`

Agents complete work by updating the same block:

`Status: [ ] Ready [ ] In-flight [ ] Blocked [x] Complete`

`Owner: Agent <letter or name>; Updated: yyyy-mm-dd; Notes: <verification command and result; coverage result; remaining known unrelated failures>`

Agents block work by updating the same block:

`Status: [ ] Ready [ ] In-flight [x] Blocked [ ] Complete`

`Owner: Agent <letter or name>; Updated: yyyy-mm-dd; Notes: <blocking package, missing contract, exact decision needed>`

## Shared Gates

### Gate 1: Test Infrastructure

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: 2026-09-09; Notes: `tests/unit/vc.Ifx.UnitTests` and `tests/integration/vc.Ifx.IntegrationTests` project scaffolds exist and build. Both are registered in `vc.Ifx.slnx`.

- [x] Decide whether to keep one unit-test project or split test projects per package: one `vc.Ifx.UnitTests` project for unit coverage, one `vc.Ifx.IntegrationTests` project for cross-package integration coverage.
- [x] Add package-level coverage collection and reporting: coverlet is the coverage tool. `Directory.Build.props` enables `CollectCoverage`, `CoverletOutputFormat=opencover`, per-project output under `$(OutputPath)coverage.opencover.xml`, and excludes `*.Tests.*`, `*.UnitTests.*`, `*.IntegrationTests.*`, `*.Benchmarks.*`.
- [ ] Add branch coverage threshold enforcement at 100% using coverlet's `Threshold`, `ThresholdType=line,branch`, and `ThresholdStat=total` MSBuild properties (or `coverlet.msbuild` equivalent), wired into the CI job defined in Gate 4.
- [ ] Define generated-code exclusions for gRPC, Roslyn generated fixtures, and compiler artifacts.
- [ ] Add deterministic test helpers for clocks, random values, file systems, HTTP, Azure SDK clients, and logging.
- [ ] Verify every project under `src/` is included in `vc.Ifx.slnx` (see Gate 3 for the recurring check). At time of writing, all 27 `src/` projects and both `tests/` projects are present.

### Gate 2: Package Metadata

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Normalize `PackageId` for every project to its project name.
- [ ] Add package descriptions and tags per package.
- [ ] Ensure each package has a package readme.
- [ ] Ensure source link and symbols are preserved.
- [ ] Prevent aggregator-only dependencies from leaking into small packages.

### Gate 3: Contract Boundaries

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Define which contracts belong in abstractions versus implementation packages.
- [ ] Remove duplicate contracts across proxy, storage, secrets, and pipeline packages.
- [ ] Keep provider packages behind interfaces and options records.
- [ ] Confirm no shared domain-object library is introduced.
- [ ] Ensure cross-boundary types are commands, facts, records, and results.
- [ ] Verify `vc.Ifx.slnx` lists every project directory under `src/` and `tests/`. Run a directory-vs-solution diff before marking this gate `Complete`, and repeat the diff whenever a new project is added.

### Gate 4: CI/CD Wiring

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes: CI/CD objects live under `.infra/yaml` (pipeline definitions) and `.infra/bicep` (Azure infrastructure). Both directories exist and are currently empty.

- [ ] Author the build/test/coverage pipeline definition under `.infra/yaml`, replacing or supplementing `.github/workflows/publish.yml` restore/build/test/pack steps.
- [ ] Wire the coverlet coverage threshold (see Gate 1) into the `.infra/yaml` pipeline so a coverage regression fails the build, not just a local run.
- [ ] Add a pipeline step that fails fast when zero test projects are discovered, preventing the current false-green state where `dotnet test` passes trivially.
- [ ] Author any required Azure infrastructure (build agents, artifact feeds, Key Vault-backed publish credentials) under `.infra/bicep`.
- [ ] Document the relationship between `.infra/yaml`, `.infra/bicep`, and the existing `.github/workflows/*.yml` files so agents know which pipeline is authoritative.

## Project Plans

### vc.Ifx.Abstractions

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Audit all abstractions for minimal framework contracts.
- [ ] Remove implementation-specific types and dependencies.
- [ ] Define result, request, correlation, diagnostics, and options contracts that other packages can depend on.
- [ ] Add 100% tests for contract defaults, records, equality, validation, and null handling.
- [ ] Verify no provider-specific package references remain.

### vc.Ifx.Primitives

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Review entity IDs, value objects, JSON converters, EF converters, and model binding.
- [ ] Lock public semantics for equality, parsing, serialization, and conversion failures.
- [ ] Separate ASP.NET Core and EF concerns if package dependencies are too broad.
- [ ] Add 100% tests for all primitive construction, formatting, serialization, EF conversion, and model binding paths.

### vc.Ifx.Storage.Abstractions

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Define storage interfaces, request/response records, metadata records, and provider capabilities.
- [ ] Ensure abstractions do not depend on local file, FTP, or Azure SDK types.
- [ ] Define cancellation and failure semantics.
- [ ] Add 100% tests for options validation, DTO equality, and default contract behavior.

### vc.Ifx.Storage.Local

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Implement local storage strictly behind storage abstractions.
- [ ] Normalize path validation and idempotent delete behavior.
- [ ] Add deterministic temporary filesystem tests.
- [ ] Add 100% tests for read, write, exists, list, delete, metadata, invalid paths, cancellation, and concurrency.

### vc.Ifx.Storage.Ftp

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Wrap FluentFTP behind a testable client abstraction.
- [ ] Validate connection, credential, timeout, and path options.
- [ ] Avoid live network dependency in unit tests.
- [ ] Add 100% tests with fake FTP client for all success, failure, retry, cancellation, and disposal paths.

### vc.Ifx.Storage.Azure.Blobs

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Wrap Azure Blob SDK behind adapter interfaces.
- [ ] Validate account, container, credential, retry, and naming options.
- [ ] Define behavior for missing blob/container and conditional writes.
- [ ] Add 100% tests with fake Azure adapters for all read, write, list, delete, metadata, error, retry, and cancellation paths.

### vc.Ifx.Secrets.Abstractions

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Define secret provider contracts and result semantics.
- [ ] Keep abstractions free of configuration and Key Vault packages.
- [ ] Define missing-secret, empty-secret, and secret-version behavior.
- [ ] Add 100% tests for DTOs, option validation, and null object behavior.

### vc.Ifx.Secrets.Local

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Implement configuration-backed secret retrieval.
- [ ] Validate section names, key normalization, and reload behavior.
- [ ] Add 100% tests with in-memory configuration for found, missing, empty, invalid, and hierarchical keys.

### vc.Ifx.Secrets.Azure.KeyVault

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Wrap Key Vault SDK behind a testable adapter.
- [ ] Validate vault URI, credential mode, cache, retry, and version options.
- [ ] Define fallback behavior to local secrets if retained.
- [ ] Add 100% tests with fake Key Vault adapter for caching, misses, disabled secrets, failures, retry, cancellation, and options validation.

### vc.Ifx.Data.Azure.Tables

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Wrap Azure Tables SDK behind adapter interfaces.
- [ ] Define table, partition, row key, paging, and concurrency behavior.
- [ ] Validate options and entity mapping rules.
- [ ] Add 100% tests with fake table adapter for CRUD, query, paging, conflicts, missing rows, and cancellation.

### vc.Ifx.Messaging.Azure.Queues

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Wrap Azure Queue SDK behind adapter interfaces.
- [ ] Define message encoding, visibility timeout, poison handling, and delete semantics.
- [ ] Validate queue naming, credential, retry, and payload options.
- [ ] Add 100% tests with fake queue adapter for send, receive, peek, delete, update, empty queue, encoding, failures, and cancellation.

### vc.Ifx.Filtering

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Lock filter AST records and operator semantics.
- [ ] Fix expression translation for nested collection predicates.
- [ ] Validate unsupported expressions with clear exceptions.
- [ ] Add 100% tests for comparison, string, collection, group, negation, constants, captured variables, invalid expressions, and serialization compatibility.

### vc.Ifx.Filtering.EntityFrameworkCore

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Keep EF-specific expression execution out of core filtering.
- [ ] Validate operator translation to EF-compatible expressions.
- [ ] Add in-memory EF tests for query semantics.
- [ ] Add 100% tests for supported operators, unsupported operators, null values, collections, paging, and projection interactions.

### vc.Ifx.Querying

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Fix embedded schema resource naming and loading.
- [ ] Lock query filter JSON schema and serializer behavior.
- [ ] Define rehydration failure semantics.
- [ ] Add 100% tests for serialization, deserialization, schema access, validation, malformed payloads, unknown operators, and versioning.

### vc.Ifx.Pipeline

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Lock request, handler, interceptor, dispatcher, resolver, cache, tracing, and metrics contracts.
- [ ] Verify interceptor ordering and failure propagation.
- [ ] Validate local dispatch and endpoint resolution behavior.
- [ ] Add 100% tests for successful calls, failed calls, cancellation, ordering, caching, tracing, metrics, auth, logging, and retry behavior.

### vc.Ifx.Pipeline.Grpc

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Isolate generated gRPC code and suppress generated warnings only at generated boundaries.
- [ ] Validate serializer and gRPC dispatcher contracts.
- [ ] Avoid network-bound unit tests.
- [ ] Add 100% tests around adapters, request mapping, response mapping, failures, cancellation, and generated-code exclusions.

### vc.Ifx.Observability

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Define trace, metric, and log event contracts.
- [ ] Keep observability independent from concrete hosting.
- [ ] Verify no silent exception swallowing unless documented.
- [ ] Add 100% tests with fake activity/metric sinks for success, failure, tags, durations, nested spans, and no-op behavior.

### vc.Ifx.Proxy

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Lock proxy context, pipeline, transport, response, options, and interceptor contracts.
- [ ] Fix interceptor ordering, caching behavior, correlation setter semantics, auditing defaults, timing warning messages, auth DI, and resilience semantics.
- [ ] Remove duplicate or stale interceptor types.
- [ ] Add 100% tests for all interceptors, pipeline ordering, transports, options validation, caching hit/miss, retries, circuit breaker, correlation, auditing, security, authentication, and logging.

### vc.Ifx.Proxy.Http

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Keep HTTP transport behavior separate from proxy core.
- [ ] Use `HttpMessageHandler` fakes for all tests.
- [ ] Define serialization, headers, timeout, cancellation, and error classification behavior.
- [ ] Add 100% tests for HTTP methods, headers, content, success, non-success, timeout, cancellation, and disposal.

### vc.Ifx.Proxy.AspNetCore

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Isolate ASP.NET Core user, tenant, JWT, and model-binding integrations.
- [ ] Register required hosting dependencies explicitly.
- [ ] Define claims/header/path/subdomain tenant extraction semantics.
- [ ] Add 100% tests for DI registration, user extraction, tenant extraction, token validation, enrichers, invalid contexts, and anonymous/default contexts.

### vc.Ifx.WebApi

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Treat Web API hosting infrastructure as a separate cross-cutting package, not part of proxy core or the aggregator.
- [ ] Define `docs/webapi/http-response-catalog.md` as the source-of-truth design artifact before implementation. The implementation and README must match this catalog.
- [ ] Define a stable HTTP response catalog that covers every standard IANA/RFC HTTP status code exposed by ASP.NET Core: informational, success, redirection, client error, and server error responses. Non-standard vendor codes are excluded unless explicitly approved and documented.
- [ ] For each response catalog entry, define the status code, reason phrase, default title, default safe detail message, RFC/problem-details type URI, retryability classification, and whether the detail is safe for clients.
- [ ] Include common production failures explicitly: `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `409 Conflict`, `422 Unprocessable Entity`, `429 Too Many Requests`, `500 Internal Server Error`, `502 Bad Gateway`, `503 Service Unavailable`, and `504 Gateway Timeout`.
- [ ] Expose the response catalog as immutable lookup APIs, not mutable public dictionaries. Add direct lookup by status code and typed helpers for common responses.
- [ ] Ensure global exception handling maps known exceptions to catalog entries and never leaks exception details unless explicitly configured.
- [ ] Keep resilience artifacts policy-based and replaceable: Polly retry, timeout, and transient HTTP/server-error classification belong behind small framework abstractions.
- [ ] Add 100% tests for response catalog completeness, immutability, lookup behavior, exception mapping, problem-details output, retryability classification, DI registration, and resilience defaults.

### vc.Ifx.Roslyn

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Keep `netstandard2.0` for Roslyn compatibility.
- [ ] Define shared diagnostic IDs, categories, descriptors, and helper APIs for vc.Ifx-owned diagnostics.
- [ ] Define `docs/roslyn/diagnostic-catalog.md` as the source of truth for `IFX####`, selected `CA####`, and selected `CS####` remediation support. Catalog entries must record owner, category, severity, title, help link, fix availability, and whether the diagnostic is emitted by vc.Ifx, the C# compiler, or Microsoft.CodeAnalysis.NetAnalyzers.
- [ ] Do not re-emit `CS####` compiler diagnostics or `CA####` Microsoft analyzer diagnostics just to own the ID. Add vc.Ifx analyzers only when no upstream diagnostic exists or when framework-specific policy cannot be expressed by an upstream rule.
- [ ] Avoid runtime-framework dependencies.
- [ ] Add 100% tests for descriptors, catalog integrity, syntax helpers, semantic helpers, and diagnostic metadata.

### vc.Ifx.Analyzers

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Define analyzer scope and diagnostic catalog, including the existing diagnostic-debt analyzer behavior for `CS####`, `CA####`, and `IFX####` references in comments, suppressions, and pragmas.
- [ ] Inventory common `CA####` and `CS####` issues found in the repository. For each issue, classify it as upstream-owned, vc.Ifx-policy-owned, code-fix-only, ignored with rationale, or obsolete.
- [ ] Add analyzers for missing framework-level rules only when an equivalent `CA####` or `CS####` does not exist. New rules use `IFX####` IDs, not borrowed `CA####` or `CS####` IDs.
- [ ] Add analyzer options for project-specific policy only where configuration is necessary for repeatable builds; avoid rules that depend on local machine state or live network calls.
- [ ] Ensure analyzers are deterministic and cancellation-aware.
- [ ] Add analyzer test harness.
- [ ] Add 100% tests for every diagnostic: no diagnostic, diagnostic, severity, location, message, config, generated-code exclusion, concurrent execution, and cancellation where practical.

### vc.Ifx.CodeFixes

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Pair each code fix with a cataloged diagnostic source: vc.Ifx analyzer, C# compiler, or Microsoft.CodeAnalysis.NetAnalyzers.
- [ ] Add code fixes for selected `CA####` and `CS####` diagnostics when no existing Microsoft/Roslyn fix covers the framework's expected remediation. Code fixes may target upstream diagnostics, but analyzers must not duplicate upstream diagnostics.
- [ ] Start with high-value recurring fixes: unused or misordered usings, nullable guard clauses, disposal patterns, cancellation-token propagation, file-scoped namespace conversion, XML documentation gaps for public APIs, analyzer release tracking entries, and safe suppression documentation.
- [ ] Maintain `docs/roslyn/code-fix-support-matrix.md` listing diagnostic ID, source diagnostic package, fix provider type, Fix All support, limitations, behavior-risk level, and target test file.
- [ ] Preserve formatting and trivia.
- [ ] Preserve semantics. Any fix that may change behavior must be opt-in, single-diagnostic, and documented in the support matrix.
- [ ] Add code fix test harness.
- [ ] Add 100% tests for every fix, no-op cases, multi-location fixes, formatting, and Fix All behavior.

### vc.Ifx.Generators.Abstractions

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Define public marker attributes for source generators, including the minimal API endpoint attribute.
- [ ] Keep attributes passive: no runtime behavior, no ASP.NET Core dependency unless the public contract explicitly requires it and the dependency is documented.
- [ ] Version attribute constructor parameters and settable properties conservatively so generated-code contracts remain source-compatible.
- [ ] Add 100% tests for attribute construction, defaults, property assignment, null handling, equality expectations where applicable, and XML documentation examples.

### vc.Ifx.Generators

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Define generator inputs, generated API, diagnostics, and incremental behavior.
- [ ] Add a minimal API endpoint generator driven by an attribute in `vc.Ifx.Generators.Abstractions`. The attribute must be usable without referencing the generator implementation package.
- [ ] Define `docs/roslyn/minimal-api-generator-contract.md` before implementation: accepted target shapes, route template source, HTTP method source, request/response mapping, DI dependency access, authorization metadata, OpenAPI metadata, cancellation-token handling, problem-details behavior, generated extension method naming, and binary compatibility expectations.
- [ ] Prefer explicit attributes over naming conventions. Avoid runtime reflection and avoid scanning assemblies at startup.
- [ ] Generate deterministic endpoint registration code that can be called from application startup, for example `app.MapGeneratedIfxEndpoints()` or a similarly stable extension method.
- [ ] Emit diagnostics for invalid endpoint attributes, unsupported method signatures, duplicate routes, ambiguous HTTP methods, inaccessible target types, non-serializable public contracts, and missing required route metadata.
- [ ] Ensure generated minimal API endpoints integrate with `vc.Ifx.WebApi` problem-details and response catalog behavior without introducing a hard runtime dependency cycle.
- [ ] Keep the generator independent from ASP.NET Core runtime execution. Generated source may reference ASP.NET Core abstractions in the consumer project, but the generator itself must stay Roslyn-compatible and `netstandard2.0`.
- [ ] Ensure generated output is deterministic.
- [ ] Add snapshot or golden-file tests for generated code.
- [ ] Add 100% tests for valid inputs, invalid inputs, incremental caching, diagnostics, cancellation, generated source compile verification, and runtime smoke tests using ASP.NET Core in-memory hosting.

### vc.Ifx

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: unassigned; Updated: not-started; Notes:

- [ ] Decide whether this remains an aggregator package or becomes a compatibility shim.
- [ ] Keep direct dependencies intentional and documented.
- [ ] Avoid reintroducing implementation dependencies into abstraction-only consumers.
- [ ] Add integration-style unit tests for package-level extension methods and aggregate DI registration.

## Parallel Assignment Map

- Agent A: abstraction packages and primitive contracts.
- Agent B: storage packages.
- Agent C: secrets packages.
- Agent D: filtering and querying packages.
- Agent E: pipeline, gRPC, and observability.
- Agent F: proxy core, HTTP, ASP.NET Core proxy integration.
- Agent G: Roslyn shared contracts, analyzers, code fixes, and diagnostic catalog.
- Agent H: package metadata, coverage infrastructure, CI gates.
- Agent I: aggregator package and cross-package verification.
- Agent J: WebApi response catalog, global exception handling, problem-details integration, and Polly resilience policies.
- Agent K: generator abstractions, generators, minimal API endpoint attributes, generated endpoint registration, and generator integration tests.

Agents may work in parallel under the dispatch readiness rules above. If a project requires a contract change in another package, the agent must mark the dependent project `Blocked`, document the needed contract, and stop before making cross-boundary edits.

## Agentic Implementation Rules

- [ ] Each agent claims exactly one project section or gate by setting `In-flight`, owner, date, and a one-line scope before editing.
- [ ] Each agent checks the dependency order and dispatch readiness rules before claiming project implementation work.
- [ ] Each agent writes or updates the package README as part of the same workstream as code changes.
- [ ] Shared contracts are changed only by the owning package agent. Dependent agents document requested changes in the blocked project's Notes line.
- [ ] An agent adding a diagnostic, code fix, generator attribute, or response catalog entry also adds the matching tests and package documentation in the same change.
- [ ] Agents use the support matrix or response catalog as the source of truth before adding new public API. Duplicate concepts are consolidated before implementation begins.
- [ ] Agents create or update the named design artifact first, then implement code and tests against that artifact. If the artifact and implementation disagree, the project stays `In-flight`.
- [ ] Agents record the exact verification command and result in the project Notes line before moving from `In-flight` to `Complete`.
- [ ] A project cannot be marked `Complete` when it depends on a `Blocked` project unless the dependency is optional and explicitly documented.
- [ ] Agents do not edit another agent's `In-flight` section unless the owner explicitly hands it off in the Notes line.

## Plan Review Loop

### Review Pass 1: Boundary Check

- [x] Confirmed the added work keeps WebApi, proxy, Roslyn, analyzers, code fixes, generators, and aggregator responsibilities separate.
- [x] Clarified that `CS####` and `CA####` diagnostics remain compiler/Microsoft-owned unless vc.Ifx adds a truly missing framework policy.
- [x] Added WebApi as a first-class cross-cutting package instead of hiding HTTP response behavior in proxy or aggregator work.

### Review Pass 2: Parallelism Check

- [x] Split Roslyn diagnostic catalog, WebApi response catalog, and minimal API generator work into separate agent lanes.
- [x] Added source-of-truth artifacts so agents can work independently without inventing duplicate APIs.
- [x] Added blocking rules for cross-package contract changes.

### Review Pass 3: Verification Check

- [x] Added explicit 100% test expectations for catalog completeness, code-fix behavior, generator diagnostics, and runtime smoke tests.
- [x] Required every diagnostic, code fix, generator attribute, and response catalog entry to ship with tests and README updates.
- [x] Required agents to record exact verification commands before marking work complete.

## Regression Re-Verification

When a `Complete` project's dependency changes contract (a project it consumes, or a project it is consumed by), the owning agent re-runs that project's targeted test command and re-confirms its coverage threshold before any other status change is accepted. Agents record the re-verification date and result in the project's Notes line. A `Complete` project that fails re-verification reverts to `In-flight` until fixed.

## Escalation

Agents do not auto-resolve `Blocked` status. When a project is `Blocked`, the agent documents the blocking contract question in that project's Notes line and stops. The plan owner decides how to proceed and updates the status.

## Verification Commands

Use these commands as the default evidence set.

```powershell
dotnet restore vc.Ifx.slnx
dotnet build vc.Ifx.slnx --no-restore -v:minimal
dotnet test vc.Ifx.slnx --no-build -v:minimal
```

Coverage command will be finalized by Gate 1. Until then, agents must document the exact targeted test command they used in their project status notes.
