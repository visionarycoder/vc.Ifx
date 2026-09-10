# Package Contract Boundaries

Owner: Nash, Contract Boundaries; Updated: 2026-09-10; Status: bounded implementation/docs reconciliation ready; final verification handed to Orchestrator, global gates In-flight.

The dependency graph in project references determines dispatch readiness. Waves in
the parallel plan are guidance; a later-listed package may be a direct prerequisite
of an earlier-listed package. Run `pwsh -File scripts/Test-FrameworkDependencies.ps1`
after adding or changing project references.

## Ownership

| Owner | Contract and variation isolated | Direct framework prerequisites |
| --- | --- | --- |
| vc.Ifx.Abstractions | Existing service results, requests, and service lifecycle compatibility | None |
| vc.Ifx.Primitives | Identifiers, value validation, serialization and existing conversion adapters | None |
| vc.Ifx.Storage.Abstractions | Provider-neutral storage operations and compatibility configuration | None |
| vc.Ifx.Secrets.Abstractions | Secret lookup, missing-secret and cancellation semantics | None |
| vc.Ifx.Filtering | Predicate composition and portable filter AST, independent of database execution | None |
| vc.Ifx.Querying | Query specifications, serialization and explicit portable AST adaptation | Filtering (now directly referenced) |
| vc.Ifx.Pipeline | Invocation, handler, interceptor, routing, cache and observation ports | None |
| vc.Ifx.Storage.Local | Local filesystem behavior | Storage.Abstractions |
| vc.Ifx.Storage.Ftp | FTP transport behavior | Abstractions, Storage.Abstractions |
| vc.Ifx.Storage.Azure.Blobs | Blob storage behavior | Abstractions, Storage.Abstractions |
| vc.Ifx.Secrets.Local | Configuration-backed lookup | Secrets.Abstractions |
| vc.Ifx.Secrets.Azure.KeyVault | Vault authentication, versioning, caching and optional local fallback | Secrets.Abstractions, Secrets.Local |
| vc.Ifx.Data.Azure.Tables | Table persistence and optimistic concurrency | Abstractions |
| vc.Ifx.Messaging.Azure.Queues | Queue delivery, encoding and visibility | Abstractions |
| vc.Ifx.Filtering.EntityFrameworkCore | EF expression translation | Filtering |
| vc.Ifx.Proxy | Proxy invocation and interceptor contracts | Abstractions, Querying, Secrets.Abstractions |
| vc.Ifx.Proxy.Http | HTTP proxy transport | Proxy |
| vc.Ifx.Proxy.AspNetCore | Host identity and tenant extraction | Proxy, Secrets.Abstractions |
| vc.Ifx.Pipeline.Grpc | gRPC transport and serialization adapters | Pipeline |
| vc.Ifx.Observability | Activity and metric implementations of pipeline observation ports | Pipeline |
| vc.Ifx.WebApi | HTTP status catalog, exception-to-problem mapping, resilience configuration | None |
| vc.Ifx.Roslyn | Diagnostic identities and compile-time shared contracts | None |
| vc.Ifx.Analyzers | Deterministic source diagnostics | Roslyn |
| vc.Ifx.CodeFixes | Diagnostic remediation | Roslyn, Analyzers as currently referenced |
| vc.Ifx.Generators.Abstractions | Passive public generator marker attributes | None |
| vc.Ifx.Generators | Compile-time source generation | Roslyn; attribute names resolved in consumer compilation |
| vc.Ifx.Roslyn.Reporting | In-memory build/source/coverage evidence to versioned report facts and JSON/Markdown | None; Roslyn syntax SDK is an external package, not an analyzer implementation reference |
| vc.Ifx | Compatibility facade and intentionally broad runtime aggregation | 21 explicitly referenced runtime packages; compiler tooling, Reporting and passive generator attributes remain opt-in |

This is an ownership map, not permission to add every listed conceptual contract.
Keep public interfaces minimal and add abstractions only for observed variation.
Do not introduce a shared business-domain object package. Application boundaries
exchange explicit commands, facts, records and results; framework implementation
objects and provider SDK objects remain behind the owning boundary.

## Stable Contracts and Compatibility

The package worker records contract stability in its plan Notes before consumers
start implementation. Stability can precede full coverage completion, but it must
identify the APIs frozen for consumers. Changes after this point require notifying
consumers and rerunning their relevant tests.

Existing `ServiceBase<T>` logging/disposal in Abstractions is a compatibility
exception. Do not move it merely to satisfy a folder naming preference. Similarly,
passive legacy provider option types require an explicit migration strategy before
moving between assemblies. Removing a public type from an old assembly can break
already compiled consumers even when its namespace remains unchanged.

`vc.Ifx.Proxy/Proxies` is excluded from compilation and contains an old alternative
proxy implementation. It is not a second supported contract. The Proxy worker must
document retirement or complete a tested migration before removing that archive;
do not enable both implementations or use the archive as a new dependency.

Pipeline observation ports belong to Pipeline; Observability implements them.
Proxy transport ports belong to Proxy; HTTP and ASP.NET integrations consume them.
Do not move these contracts into general Abstractions solely to share a name.
Provider-specific Azure configuration in Proxy is an approved 1.x compatibility
retention. Preserve public identity and document the minimum dependency footprint;
any extraction is a future versioned migration, not part of this release.

Primitives deliberately retains EF converters and ASP.NET model binders in its
existing assembly. Its EF package and ASP.NET framework reference are not evidence
of an abstraction-project dependency leak; `src/vc.Ifx.Primitives/README.md` records
the versioned migration requirement. Likewise, Secrets.Abstractions retains the
passive BCL-only `KeyVaultOptions` type, and Storage.Abstractions retains the legacy
FileInfo/DirectoryInfo API and Type/object factory-registration descriptors. These
composition/compatibility types are not portable application messages. Do not move
them between assemblies without binary/source compatibility evidence.

Gate 3 acceptance (2026-09-10): the plan's "options records" wording describes
boundary intent, not a requirement to convert existing option classes into C#
records. Retain public class identity, mutability, inheritance and equality
semantics where already published. The BCL-only KeyVault options, filesystem
overloads, Type/object registration descriptors, Primitives adapters and Azure
Queue/Table SDK ports are explicit compatibility exceptions; no duplicate wrapper
or universal shared DTO library is required to check the bounded implementation.

The two `IInterceptor` names in Proxy and Pipeline are not duplicate contracts:
Proxy's legacy marker supplies ordering, while Pipeline's port wraps typed request
continuations. `Models.Month` in the aggregator is a legacy month-name model;
`Primitives.Month` is a year/month value. Do not merge distinct semantics merely
because the short names match. No new shared business-domain object library was
found; owner-tagged primitive values and passive framework messages are not such a
library.

## Design Artifacts

| Producer | Artifact | Consumers |
| --- | --- | --- |
| Roslyn | `docs/roslyn/diagnostic-catalog.md` | Analyzers, CodeFixes, reporting |
| CodeFixes | `docs/roslyn/code-fix-support-matrix.md` | Code-fix tests and package docs |
| WebApi | `docs/webapi/http-response-catalog.md` | Exception mapper, HTTP helpers, generated endpoint integration tests |
| Generators.Abstractions and Generators, sequential ownership | `docs/roslyn/minimal-api-generator-contract.md` | Attribute consumers, generator and host smoke tests |
| Roslyn.Reporting | `docs/reporting/report-contract.md` and `docs/reporting/verification.md` | Reporting command host, CI artifacts, Orchestrator |

Generators must inspect attributes using Roslyn metadata names; their implementation
does not reference the net10.0 marker assembly from netstandard2.0. Generated code
uses the consumer's ASP.NET Core APIs. Integration with WebApi occurs through host
configuration, avoiding runtime references back into the generator.

## IFX1100 Reporting Compatibility

Compared the two reporting documents with `MethodNestingAnalyzer`, reporting's
`SourceMetrics`/`ReportEngine`, and their existing tests on 2026-09-09. The reporting
worker's relay request is satisfied: no analyzer or reporting package change is
needed for the published metric contract.

| Contract | Compatibility result |
| --- | --- |
| SARIF metric fields | Exact: `MetricName=ControlFlowNestingDepth`, `MetricVersion=ifx-control-nesting-v1`, invariant string `MeasuredValue` and `Threshold`. Reporting reads direct properties and Roslyn's nested `properties.customProperties`. |
| Measurement over the common scope | Identical stack traversal of ordinary block-bodied methods: body starts at zero; if, switch statement, for, both foreach forms, while, do and try increment depth. Siblings do not accumulate. Else-if adds another if; catch/finally retain the enclosing try depth. |
| Nested functions | Both prune local functions and anonymous functions; their branches/nesting are not attributed to the enclosing method. |
| Threshold | Analyzer defaults to 4 and reports only above its per-file configured 0..64 limit. Reporting independently labels nesting above 4 `Review`. With analyzer limit 8, depth 5 is a reporting Review but emits no IFX1100; with limit 2, depth 3 emits IFX1100 but is not a reporting Review. This is intentional, not contradictory evidence. |
| Method population | Reporting measures all ordinary methods, including expression bodies, and reports null/Unknown for bodyless declarations. IFX1100 only visits block-bodied methods and only emits threshold violations. Missing diagnostics cannot supply complete metrics or imply zero. |
| Generated population | IFX1100 uses Roslyn generated-code classification/configuration; reporting uses its explicit file/member exclusions and Compile manifest. Reporting does not consume per-tree analyzer configuration. Population equivalence is not promised; compare the same handwritten source and symbols. |
| Other metrics | Reporting's `ifx-method-source-v1` includes its own decision/physical-line/parameter metrics. It is neither CA1502 nor legacy CQ104; nesting is the compatible submetric, not an assertion that the complete policy versions are interchangeable. |

Verification: `pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -Filter 'FullyQualifiedName~MethodNestingPolicyTests|FullyQualifiedName~Reporting.ReportEngineTests'`
passed 35/35 tests, no skips. Evidence:
`TestResults/tests/vc.Ifx.UnitTests/49c1e02655b747cba37d755d49533f48/tests.trx`.
This reruns existing metric/property tests; it is not a new end-to-end SARIF build
capture or a new coverage measurement. Reporting's previously recorded strict
229/229 lines and 244/244 branches remain that worker's evidence, not this audit's.

## Current Inventory and Validator

Independent directory-vs-solution XML comparison: 28 libraries under `src/`, two
centralized test projects, four benchmark projects, one script host and one docs
project, totaling 36 solution projects on 2026-09-10. Directory-only,
solution-only and duplicate project entries
are all empty. `vc.Ifx.Roslyn.Reporting` is the 28th library;
`scripts/reporting/FrameworkReport.Host.csproj` is a separate non-packable command
host referencing Reporting. `docs/docs.csproj` is now a real SDK solution Project,
not a File item or a 29th library. The older 35-project snapshot predates it.

The old validator's 34-project result omitted script hosts from the graph. The
updated `scripts/Test-FrameworkDependencies.ps1` includes `scripts/` and `docs/`, checks exact
inventory and duplicate entries, rejects absent/unresolved references, traverses
host cycles, and rejects reverse aggregator, abstraction-to-implementation,
abstraction provider/host-framework, runtime-to-compiler and compiler-to-runtime
project edges. Empty inventories cannot pass. Source libraries cannot reference
test/benchmark/command/docs projects. Exactly one documentation project must exist
at docs/docs.csproj with an explicit unconditional IsPackable=false declaration;
missing, duplicate, extra, unlisted and File-only docs entries cannot pass.
Conditional XML references are conservatively
checked even if inactive; imported or generated MSBuild references and transitive
NuGet assets are not evaluated by this static validator. It does not certify
public API semantics, DI wiring, NuGet packaging, or global quality gates.

Verification commands:

```powershell
pwsh -NoProfile -File scripts/Test-FrameworkDependencies.ps1
pwsh -NoProfile -File scripts/Test-FrameworkDependencies.ps1 -SelfTest
```

The live graph passed for 36 projects/28 source libraries. All 25 synthetic
positive/negative probes passed; retained XML fixtures are under
`TestResults/dependency-audit/112648a24d724a40a02974dae33cd863/`. The original 16
boundary probes remain; nine additional probes cover docs inventory, declared
packability, source-to-docs references and docs/host cycles. No build or coverage
instrumentation is performed by this validator. No package implementation,
solution, central package/build file or test project was edited in Gate 3;
the authorized Grpc README change is documentation only.

The documentation owner separately passed 14 CLI checks in
`TestResults/docs-project/97a0183382a744cb9ffcb6d700d3678c`, including recursive
relative items, no-output targets and a mixed SDK solution. See
`docs/packaging/documentation-project-integration.md`. Static XML validation does
not evaluate imported packability/references or prove Visual Studio behavior.
No development Visual Studio is installed; actual IDE loading/refresh remains
explicitly unverified, not a current missing docs Project implementation.

## Owner Fix Requests

The numbered findings below preserve the original audit snapshot and delegation,
not current unresolved source defects. The 2026-09-10 resolution annotations
supersede their pending language. No active package code is assigned to Gate 3;
global acceptance remains In-flight with Orchestrator.

1. **Proxy owner, high priority: bridge or retire the disconnected authorization registration surface.**
   `src/vc.Ifx.Proxy/Interceptors/Authorization/AuthorizationExtensions.cs:51` and
   `:70` register `IAuthorizationPolicy`, while
   `src/vc.Ifx.Proxy/Interceptors/Security/SecurityInterceptor.cs:28` consumes only
   `IEnumerable<IProxyAuthorizationPolicy>`. The active source scan found no
   adapter between them. A legacy deny policy registered through those helpers
   does not by itself participate in this interceptor; an empty consumed policy
   collection proceeds to the continuation. Choose a canonical enforcement port
   and a compatibility adapter/deprecation strategy, then prove deny/allow and
   cancellation through DI and the actual proxy pipeline. Do not silently remove
   existing public types. ASP.NET's explicit IProxyAuthorizationPolicy registration
   is already the matching path; it does not repair the legacy helpers.
   **Decision (2026-09-09):** Orchestrator assigned Kuhn the canonical
   `IProxyAuthorizationPolicy` port plus a legacy-helper compatibility adapter and
   enforcement tests. Verification belongs to that owner and global integration.
2. **Aggregator owner: named-options finding repaired concurrently; retain regression evidence.**
   The initial snapshot used unkeyed `TryAddSingleton(options)` for named storage
   providers, allowing the first options instance to cross names. Final source
   recheck confirms the owner's keyed factories now capture each supplied options
   instance for Local, FTP, Blob, Queue and Table. This is no longer an untouched
   code defect. Owner reports 416 Extensions plus 5 Aggregator tests passing and
   has fresh aggregator coverage queued; this audit did not independently rerun
   them. Preserve public signatures and retain two-name root/endpoint/credential
   isolation tests in the final owner verification.
3. **Aggregator and storage owners: finish portable storage composition.**
   Local, FTP and Blob now implement IObjectStorageProvider, but aggregator
   `StorageExtensions` still registers only IStorageProvider. Add explicit portable
   keyed/unkeyed composition or clearly separate legacy-only registration and
   document the supported alternative; validate ownership/disposal rather than
   inadvertently creating two provider instances. Storage.Abstractions owner:
   update README lines 15-17 and the future-tense provider handoff, which still say
   no provider implements the new interface. Preserve frozen request/result types.
   **Decision (2026-09-09):** Aggregator owner will document legacy-only registration
   and tested explicit portable composition, without silently changing
   `IStorageProvider` semantics. Orchestrator also authorized the narrow
   Storage.Abstractions README refresh; request/result source remains frozen.
4. **Proxy owner: close the documented provider-dependency decision.**
   Core Proxy still directly references Azure.Identity and Azure App Configuration,
   and compiles `Interceptors/Configuration/Azure/AzureConfigurationProvider.cs`.
   Its README explicitly leaves this footprint under review. Implement an approved
   provider extraction with a compatibility strategy, or record the versioned
   retention decision and minimum-consumer impact before claiming a minimal core.
   This is existing declared debt, not a new dependency introduced by this audit.
   **Decision (2026-09-09):** Retain these dependencies for the current 1.x public
   identity. Proxy owner must make retention and minimum-consumer impact explicit
   in README; extraction is a future versioned change, no longer under review.

### Resolution Annotations (2026-09-10)

1. **Authorization bridge implemented and accepted.** Proxy's
   `LegacyAuthorizationPolicyAdapter` and `AuthorizationExtensions` bridge legacy
   helpers to canonical IProxyAuthorizationPolicy. Real DI pipeline tests in
   `tests/unit/vc.Ifx.UnitTests/Proxy/AuthorizationBridgeContractTests.cs` cover
   enforcement. Owner evidence: 463 tests, 2294/2294 lines, 692/692 branches,
   `TestResults/coverage/vc.Ifx.Proxy/9dd7e51162384aa586c68073ef9dd05d`.
2. **Named options and collection regression resolved.** Aggregator source freeze
   was accepted after Release and Debug verification: 1095 selected tests each;
   Release 1137/1137 lines and 803/803 branches in
   `TestResults/coverage/vc.Ifx/68d0069ce1df4c438fd3c10454642b48`.
   RegistrationMatrixTests and the legacy collection tests preserve identities
   and supplied-input AddRange behavior; the original self-copy finding is closed.
3. **Portable composition documented and tested.** Aggregator README explicitly
   keeps legacy-only registration and shows resolve-once portable casting, tested
   in RegistrationMatrixTests. Storage.Abstractions README now identifies its
   Local/FTP/Blob implementations. No silent IStorageProvider semantic replacement.
4. **Azure retention accepted.** Proxy README documents the current 1.x
   Azure.Identity/AppConfiguration dependency footprint and versioned future
   extraction. This release does not claim a provider-free Proxy core.

Observability README now explicitly documents `Observibility`; aggregator README
documents the Wa.Wsdot public identities. Neither calls for a namespace rename.
The later Filtering TimeOnly review is also resolved by round-trip `O` formatting
and TimeOnlyRoundTripTests; owner evidence is 357/357 lines, 354/354 branches in
`TestResults/coverage/vc.Ifx.Filtering/59d0f9849c0b445993ae51def829e1eb`.
These are accepted owner measurements, not new Gate 3 test runs. Final gate
acceptance is handed to Orchestrator.

Queue/Table interfaces intentionally live in their Azure-specific packages and
expose `QueueMessage`, `ITableEntity`, `ETag` and table update modes. They are SDK
adapter boundaries, not provider-neutral domain contracts; application Access
components must translate these before sending cross-service facts/results.
Passive KeyVault options, legacy filesystem overloads, Primitives adapters and
the excluded Proxy prototype are documented compatibility exceptions, not requests
for blanket deletion or duplicate replacement contracts.

## Namespace and API Compatibility Inventory

This is a public-identity audit, not a rename request. The source scan excluded
obj/bin, the explicitly uncompiled Proxy `Proxies/` archive, and excluded Sample
trees. Raw generated-source strings are not counted as declarations of the
generator assembly. Preserve published namespaces and assemblies; a different
namespace is a different API even when the short type name is unchanged.

| Surface and evidence | Classification / owner request |
| --- | --- |
| `src/vc.Ifx/Errors/{Error,ErrorsCollection,IsRecoverableErrorSpecification}.cs` use `Wa.Wsdot.Fin.Idl.Ifx.Errors`; `Generics/GenericReadOnlyCollection.cs` uses `Wa.Wsdot.Fin.Idl.Ifx.Generics` | Legacy organization-specific public identities in the aggregator, not newly introduced shared business-domain types. Aggregator owner is preserving binary compatibility and adding explicit README guidance. Keep those identities; test old consumer binding and collection behavior. Orchestrator reports the local AddRange self-copy bug is being fixed by that owner; this audit neither changes it nor claims its verification. |
| `Pipeline/Observibility/Abstractions/{IMetrics,ITracer}.cs` and `Observability/Observibility/{OpenTelemetryMetrics,OpenTelemetryTracer}.cs` use `VisionaryCoder.Framework.Pipeline.Observibility[.Abstractions]` | Published spelling and namespace ownership differ from the Observability package name. Pipeline/Observability owners should document the deliberate compatibility spelling and lock adapter-to-port assignability. Do not fix the spelling by silently renaming public namespaces. |
| Abstractions `Time/IClock.cs` uses `vc.Ifx.Abstractions.Time`; aggregator `Time/{SystemClock,NullClock}.cs` use `vc.Ifx.Time`, unlike most runtime packages' `VisionaryCoder.Framework` prefix | Mixed public naming convention, not duplicate clock contracts. Abstractions owns the port; aggregator owns implementations. Preserve current identities and document them in consumer examples; do not invent a second IClock solely for naming consistency. |
| Storage.Azure.Blobs uses `VisionaryCoder.Framework.Storage.Azure.Blob`; Messaging.Azure.Queues uses `VisionaryCoder.Framework.Messaging.Azure.Queue`; Data.Azure.Tables uses `VisionaryCoder.Framework.Data.Azure.Table`; Filtering.EntityFrameworkCore uses `VisionaryCoder.Framework.Filtering.EFCore` | Singular/provider-short namespaces differ from package names but are established public identities. Provider/aggregator examples and registration code must use the actual namespaces. No rename or forwarding shim is requested by this audit. |
| KeyVault `Azure/SecretOptions.cs` uses `VisionaryCoder.Framework.Secrets.Azure`; retained KeyVaultOptions uses `VisionaryCoder.Framework.Secrets.Azure.KeyVault` but is declared in Secrets.Abstractions | Two passive legacy options APIs with different ownership/behavior. KeyVault README explicitly says SecretOptions is not implicitly mapped or consumed by registration. Keep the distinction; no automatic merge or type move. |
| IFX001 recognizes `Ifx.Proxy.ProxyContractAttribute`; IFX002 recognizes `Wsdot.Idl.Ifx.Attributes.RefactorAttribute` | External legacy metadata hooks retained by the analyzer audit. No declarations of these attribute identities were found in active source; consumers must supply the legacy attributes. They are not aliases for current generator markers. Any new attribute migration is a separate versioned owner decision, not a namespace sweep. |
| MapperGenerator contains `namespace Generated.Mappers` inside emitted-source text; Roslyn/Generators/Reporting use `vc.Ifx.*` namespaces | Generated output contract and tooling namespace convention, respectively. Neither is evidence of an accidental runtime domain namespace. Generator-owned output naming and IFX2000-2007 remain unchanged. |

Only the four aggregator source files above retain the `Wa.Wsdot.Fin.Idl.Ifx`
namespace in the audited active source set. The table preserves original owner
requests; the resolution annotations above close their implementation follow-ups.
Remaining compatibility identities are deliberate exceptions, not unassigned API
removal work. Do not treat every package/namespace spelling difference as a defect.

Orchestrator retains the whole-solution warning-free build, complete test/coverage
run, final all-package inventory and package/host integration. No global gate was
run or marked complete by this final contract audit.

## Parallel Verification

The infrastructure worker owns shared build properties, central package versions,
test project references and the solution file. Package workers own source, scoped
tests and README content. The metadata worker owns packaging elements in package
project files. Requests crossing those write scopes go through the orchestrator.
Workers edit only their plan section using contextual patches; never rewrite the
whole shared plan from an earlier snapshot.

Shared build and test output directories require serialized verification or fully
isolated output paths. Coverage cannot be collected while another process rebuilds
or instruments the same assembly. Record exact commands and module-level line and
branch counts. A successful build is not proof of test discovery or coverage.
