---
title: vc.Ifx Follow-on Optimization Plan
doc_type: plan
status: active
last_updated: 2026-09-10
---

# vc.Ifx Follow-on Optimization Plan

## Goal

Follow the [modernization plan](framework-upgrade-parallel-plan.md) with measured
.NET 10/C# 14 optimization, stronger compatibility guarantees, deliberate trimming
and Native AOT support, and tests that verify correctness beyond execution coverage.
This is Plan 2, not permission to interrupt Plan 1's active workers or final capture.

Plan owner: Orchestrator. Authored: 2026-09-10.
Document state: Reviewed for gated agentic dispatch; only P2-00 is initially dispatchable.
Implementation state: Not started. No performance or AOT improvement is claimed.

Agent reading order: read Scope and Decisions, Status Protocol, Dispatch and Shared
Ownership, and Completion Standard first. Then read only the assigned workstream,
its direct dependency handoffs, relevant package-routing rows and named artifacts.
Do not load every historical Plan 1 verification log to begin a bounded assignment.

## Scope and Decisions

- Preserve VBD boundaries, public identities, cancellation, ownership, security and
  observable behavior. QuerySpec remains database-access-oriented; FilterSpec remains
  predicate-oriented. Do not create a shared business-domain model.
- Start from the accepted Plan 1 SDK/runtime/language versions. The recorded baseline
  is SDK 10.0.401, net10.0 and C# 14.0; this is not a claim that every pinned dependency
  remains the newest release when an agent eventually starts.
- P2-01 verifies the then-current stable baseline using official release sources.
  A newer SDK/language major version needs a coordinated plan-owner decision and
  compatibility rerun. Never use preview or floating `latest` language settings.
- Keep Roslyn, Analyzers, CodeFixes and Generators on netstandard2.0 for the supported
  compiler hosts. Generators.Abstractions remains a net10.0 runtime attribute package.
- Use language features for clarity and domain expression, not feature-count targets.
  No wholesale extension-block, `field`, Span, ValueTask or collection conversion.
  Benchmark performance-sensitive changes and test overload/behavior compatibility.
- Compatible work is Track A (P2-00 through P2-13). The primitives dependency split is
  Track B (P2-14 and P2-15), blocked pending an explicit major-version decision.
  Track A can finish while Track B remains Blocked; the complete follow-on program
  cannot be described as fully delivered while Track B is unresolved.
- Retain Plan 1's documented compatibility exceptions until the relevant approved
  workstream replaces them with a tested migration. Do not silently rename legacy APIs.
- No publish, repository-settings change, toolchain installation or remote mutation
  is authorized by this document. Record required external acceptance separately.

## Status Protocol

Each `### P2-NN` section has exactly one active Status and one current Owner line.
The section is authoritative; tables are navigation, not duplicate status ledgers.

`Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete`

`Owner: Unassigned; Updated: yyyy-mm-dd; Notes: prerequisites and claim scope.`

- Ready means queued and unclaimed, not necessarily dispatchable. All listed direct
  dependencies must be Complete, required artifacts accepted, and files released.
- In-flight means one agent has claimed exactly that workstream. Before editing,
  record Owner, Updated, Notes, exact file slice and planned verification commands.
- Blocked means an unresolved decision, missing environment or incompatible contract.
  Notes must state the exact decision/evidence needed and its owner. Only the plan
  owner releases a Blocked section after recording the resolution.
- Complete means the workstream's stated output and applicable completion criteria
  have evidence. Design completion is not implementation completion. A rejected
  optimization experiment is a measured decision, not a delivered speedup.
- Preserve prior Notes as dated history below the current header when handing off.
  An implementation-ready handoff stays In-flight until accepted by the orchestrator.

## Dispatch and Shared Ownership

1. P2-00 alone is initially dispatchable, for read-only handoff inspection and this
   plan's documents. It must not launch work against an active Plan 1 capture.
2. After P2-00: P2-01, P2-03 and P2-04 can run concurrently on disjoint files.
   P2-02 and P2-05 follow P2-01; they may then run beside design work.
3. Runtime lanes P2-06, P2-07 and P2-08 follow their dependencies below. Their exact
   source slices are separate; package-wide builds still share a serialized queue.
4. P2-09 follows proxy optimization. P2-10 and P2-11 follow the affected runtime
   changes; P2-12 also waits for P2-10's annotation changes. P2-13 is the single
   Track A integration/acceptance lane.
5. Track B is never dispatched by inference from Track A acceptance. P2-14 needs the
   named user decision; P2-15 performs a new major-version integration checkpoint.

- Read AGENTS.md, Plan 1's applicable boundary contracts and this plan before claiming.
  Never edit another agent's In-flight section or source slice without explicit handoff.
- One agent may progress through multiple assignments sequentially, never claim two.
  The orchestrator relays cross-package requests, freezes contracts and queues builds.
- P2-01 initially owns shared project metadata, central dependencies, solution/project
  inventory, baseline configuration and common runner changes. Later workers submit
  a path/diff/reason/test request in their Notes; the orchestrator grants a bounded
  handoff or reopens P2-01. No simultaneous shared-file edits are permitted.
- During P2-06/07/08, package README additions are first drafted in each lane's design
  artifact. The orchestrator grants serial README edit windows; no simultaneous
  edits to the same README, csproj or centralized test-project file.
- P2-12 is a comment-only source pass after runtime/AOT writers stop. P2-11 is a test-only
  pass; defects are returned to source owners, not independently patched across lanes.
- All builds/tests use the existing repository mutex and single-node settings through
  `scripts/Invoke-FrameworkTests.ps1`. Benchmark runs use the existing mutex wrapper.
  New API/AOT/mutation runners must acquire the same lock; P2-01/P2-05 own that work.
- Mutation tools must use disposable isolated copies outside source build globs, with
  explicit input manifests. Never mutate the shared checkout or recorded build output.
- No source, shared settings or generated inputs may change during an authoritative
  build/coverage/report/package capture. Benchmark-generated binaries are not reused
  as tested release binaries. A contract change reopens affected dependent acceptance.

## Completion Standard

Apply Plan 1's completion standard to every modified/new library, plus these rules:

- [ ] Accepted design artifact names exact APIs, compatibility constraints, owner and
  consuming packages before implementation begins.
- [ ] Targeted Release tests pass; changed library modules retain exact 100% line and
  branch coverage with existing generated-only exclusions. Zero tests is failure.
- [ ] Public API/behavior baseline comparison passes or each intended break has the
  Track B approval. Do not regenerate a baseline merely to hide a difference.
- [ ] README/XML contracts explain nulls, failures, cancellation, ownership, DI,
  thread safety and relevant limitations. No blanket warning or coverage suppressions.
- [ ] Performance claims have comparable baseline/candidate measurements and a decision
  under P2-02's predeclared budgets. Dry smoke and a noisy Short run are insufficient.
- [ ] AOT/trimming claims identify exact supported package/API/host/RID combinations,
  with publish-and-run proof. An annotation alone is not compatibility evidence.
- [ ] Source and dependency boundaries are validated. Test/benchmark/consumer fixtures
  remain non-packable and outside runtime public dependencies and coverage denominators.
- [ ] Notes record exact commands, exit codes, counts, artifact paths/hashes and remaining
  external gates. Failed and inconclusive experiments remain visible.
- [ ] P2-13 or P2-15 performs a fresh warning-free full solution build, unfiltered full
  suite, strict current-inventory coverage, paired report and tested-binary package
  validation before integrated delivery is accepted.

Design-only sections require accepted artifacts and checked decisions, not invented
coverage. Tooling sections require positive and fail-closed regression tests plus
integration proof; do not claim library coverage measures PowerShell tooling.

## Workstreams

### P2-00: Accept the Modernization Handoff

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Only initial dispatch. No Plan 1 source edits or build activity; await explicit source release from its orchestrator.

Direct dependencies: Plan 1 local acceptance and explicit handoff, not an assumed status.
Write scope: this plan; `docs/planning/optimization-baseline.md` (new).
Required artifact: current `implementation-progress.md` and `implementation-acceptance-audit.md` as inputs.

- [ ] Record accepted revision, dirty/untracked source identity, full build/coverage/report
  identity and final package-manifest hashes. HEAD alone does not identify a dirty build.
- [ ] Confirm the handoff inventory (currently 28 libraries/36 solution projects), exact
  SDK/language versions and release baseline archives. Reconcile actual inventory.
- [ ] Preserve outstanding hosted CI, Visual Studio and release-feed checks as named
  external gates with owner and required evidence. Do not rewrite Plan 1 statuses.
- [ ] Obtain explicit release of shared files and worker-owned source slices.
- [ ] Accept `optimization-baseline.md`; release the first parallel wave.

Verification: read existing manifests and source hashes; record discrepancies without
launching a rebuild. Missing final evidence keeps this section In-flight or Blocked.

### P2-01: Stable Configuration and API Compatibility

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P2-00. Own shared configuration and API-baseline infrastructure; no runtime algorithm rewrites.

Direct dependencies: P2-00.
Write scope: shared props/targets/central package settings; approved csproj properties;
`scripts/compatibility/`, `tests/infrastructure/compatibility/` (new); related CI glue by handoff.
Required artifact: `docs/architecture/api-compatibility-policy.md` (create before code).

- [ ] Remove the aggregator's misleading local `LangVersion=preview`; inherit or explicitly
  match stable 14.0. Verify every project's evaluated settings, not XML strings alone.
- [ ] Record how future stable SDK/C# updates are proposed, assessed and accepted. Review
  pinned dependencies against current official releases/security notices; do not blindly
  upgrade private Roslyn-host compatibility dependencies to runtime-framework versions.
- [ ] Identify actual released package versions and archive hashes. Use the accepted
  modernization artifacts as an explicitly labeled pre-release baseline where no release
  exists; never invent a published version or fetch an untrusted baseline silently.
- [ ] Add SDK package validation/API comparison for runtime libraries. For analyzer-style
  packages without lib assets, compare the intended assemblies and exercise compiler/MEF
  consumers; ordinary runtime package validation alone is insufficient.
- [ ] Validate source/binary compatibility with sample consumers, nullable contracts,
  overload resolution, extension discovery, DI registration and retained namespace identities.
- [ ] Add failing fixtures for removed/changed members, missing baseline, unsupported
  package layout and unauthorized suppression; additive compatible fixtures must pass.
- [ ] Provide mutex-aware API/AOT consumer runner contracts; keep existing two centralized
  test projects intact. Register additional non-packable smoke hosts explicitly in inventories.
  Update test/coverage/package discovery to distinguish consumer csproj files from the two
  test projects; do not let a recursive tests/**/*.csproj scan treat every consumer as a test.
  Add missing/extra-test and consumer-exclusion regressions before adding smoke hosts.
- [ ] Freeze baseline policy and a minimal C# style policy. No cosmetic bulk migration;
  document justified modern syntax and analyzer-host restrictions.

Verification: evaluated settings inventory, compatibility fixture suite, real baseline
comparison, old/new consumer compilation and existing package-host checks. Publish the
exact new runner commands in the policy before consumers are dispatched.

### P2-02: Reliable Performance Baselines

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P2-01. Performance measurement only; reserve the host and benchmark files before use.

Direct dependencies: P2-01.
Write scope: `performance/`, benchmark runner; `docs/performance/` (new).
Required artifact: `docs/performance/baseline-and-budgets.md` (create before timing).

- [ ] Retain all existing 54 execution cases; define representative size, hit/miss,
  concurrency, cancellation and failure scenarios without timing setup or fixture cleanup.
- [ ] Add dedicated measurements where relevant for WebApi, gRPC, secrets, observability
  and analyzer/generator execution. Separate CPU/allocation costs from network-service latency.
- [ ] Record source/build/package identity, SDK/runtime, OS/CPU/architecture, GC, JIT/PGO,
  power configuration, inputs, warmup and repetitions. Require an idle reserved machine;
  do not silently change its power plan or infer isolation from the repository mutex.
- [ ] Use repeated Default jobs and comparable baseline/candidate source snapshots on the
  same environment. Measure allocations, throughput and cold-start separately; measure
  tail latency in a suitable repeated concurrent workload, not from a mean-only microbenchmark.
  Reserve a source/host freeze for each measurement window: no edits to measured inputs
  and no competing build, AOT or mutation job, even in a different checkout on the host.
- [ ] Establish per-scenario regression budgets from repeatability and product constraints
  before candidate measurements. Record minimum meaningful effect and uncertainty rules;
  inconclusive results cannot prove an improvement or justify relaxed budgets.
- [ ] Add deterministic comparison-tool fixtures for mismatched environments, missing cases,
  stale snapshots and noisy results. Keep statistical baselines separate from CI smoke jobs.
- [ ] Release owned benchmark files by handoff to P2-06/07/08/09. Record immutable baseline
  artifact hashes for their before/after comparisons.

Verification: `Invoke-FrameworkBenchmarks.ps1 -Job Dry` for execution, then `-Job Default`
on the reserved machine. Baselines require the accepted protocol, not just an exit code.
Block with the exact host/time reservation needed if reliable measurement is unavailable.

### P2-03: Trimming and Native AOT Contracts

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P2-00. Design/inventory only; do not enable global AOT properties.

Direct dependencies: P2-00.
Write scope: `docs/architecture/aot-support-matrix.md` and `docs/architecture/aot-contracts.md` (new).
Required inputs: accepted package boundaries, endpoint generator contract and source inventory.

- [ ] Classify every library/API slice as candidate, supported-with-limits, unsupported
  or host-only. Record dynamic proxy generation, reflection, runtime Type serialization,
  expression execution, DI activation and third-party dependency risks.
- [ ] Choose representative JIT, trimmed-JIT and Native AOT consumer scenarios with explicit
  Windows/Linux RIDs and toolchain prerequisites. Missing environments remain unverified.
- [ ] Freeze additive JSON metadata injection, explicit compiled predicate reuse and optional
  generated-proxy contracts for P2-06/08/09. Resolve cross-package ownership before coding.
- [ ] Preserve interpreter-compatible expression paths where proven; do not equate all
  `Expression.Compile` usage with an unconditional AOT failure or claim equal performance.
- [ ] Specify warning expectations, unsupported-path diagnostics, metadata preservation and
  consumer-owned serialization of arbitrary DTOs. No automatic blanket reflection fallback.
- [ ] Accept the matrix with concrete test requirements; marking design Complete grants no
  `IsAotCompatible` promise. P2-10 owns actual support certification.

Verification: source/API review with exact paths and official platform references;
consumer acceptance cases and required user/toolchain decisions recorded.

### P2-04: Primitives Dependency-Split Design

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P2-00. Design only; the current compatibility exception remains active.

Direct dependencies: P2-00.
Write scope: `docs/architecture/primitives-package-migration.md` (new).
Required inputs: Primitives API inventory, current dependency graph and aggregator boundary.

- [ ] Inventory pure identifier/value APIs versus EF converters and ASP.NET model binders;
  record assembly identity, namespaces, extension discovery, DI and transitive dependencies.
- [ ] Propose `vc.Ifx.Primitives.EntityFrameworkCore` and `vc.Ifx.Primitives.AspNetCore` as
  candidate names only; confirm names/versioning before any package creation.
- [ ] Design a dependency-light Primitives core with adapters depending inward on it. No
  duplicate EntityId/value types, dependency cycles, or core references back to adapters.
- [ ] Evaluate source/binary migration and type-forwarding limits. Do not claim that a
  forwarding shim removes core dependencies when it necessarily references an adapter.
- [ ] Document consumer before/after references, namespace/serialization compatibility,
  aggregator behavior, migration examples and a rollback/release strategy.
- [ ] Present the exact Track B decision: approved major version, final package names,
  migration/compatibility policy and supported consumers. Design may Complete while
  implementation remains Blocked; record the unresolved decision explicitly.

Verification: reviewed dependency DAG and API movement table; acceptance of a design
does not authorize removal of compatibility APIs in the current version.

### P2-05: Property-Based and Mutation Infrastructure

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P2-01. Own quality tooling, not package test rewrites or production defects.

Direct dependencies: P2-01.
Write scope: `scripts/testing/quality/`, `tests/infrastructure/quality/` (new);
test-tool dependencies/shared runner integration only through the shared-file handoff.
Required artifact: `docs/testing/property-and-mutation-policy.md` (create before code).

- [ ] Evaluate maintained .NET 10/C# 14-compatible property/mutation tools and licenses;
  prefer a proven tool over a custom mutation engine. Freeze versions and exact commands.
- [ ] Require deterministic recorded seeds, shrinking/minimal counterexamples and replay
  commands; never use wall-clock or live-cloud nondeterminism as a correctness oracle.
- [ ] Define contract-driven initial mutation slices and per-slice acceptance before runs.
  Report killed, survived, timeout, not-covered, invalid and excluded mutants separately.
  Keep raw tool scores alongside any justified equivalent-mutant review; do not hide survivors.
- [ ] Reject empty selections/zero mutants, missing test discovery and blanket exclusions.
  Require documented disposition of every survivor; distinguish equivalent mutants from
  actual test gaps. Mutation scores do not replace strict line/branch coverage.
- [ ] Isolate mutation copies outside main source/output globs; use source manifests,
  the shared lock and bounded resource/time budgets. Never touch accepted binaries.
- [ ] Add fail-closed runner regressions and a real small seeded mutation demonstration
  proving an intentionally weakened assertion is detected, then restore the fixture.

Verification: tool smoke, deterministic replay/shrink tests, isolation/hash checks,
positive/negative process-exit regressions. Publish the P2-11 invocation contract.

### P2-06: Reusable Filtering Execution

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P2-01/02/03. Own Filtering execution only; serialization belongs to P2-08.

Direct dependencies: P2-01, P2-02, P2-03.
Write scope: FilterSpec, POCO execution and EF execution strategy; dedicated new tests;
Filtering benchmark execution cases after P2-02 handoff. No AST/JSON-format redesign.
Required artifact: `docs/performance/filter-execution-design.md` (create before code), plus accepted AOT contracts.

- [ ] Measure current translate/compile/repeated-apply costs; separate IQueryable provider
  translation from in-memory execution so optimization cannot cause client-side enumeration.
- [ ] Evaluate explicit reusable compiled delegates/prepared execution versus per-instance
  caching. Choose the smallest justified API; reject global unbounded expression caches.
- [ ] Preserve captured-variable behavior, portable snapshot semantics, deferred enumeration,
  repeated use, short-circuiting, exceptions, thread safety and collection membership precision.
- [ ] Test JIT and accepted interpreted paths; do not promise identical AOT throughput.
- [ ] Rerun Querying and SQLite EF consumers, strict package coverage and P2-02 comparisons.
  Retaining current behavior is acceptable only with recorded negative/inconclusive evidence,
  not an unmeasured claim that caching is unnecessary.

Verification: scoped whole-module coverage for Filtering/Querying/EF; integration suite;
baseline/candidate execution measurements; API comparison. Record actual filters/scopes.

### P2-07: Proxy Hot-Path Optimization

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P2-01/02/03. Own BoundaryProxy invocation/adaptation; no JSON/token/cache-key serializer edits.

Direct dependencies: P2-01, P2-02, P2-03.
Write scope: `src/vc.Ifx.Proxy/Proxies/BoundaryProxy.cs`, directly required invocation helpers,
dedicated tests and proxy dispatch benchmark cases by handoff.
Required artifact: `docs/performance/proxy-dispatch-design.md` (create before code).

- [ ] Measure parameter metadata lookup, MethodArgument/context allocations, delegate
  composition, reflection dispatch and sync/Task/ValueTask adaptation independently.
- [ ] Evaluate metadata/dispatch-plan reuse without caching request-scoped state, target
  instances, mutable arguments or closures across calls. Bound caches and test unloading
  where collectible types are supported; otherwise document unsupported scenarios.
- [ ] Preserve interceptor order, replacement context semantics, exceptions, cancellation,
  reentrancy, concurrent calls, DI lifetimes and supported return shapes.
- [ ] Keep retries replay-safe and SDK resilience ownership unchanged; performance work must
  not duplicate retry layers or weaken authorization, tenant isolation or cache boundaries.
- [ ] Record accepted improvements and rejected experiments using P2-02 budgets. Freeze the
  invocation contract and hand off to P2-09; retain dynamic proxy compatibility.

Verification: strict Proxy coverage, HTTP/ASP.NET cross-package regressions, API comparison,
concurrent-call tests and comparable allocation/throughput measurements.

### P2-08: Metadata-Driven JSON Serialization

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P2-01/02/03. Bounded serialization lane; shared README/csproj edits require handoff.

Direct dependencies: P2-01, P2-02, P2-03.
Write scope: serialization files in Filtering, Querying, Primitives, Pipeline, Proxy.Http,
Proxy token/config/cache-key handling, Queues and Roslyn.Reporting; dedicated JSON tests.
Exclude P2-06 execution files and P2-07 invocation/adaptation files.
Required artifact: `docs/architecture/json-metadata-contract.md` (create before code).

- [ ] Inventory each reflection-based path and its current options/converter/polymorphism
  behavior. Define fixed framework DTOs versus arbitrary consumer types before generating.
- [ ] Add source-generated metadata for fixed supported framework payloads where justified;
  provide additive `JsonTypeInfo<T>`/context/resolver injection for consumer-owned types.
  Preserve old overloads and explicit compatibility fallbacks, with truthful annotations.
- [ ] Preserve wire names, casing, null/default handling, numeric/enum formats, errors,
  custom converters, streams, cancellation and response content-type behavior.
- [ ] Keep existing cache-key bytes/identity stable unless a separate migration is approved.
  Do not silently change key canonicalization or log secret/token payloads.
- [ ] Test with JSON reflection defaults disabled, missing metadata, custom options,
  malformed/unknown values and representative generic DTOs. No universal DTO/AOT promise.
- [ ] Measure cold metadata cost and steady-state allocations separately. Record per-package
  implementation or evidence-backed no-change disposition in the support matrix.

Verification: old/new byte and behavior parity, strict coverage for every modified module,
consumer compile tests and JSON benchmarks. AOT publication is certified by P2-10.

### P2-09: Optional Generated Proxy Dispatch

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P2-01/02/03/07. Evaluate and implement only the accepted bounded generated path; retain dynamic fallback for existing JIT consumers.

Direct dependencies: P2-01, P2-02, P2-03, P2-07.
Write scope: new proxy generator files in Generators, approved passive marker files in
Generators.Abstractions, dedicated generated-proxy registration files/tests/benchmarks.
Required artifact: `docs/roslyn/generated-proxy-contract.md` (create before code).

- [ ] Compare the current generator facilities before adding another mechanism. Define
  supported interface shapes, constraints, accessibility, return values, cancellation,
  DI lifetimes, name collisions and diagnostics. Reuse frozen invocation semantics.
- [ ] Benchmark a bounded candidate against P2-07 dynamic dispatch. The plan owner accepts
  implementation based on measured benefit or an explicit required AOT scenario. If neither
  holds, Complete the evaluation with a no-ship decision and mark that matrix path unsupported.
- [ ] If accepted, add opt-in deterministic incremental generation; no startup scanning,
  runtime code emission, host runtime dependency cycle or implicit behavior switch.
- [ ] Reject unsupported shapes with stable cataloged diagnostics; add passive attributes
  conservatively and update diagnostic catalog/host tests through a scoped handoff.
- [ ] Prove generated/dynamic parity for all supported invocation shapes, concurrent calls,
  exceptions and cancellation; include snapshots, incremental invalidation, malformed
  source, compiler-host discovery and packaged consumer compilation.

Verification: strict Generators/Generators.Abstractions and changed Proxy coverage,
actual packaged consumer tests, runtime parity and baseline/candidate measurements.
Evaluation-only completion must be labeled explicitly; it is not generated-proxy delivery.

### P2-10: Certify Trimming and AOT Consumers

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P2-03/06/08/09. Claims follow tested support slices, not whole-repository annotations.

Direct dependencies: P2-03, P2-06, P2-08, P2-09.
Write scope: non-packable `tests/consumers/aot/` fixtures (new), AOT runner/tests and
approved per-package annotations/properties; shared files only by handoff.
Required artifact: accepted AOT support matrix and P2-01 consumer runner contract.

- [ ] Publish and execute representative JIT, trimmed-JIT and Native AOT consumers from
  actual package archives, including DI, JSON, primitives, predicates and generated
  minimal endpoints. Include generated proxies only if P2-09 accepted that path.
- [ ] Verify Windows/Linux RID scenarios and toolchain availability. Missing native
  prerequisites block those rows; never substitute a normal build for publish/run proof.
- [ ] Resolve warnings at their owning APIs. Add accurate dynamic-code/unreferenced-code
  annotations and narrowly justified metadata preservation; no blanket warning suppression.
- [ ] Enable `IsAotCompatible` only for proven libraries/contracts. A failed optional
  path may remain explicitly unsupported if accepted by the plan owner; do not lower
  a previously promised supported row simply to make the gate green.
- [ ] Check startup, allocations and binary size independently of throughput. Native AOT
  is an opt-in deployment capability, not a mandate for every server application.
- [ ] Record exact package/source/toolchain/RID identities and successful process behavior;
  keep compiler-host libraries outside runtime AOT claims.

Verification: accepted matrix cells have publish logs, warning review, executable smoke
assertions and process exits. Runner negative tests reject omitted/failed/mislabeled cells.

### P2-11: Stronger Behavioral Tests

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P2-05/06/07/08/09. Test-only lane after runtime changes freeze; return defects to their source owners.

Direct dependencies: P2-05, P2-06, P2-07, P2-08, P2-09.
Write scope: new property/mutation regression test files under existing package test
folders; `docs/testing/quality-results.md` (new). No unchecked production source edits.
Required artifact: accepted property/mutation policy and each package's public contracts.

- [ ] Add deterministic properties for identifiers/equality/round trips, filtering Boolean
  laws and portable serialization, QuerySpec paging/order semantics, result propagation,
  response catalog invariants and cache/authorization isolation where contracts support them.
- [ ] Include null, boundaries, Unicode/culture, overflow, cancellation and malformed input.
  Restrict algebraic properties to pure supported predicates; do not assume side-effecting
  user expressions obey transformations that change evaluation order.
- [ ] Run contract-driven mutation slices for validation, retry eligibility, authorization,
  cache partitioning, exception mapping and generator diagnostics. Follow predeclared budgets.
- [ ] Turn surviving meaningful mutants into focused tests; record equivalent/invalid
  cases with reviewable evidence. Use actual stable seeds and minimal failing examples.
- [ ] Do not infer universal correctness from a 100% coverage or mutation score. Retain
  cross-package SQLite/HTTP/DI tests and explicitly unsupported external-service cases.

Verification: recorded seed replay, accepted mutation reports with all survivors disposed,
strict coverage for tested package modules and unfiltered integration regressions.

### P2-12: Public XML Documentation Enforcement

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P2-01/06/07/08/09/10. Comment-only source pass after runtime/AOT file release; no API renames or algorithm edits.

Direct dependencies: P2-01, P2-06, P2-07, P2-08, P2-09, P2-10.
Write scope: XML comments and package READMEs for the current library inventory;
documentation-policy configuration only by the shared-file handoff.
Required artifact: `docs/architecture/public-documentation-policy.md` (create before code).

- [ ] Inventory missing public API documentation per package. Use ordered package batches
  with explicit file release; preserve meaningful existing comments and compatibility notes.
- [ ] Document semantics, units, nulls, exception conditions, cancellation, ownership,
  concurrency and DI behavior. Avoid mechanically restating identifier names.
- [ ] Compile examples against actual packages; verify extension and generated API samples.
- [ ] Remove global `CS1591` suppression progressively with explicit temporary package-local
  debt entries; Track A acceptance requires removal of handwritten-public-API debt, not
  moving the blanket suppression into every project. Generated-only exceptions need rationale.
- [ ] Preserve schema/wire/API behavior; verify XML documentation is present in archives.
  Continue no-output documentation-project rules and automatic relative file inclusion.

Verification: warning-as-error package batches, API comparison showing no signature changes,
documentation example tests and archive XML/README validation; final source capture follows.

### P2-13: Track A Integrated Acceptance

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on all preceding Track A streams. Sole final build/coverage/report/package coordinator; excludes the explicitly blocked Track B split.

Direct dependencies: P2-00, P2-01, P2-02, P2-03, P2-04, P2-05, P2-06, P2-07,
P2-08, P2-09, P2-10, P2-11, P2-12.
Write scope: this plan, `docs/planning/optimization-acceptance.md` (new), approved CI glue;
no unreviewed package refactoring during acceptance.
Required artifacts: all accepted workstream contracts/results and Plan 1 external-gate ledger.

- [ ] Independently review cross-package API, security/cancellation, performance and
  AOT claims. Reopen owners for defects; do not waive a failed gate as an unrelated warning.
- [ ] Run infrastructure/compatibility/property/mutation/AOT tooling tests and smoke
  consumers before the authoritative source freeze. Finish benchmark writes first.
- [ ] Take a fresh Release solution capture; run full unfiltered tests and exact 100%
  line/branch coverage against it, generate the bound report, then validate all tested
  library package/symbol pairs without rebuilding or reusing stale archives.
- [ ] Verify current project/package inventory, API baselines, symbols/Source Link, XML
  documentation, README and exact staged archive manifest. New fixtures are non-packable.
- [ ] Wire deterministic smoke/API/coverage/AOT checks into CI; keep noise-sensitive
  performance baselines on controlled runners and budgeted mutation runs explicit.
- [ ] Report measured improvements, no-change decisions and unsupported AOT paths separately.
  Publish an external acceptance ledger for hosted Linux runs, Visual Studio behavior,
  feed permissions and publication. External actions require separate authorization.
- [ ] Mark Track A Complete only for accepted local evidence and clearly label any separate
  external release gates. Do not mark Track B Complete or call the entire program finished.

Verification: command protocol below plus P2-01/02/05/10 exact runner contracts; record
actual counts, hashes and failure evidence, not the historical Plan 1 totals as new results.

### P2-14: Approved Primitives Integration Split

Status: [ ] Ready [ ] In-flight [x] Blocked [ ] Complete
Owner: Plan owner awaiting user decision; Updated: 2026-09-10; Notes: Exact decision required after P2-04: approve a breaking major release, final adapter package names and migration/compatibility policy. Adding this plan does not authorize those breaks. Also requires P2-13 acceptance.

Direct dependencies: P2-04, P2-13 and explicit recorded user approval of the major-version decision.
Write scope: approved Primitives core/adapters, their tests/READMEs and migrated consumers;
shared solution/metadata/reference updates only by a bounded orchestrator handoff.
Required artifact: approved `primitives-package-migration.md` and immutable Track A API baseline.

- [ ] Create only approved integration packages. Keep pure Primitives free of EF/ASP.NET
  dependencies; place adapters behind inward references and preserve one canonical ID type.
- [ ] Migrate consumer references, converters, model binding and aggregator composition;
  do not reintroduce adapter dependencies into the pure core through forwarding cycles.
- [ ] Implement the agreed migration policy with real old/new consumer examples, explicit
  break inventory, release notes and package version changes approved as one release set.
- [ ] Update inventory/coverage/packaging/benchmark assumptions for the actual new package
  count. New libraries require mission READMEs, XML docs, symbols and strict 100% coverage.
- [ ] Repeat relevant property, API, serialization, EF/ASP.NET, performance and AOT tests
  against the new graph. Record intentional breaks rather than silently rebasing them away.

Verification: approved-major API diff, pure-core dependency graph, real provider/binding
consumer tests, strict new/core module coverage and migration sample compilation.

### P2-15: Track B Major-Release Acceptance

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Not dispatchable until P2-14 Complete; no publication or branch creation is implied.

Direct dependencies: P2-14.
Write scope: `docs/planning/optimization-major-acceptance.md` (new), this plan and approved CI inventory glue.
Required artifacts: approved migration, package/API baselines, updated AOT matrix and Track A acceptance.

- [ ] Repeat P2-13's fresh capture/full suite/strict coverage/report/tested-package acceptance
  using the new inventory. Do not reuse Track A reports to certify changed assemblies.
- [ ] Verify all intentional breaking differences match approval and every migration sample
  installs the intended versions. Check package dependency resolution and asset selection.
- [ ] Reconfirm performance budgets, trimming/AOT rows, documentation and external release
  checks affected by the split. Preserve unsupported scenarios honestly.
- [ ] Record final Track A/Track B status separately. Actual publishing remains an explicitly
  authorized release operation after local and required external acceptance.

Verification: complete new-inventory command protocol and approved-major consumer matrix;
record exact revision/source identities and staged archive hashes.

## Package Sub-plan Routing

Each row scopes the package-specific checklist within its owning workstream; it is not
a second claimable task. All current libraries receive P2-01 API checks, P2-03 AOT
classification, P2-11 risk-based test review, P2-12 documentation and P2-13 acceptance.
Rows without runtime changes need review evidence, not speculative rewrites. Workers
record per-package commands/results in their workstream artifact before handoff.

| Package | Specific follow-on work and owner |
| --- | --- |
| vc.Ifx.Abstractions | P2-11 result/null/error laws and lifecycle invariants; preserve published service contracts. |
| vc.Ifx.Primitives | P2-08 generic JSON metadata; P2-11 identity/equality/round trips; P2-04/14 adapter split. |
| vc.Ifx.Filtering | P2-06 prepared predicate execution; P2-08 portable value JSON; P2-11 Boolean/round-trip properties. |
| vc.Ifx.Querying | P2-08 AST serialization parity; P2-06 consumer verification; P2-11 order/paging/provider properties. |
| vc.Ifx.Filtering.EntityFrameworkCore | P2-06 provider-versus-memory boundary and SQLite translation regressions. |
| vc.Ifx.Storage.Abstractions | P2-11 capability/request invariants and ownership contracts; keep provider SDKs out. |
| vc.Ifx.Storage.Local | P2-02 allocation/streaming baseline; P2-11 path/concurrency/ownership properties using temporary files. |
| vc.Ifx.Storage.Ftp | P2-03 dependency/AOT classification; P2-11 cancellation/stream ownership; no live-latency promise. |
| vc.Ifx.Storage.Azure.Blobs | P2-03 dependency/AOT classification; P2-11 write-mode/metadata/stream ownership. |
| vc.Ifx.Secrets.Abstractions | P2-11 absence/cancellation/default contracts; preserve compatibility options. |
| vc.Ifx.Secrets.Local | P2-11 missing/empty/configuration semantics and deterministic lookup invariants. |
| vc.Ifx.Secrets.Azure.KeyVault | P2-02 cache/clock measurements; P2-11 version/cache isolation; SDK-owned resilience. |
| vc.Ifx.Data.Azure.Tables | P2-03 SDK/generic reflection inventory; P2-11 concurrency-token/error mapping. |
| vc.Ifx.Messaging.Azure.Queues | P2-08 metadata-driven message JSON; P2-11 encoding/cancellation parity. |
| vc.Ifx.Pipeline | P2-08 serializer metadata; P2-11 dispatch/retry/return-shape properties. |
| vc.Ifx.Pipeline.Grpc | P2-02 framing/serialization scenarios; P2-03 generated/SDK AOT support; P2-11 transport parity. |
| vc.Ifx.Proxy | P2-07 dispatch; P2-08 serializer paths; P2-09 opt-in generated proxy; P2-11 tenant/retry/cache safety. |
| vc.Ifx.Proxy.Http | P2-08 body/response metadata; P2-11 replay/header/stream ownership and cancellation. |
| vc.Ifx.Proxy.AspNetCore | P2-07/09 consumer verification; P2-11 scoped identity/tenant isolation; no credential relaxation. |
| vc.Ifx.WebApi | P2-02 catalog/exception benchmarks; P2-10 generated endpoint hosting; P2-11 catalog/error invariants. |
| vc.Ifx.Observability | P2-02 Activity/Meter allocation cases; P2-11 registration/idempotence and cancellation. |
| vc.Ifx.Roslyn | P2-01 host/API baseline; P2-09 diagnostic contract handoff; no runtime AOT retargeting. |
| vc.Ifx.Analyzers | P2-02 execution/cancellation benchmarks; P2-11 diagnostics properties/mutations; deterministic host behavior. |
| vc.Ifx.CodeFixes | P2-01 packaged provider/API baseline; P2-11 semantic/trivia/Fix All properties; preserve retired-provider policy. |
| vc.Ifx.Generators.Abstractions | P2-09 optional passive marker contract; P2-11 defaults and source-compatibility fixtures. |
| vc.Ifx.Generators | P2-09 generated dispatch; P2-02 incremental execution; P2-10 compiled endpoint/proxy consumer proof. |
| vc.Ifx.Roslyn.Reporting | P2-08 fixed-schema JSON metadata; P2-11 deterministic report/schema invariants; separate host limits. |
| vc.Ifx | P2-01 preview cleanup; P2-11 aggregate DI/compatibility; P2-14 explicit adapter dependency migration. |

P2-14 adds rows for approved new packages before implementation. P2-13/P2-15 reconcile
this table with actual source projects and fail on missing or duplicated package coverage.

## Verification Command Protocol

Run from the repository root. Commands below use existing runners; new compatibility,
AOT and mutation commands are deliverables of P2-01/P2-05, not commands that exist today.
Never replace a failing runner with an uncoordinated direct build/test invocation.

```powershell
# Targeted coverage example; omit TestPackage/SourceScope when their references are insufficient.
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 `
  -CoveragePackage vc.Ifx.Filtering -TestSourceScope Filtering `
  -TestPackage vc.Ifx.Filtering -Configuration Release -WarningsAsErrors

# Smoke only, then reserved-host measurements under P2-02's accepted protocol.
pwsh -NoProfile -File scripts/Invoke-FrameworkBenchmarks.ps1 -Job Dry
pwsh -NoProfile -File scripts/Invoke-FrameworkBenchmarks.ps1 -Job Default

# Inventory checks; new non-packable consumer fixtures require a coordinated validator update.
pwsh -NoProfile -File scripts/Test-FrameworkDependencies.ps1
pwsh -NoProfile -File scripts/Test-FrameworkDependencies.ps1 -SelfTest

# Orchestrator only, after all benchmark/mutation/AOT/build-input writes have stopped.
$build = Join-Path $PWD ("TestResults/reporting/optimization-" + [Guid]::NewGuid().ToString('N'))
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly `
  -Project vc.Ifx.slnx -Configuration Release -WarningsAsErrors -ReportBuildDirectory $build
$env:IFX_REPORT_BUILD = $build
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 `
  -FullCoverage -Configuration Release -NoBuild -WarningsAsErrors
```

Stop on any nonzero exit. Record the fresh coverage directory printed by that invocation,
then pass that exact directory and current revision to the existing
`scripts/reporting/Invoke-FrameworkReport.ps1` and
`scripts/packaging/Invoke-ValidatedPackageArtifacts.ps1` with `-BuildDirectory`,
`-CoverageRunDirectory`, and `-Revision`. Never select an arbitrary latest/historical run.
Verify the bound report, actual archive validator regressions and staged manifest again.
The build identity must include embedded resources/protobuf and any new generated inputs.

## Agent Handoff Template

```text
Read AGENTS.md, the applicable modernization contracts and this follow-on plan.
Assigned workstream: P2-NN (replace with one exact existing ID before dispatch).
Do not start until its direct dependencies are Complete and its file slice is released.
Claim only that section: In-flight, Owner, Updated, Notes, exact paths and commands.
List accepted required artifacts and direct package dependencies before implementation.
Edit only the assigned slice. Request shared-file/contract changes through the orchestrator.
Use the serialized runner; never reuse unbound build outputs or mutate another worker's files.
If blocked, record the exact decision/environment needed and stop that scope.
Return changed paths, API decisions, tests/coverage, measured results and artifact identities.
Leave In-flight for acceptance; only mark Complete when all applicable criteria are proven.
```

## Plan Review Loop

### Review Pass 1: Scope and Boundary Check

- [x] Map every suggested improvement to a workstream and preserve runtime/compiler boundaries.
- [x] Separate compatible work, measured no-ship decisions and explicitly approved major changes.
- [x] Confirm all current packages have scoped routing and named contract artifacts.

Review result (2026-09-10): all six recommendations plus property/mutation testing are
included. Configuration/API compatibility: P2-01; baseline measurements: P2-02;
Filtering/Proxy hot paths: P2-06/07/09; JSON/AOT: P2-03/08/10; documentation: P2-12;
dependency split: P2-04/14/15; test quality: P2-05/11. No speculative runtime retargeting
of compiler tooling, automatic major-version migration or blanket AOT guarantee.

### Review Pass 2: Agentic Dispatch Check

- [x] Verify unique IDs/statuses, existing dependencies, an acyclic graph and P2-00-only initial dispatch.
- [x] Verify shared-file ownership, README windows, serialized verification and mutation isolation.
- [x] Verify blocked Track B does not accidentally block Track A or become implicitly authorized.

Review result (2026-09-10): structural checks found 16 unique workstreams with one
active status/Owner each, an acyclic dependency graph and 28 exact package-routing
rows. Refined P2-12 to wait for AOT annotation edits, added reserved benchmark
source/host windows and kept shared README/csproj changes behind serial handoffs.

### Review Pass 3: Evidence and Completion Check

- [x] Verify exact-count coverage, API-baseline integrity and current-inventory package validation.
- [x] Distinguish performance smoke from measurements, annotations from AOT proof and coverage from correctness.
- [x] Verify no nonexistent tool commands are presented as executable and external gates remain explicit.

Review result (2026-09-10): checked existing runner parameter sets; new tool commands
are explicitly future deliverables. Added consumer-versus-test discovery regressions
to prevent new AOT smoke projects breaking the two-test-project coverage/pack gates.
Missing published baselines, noisy measurements, absent native toolchains and
external release acceptance remain explicit evidence gaps, not automatic passes.
These reviews validate the plan, not implementation or runtime correctness.

## References

- [Modernization plan](framework-upgrade-parallel-plan.md), [current acceptance audit](implementation-acceptance-audit.md),
  [package boundaries](../architecture/package-boundaries.md), [benchmark limits](../packaging/performance-audit.md).
- [Microsoft C# 14 feature reference](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
  for stable feature semantics; syntax modernization is not itself a measured speedup.
- [Microsoft package validation](https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/overview)
  and [baseline validation](https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/baseline-version-validator)
  inform P2-01; analyzer package layouts additionally require actual host consumers.
- [Microsoft Native AOT guidance](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
  informs P2-03/P2-10's analyzer, dynamic-code and publish/run requirements.
- [System.Text.Json source generation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation)
  informs P2-08's metadata/context/resolver options; existing wire contracts remain authoritative.
