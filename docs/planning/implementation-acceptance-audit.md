# Implementation Acceptance Audit

Owner: Orchestrator; Updated: 2026-09-10; Status: local integration accepted; external hosted/IDE verification remains open.

## Final Acceptance

The [final local checkpoint](local-verification-20260910.md) supersedes the
historical pending-global handoffs below. Fresh build `global-20260910-03` passed
zero warnings/errors; paired coverage `d27e86fffbba406da1c533510c9948fb` passed
3,617 unit plus 15 integration tests, no skips, all 28 exact 100% line/branch gates;
the bound report passed with zero issues. Final archive run
`59468a485ff34d26884375e111912ac6` validated all 28 package/symbol pairs against
the tested payloads; 19 archive-validator checks and staged manifest recheck passed.
The current inventory is 36 projects/28 libraries. All library sections and local
Gates 1, 2, 3 and 5 are Complete; Gate 4 retains external acceptance.

A1's actual Visual Studio documentation behavior, compiler IDE interaction,
hosted Linux execution/artifact transfer and repository/feed configuration remain
unverified. A2/A3/A4 are resolved as recorded below. No remote publication or
settings change is claimed. Nash's bounded review and its historical measurements
are preserved below; the original reviewer did not execute these final global gates.

## Historical Review Scope

Scope: unchecked implementation tasks in the parallel plan, all 28 library
sections, and related gates/design artifacts. The initial read-only audit edited
only this document. The subsequent explicit Gate 3 handoff authorized the catalog,
support matrix, endpoint contract, Grpc README, package-boundary annotations,
dependency validator and narrow inactive implementation checklist reconciliation.
Aggregator and other package source remain frozen. Active Gate 1/2/4 notes, shared
metadata and solution entries were not changed by Gate 3. Only the no-build
dependency validator and its SelfTest were executed; no build, framework test,
pack, IDE session or global gate was run for this reconciliation.

## Decision Summary

There is no library without recorded local 100% coverage evidence. That does not
prove every proposed feature exists or that every report matches the final combined
source snapshot. The initial discrepancies have the following current dispositions:

1. **Documentation project integration changed during this audit.** The missing
   Project entry and 14 CLI checks are complete. The validator now includes all
   36 projects/28 libraries. Actual IDE behavior remains unverified without an IDE.
2. **Generator serialization scope accepted:** Orchestrator explicitly accepts
   the frozen bounded v1 policy, not universal static DTO/converter proof.
3. **Diagnostic catalog and support matrix reconciled:** active/retired providers,
   Fix All bounds, legacy GEN002 and IFX2006 locations now agree with source.
   CodeFixes' separate analyzer installation requirement is also documented.

Most remaining unchecked boxes are implemented behavior, conditional requirements,
or compatibility decisions already accepted. Do not reopen frozen APIs, restore
unsafe code fixes, create redundant SDK wrappers, or rename public types to clear
those boxes. Final all-source coverage, warning-free solution/packaging, hosted CI,
feed permissions and publication remain separate Orchestrator gates.

## Owner Changes Required

### A1: Documentation Project Integration

Disposition: implementation and CLI acceptance resolved. Actual IDE verification
remains with infrastructure/Orchestrator; no development Visual Studio is installed.

- Initial inspection found `docs/docs.csproj` as a File, zero documentation
  Project entries and 31 manually listed documentation files. **Final recheck
  supersedes that finding:** `vc.Ifx.slnx:58` now contains a real Project entry,
  and the explicit docs File entries are gone.
- `docs/docs.csproj` now imports Microsoft.NET.Sdk props/targets, sets
  `RestoreProjectStyle=Unknown`, disables default items/implicit references,
  retains recursive None/Link items, and overrides build/design-time output
  targets. This is the implemented correction, not a missing project definition.
  Final disk inspection still found no docs obj/bin/.no-output directories.
  Gate 3 did not rerun its build or exercise Visual Studio.
- Existing evidence in `docs/packaging/documentation-project-integration.md:3`
  now records 14 passing CLI checks, artifact
  `TestResults/docs-project/97a0183382a744cb9ffcb6d700d3678c`.
  `TestResults/docs-solution-probe/build.log` records the **old** isolated
  plain-MSBuild probe's NU1503/missing-restore warning, not a proven failure of the
  new SDK correction. Its items.json proves old standalone items, not IDE behavior.

Resolved CLI evidence covers warning-free standalone targets and mixed SDK
solution restore/build, a newly added nested document with relative item paths,
zero Compile/package/project references, and no docs outputs after tested targets.
The live Gate 3 validator now proves exact 36-project inventory with docs as one
non-packable nonlibrary, retaining reference/cycle safeguards; 25 SelfTest probes
pass. CLI CompileDesignTime is not the whole Visual Studio design-time pipeline.
Remaining owner action is actual IDE load/tree refresh/no-output verification on
a target installation, plus final global acceptance. Do not recreate the project
or treat its predecessor's probe as a current A1 implementation failure.

### A2: Generator Serialization Scope Accepted

Disposition: Orchestrator explicitly accepted the bounded v1 policy on 2026-09-10.
The endpoint contract and inactive generator checklist now record this decision;
no source reopening, new diagnostic or broader API is required.

The original unchecked item broadly included non-serializable public contracts.
The accepted implementation is deliberately narrower:

- `Generators/Endpoints/EndpointValidation.cs:79` checks only delegate payloads
  on returns and non-service parameters. `IsDelegatePayload` at line 203 recurses
  through arrays, but not generic collection elements or DTO members.
- `InvalidType` at lines 198-200 rejects pointer/ref-like/error/open shapes through
  arrays and generic arguments as unsupported signatures (IFX2001), not IFX2006.
- `MinimalEndpointGenerator.CreateOutput` emits declarations with no diagnostics;
  it performs route/name/host checks, not another serialization validation pass.
- Existing `MinimalEndpointGeneratorTests.cs:116`, `:120`, `:128`, and `:282`
  cover direct delegates, delegate arrays, parameter location and service-bound
  delegates. Endpoint hosting tests cover an ordinary DTO, not a general invalid
  public-contract inventory.
- The frozen v1 contract deliberately delegates arbitrary DTO serialization,
  custom converters and JSON options to consumer tests. Therefore the observed
  implementation matches that **bounded design**, but does not discharge the
  original broader checklist literally. That wording is now bounded explicitly.

Examples outside the accepted static policy: a direct response
`List<Action>` or a DTO with a public delegate property is not inspected by the
current payload predicate. This is a source-path inference, not an executed
runtime failure assertion; converters and binding configuration can affect runtime
behavior. Do not label every such type unconditionally non-serializable.

Acceptance: IFX2001 covers declared unsupported signature shapes; IFX2006 covers
direct delegates and delegate arrays, excluding service-bound parameters.
Ordinary supported arrays are not universally rejected. Arbitrary DTO members,
generic payload collections and custom-converter correctness remain consumer
tests, not an unimplemented universal static-proof obligation. No new diagnostic
ID or frozen marker change is authorized. Existing invalid attributes, method tokens, accessibility, route/name
duplicates and missing hosting checks are implemented, not missing features.

### A3: CodeFixes-To-Catalog Handoff Resolved

Disposition: reconciled by Gate 3 under explicit Orchestrator handoff. The bounded
selection is accepted; no source rewrite or retired-provider restoration.

`docs/roslyn/code-fix-support-matrix.md` matches the current three exported provider
types: debt (IFX1000/1001), IFX005 and CA1062. Source confirms reviewed null-guard
insertion at `Providers/CodeAnalysis/Ca1062CodeFixProvider.cs:26`, no Fix All for
CA1062/IFX005, and custom debt Fix All. `Common/RetiredCodeFixProvider.cs:11`
advertises no IDs/actions/Fix All; the 15 retired subclasses remain constructible
but unexported. The strict CodeFixes report is 194/194 lines, 176/176 branches,
96 passing tests. Retirement is implemented, not untested missing remediation.

Reconciliation applied in `docs/roslyn/diagnostic-catalog.md`:

- Replaced CA1062's former suppression description with the reviewed, bounded
  single reference-parameter null guard, earlier null-failure behavior and no Fix All.
- Marked CA1051, CA1303, CA1413, CA1704, CA1707, CA1801, CA1806, CA1812, CA1819,
  CA1822, CA1823, CA1824, CA1825, CA2207 and MSTEST0017 as retired/unexported.
  Removed claims that they currently convert fields, discard values, suppress
  files, remove declarations or rewrite arrays/enums. CA2207 is withdrawn, not
  renamed to CA1008 and not awaiting repair of its former enum transformation.
- Advertised debt Fix All at document/project/solution scope, ordinary-comment
  scaffolding bounds, attribute/pragma justification handling and unresolved TODOs.
  Described IFX005's reviewed metadata-only bounds and lack of Fix All precisely.
- Replaced stale consumer-handoff/future CodeFixes-review prose with the accepted
  matrix and evidence link. Closed the matrix's pending catalog-handoff note;
  retained independent package-host limitations and separate analyzer composition.
- Reconciled two generator-owned catalog details: IFX2006 can point to a parameter
  as well as a return type, and GEN002 exists in the generator README/release file
  and is now present in the legacy generator inventory. Recorded its legacy-prefix
  exception explicitly rather than silently renumbering a shipped diagnostic.

The broad CodeFixes starter list is not an instruction to implement every listed
CA/CS area. The matrix explicitly defers upstream-provided using/style/disposal/
cancellation fixes and meaningful XML documentation, and implements selected
CA1062 and debt scaffolds. Accept that documented bounded selection; do not restore
retired duplicate or semantics-changing fixes to satisfy the list.

### A4: Stale Local Handoff Text Reconciled

Disposition: the historical findings below are resolved in the authorized Grpc
README, generator contract and package-boundary annotations. Their stale pending
language is not a new owner assignment. Only permitted inactive checklists changed.

- Pipeline.Grpc README formerly requested removal of package-wide CS8981 suppression.
  The current project has no NoWarn entry; generated protobuf code under obj owns
  its pragma, and shared exclusions are GeneratedCodeAttribute/obj only. The
  README and the generated-boundary item are now reconciled to recorded evidence;
  no additional adapter source work is identified.
- The generator contract formerly requested centralizing Routing 2.3.12/removing a
  temporary VersionOverride. `Directory.Packages.props:41` supplies the version;
  the generator project uses a private PackageReference without VersionOverride.
  This implementation request is stale; final packaged loading remains separate.
- `docs/architecture/package-boundaries.md` preserves historical requests for
  the authorization bridge, portable composition, Azure retention and namespace
  documentation. Current source/READMEs implement these (details below); explicit
  resolution/evidence annotations now distinguish them from current assignments.
- The prior TimeOnly review finding is repaired: Filtering's formatter now uses
  round-trip `O` at `ExpressionToFilterNode.cs:214`, with
  `Filtering/TimeOnlyRoundTripTests.cs` and a 357/357-line, 354/354-branch report.
  Do not delegate the same implementation fix again from historical Gate 3 prose.

## Originally Unchecked Implementation Items

This table records the initial audit and the current bounded disposition. Gate 3,
Proxy, Grpc, Generators and CodeFixes items below were narrowly reconciled under
explicit handoff. Gate 1/2/4/5 checkboxes and other active owner notes were untouched.

| Plan section/item | Actual state | Current disposition |
| --- | --- | --- |
| Gate 1: CI 100% enforcement | `.infra/yaml/framework-quality/action.yml` invokes unfiltered `-FullCoverage -Configuration Release -NoBuild -WarningsAsErrors` before pack; publish workflow consumes the composite. | Stale implementation checkbox; hosted execution is a release gate. |
| Gate 1: deterministic shared helpers only when needed | Package suites use actual clock, temporary filesystem, fake HTTP/SDK and logger fixtures; infrastructure has 48 recorded selection/coverage checks. | Conditional policy, not an obligation to invent one universal helper package. |
| Gate 3: remove duplicate contracts | Legacy authorization now bridges to the canonical port; other similarly named ports have distinct contracts or approved compatibility identities. | Accepted exceptions recorded; checklist reconciled, public APIs preserved. |
| Gate 3: providers behind interfaces/options records | Object storage and secret ports exist; SDK-backed providers retain their explicit options classes and SDK virtual testing seams. | Accepted option-class exception recorded; checklist reconciled, no blanket conversion. |
| Gate 3: cross-boundary facts/records/results | New storage requests/metadata are provider-neutral; legacy filesystem, Type/object factory descriptors and Azure Queue/Table SDK contracts are documented exceptions. | Accepted boundary exceptions recorded; checklist reconciled, no shared domain-object library. |
| Gate 5: analyzers deterministic/no report files | Source scan found no file/network/clock/environment reporting operations in Analyzers/Roslyn; analyzer tests cover cancellation/concurrent reuse; persistence belongs to Reporting/script host. | Stale checkbox; use analyzer and reporting evidence. |
| Pipeline.Grpc: generated warnings only at generated boundaries | Generated files are under obj with own pragmas; redundant project CS8981 NoWarn is gone. | A4 resolved and checklist reconciled; no missing implementation. |
| Proxy: lock contracts | Current README and owner handoff freeze pipeline/context/transport/response/options/interceptor signatures. | Checklist reconciled to accepted frozen contracts. |
| Proxy: ordering/cache/correlation/audit/timing/auth/resilience | Owner's 463-test strict report passes entire module; source includes canonical legacy authorization adapter and corresponding real DI pipeline tests. | Checklist reconciled; global cross-package rerun remains separate. |
| Proxy: remove duplicate/stale interceptors | Pre-existing Proxies archive remains excluded/documented; active compatibility classes remain intentional. | Accepted retention/exclusion recorded; checklist reconciled without blanket deletion. |
| Proxy: full interceptor tests | 2294/2294 lines, 692/692 branches, 463 tests recorded. | Checklist reconciled to local evidence, not a claim of every future policy feature. |
| CodeFixes: pair fixes with diagnostic source | Matrix, catalog and source now pair all active providers. | A3 resolved; checklist reconciled. |
| CodeFixes: selected upstream gaps | Bounded CA1062 action exists and is tested against the actual SDK analyzer; no unsupported CS fix is promised. | Bounded selection explicitly accepted; checklist reconciled, no duplicate emitter/fixer. |
| CodeFixes: high-value starter list | Debt/justification and reviewed guards implemented; unsafe/duplicate areas explicitly retired/deferred. | Bounded selection explicitly accepted and checklist reconciled; no unsafe restoration. |
| Generators: diagnostic set including non-serializable contracts | Seven other bounded validation categories implemented; payload check covers delegates/arrays only. | A2 bounded policy accepted, A3 locations corrected; checklist reconciled. |

Status protocol templates, generic completion-standard boxes and agent operating
rules are not individual unimplemented library features. All other package-specific
implementation checklists are checked in the inspected plan. Gate 2/4 packaging,
compiler-host probes and hosted CI work is already assigned elsewhere; this audit
does not duplicate those workers or claim their global acceptance.

## Compatibility Requests Already Implemented

- `LegacyAuthorizationPolicyAdapter` implements `IProxyAuthorizationPolicy`, denies
  empty/null/failed legacy results and preserves cancellation. Legacy registration
  helpers install one scoped bridge; `Proxy/AuthorizationBridgeContractTests.cs`
  covers actual DI enforcement. No rename/removal of IAuthorizationPolicy is needed.
- Local/FTP/Blob implement IObjectStorageProvider alongside IStorageProvider.
  Aggregator registration intentionally remains legacy-only; its README documents
  resolve-once portable casting, backed by `Aggregator/RegistrationMatrixTests.cs`.
  Storage.Abstractions README no longer claims implementations are missing.
- Proxy README now explicitly accepts Azure.Identity/AppConfiguration's 1.x
  dependency footprint; extraction requires a future versioned migration.
- Observability README explicitly retains the public `Observibility` namespace.
  Aggregator retains Wa.Wsdot public identities; Primitives retains existing EF/ASP.NET
  adapters; Secrets.Abstractions retains passive BCL-only KeyVaultOptions. These are
  compatibility exceptions, not remaining namespace-removal assignments.
- Proxy/Pipeline IInterceptor names describe different execution contracts.
  Queue/Table SDK-specific ports and SecretOptions versus KeyVaultOptions are
  distinct boundaries/passive compatibility surfaces, not duplicate domain DTOs.

## All-Library Evidence Inventory

These are existing reports read during this audit, not newly executed tests or a
combined final-source certification. Counts are copied from passing package summary
JSON; older Roslyn/Secrets.Abstractions counts are OpenCover sequence points.
For ordinary rows, the artifact is
`TestResults/coverage/<package>/<run>/summary.json`. A zero branch count is not a
missing branch-coverage report. Local evidence exists for all 28 source libraries.

| Package | Lines covered/total | Branches covered/total | Run |
| --- | --- | --- | --- |
| vc.Ifx | 1137/1137 | 803/803 | efaccb39823f41daa0d1ad94f5b7c968 |
| vc.Ifx.Abstractions | 95/95 | 18/18 | cde87fa6abca4fc18cd2f20ecb8d9c14 |
| vc.Ifx.Analyzers | 2024/2024 | 1090/1090 | 4803bd0e0a104e059b89cc2595043c1c |
| vc.Ifx.CodeFixes | 194/194 | 176/176 | 519d738e3ea74577b777bfa81c31bf07 |
| vc.Ifx.Data.Azure.Tables | 281/281 | 98/98 | 31e7f1544a6c4db6b3a8e2fe3236d64f |
| vc.Ifx.Filtering | 357/357 | 354/354 | 59d0f9849c0b445993ae51def829e1eb |
| vc.Ifx.Filtering.EntityFrameworkCore | 14/14 | 4/4 | e22a277ba26d4877b0ee90d3f74b9b69 |
| vc.Ifx.Generators | 1437/1437 | 516/516 | 8c69b8bfb566437b8adb8a449ea31009 |
| vc.Ifx.Generators.Abstractions | 43/43 | 0/0 | 10971deb59764e1bb0c3b3cf0efa21a4 |
| vc.Ifx.Messaging.Azure.Queues | 248/248 | 72/72 | 0182bec1f2dc45b3acf54858ef43bea5 |
| vc.Ifx.Observability | 70/70 | 16/16 | a0ef05be9bfc4d559725313b4fc35608 |
| vc.Ifx.Pipeline | 314/314 | 92/92 | 292ab125e54744308434cc6310e17e3a |
| vc.Ifx.Pipeline.Grpc | 53/53 | 18/18 | efef7400610c4b3f825f9ac7e30e9c72 |
| vc.Ifx.Primitives | 403/403 | 196/196 | 20868752736d4279bd5e812d8b3a35bf |
| vc.Ifx.Proxy | 2294/2294 | 692/692 | 9dd7e51162384aa586c68073ef9dd05d |
| vc.Ifx.Proxy.AspNetCore | 561/561 | 190/190 | 2ac7ef6f49a64ba7b413a49fb68c5739 |
| vc.Ifx.Proxy.Http | 153/153 | 110/110 | 53757d0fa3a545cca8f711f021111d96 |
| vc.Ifx.Querying | 323/323 | 156/156 | 5db3e65db1004c6aa9daaaf79c08bce7 |
| vc.Ifx.Roslyn | 22/22 sequence points | 0/0 | artifacts/coverage/roslyn-shared/coverage.opencover.xml |
| vc.Ifx.Roslyn.Reporting | 229/229 | 244/244 | 83965187fcd14149931c73851dda53c3 |
| vc.Ifx.Secrets.Abstractions | 19/19 sequence points | 2/2 | artifacts/coverage/secrets-abstractions/coverage.opencover.xml |
| vc.Ifx.Secrets.Azure.KeyVault | 172/172 | 78/78 | 0245b5693c5c4d5388c67aaf9f992407 |
| vc.Ifx.Secrets.Local | 39/39 | 10/10 | 5a9033b2c9f840ba924d2b09c94c2143 |
| vc.Ifx.Storage.Abstractions | 68/68 | 14/14 | c855ed1a03ec4130ae0a5dc08c39154d |
| vc.Ifx.Storage.Azure.Blobs | 262/262 | 114/114 | ebbf9f25fcd041cca1846a3814126445 |
| vc.Ifx.Storage.Ftp | 278/278 | 124/124 | e8af4155d5ae4c47bf795da35698b25f |
| vc.Ifx.Storage.Local | 211/211 | 40/40 | 030fa348d1e447708035c08516cddf14 |
| vc.Ifx.WebApi | 327/327 | 116/116 | 6cd18b8e77bc466983bc22bc05992885 |

## Inventory And Verification Limits

Initial XML/directory comparison found 28 source libraries and 35 solution
projects. **Final concurrent-state recheck found 36 solution projects:** the same
28 libraries, two centralized tests, four benchmarks and reporting host, plus
`docs/docs.csproj`. No project under src/tests/performance/scripts is missing;
the only additional solution project is the intended docs project. It is not a
29th library. No docs output directories existed at final inspection. Gate/validator
inventory is now enforced by the Gate 3 validator, including docs. Older 35-project
notes in active owners' sections remain historical; Gate 3 did not edit them.

Read-only verification used the plan's unchecked-item inventory, actual provider
exports/validation/registration source, current project/central metadata, package
READMEs, existing coverage JSON/OpenCover, and the recorded docs integration probe.
No new generator counterexample was compiled, no serializer experiment was run,
and Visual Studio behavior was not directly exercised. Priority A2 therefore
distinguishes a verified source-scope gap from unproven runtime outcomes.

## Reconciliation Verification and Handoff

Direct no-build commands passed on 2026-09-10:

```powershell
pwsh -NoProfile -File scripts/Test-FrameworkDependencies.ps1
pwsh -NoProfile -File scripts/Test-FrameworkDependencies.ps1 -SelfTest
```

Live result: 36 projects, 28 source libraries, exactly one non-packable docs
project; no inventory/reference/cycle/boundary failures. SelfTest: 25 probes;
fixtures `TestResults/dependency-audit/112648a24d724a40a02974dae33cd863`.
Static declared XML checks do not prove evaluated imported references/packability.

CodeFixes package-owner handoff is accepted: no Analyzers project reference or
bundled analyzer; separate Analyzers installation is required for IFX emission.
Run `b096e65d29104d539734b3e9247c8502` records 48 host checks, 19 validator checks,
six compiler probes and two fresh-cache consumers, with combined IFX1000 emitted
once. Provider source is unchanged; these are delegated results, not Gate 3 runs.

Orchestrator's prior `global-20260910-01` Release zero-warning build and coverage
`3c71dc66f5aa4fe9a29a5f20af0f7554` (3617 unit plus 15 integration tests, all 28
strict package gates and report passed) are historical until the active packaging
and infrastructure changes are integrated. No fresh global acceptance is claimed.

Return assignments: actual IDE verification and final combined gates remain with
Orchestrator/infrastructure. A2/A3/A4 are resolved by the explicit bounded decisions
and documentation reconciliation. Compatibility option classes and published
provider identities remain accepted exceptions, not missing record conversions.
Implementation ready; awaiting global integration. No package source work is
authorized by this audit; final verification is explicitly handed to Orchestrator.
