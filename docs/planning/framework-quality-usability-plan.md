---
title: vc.Ifx Quality and Developer Usability Plan
doc_type: plan
status: active
last_updated: 2026-09-10
---

# vc.Ifx Quality and Developer Usability Plan

## Goal

Plan 3 makes the framework easier to choose, learn, configure, troubleshoot and
operate correctly over a long application lifetime. It follows
[Plan 1: Modernization](framework-upgrade-parallel-plan.md) and
[Plan 2: Optimization](framework-optimization-follow-on-plan.md).

Plan owner: Orchestrator. Authored: 2026-09-10.
Document state: Reviewed for gated agentic dispatch; only P3-00 is initially dispatchable.
Implementation state: Not started. No improved usability or runtime result is claimed.

Only P3-00 is initially dispatchable, for read-only prerequisite inspection and plan
handoff records. All implementation waits for the accepted Plan 2 Track A snapshot
and explicit release of affected files. Plan 2 Track B is optional for this plan:
select one accepted package graph, never mix pre-split and post-split contracts.

## Why a Third Plan

The reviewed root README starts with contributor clone/build/coverage instructions;
`docs/index.md` leads with general architecture guidance. Both are useful, but neither
is a task-first route for an application developer choosing a small framework package.
Package-level tests and API documentation do not alone prove that common user journeys,
provider substitution or repeated host lifecycles remain straightforward and reliable.

| Additional improvement | Workstream |
| --- | --- |
| Task-first package selection and discoverability | P3-01, P3-08 |
| Usable public APIs without unnecessary wrappers or overloads | P3-02 |
| Predictable DI registration and early configuration feedback | P3-03 |
| Shared provider contract/conformance tests | P3-04 |
| Actionable runtime and compiler diagnostics | P3-05 |
| Long-running lifecycle and bounded concurrency/soak verification | P3-06 |
| Executable application recipes and maintained documentation | P3-07, P3-08 |
| Integrated acceptance and honest developer walkthrough evidence | P3-09 |

## Scope and Boundaries

- Use Plan 2's accepted stable .NET/C# baseline and compatibility policy; the current
  recorded baseline is .NET 10/C# 14. Runtime and compiler-host target differences
  remain intentional. New language syntax is not an independent quality target.
- Apply VBD: keep application policy out of framework providers; do not add a new
  all-purpose facade, shared domain model, configuration system or test-framework package.
- Plan 2 owns API baselines, performance budgets, security/fuzzing, JSON/AOT, mutation
  infrastructure, dependency integrity, clean consumer compatibility and support policy.
  Reuse these artifacts and runners. Plan 3 tests task completion and sustained behavior;
  it does not duplicate or lower those acceptance gates.
- Preserve published type/assembly/namespace identities, stable error/catalog IDs,
  wire formats, DI semantics and supported host behavior. Ergonomic changes must be
  evidence-backed and compatible; an unresolved break is Blocked pending an explicit
  user/versioning decision, not silently accepted as part of this plan.
- Existing WebApi startup validation and registration replaceability are reference
  behavior to audit, not missing features to reimplement indiscriminately.
- No cloud resources, credentials, package publication, IDE installation, repository
  settings or remote mutations are authorized by this plan. External-service/IDE/human
  checks require their own available environment and explicit evidence.
- Planning edits do not claim or complete implementation work in any other plan.

## Agent Reading and Status Protocol

Read AGENTS.md, Scope and Boundaries, this protocol, dispatch rules and completion
standard first. Then read only the assigned section, relevant package rows, accepted
dependency handoffs and named artifacts. Do not preload every prior execution log.

Each `### P3-NN` section has one authoritative current Status and Owner line:

`Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete`

`Owner: Unassigned; Updated: yyyy-mm-dd; Notes: scope, prerequisites and verification.`

- Ready means queued. Claim only when all direct dependencies are Complete, required
  artifacts accepted and the exact file slice released. Numeric order is not scheduling.
- Claim exactly one workstream by setting In-flight, Owner, Updated and Notes before
  edits. Name exact paths, current contracts, expected commands and unresolved decisions.
- Never edit another worker's In-flight section or file slice without explicit handoff.
- Blocked Notes name the exact decision/environment/evidence needed and responsible
  owner. Only the plan owner releases a Blocked workstream after recording resolution.
- Complete requires actual acceptance evidence. Design completion does not certify
  runtime behavior; an agent walkthrough is not human usability research.
- Preserve dated prior Notes below the current header. A worker may hand off an
  implementation-ready section In-flight; the orchestrator accepts it or reopens it.

## Dispatch and Ownership

1. P3-00 accepts the Plan 2 baseline; P3-01 then defines and prioritizes user journeys.
2. P3-02 and P3-03 can run in parallel on released non-overlapping API/registration
   files. P3-01's artifact must assign these exact slices before either is dispatched.
3. P3-04 follows API and registration contracts. P3-05 follows the same changes;
   it may run alongside provider conformance on separate files.
4. P3-06 lifecycle verification and P3-07 executable recipes follow the API/provider/
   diagnostic changes and may run in parallel on separate test/sample slices.
5. P3-08 publishes navigation/snippets after recipes pass. P3-09 performs final
   integrated acceptance after P3-06 and P3-08 are complete.

- P3-01 owns the journey/package index, P3-02 call-site ergonomics, P3-03 registration
  and options, P3-04 provider conformance, P3-05 diagnostic remediation content and
  bounded diagnostics, P3-06 lifecycle tests, P3-07 sample source, P3-08 published docs.
- Existing sources are read broadly but edited narrowly: each design artifact lists
  exact approved implementation paths before changes start. The orchestrator routes
  shared-file requests as path/diff/reason/test handoffs; no simultaneous csproj,
  README, diagnostic catalog, root metadata or central test-project edits.
- Runtime defects discovered in P3-04/06/07 go back to the appropriate package owner
  with a regression. Reopen affected upstream acceptance and rerun dependents; no
  test worker independently repairs another package or masks a contract failure.
- P3-08 does not rewrite runtime contracts. Other workers draft documentation in their
  own artifacts until it owns a serial README/index publication window.
- Reuse existing mutex-aware build/test/benchmark and Plan 2 consumer runners. New
  lifecycle/sample tooling must use the same serialization and one-node builds.
  Editing may be parallel; shared build outputs and final captures may not be.
- Stress tests use isolated scratch resources, bounded workers/time/memory/disk and
  cooperative cleanup. Do not run them beside reserved performance measurements or
  against user files, live credentials, production endpoints or the global NuGet cache.
- Finish all source/example/generated-input changes before authoritative capture.
  Subsequent changes invalidate affected API/coverage/consumer/package evidence.

## Completion Standard

Retain Plans 1 and 2's applicable criteria, plus the following:

- [ ] Named design artifacts and exact write slices accepted before implementation.
- [ ] Changed libraries have strict 100% line/branch coverage, zero skipped required
  tests and warning-free Release builds, retaining generated-only exclusions.
- [ ] API/behavior, security, performance, AOT and telemetry contracts still pass;
  usability improvements do not justify hidden breaking changes or weakened gates.
- [ ] Each selected developer journey has actual package references, runnable code,
  expected outcomes, failure recovery and a tested cleanup path.
- [ ] Examples compile and execute independently of repository-wide implicit settings;
  no hidden aggregator, test-only dependency or placeholder success is permitted.
- [ ] Provider differences have explicit capability-based expectations. Unsupported
  behavior is a reviewed matrix entry, not a blanket skip or a fabricated guarantee.
- [ ] Lifecycle evidence separates deterministic defects from noisy process-memory
  observations; budgets and acceptance conditions are declared before measurements.
- [ ] Documentation links/snippets/reference versions pass verification and the docs
  project remains output-free with automatic relative file inclusion.
- [ ] Notes contain exact commands/results, package/source identities, artifact paths,
  coverage counts and external checks. Do not report inherited counts as new results.
- [ ] P3-09 accepts a fresh full build/test/coverage/report/package checkpoint and records
  outstanding external/human gates separately before describing delivery as complete.

Design-only work needs reviewed artifacts, not invented coverage. Tooling needs real
positive/negative regression tests; library coverage does not measure scripts. No new
packable project is implied by a sample or an internal conformance fixture.

## Workstreams

### P3-00: Accept the Quality Baseline

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Initial read-only handoff only; implementation awaits Plan 2 Track A acceptance and explicit file release.

Direct dependencies: accepted Plan 2 P2-13; if Track B is selected, also accepted P2-15.
Write scope: this plan and `docs/planning/usability-baseline.md` (new).
Required artifacts: Plan 2 acceptance, current package/API/security/support matrices and final archive manifest.

- [ ] Record revision plus dirty/untracked source identity, SDK/language, package graph,
  tested archive hashes and accepted build/coverage/report identities.
- [ ] Choose the accepted pre-split or post-split graph; record every current package,
  optional feature and unsupported environment without guessing from old plan totals.
- [ ] Carry forward external CI/IDE/release gates with owner and required evidence.
  Confirm no active worker/capture owns the planned files.
- [ ] Release P3-01 only after the baseline and planning handoff are accepted. Do not
  reopen completed modernization work just because historical Notes contain failures.

Verification: read accepted manifests/contracts and compare current identities; missing
or changed prerequisite evidence keeps the handoff In-flight or Blocked.

### P3-01: Developer Journeys and Package Selection

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P3-00. Design first; do not rewrite README/navigation before the publication lane.

Direct dependencies: P3-00.
Write scope: `docs/usability/developer-journeys.md` and `docs/usability/package-selection.md` (new).
Required artifacts: accepted baseline and existing package mission READMEs.

- [ ] Separate application consumers from framework contributors. Describe the smallest
  package set for an application task; keep the broad aggregator an explicit choice.
- [ ] Cover pure filtering, database QuerySpec/EF execution, minimal API error handling,
  provider-neutral storage, secrets, composed proxy/pipeline calls and compiler tooling.
- [ ] For each journey record prerequisites, target developer, first useful outcome,
  package/version/namespace choices, expected errors, cleanup and evidence needed.
- [ ] Record observed friction as reproducible call sites or missing navigation, not
  subjective demands for fewer lines. Identify intentional compatibility quirks separately.
- [ ] Define minimum success/failure journeys before implementation. Assign exact source
  slices to P3-02/P3-03/P3-05, existing tests to reuse and bounded new tests to add.
- [ ] Distinguish features included in the accepted release from optional/no-ship Plan 2
  experiments. Do not advertise generated proxies or AOT paths that were not delivered.

Verification: every current package has a selection/disposition row; approved journey
matrix maps to real APIs and disjoint implementation owners. This is design, not user research.

### P3-02: Public API Ergonomics

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P3-01. Own approved non-registration call-site slices; changes require demonstrated consumer friction.

Direct dependencies: P3-01.
Write scope: approved public factory/composition/extension slices and dedicated tests;
`docs/usability/api-ergonomics-decisions.md` (new). Exclude options/DI and compiler diagnostics.
Required artifacts: journey matrix, Plan 2 API baseline and compatibility policy.

- [ ] Compile realistic C# call sites for generic inference, overload resolution, nullable
  inputs, extension discovery, async return handling and explicit cancellation.
- [ ] Clarify existing APIs before adding new ones. Prefer standard BCL/ASP.NET idioms;
  do not add redundant wrappers, duplicate result types or a universal fluent builder.
- [ ] Where evidence warrants it, implement the smallest compatible improvement. Test
  old and new call sites together, including optional arguments and ambiguous imports.
- [ ] Preserve deferred execution/provider translation and immutable-versus-captured
  state semantics; convenience must not add hidden I/O, blocking or unbounded caching.
- [ ] Record each proposal as implemented, documented existing behavior, rejected with
  rationale, or blocked by a breaking-change decision. No blanket renaming of legacy APIs.

Verification: source/binary API comparisons, actual consumer compile/run tests, strict
affected-module coverage and existing performance/security/serialization regressions.

### P3-03: Predictable Configuration and Registration

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P3-01. Own approved options/DI slices; reuse existing startup-validation behavior where already correct.

Direct dependencies: P3-01.
Write scope: approved options validators/registration extensions and dedicated startup tests;
`docs/usability/configuration-and-registration.md` (new).
Required artifacts: journey matrix, existing options/lifetime contracts and Plan 2 support policy.

- [ ] Inventory registration idempotence, replaceability, named/keyed options, validation
  timing, service lifetimes, disposal and configuration reload behavior per package.
- [ ] Verify invalid local configuration is reported at the documented boundary, preferably
  host startup where appropriate, without requiring live cloud/network connectivity.
  Changing established lazy validation timing needs an explicit compatibility decision.
- [ ] Reuse Microsoft options validation and host/container checks. Do not construct an
  extra ServiceProvider inside registration or implement a second DI/options framework.
- [ ] Test user overrides before/after registration, repeated calls, independent named
  registrations, scoped dependencies, captured versus monitored options and disposal ownership.
- [ ] Separate local validation from remote readiness; bad credentials or a temporarily
  unavailable service must not be presented as an invalid static option by default.
- [ ] Return actionable option names and configuration locations without echoing secrets.
  P3-05 owns message/help consistency; retain Plan 2 redaction and telemetry contracts.

Verification: real host startup and scope-validation fixtures, named/keyed/reload tests,
negative lifetime/override cases, API compatibility and strict affected-module coverage.

### P3-04: Provider Conformance Suites

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P3-01/02/03. Own internal shared provider tests and capability expectations, not a new public testing package.

Direct dependencies: P3-01, P3-02, P3-03.
Write scope: internal provider-contract fixtures in existing test projects and
`docs/testing/provider-conformance-matrix.md` (new). Production fixes require an owner handoff.
Required artifacts: storage/secrets/provider contracts, capability inventory and registration decisions.

- [ ] Define shared behavioral cases for read/write/delete/list or secret lookup only
  where the interface promises them: missing values, cancellation, metadata, ownership,
  repeated calls and advertised capabilities.
- [ ] Run the same contract cases against each applicable provider. Keep transport-specific
  assertions separate; do not require FTP to offer atomic create or stronger path isolation
  than its documented capability, or equate local metadata with cloud version semantics.
- [ ] Use real temporary local storage and established SDK testing seams for deterministic
  provider tests. Label fake/emulator/live evidence distinctly; a fake does not prove service interoperability.
- [ ] Test substitutes used by consumers against the same contract fixture where feasible.
  Prefer small internal adapters over a separately shipped testing abstraction library.
- [ ] Record capability-based not-applicable cases with exact rationale. Required cases
  cannot be silently skipped; gaps route to the provider owner with a failing regression.
- [ ] Keep optional emulator/live suites isolated and separately authorized, with explicit
  resource cleanup/cost boundaries. Lack of credentials does not justify a fake live-service pass.

Verification: per-provider conformance reports, required-case discovery checks, strict
package coverage and reviewed capability exceptions. Record scope of actual interoperability proof.

### P3-05: Actionable Diagnostics and Troubleshooting

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P3-01/02/03. Own approved runtime/compiler diagnostic slices; do not broaden analyzer or code-fix policy merely for discoverability.

Direct dependencies: P3-01, P3-02, P3-03.
Write scope: approved diagnostic messages/properties/help links and dedicated tests;
`docs/usability/troubleshooting-catalog.md` (new). Shared catalogs require a serial handoff.
Required artifacts: existing diagnostic/code-fix matrix, WebApi response catalog and support policy.

- [ ] For common startup, filtering/query, provider, proxy and generator failures, document
  the observed symptom, stable identifier where one exists, likely cause, safe next action
  and minimal reproduction. Avoid assigning a new global error-code system to every exception.
- [ ] Improve parameter names/source locations/help content only within accepted compatibility
  rules. Preserve exception type/identity/inner cause and stable HTTP catalog/wire contracts.
- [ ] Verify compiler diagnostics point to the user's relevant syntax and help links resolve;
  document why unsupported code shapes have no safe fix. No duplicate CA/CS emitters or
  automatic suppressions that conceal behavior risks.
- [ ] Exercise CLI diagnostics and actual packaged IDE-host discovery separately. Light-bulb,
  Fix All and navigation UX require hands-on evidence on supported IDE hosts, not a CLI proxy.
- [ ] Keep troubleshooting data synthetic/redacted; connect errors to the accepted trace/log
  contract without exposing tokens, configuration secrets or high-cardinality metric labels.
- [ ] Pair examples of failures with executable recovery cases for P3-07. Preserve previous
  failure evidence when correcting a misleading message or diagnostic location.

Verification: diagnostic/error snapshot and behavior tests, link checks, package-host/CLI
regressions, strict changed-module coverage; explicit external ledger for unperformed IDE checks.

### P3-06: Sustained Lifecycle and Concurrency Quality

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P3-02/03/04/05. Test-only lifecycle lane with bounded isolated resources; no uncoordinated provider/proxy repairs.

Direct dependencies: P3-02, P3-03, P3-04, P3-05.
Write scope: dedicated lifecycle tests/tooling and `docs/testing/lifecycle-quality-results.md` (new).
Required artifacts: lifetime/ownership contracts, Plan 2 resource/performance budgets and conformance matrix.

- [ ] Exercise repeated host start/stop, scope creation/disposal, configuration reload,
  cancellation during teardown and mixed success/failure operations over many bounded cycles.
- [ ] Verify event subscriptions, timers, streams, registrations, rate-limit permits and
  cache entries do not accumulate beyond documented retention. Check named/tenant isolation
  across cycles; do not focus only on a single successful invocation.
- [ ] Use deterministic schedules/barriers or recorded seeds for race regressions. Keep
  prolonged soak checks separate from fast unit tests and record their resource/time budgets.
- [ ] Measure retained-object/resource trends after warmup with repeatable criteria. Do not
  infer a leak solely from process working-set growth or use forced GC as the only oracle.
  Test collectible type unload only where the accepted contract supports it.
- [ ] Verify late completions after cancellation cannot corrupt a new scope or release
  another operation's resource; preserve cooperative cancellation limits from Plan 2.
- [ ] Reproduce each meaningful failure in a focused regression and return it to the source
  owner. Reopen dependent acceptance after repairs; do not loosen budgets to dismiss a defect.

Verification: deterministic lifecycle tests, bounded soak artifacts, object/resource counts
and failing-schedule replay; strict affected-package coverage and existing security/retry tests.

### P3-07: Executable Application Recipes

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P3-01/02/03/04/05. Own sample source and assertions; published navigation belongs to P3-08.

Direct dependencies: P3-01, P3-02, P3-03, P3-04, P3-05.
Write scope: `samples/` (new) and dedicated sample verification; approved solution/discovery
updates by handoff. Keep all samples non-packable and out of library coverage denominators.
Required artifacts: approved journey matrix, troubleshooting catalog and Plan 2 clean-consumer runner contract.

- [ ] Create small runnable recipes for the accepted journeys: in-memory FilterSpec;
  QuerySpec with real SQLite/EF; attributed minimal endpoints with WebApi errors;
  local object storage/secrets; composed proxy/HTTP cancellation; compiler diagnostics/fixes.
- [ ] Reference exact accepted NuGet archives, not vc.Ifx source projects. Use Plan 2's
  isolated consumer runner; normal documented local package feeds are valid before publication.
  When recipes use new Plan 3 APIs, the orchestrator first creates a verified candidate
  archive snapshot from completed runtime lanes using existing build/test/package runners.
  Record its source/archive identities in the recipe evidence; this is not P3-09 final
  acceptance and does not depend on P3-09 completing. Final archives are checked again later.
- [ ] Every recipe states package choices, imports, configuration, ownership, expected
  result and failure/recovery paths. Use real supported APIs, no ellipses in executable code.
- [ ] Keep examples minimal without hiding required safety steps. Include cancellation,
  disposal, provider capabilities and safe synthetic configuration rather than production
  secrets or unreliable Internet endpoints. Avoid recommending the aggregator everywhere.
- [ ] Add observable assertions and bounded startup/shutdown, not only compilation. Use
  real in-memory/test-host or temporary-local behavior; label mocked transport examples.
- [ ] Include the accepted AOT/generated-proxy path only if delivered by Plan 2. Link to
  existing migration examples instead of inventing an incompatible parallel upgrade recipe.
- [ ] Register sample/host inventory explicitly without turning them into extra centralized
  test projects. Document exact verified launch/check commands and artifact identities.

Verification: fresh package restore, compile/run/assert/cleanup for every required journey;
missing/stale sample and zero-assertion runner regressions; no live-service requirement by default.

### P3-08: Task-First Documentation and Discoverability

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on P3-01/07. Own the serial README/index publication window; preserve general architecture content and contributor guidance.

Direct dependencies: P3-01, P3-07.
Write scope: root README, `docs/index.md`, package README links and `docs/usability/` guides;
snippet/link verification tooling through the shared-file handoff. No runtime API edits.
Required artifacts: accepted recipes, package-selection map, troubleshooting catalog and existing XML policy.

- [ ] Provide clear entry points for application consumers and contributors. Lead consumer
  navigation with tasks and minimal package selection; retain contributor build instructions
  and general architecture material in clearly linked sections rather than deleting them.
- [ ] Link package READMEs to executable recipes, API references, capability limitations,
  troubleshooting and approved upgrade guidance. Preserve existing useful anchors or redirects.
- [ ] Keep runnable snippet source canonical in tested sample files. Use deterministic
  extraction or verification to detect stale copied code; generated outputs stay outside
  docs build directories and do not violate the no-output documentation-project contract.
- [ ] Validate relative links, headings, snippet references and package-version consistency.
  Use plain navigable Markdown with descriptive links and readable tables; no new docs
  website/toolchain is required unless a separate evidence-backed decision approves it.
- [ ] Check supported/optional/unverified labels against release artifacts. Do not market
  AOT, throughput, provider semantics or human usability outcomes beyond recorded evidence.

Verification: recipe/snippet consistency, Markdown/link checks, docs project no-output
regressions and package README validation. Actual IDE tree behavior remains a distinct check.

### P3-09: Integrated Quality and Usability Acceptance

Status: [x] Ready [ ] In-flight [ ] Blocked [ ] Complete
Owner: Unassigned; Updated: 2026-09-10; Notes: Depends on all Plan 3 delivery streams. Sole final integration coordinator; no publication or unsupported human-usability claims.

Direct dependencies: P3-00, P3-01, P3-02, P3-03, P3-04, P3-05, P3-06, P3-07, P3-08.
Write scope: this plan, `docs/planning/usability-acceptance.md` (new) and approved verification/CI glue.
Required artifacts: all accepted Plan 3 results and inherited Plan 2 acceptance policies.

- [ ] Reconcile every journey/package row and accepted implementation/no-change decision.
  Run provider conformance, lifecycle, diagnostic, sample and documentation checks before
  the final source freeze. Reopen owners for actual defects or missing required scenarios.
- [ ] Capture a fresh warning-free Release solution build, run unfiltered tests and strict
  100% per-library line/branch coverage, generate the paired report and validate exact
  tested-library package/symbol archives without rebuilding between those operations.
- [ ] Recheck API baselines, security/resource/retry/telemetry contracts, AOT and dependency/
  license/SBOM policy for changed packages. Rerun recipes against final archives in isolation;
  README/source changes mean earlier archive-bound evidence cannot simply be reused.
- [ ] Conduct task walkthroughs from the consumer entry page: select packages, reach the
  first useful result, trigger a documented failure and recover. Record observed friction,
  evaluator and environment. Automated/agent walkthroughs must be labeled as such.
- [ ] Request representative human/IDE walkthroughs as a separate external acceptance gate
  when participants/hosts are available. Record completion/errors/help needed; do not invent
  participant counts, satisfaction or task-time improvements from self-review.
- [ ] Publish a concise local acceptance record with current inventories, exact commands,
  artifact identities, measured outcomes and outstanding human/IDE/hosted release checks.
  Local Complete is not a claim of publication or universal usability/correctness.

Verification: all accepted matrix cells, fresh build/full coverage/report/archives and
final-package recipe checks; explicitly distinguish local completion from external gates.

## Package Sub-plan Routing

The following rows are scoped routing, not extra claimable sections. Every current
library receives P3-01 package-selection review, P3-02 ergonomics disposition, P3-05
troubleshooting coverage, P3-08 documentation links and P3-09 integrated acceptance.
An applicability decision does not require a speculative source rewrite.

| Package | Specific quality/usability focus |
| --- | --- |
| vc.Ifx | P3-03 aggregate registration; P3-07 explicit aggregator versus small-package recipe. |
| vc.Ifx.Abstractions | P3-02 result/request call sites; P3-06 service lifecycle/disposal. |
| vc.Ifx.Primitives | P3-02 identifier construction/parsing/JSON ergonomics; P3-07 actual accepted adapter graph. |
| vc.Ifx.Filtering | P3-02 composition/provider distinctions; P3-07 pure predicate recipe. |
| vc.Ifx.Querying | P3-02 database query shaping and round trips; P3-07 SQLite query recipe. |
| vc.Ifx.Filtering.EntityFrameworkCore | P3-07 real relational execution, no hidden client-side evaluation. |
| vc.Ifx.Storage.Abstractions | P3-04 common capabilities/ownership conformance without duplicate public ports. |
| vc.Ifx.Storage.Local | P3-04 real filesystem conformance; P3-06 repeated resource cleanup. |
| vc.Ifx.Storage.Ftp | P3-04 capability-specific conformance; P3-06 client/stream lifecycle. |
| vc.Ifx.Storage.Azure.Blobs | P3-04 cloud metadata/write semantics; P3-06 SDK-client and stream ownership. |
| vc.Ifx.Secrets.Abstractions | P3-04 common lookup/absence/cancellation expectations. |
| vc.Ifx.Secrets.Local | P3-03 configuration feedback; P3-04 lookup conformance; P3-07 synthetic recipe. |
| vc.Ifx.Secrets.Azure.KeyVault | P3-03 local validation versus remote readiness; P3-04/06 cache/client lifecycle. |
| vc.Ifx.Data.Azure.Tables | P3-03 setup clarity; P3-04 advertised SDK adapter behavior; P3-05 optimistic-concurrency diagnosis. |
| vc.Ifx.Messaging.Azure.Queues | P3-03 configuration; P3-04 delivery/encoding contract; P3-06 cancellation cleanup. |
| vc.Ifx.Pipeline | P3-02 invocation call sites; P3-03 registrations; P3-06 repeated dispatch/teardown. |
| vc.Ifx.Pipeline.Grpc | P3-05 framing/transport troubleshooting; P3-06 channel lifecycle and ownership. |
| vc.Ifx.Proxy | P3-02 invocation ergonomics; P3-03 policies/DI; P3-06 scoped-state/caching lifecycle. |
| vc.Ifx.Proxy.Http | P3-05 failure recovery; P3-06 response/body/client ownership; P3-07 composed call recipe. |
| vc.Ifx.Proxy.AspNetCore | P3-03 scoped registration; P3-06 host/request isolation; P3-07 integration recipe. |
| vc.Ifx.WebApi | P3-03 validated startup; P3-05 actionable stable errors; P3-07 minimal API recipe. |
| vc.Ifx.Observability | P3-03 replaceable registration; P3-06 listener/subscription lifecycle. |
| vc.Ifx.Roslyn | P3-05 stable diagnostic metadata/help without a new runtime error taxonomy. |
| vc.Ifx.Analyzers | P3-05 useful locations/options/help; P3-07 packaged diagnostic recipe. |
| vc.Ifx.CodeFixes | P3-05 supported fix/no-fix explanations; P3-07 safe correction recipe. |
| vc.Ifx.Generators.Abstractions | P3-02 passive attribute usability; P3-07 supported annotation examples. |
| vc.Ifx.Generators | P3-05 generated-name/shape diagnostics; P3-07 supported compiled endpoint/proxy recipes. |
| vc.Ifx.Roslyn.Reporting | P3-05 report failure remediation; P3-07 report-reading/tool invocation guidance. |

P3-00 updates this table for an accepted post-split graph before releasing assignments.
Final verification compares rows with actual source projects; historical 28-package
counts are not hard-coded acceptance rules for a graph that legitimately changes.

## Verification and Evidence Protocol

- Use `scripts/Invoke-FrameworkTests.ps1` for scoped Release coverage and full captured
  build/coverage. Reuse Plan 2's proven source selection and consumer isolation contracts.
- New conformance/lifecycle/sample/link runners are deliverables, not available commands
  today. Their owners document exact invocations and fail-closed regressions before handoff.
- Ordinary source test references may remain inside the centralized projects; executable
  consumer recipes must use actual package archives, isolated from repository imports.
- Run infrastructure/stress/benchmark/sample generation before the final source freeze.
  Final-package recipe verification must not rebuild or change the accepted library outputs.
- For final acceptance use a fresh `-ReportBuildDirectory` with `-BuildOnly -Project
  vc.Ifx.slnx -Configuration Release -WarningsAsErrors`, set `IFX_REPORT_BUILD`, then
  `-FullCoverage -NoBuild -Configuration Release -WarningsAsErrors` through the same runner.
- Stop on nonzero exits. Pass that exact build and printed coverage run, with the current
  revision, to existing reporting and tested-package artifact runners. Recheck the staged
  manifest and final-package consumers. Never select an arbitrary latest historical report.
- Archive evidence paths/hashes and exact results in the workstream artifact and Notes;
  retain failed/inconclusive runs. New projects are explicitly classified in solution,
  package/test/coverage discovery; samples and fixtures must not become runtime dependencies.

## Agent Handoff Template

```text
Assigned workstream: P3-NN (replace with one exact ID before dispatch).
Read AGENTS.md and Plan 3 global rules, then the assigned section and accepted prerequisites.
Verify the baseline graph and exact file slice are released; otherwise record the blocker.
Claim exactly one section with In-flight, Owner, Updated, Notes, paths and commands.
Read/create and obtain acceptance of named design artifacts before changing public behavior.
Use existing serialized verification and consumer runners; request shared-file edits by handoff.
Preserve other workers' changes. Return defects to source owners rather than widening scope.
Record exact tests/coverage, API decisions, recipe/conformance/lifecycle outcomes and hashes.
Leave In-flight for orchestrator acceptance unless every applicable completion criterion is proven.
```

## Three-Pass Review

### Pass 1: Scope and Reuse

- [x] Confirm each additional quality/usability suggestion has an owner and concrete output.
- [x] Reuse Plan 2 compatibility/security/AOT/consumer/support work rather than duplicating it.
- [x] Preserve package boundaries, source identity and explicit compatibility exceptions.

Review result (2026-09-10): the suggestion map covers discovery, API ergonomics,
configuration, provider conformance, diagnostics, lifecycle quality and executable
recipes. Existing options validation is audited rather than assumed absent; Plan 2
remains authoritative for security, compatibility, AOT and supported release behavior.

### Pass 2: Agentic Consumption

- [x] Validate unique statuses/owners, dependency references, acyclic scheduling and package rows.
- [x] Check shared-file ownership, source release, stress isolation and explicit handoffs.
- [x] Verify no Plan 3 work is implicitly authorized against an active earlier-plan capture.

Review result (2026-09-10): structural validation passed for 10 unique workstream
status/owner blocks, an acyclic dependency graph, required artifacts/verification,
all 28 current package rows and local links. Provider conformance waits for both
API and registration changes. Earlier-plan implementation and major-version gates
are not bypassed by creating this document.

### Pass 3: Acceptance Evidence

- [x] Require actual provider/sample behavior, strict coverage and fresh final-package proof.
- [x] Separate fake/live, deterministic/soak, automated/human and local/external evidence.
- [x] Keep future commands labeled as deliverables and decisions/environment blockers explicit.

Review result (2026-09-10): clarified verified candidate archives for recipes using new
Plan 3 APIs, without a circular dependency on final acceptance. Required recipes rerun
against final archives. Fake SDK behavior, finite soak runs and agent walkthroughs
cannot be reported as live-service, leak-free or human-usability proof. No runtime
tests or implementation work were performed by these planning reviews.

## References

- [Plan 1](framework-upgrade-parallel-plan.md) and [Plan 2](framework-optimization-follow-on-plan.md)
  remain the baseline and source of inherited acceptance rules.
- [Microsoft library guidance](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/)
  informs usability, evolution and debuggability; this repository's approved target policy prevails.
- [Microsoft options pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
  informs P3-03's reuse of existing startup validation rather than a separate options framework.
- [Breaking-change guidance](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/breaking-changes)
  informs review of source, binary and behavioral compatibility for ergonomic improvements.
