---
title: Verified Package Correctness Review
doc_type: reference
status: active
last_updated: 2026-09-10
---
# Verified Package Correctness Review

Date: 2026-09-09. Reviewer: independent Gate 3 correctness reviewer.
Scope: current Filtering, Querying and Primitives implementations and their
existing regression evidence, not a redesign or a repeat of Nash's dependency
audit. Package sources, tests, references and shared infrastructure were not edited.
Gate 3 remains In-flight; Orchestrator owns fixes and final global verification.

## Findings

### F1 [P2]: Portable TimeOnly Membership Loses Seconds And Fractional Precision

Location: `src/vc.Ifx.Filtering/ExpressionToFilterNode.cs:214`, reached by captured
membership at lines 152-154 and collection Contains at line 150. Rehydration is in
`src/vc.Ifx.Filtering/Poco/PocoFilterExpressionBuilder.cs:151`.

`Format` handles DateTime and DateTimeOffset with round-trip formatting, but routes
TimeOnly through `IFormattable.ToString(null, InvariantCulture)`. TimeOnly's default
format contains only hours and minutes. The reader parses the shortened text as
that exact minute. Consequently a supported captured `List<TimeOnly>.Contains`
predicate changes its meaning during translation: it stops matching the original
precise value and starts matching an unlisted minute-aligned value instead.
This is silent data loss, not an unsupported expression rejected by the contract.

Minimal package regression for the assigned fix owner (source-derived, not executed
as a new package test in this read-only review):

```csharp
var exact = new TimeOnly(10, 30, 45, 123);
List<TimeOnly> allowed = [exact];
Expression<Func<TimeOnly, bool>> original = value => allowed.Contains(value);
FilterNode node = ExpressionToFilterNode.Translate(original);
var restored = FilterExpression.Create<TimeOnly>(node).Compile();

// Current source produces node.Value == "[\"10:30\"]".
// original.Compile()(exact) is true; restored(exact) is false.
// original.Compile()(new TimeOnly(10, 30)) is false;
// restored(new TimeOnly(10, 30)) is true.
```

Runtime evidence obtained without building or instrumenting framework assemblies:

```powershell
pwsh -NoProfile -Command "[TimeOnly]::new(10,30,45,123).ToString([Globalization.CultureInfo]::InvariantCulture)"
# 10:30
pwsh -NoProfile -Command "[Environment]::Version.ToString(); [TimeOnly]::Parse('10:30', [Globalization.CultureInfo]::InvariantCulture).ToString('O')"
# 10.0.11
# 10:30:00.0000000
```

The source path and the two BCL operations establish the precision loss. An
attempt to launch a longer reflection-based package probe was denied by the host
before execution; it supplies no package-test evidence and was not worked around.
No new package test or coverage run is claimed here.

Existing proof gap: `tests/unit/vc.Ifx.UnitTests/Filtering/FilterContractTests.cs:244`
only builds a TimeOnly condition from explicit text `10:30:00` against a
minute-aligned value. It never translates a captured TimeOnly containing seconds
or fractions. The general round-trip matrix at line 59 does not include TimeOnly.
Therefore the recorded 100% line/branch result does not cover this semantic case.

Fix assignment requested: preserve TimeOnly precision in the existing value
formatter, then prove original/portable/JSON-restored membership equivalence for
seconds and fractional ticks, including the false-positive minute-aligned value.
Include collection Contains and nullable membership if those existing accepted
paths share the formatter. No public contract expansion is needed.

## Reviewed Behavior

No P0/P1 defect was identified. No additional actionable contract violation was
identified in Querying or Primitives within this bounded review.

- Filtering: examined parameter scoping and substitution, unsupported-expression
  rejection, invariant captured values, explicit null guards, nullable/NaN
  negation, Boolean identities, immutable group snapshots, public-member lookup,
  collection operators and provider-preserving expression construction.
- Querying: examined immutable query composition, repeated ordering/window
  replacement, pagination prerequisites, deferred execution, projection placement,
  parameter substitution, strict structural JSON reading, canonical operator
  mapping, malformed-child rejection, and rehydration through Filtering.
- Primitives: examined constructor/parse/default boundaries, numeric ranges,
  owner-specific equality, invariant formatting, Money currency arithmetic,
  Month boundaries, scalar JSON delegation, raw EF conversion, and model-binding
  validation/error handling. The permissive EntityId raw/default representations
  are explicitly documented compatibility behavior, not a new validation defect.

## Existing Regression Evidence

Read the recorded `summary.json` and adjacent `vc.Ifx.UnitTests/tests.trx` files;
these are previous runs, not coverage rerun by this reviewer:

| Package | Tests Passed | Lines | Branches | Coverage Run |
| --- | ---: | ---: | ---: | --- |
| Filtering | 81/81 | 356/356 | 352/352 | `009b04b110a6461c94da71f5d237cbba` |
| Querying | 110/110 | 323/323 | 156/156 | `dc028f14b3b347e5b0476b6f9d04a1d3` |
| Primitives | 376/376 | 403/403 | 196/196 | `20868752736d4279bd5e812d8b3a35bf` |

Artifacts live under `TestResults/coverage/<package>/<run>/`. Each inspected TRX
records zero failures and zero not-executed cases. Inspected regression source
includes Filtering's full predicate matrix and malformed-node tests, Querying's
schema/operator/SQLite tests, and Primitives' parsing, JSON, EF metadata and binder
tests. The accepted Gate 1 integration run
`TestResults/tests/vc.Ifx.IntegrationTests/6c57f007ce8b45949b8dadafe7146a9f/tests.trx`
also contains five real SQLite cross-package cases, but none tests TimeOnly.

## Residual Risks And Deliberate Boundaries

- External filters still require application-owned field allowlists, authorization
  and size/depth limits. Public property getters can run during POCO evaluation;
  captured getters can run during translation. These APIs are not a sandbox.
- CLR string dereferences require explicit null guards. Database collation,
  translation and null behavior remain provider-specific. SQLite evidence is not
  SQL Server/PostgreSQL evidence, and no live provider was exercised here.
- Querying's legacy IgnoreCase helpers lower strings invariantly, while serialized
  IgnoreCase filters use OrdinalIgnoreCase. These are not interchangeable for all
  Unicode input: a BCL probe returned false for lower-invariant Contains of
  U+03A3 against U+03C2, but true for OrdinalIgnoreCase Contains. The current
  query contract explicitly preserves the helper's existing CLR behavior, so this
  is recorded as a compatibility limitation, not an unrequested behavior change.
- QuerySpec requires explicit ordering but cannot establish uniqueness; stable
  paging across ties/concurrent data changes remains the caller's responsibility.
- Primitives' default structs can bypass validation, and EntityId parse/JSON/EF
  deliberately accept zero/empty GUIDs. Domain validation remains necessary.
  Existing EF converter tests establish conversion/model metadata, not every
  provider's storage mapping. Real ASP.NET routing/binding combinations are not
  exhausted by direct model-binder tests.
- No full solution build, new package test, new coverage measurement or release
  verification was performed. Orchestrator must assign F1, verify the fix and
  retain ownership of combined tests/coverage/build/packaging and gate acceptance.

## WebApi Review Addendum

Date: 2026-09-10. Bounded read-only review of `vc.Ifx.WebApi` against
`docs/webapi/http-response-catalog.md` and its package README. Only this document
was changed. F1 above remains historical evidence; Orchestrator owns its accepted
fix and pending verification. This addendum does not reassess or close F1.

### F2 [P2]: Hosting Middleware Assigns 499 Before Cancellation Reaches The Handler

Location: `src/vc.Ifx.WebApi/DependencyInjection/WebApiServiceCollectionExtensions.cs:49`.
The supplied hosting extension directly uses ASP.NET's `UseExceptionHandler`.
The catalog and README promise cancellation propagation without synthetic 499.

ASP.NET Core 10's middleware handles an OperationCanceledException or IOException
with canceled RequestAborted before calling registered IExceptionHandler services.
For an unstarted response it assigns 499 and returns. Thus the guards in
`GlobalExceptionHandler.cs:31` cannot enforce the documented hosting contract.
This describes response state, not a claim that a disconnected client receives
a response body. Evidence: [ASP.NET Core 10 middleware, HandleException](https://github.com/dotnet/aspnetcore/blob/v10.0.0/src/Middleware/Diagnostics/src/ExceptionHandler/ExceptionHandlerMiddlewareImpl.cs#L108-L118).

Regression needed: build an in-memory ApplicationBuilder delegate using the
public registration/hosting extensions; have its terminal delegate cancel
RequestAborted and throw the original cancellation exception. Observe completion,
status and handler invocation. Test the public hosting path, not a direct call to
TryHandleAsync. No listening server is required. This review traced the source;
it did not execute this new regression.

Fix assignment: reconcile the advertised hosting cancellation contract with the
middleware actually selected, or explicitly narrow the documented guarantee to
the mapper/handler boundary and acknowledge the inherited hosting behavior.
Do not claim end-to-end cancellation propagation from direct-handler tests.

### F3 [P2]: Pre-Exception Required Headers Are Lost And Mapped Errors Fall Back To 500

Locations: `WebApiServiceCollectionExtensions.cs:49` and
`src/vc.Ifx.WebApi/ExceptionHandling/GlobalExceptionHandler.cs:44`.
The README/catalog promise preservation of application-supplied required headers.

ASP.NET's exception middleware clears the response before invoking registered
handlers and sets its default status to 500. Clearing removes response headers.
Therefore a previously supplied WWW-Authenticate, Proxy-Authenticate, Allow or
Upgrade is missing when this handler checks it. The handler returns false before
setting the mapped status; with the default registration, middleware Problem
Details fallback retains 500. Sources: [middleware dispatch](https://github.com/dotnet/aspnetcore/blob/v10.0.0/src/Middleware/Diagnostics/src/ExceptionHandler/ExceptionHandlerMiddlewareImpl.cs#L149-L195)
and [response Clear implementation](https://github.com/dotnet/aspnetcore/blob/v10.0.0/src/Http/Http.Extensions/src/ResponseExtensions.cs#L18-L30).

Concrete scenario: map an application exception to 401, set
`Response.Headers.WWWAuthenticate = "Bearer"` in the terminal delegate, then throw
before the response starts. Invoke the delegate built with the public hosting
extension. The documented preserved challenge/401 path is not reached. Repeat
with 405/Allow, 407/Proxy-Authenticate and 426/Upgrade. This assumes headers were
assigned before the exception, not re-added by a custom mapper or another handler
after the middleware reset. The source-derived regression was not run here.

Fix assignment: establish and test where trusted required headers are supplied
in the actual hosting flow, or correct the unconditional preservation claim.
Do not restore arbitrary stale content/framing headers or invent authentication
schemes to satisfy the guard.

### WebApi Evidence And Proof Gaps

Inspected all WebApi implementation files, its response catalog and README, plus
catalog, mapper/options, handler, DI and resilience regression source. Read
`TestResults/coverage/vc.Ifx.WebApi/6cd18b8e77bc466983bc22bc05992885/summary.json`
and adjacent `vc.Ifx.UnitTests/tests.trx`: 98/98 passed, zero failures/not-executed,
327/327 lines and 116/116 branches. These are existing measurements, not a new run.

`DependencyInjectionTests.DefaultRegistrationsResolveAndProduceProblemJson`
resolves IExceptionHandler and calls TryHandleAsync directly. Its ApplicationBuilder
assertion only registers the extension; it does not build/invoke the resulting
middleware delegate. `GlobalExceptionHandlerTests.RequiredHeadersMustExistAndArePreserved`
and `NullInputsAndCancellationNeverReachTheWriter` also call the handler directly.
Neither proves the two public hosting behaviors above. Framework dependency code
is outside this package's measured line/branch coverage.

No additional actionable privacy, started-response, catalog-status or resilience
violation was identified in this bounded review. Safe default details omit the
exception message/type and query string; development disclosure is explicit.
The handler checks started responses before mapping/writing and restores
RequestAborted in finally around its linked-token writer call. Catalog body rules,
active-error mapping validation and nearest-base mapping match the documented
contract. Retry remains explicit replay-safe opt-in, outside the per-attempt
cooperative timeout, with cancellation excluded even for a custom classifier.

Residual risks: custom mappers/writers and application titles/type URIs remain
trusted extension points; request paths may themselves contain application secrets;
timeout requires operation token cooperation; callers own total deadlines and
Retry-After-aware policy. Host middleware ordering, real transport disconnects and
custom writer interactions are not exhaustively established by direct-handler
tests. The dependency behavior above was checked against official ASP.NET Core
10.0.0 source, not a newly executed test on every servicing runtime.

No package/test/shared-file edits, live host, build or coverage rerun were performed.
F2/F3 are handed to Orchestrator for assignment and contract resolution; global
verification and Gate 3 acceptance remain with Orchestrator.

## F1 Resolution Addendum

Date: 2026-09-10. F1 is resolved at the Filtering package boundary. Orchestrator
implemented the fix; this reviewer inspected the changed formatter, regression
source and recorded results without editing package code or rerunning tests.
The original finding and earlier review checkpoints above remain historical.

`src/vc.Ifx.Filtering/ExpressionToFilterNode.cs:214` now formats TimeOnly with
`time.ToString("O", CultureInfo.InvariantCulture)`, retaining seconds and ticks.
`tests/unit/vc.Ifx.UnitTests/Filtering/TimeOnlyRoundTripTests.cs` compares original,
portable and JSON-restored predicates for fractional ticks, the minute-aligned
false-positive case, nullable captured membership including null, and collection
Contains for both TimeOnly and nullable TimeOnly elements.

Verified recorded evidence:

- Red: `TestResults/tests/vc.Ifx.UnitTests/2600a53caad24de7a1e718c9d0f5dd14/tests.trx`,
  one regression executed and failed.
- Green: `TestResults/tests/vc.Ifx.UnitTests/b7ffc55d0a4f4c73875231c60f4894b2/tests.trx`,
  the regression passed, zero failures/skips.
- Strict Filtering: `TestResults/coverage/vc.Ifx.Filtering/59d0f9849c0b445993ae51def829e1eb/summary.json`
  and adjacent `vc.Ifx.UnitTests/tests.trx`: 86/86 passed, zero failures/not-executed;
  357/357 lines and 354/354 branches, both 100%. This selection includes four
  EF adapter tests; it is not separate full EF-package coverage proof.

Querying and EF strict consumer reruns remain in progress as reported by
Orchestrator at this checkpoint. Their completion, combined global verification
and Gate 3 acceptance are not inferred from the Filtering result. F2/F3 remain
separate WebApi findings awaiting Orchestrator's resolution.

### F1 Consumer Verification And WebApi Assignment Update

Date: 2026-09-10, subsequent checkpoint. The reviewer inspected both consumer
coverage summaries and their adjacent `vc.Ifx.UnitTests/tests.trx` files:

- Querying: `TestResults/coverage/vc.Ifx.Querying/5db3e65db1004c6aa9daaaf79c08bce7/summary.json`;
  110/110 tests passed, 323/323 lines and 156/156 branches, strict 100%.
- EF adapter: `TestResults/coverage/vc.Ifx.Filtering.EntityFrameworkCore/8b6981c89bc6411ab27c23943e64ab5d/summary.json`;
  4/4 tests passed, 14/14 lines and 4/4 branches, strict 100%.

Both TRX files record zero failures and zero not-executed cases. These results
close the pending F1 consumer-rerun checkpoint above; the initial finding and
earlier evidence remain unchanged. Global verification and Gate 3 acceptance
remain Orchestrator-owned. The reviewer did not rerun tests or edit source.

Orchestrator assigned F2/F3 to Kuhn after completion of Proxy. The authorized
resolution retains the built-in ASP.NET exception middleware, narrows documentation
to accurate boundary-specific guarantees, and adds real hosting regressions,
including a trusted custom mapper supplying required headers after the middleware
reset. This records the assignment and agreed approach, not completed F2/F3
verification; those findings remain pending Kuhn's evidence and acceptance.

## F2/F3 Independent Resolution Review

Date: 2026-09-10, subsequent checkpoint. F2 and F3 are resolved within this bounded
review through the Orchestrator-authorized contract clarification and executable
hosting regressions, not by changing inherited ASP.NET behavior. Their original
findings and evidence above remain historical. Global acceptance stays with
Orchestrator.

The reviewer read the revised README, response catalog, DI extension/direct handler,
and `tests/unit/vc.Ifx.UnitTests/WebApi/HostingPipelineContractTests.cs`. The hosting
extension still calls built-in UseExceptionHandler; the handler's executable
cancellation/header checks remain unchanged. New source remarks accurately scope
their guarantees to the direct-handler boundary.

- F2: documentation now distinguishes aborted OperationCanceledException/IOException
  from an uncanceled OperationCanceledException. The former yields server-side 499
  before mapping, without Problem Details, and does not imply delivery to a
  disconnected client. The latter propagates unchanged through the default
  pipeline. Two aborted cases assert 499, no mapper call/body/content type and
  retained RequestAborted; the uncanceled case asserts original exception identity.
  Catalog exclusion of 499 remains explicit and tested.
- F3: documentation now says pre-exception headers/status are reset and missing
  required headers cause default 500 fallback. Four hosting cases verify this for
  401/405/407/426, including removal of stale Content-Length and unrelated headers.
  Four further cases use a trusted mapper registered before AddIfxWebApi to supply
  configured headers after reset. They assert matching status and safe Problem
  Details, preserved configured headers, no restored stale header, and Connection:
  Upgrade for the 426 case. The docs prefer normal authentication/routing responses
  and do not invent a scheme or authorize arbitrary header restoration.

These 11 cases build a Production WebApplication service provider and actually
build/invoke an ApplicationBuilder delegate containing the public hosting extension.
They use the real exception middleware, registration path, default Problem Details
service and default exception mapper behind the observing/custom wrapper. They do
not merely call IExceptionHandler directly, and they do not start a listener.

Evidence independently inspected:
`TestResults/coverage/vc.Ifx.WebApi/aa310c6abd184df4b7fed25be48871a1/summary.json`,
`run-context.json`, and `vc.Ifx.UnitTests/tests.trx`. The context records Release,
source scope WebApi, test package vc.Ifx.WebApi and filter
`FullyQualifiedName~Tests.WebApi.`. All 109 tests passed with zero failures or
not-executed cases; all 11 named hosting cases are present and passed. Strict
coverage is 280/280 lines and 116/116 branches, both 100%. This is a current
package-only Release measurement, not a replacement claim for historical coverage
denominators or a full-suite run.

No remaining actionable F2/F3 mismatch was found in the agreed resolution. Residual
limits remain: no live socket/disconnect delivery proof, no exhaustive middleware
ordering/custom writer combinations, no authentication-scheme validation by the
framework, and no assertion across every ASP.NET servicing runtime. The inherited
499/reset behavior is deliberate and documented, not removed.

Only this review document was appended by the reviewer. No package/test/shared-file
edits or independent build/test rerun were performed; the verified results are
Kuhn's recorded run. Bounded review is finished and returned to Orchestrator.
