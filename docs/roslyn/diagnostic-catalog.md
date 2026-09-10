# Diagnostic Catalog

Owner: vc.Ifx.Roslyn; Updated: 2026-09-10 (bounded Gate 3 documentation reconciliation)

This is the source of truth for shared diagnostic contracts and selected remediation support. The executable shared catalog is `DiagnosticDescriptors.All`, containing only IFX1000 and IFX1001. Upstream diagnostics remain owned and emitted by their original tools. A code fix targeting an upstream ID does not transfer diagnostic ownership.

## Stable Shared Contracts

- `DiagnosticIdentifiers` retains the existing IFX1000 and IFX1001 values.
- `DiagnosticCategories.Maintainability` is the category for both shared rules.
- `DiagnosticPropertyNames.DiagnosticId` retains the existing `DiagnosticId` property key. Its value is the referenced diagnostic ID, while the diagnostic's own ID is IFX1000 or IFX1001. The referenced ID also supplies message argument zero. Locations cover that ID in the source comment or pragma.
- `DiagnosticDescriptors` exposes immutable Roslyn descriptors and an immutable, ID-ordered `All` array. Descriptors preserve existing titles, messages, severities, enabled defaults and descriptions, and add absolute help links to this catalog.
- `DiagnosticIdPattern.Pattern` and `Matcher` preserve their existing CA/CS-only behavior, including .NET regex digit and word-boundary semantics. `ReferencePattern` and `ReferenceMatcher` are additive: uppercase CA/CS with four ASCII digits, IFX with four ASCII digits, and compatibility-only IFX001 through IFX006. Matching recognizes references in text; it neither validates whether an upstream rule exists nor emits a diagnostic. Null input follows `Regex` and throws `ArgumentNullException`; empty input has no matches.
- No syntax/semantic helper or source-metric model is added: no shared consumer need was found. Reporting, analyzer execution, workspace operations, and filesystem access remain outside this package.

## IFX1000

| Field | Contract |
| --- | --- |
| Owner / emitter | vc.Ifx / vc.Ifx.Analyzers |
| Category | Maintainability |
| Default severity / enabled | Info / true |
| Title | Diagnostic reference requires disposition |
| Message | Diagnostic '{0}' is referenced without an explicit disposition |
| Help link | [IFX1000](https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#ifx1000) |
| Fix availability | `DiagnosticDebtCodeFixProvider` adds a `Tracked-by:` TODO in ordinary line/block comments; XML documentation is manual. Document/project/solution Fix All deduplicates edits; TODO remains diagnosed until human completion |

Remediation: record an actual issue, owner, removal plan, intentional decision, or fix deadline in the comment. This diagnoses missing debt disposition, not the upstream problem itself.

## IFX1001

| Field | Contract |
| --- | --- |
| Owner / emitter | vc.Ifx / vc.Ifx.Analyzers |
| Category | Maintainability |
| Default severity / enabled | Warning / true |
| Title | Diagnostic suppression requires justification |
| Message | Suppression for diagnostic '{0}' requires a justification |
| Help link | [IFX1001](https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#ifx1001) |
| Fix availability | `DiagnosticDebtCodeFixProvider` adds a justification TODO to an existing pragma or resolved suppression attribute. Document/project/solution Fix All; no new suppression or expanded scope; TODO remains diagnosed |

Remediation: remove unnecessary suppression or state the concrete reason for retaining it. A TODO is not completed justification. The original analyzer accepted any disposition marker, even empty, in both comments and pragmas. The analyzer policy update below replaces that behavior with tested meaningful-value requirements.

## Legacy IFX Inventory

These existing three-digit IDs remain owned by vc.Ifx.Analyzers. Only IFX001 is automatically exported; IFX002-006 retain their legacy opt-in Initialize APIs. They are not renumbered or promoted into the shared descriptor catalog. Release tracking now agrees with executable categories/severities, and descriptor help links resolve to this inventory.

| ID | Category | Severity | Title | Fix availability / disposition | Help |
| --- | --- | --- | --- | --- | --- |
| IFX001 | Design | Error | Proxy contract method signature | No provider; exact Task/Task<T> return and legacy request/token identities; manually update the contract | [Inventory](#legacy-ifx-inventory) |
| IFX002 | Refactoring | Warning | Refactoring Required | No provider; resolve the RefactorAttribute reason and owner, then remove the marker | [Inventory](#legacy-ifx-inventory) |
| IFX003 | Design | Warning | Unused private member | Compatibility-only; overlaps IDE0051, CA1823, CS0169/CS0414. Prefer upstream rules; no new export or provider | [Inventory](#legacy-ifx-inventory) |
| IFX004 | Design | Warning | Unused type | Compatibility-only; overlaps CA1812. Prefer upstream rule; no new export or provider | [Inventory](#legacy-ifx-inventory) |
| IFX005 | Design | Info | Controller attributes should be in preferred order | Reviewed single reorder of separate lists containing only resolved framework Authorize/AllowAnonymous/Route/ApiController attributes; no removal, ambiguous lists, directives or documentation trivia; metadata order changes, opt-in, no Fix All | [Inventory](#legacy-ifx-inventory) |
| IFX006 | Design | Warning | Multiple top-level classes in single file | No provider; framework source-organization policy | [Inventory](#legacy-ifx-inventory) |

Other analyzer-local families are VBD architecture rules, SEC security rules and CQ code-quality constants. Generator-local GEN001 (Mapping Error, Mapping, Error) and GEN002 (Enumeration value out of range, Generation, Error) have no fix or help link and are not shared descriptors. GEN002 rejects enumeration members outside the Int32 base contract; its source, release tracking and legacy-generator tests are already implemented. Gate 3 retains these existing generator IDs as an explicit legacy-prefix compatibility exception, not permission to allocate further GEN diagnostics. New framework diagnostics use IFX plus four digits; no ID is added or renumbered by this reconciliation.

IFX001 recognizes the resolved `Ifx.Proxy.ProxyContractAttribute` on interfaces, exact `System.Threading.Tasks.Task` / `Task<T>` returns, then `VisionaryCoder.Framework.ServiceRequest` and `System.Threading.CancellationToken` parameter types in that order. TaskFactory and lookalike request types fail. Nonordinary interface members and unannotated interfaces are ignored. These historical attribute/request identities are retained for compatibility, not retargeted to unrelated generator abstractions. IFX002 similarly retains `Wsdot.Idl.Ifx.Attributes.RefactorAttribute`; absent reason/owner values use the documented diagnostic defaults.

IFX003/004 normalize constructed generic references to original definitions and isolate state per compilation. Reports have stable symbol ordering. These are usage heuristics, not reachability/linker proofs; reflection and DI need explicit review. IFX004 retains discovery exemptions for recognized serializer, ORM, data-annotation and MSTest attributes. IFX005 remains a source-order preference only; it does not authorize requests or prove authorization equivalence. IFX006 counts classes through nested namespace declarations but prunes nested types; records/structs remain outside this legacy class-only policy.

### Legacy VBD Policy

Owner/emitter for every row is vc.Ifx.Analyzers through retained opt-in Initialize APIs. All are enabled when explicitly registered, have no code fix, and link here. Their scope is source declarations and assembly naming conventions, not a certification of a complete VBD architecture.

| ID | Category | Severity | Policy / remediation |
| --- | --- | --- | --- |
| VBD100 | Architecture.Volatility | Warning | A vault references multiple domain layers; isolate the volatility boundary. |
| VBD101 | Architecture.Volatility | Warning | Vault internals are exposed to a non-test/non-benchmark friend; remove the friend dependency. |
| VBD102 | Architecture.Volatility | Error | Infrastructure declarations reference domain types; invert the dependency. |
| VBD200 | Architecture | Warning | Manager contains inline domain computation; delegate that business decision to an Engine. |
| VBD201 | Architecture | Warning | Manager calls a different Manager assembly; move cross-manager coordination to the appropriate boundary. |
| VBD202 | Architecture.Layers | Warning | Manager references Access directly; use the Engine boundary. |
| VBD300 | Architecture | Warning | Engine declares mutable instance fields or settable properties; remove per-call state. |
| VBD301 | Architecture | Error | Engine base/interface/field/property references a Manager; invert the dependency. |
| VBD400 | Architecture | Warning | Access references another Access assembly; separate access responsibilities. |
| VBD500 | Architecture | Warning | Contract declares mutable fields/properties; use immutable values. |
| VBD501 | Architecture.Volatility | Error | Contract declaration references Service implementation; keep stable contracts inward. |
| VBD600 | Architecture.Volatility | Warning | Stable project declarations reference more volatile projects; move the dependency inward. |
| VBD601 | Architecture.Layers | Warning | Assembly reference violates the legacy layer matrix; align layer ownership. |
| VBD602 | Architecture | Warning | Declared type dependency violates the same layer matrix; align layer ownership. |
| VBD700 | Architecture.Volatility | Warning | Service declarations reference another Service; use stable contracts. |
| VBD800 | Architecture.Volatility | Warning | ORM is referenced outside its owning Service; encapsulate the implementation. |

Naming classification preserves precedence and case-insensitive matching from `ProjectAnalyzer`: Contract, Service, ORM, Infrastructure, WebApi, AzureFunctions, Benchmarks, Test, then Unknown. Test-like text does not override an earlier Service/Contract suffix. Legacy VBD202/601 retain case-sensitive domain prefix filtering; VBD602 uses the layer classifier directly. The layer matrix and volatility matrix are separately tested, not silently unified.

VBD100/202/601 now inspect actual referenced assembly symbols, including compilation references, in ordinal name order. Compilation-level diagnostics remain applicable when the compilation contains generated files. Symbol/syntax rules exclude generated source. The seven shared dependency walkers cover base types, interfaces, fields, properties/indexers, method returns/parameters/constraints and events, recursively expanding arrays, pointers and generic arguments. They deduplicate types and select the first source-order violation per type. This is declaration dependency analysis, not invocation-body dependency analysis. VBD301 retains its narrower historical surface.

VBD200 preserves the bounded decimal/domain-member heuristic: decimal arithmetic, comparisons touching domain members on both sides, decimal aggregations and conditional projections are business-computation signals; framework/infrastructure-only composition is allowed. Lambda method symbols do not falsely count as domain members. Unresolved Manager calls are ignored by VBD201 rather than choosing an arbitrary compiler candidate. VBD300/500 check shallow field/property mutability; readonly fields, init-only properties and static state retain their existing treatment. They do not establish deep object immutability, and events are outside that state policy. VBD101 retains its permissive Test/Benchmark name matching; assembly naming alone is not a security boundary.

### Legacy Security Policy

SEC001 and SEC002 are vc.Ifx-owned, opt-in compatibility helpers, category Security, severity Error, enabled when registered, with no code fixes. Their help links resolve here. Neither is automatically exported. Upstream CA2100 and configured security/data-flow analyzers retain their own ownership; no CA diagnostic is re-emitted.

- SEC001, **SQL Injection Vector Detected**, flags nonliteral SQL arguments at its recognized raw SQL method names. The leftmost concatenation operand is now included, and interpolation containing values is not treated as a literal. Use parameterized APIs or LINQ, not a generic SQL string sanitizer. This name-based syntax check can report unrelated lookalike calls and is not full semantic taint analysis.
- SEC002, **Unreviewed controller input in logging**, tracks recognized binding-attribute parameter names and simple assignment aliases in block-bodied controller methods. Its message now directs review of sensitive data, structured logging and output encoding, not SQL sanitization. Attribute-name matching, local declaration initializers, nested scopes, reassignment, expression-bodied methods and interprocedural flows retain legacy limitations. A zero-diagnostic result does not prove safe logging. `ControllerParameterTracker.MarkSanitized` remains an explicit helper state operation, not automatic sanitizer recognition or security certification.

### Legacy Metrics Policy

CQ100-104 remain vc.Ifx-owned opt-in compatibility helpers, category CodeQuality, severity Warning, enabled when registered, no fixes, help links here. Their fixed thresholds are not configurable options. Prefer upstream CA1502 for cyclomatic complexity; CQ104 is retained for API compatibility and is not newly exported.

| ID / legacy title | Measurement | Maximum |
| --- | --- | --- |
| CQ100 / Method is too long | Immediate body statements (not physical lines); expression body is one | 80 |
| CQ101 / Method has too many parameters | Parameter declarations | 5 |
| CQ102 / Method has too much nesting | Maximum enclosing block count; method body counts one; siblings do not accumulate; nested function bodies pruned | 4 |
| CQ103 / Method has too many local variables | Descendant variable declarators, including nested function declarations | 10 |
| CQ104 / Method cyclomatic complexity is too high | One plus if, for, foreach, while, do, case labels, ternary, && and || syntax nodes | 10 |

Only a value above the maximum reports. CQ metrics are historical syntax counts, not equivalent to Roslyn CA1502 or the separate IFX1100 control-nesting metric. No metric is persisted by analyzer execution. Cancellation is checked at callbacks and within the dependency/semantic/reporting loops that can grow with user code.

## Selected Upstream Remediation

For every CA row, owner is Microsoft and source/emitter is Microsoft.CodeAnalysis.NetAnalyzers (or legacy Microsoft FxCop for obsolete/unported rules). Category is the upstream rule category. Severity is upstream/configuration-controlled, not overridden by vc.Ifx: `configured` means consult the linked rule and the consumer's analysis level/editorconfig rather than assuming it is enabled. The table reflects the accepted CodeFixes support matrix: only CA1062 remains exported among these providers. Retired provider classes remain constructible for compatibility but advertise no diagnostic IDs, actions or Fix All. They are not automatic remediation.

| ID / title and help link | Category | Severity | Existing vc.Ifx fix availability / required remediation |
| --- | --- | --- | --- |
| [CA1051: Do not declare visible instance fields](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1051) | Design | configured | `Ca1051CodeFixProvider`: retired/unexported; field-to-property API/layout/reflection changes require application review |
| [CA1062: Validate arguments of public methods](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1062) | Design | configured | `Ca1062CodeFixProvider`: reviewed ThrowIfNull guard for one resolved reference parameter in an ordinary public block-bodied method; reject async/iterator/ref/value/error/nullable/optional ambiguity or existing guards; earlier null failure is a behavior change; no Fix All |
| [CA1303: Do not pass literals as localized parameters](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1303) | Globalization | configured | `Ca1303CodeFixProvider`: retired/unexported; select actual localization resources manually |
| [CA1413: Avoid nonpublic fields in COM visible value types](https://learn.microsoft.com/visualstudio/code-quality/ca1413) | Interoperability | legacy/configured | `Ca1413CodeFixProvider`: retired/unexported; legacy COM-layout review, no new emitter |
| [CA1704: Identifiers should be spelled correctly](https://learn.microsoft.com/visualstudio/code-quality/ca1704) | Naming | legacy/configured | `Ca1704CodeFixProvider`: retired/unexported; application naming/dictionary review |
| [CA1707: Identifiers should not contain underscores](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1707) | Naming | configured | `Ca1707CodeFixProvider`: retired/unexported; rename only with reference/API compatibility review |
| [CA1801: Review unused parameters](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1801) | Usage | deprecated/configured | `Ca1801CodeFixProvider`: retired/unexported; prefer upstream IDE0060 and explicit API review |
| [CA1806: Do not ignore method results](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1806) | Usage | configured | `Ca1806CodeFixProvider`: retired/unexported; inspect the result/HRESULT, do not mask it with a discard |
| [CA1812: Avoid uninstantiated internal classes](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1812) | Performance | configured | `Ca1812CodeFixProvider`: retired/unexported; review reflection/DI activation |
| [CA1819: Properties should not return arrays](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1819) | Performance | configured | `Ca1819CodeFixProvider`: retired/unexported; application owns collection/API decisions |
| [CA1822: Mark members as static](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1822) | Performance | configured | `Ca1822CodeFixProvider`: retired/unexported; use upstream reference-aware remediation |
| [CA1823: Avoid unused private fields](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1823) | Performance | configured | `Ca1823CodeFixProvider`: retired/unexported; use upstream tooling and preserve initializer side effects |
| [CA1824: Mark assemblies with NeutralResourcesLanguageAttribute](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1824) | Performance | configured | `Ca1824CodeFixProvider`: retired/unexported; choose the actual resource language |
| [CA1825: Avoid zero-length array allocations](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1825) | Performance | configured | `Ca1825CodeFixProvider`: retired/unexported; use upstream Array.Empty remediation |
| [CA2207: Initialize value type static fields inline](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2207) | Usage | configured | `Ca2207CodeFixProvider`: retired/unexported; mismatched enum rewrite withdrawn, not relabeled CA1008 |

For every CS row, owner is Microsoft and emitter is the C# compiler. Category is Compiler. Listed severities are compiler defaults and can be promoted/suppressed by the consumer. No vc.Ifx provider directly fixes these IDs; IFX1000/1001 only document their debt.

| ID / title and help link | Category | Severity | Fix availability / remediation |
| --- | --- | --- | --- |
| [CS0168: Variable declared but never used](https://learn.microsoft.com/dotnet/csharp/misc/cs0168) | Compiler | Warning | No vc.Ifx fix; remove unused declaration or implement intended use |
| [CS0219: Variable assigned but value never used](https://learn.microsoft.com/dotnet/csharp/misc/cs0219) | Compiler | Warning | No vc.Ifx fix; inspect side effects before removal |
| [CS8019: Unnecessary using directive](https://learn.microsoft.com/dotnet/csharp/language-reference/compiler-messages/using-directive) | Compiler | Hidden | No vc.Ifx fix; use upstream remove-unnecessary-usings tooling |
| [CS1591: Missing XML comment for publicly visible type or member](https://learn.microsoft.com/dotnet/csharp/language-reference/compiler-messages/cs1591) | Compiler | Warning | No vc.Ifx fix; write meaningful API documentation |
| [CS8602: Dereference of a possibly null reference](https://learn.microsoft.com/dotnet/csharp/language-reference/compiler-messages/nullable-warnings) | Compiler | Warning | No vc.Ifx fix; correct control flow or nullable contract; do not blindly add null-forgiving operators |

MSTEST0017 remains owned by MSTest.Analyzers. `MSTest0017CodeFixProvider` in `Providers/MSTest` is retired/unexported: use the upstream fix rather than the old syntax-only argument swap. Analyzer release tracking RS diagnostics and file-scoped namespaces are likewise upstream tooling concerns, not reasons to create duplicate IFX diagnostics.

## Analyzer Policy Update (2026-09-09)

The analyzer workstream adopts the stable shared debt descriptors and property key. The following bounded policies supersede the original permissive marker handling described above; shared descriptor IDs/titles/messages remain compatible.

### Diagnostic Debt Policy

- Analyze CA/CS four-digit and IFX four-digit references, plus legacy IFX001 through IFX006, in ordinary and XML documentation comments. Strings and inactive preprocessor regions are excluded. Repeated references retain distinct source locations.
- IFX1000 requires one nonempty `Tracked-by:`, `Fix-by:`, `Intentional:`, or `Justification:` value in the same comment. Values beginning with TODO, TBD, or FIXME (case-insensitive) do not resolve debt. Markers are case-insensitive and word-bounded. A marker cannot use another marker as its value.
- IFX1001 examines only codes actually disabled by a pragma, not IDs mentioned in its trailing explanation. Numeric compiler warning codes normalize to CS plus four digits. A same-directive trailing `//` comment must contain a meaningful `Justification:` value; a tracking marker alone is insufficient. Block comments are not valid pragma trailing comments in C#. Restore/inactive directives and blanket disables with no explicit IDs are outside this bounded rule.
- IFX1001 also examines semantically resolved `System.Diagnostics.CodeAnalysis.SuppressMessageAttribute` and `UnconditionalSuppressMessageAttribute`. CheckId may have a colon/title suffix; the exact supported diagnostic ID before the colon is reported. A meaningful constant `Justification` named argument is required. Lookalike attributes and unsupported IDs are ignored. The diagnostic location is the CheckId argument expression.
- `dotnet_code_quality.IFX1000.analyze_comments` and `dotnet_code_quality.IFX1001.analyze_suppressions` are per-file boolean options, default true. Invalid values fall back to true. Standard Roslyn diagnostic severity configuration remains authoritative. Disabled comments are not scanned; suppression configuration applies to both pragma and attribute forms.
- Roslyn generated-code classification is respected with `GeneratedCodeAnalysisFlags.None`; concurrent execution is enabled and traversal observes cancellation. There is no filesystem, network, clock, or environment dependence.
- The accepted code fixes append TODO placeholders and deliberately leave debt open until a human supplies a meaningful value. The support matrix records ordinary-comment/pragma/attribute bounds, document/project/solution Fix All and no-op/idempotence/cancellation tests.

### IFX1100

| Field | Contract |
| --- | --- |
| Owner / emitter | vc.Ifx / vc.Ifx.Analyzers |
| Category | Maintainability |
| Default severity / enabled | Warning / true |
| Title | Method control-flow nesting exceeds policy |
| Message | Method '{0}' has control-flow nesting depth {1} (maximum {2}) |
| Help link | [IFX1100](https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#ifx1100) |
| Fix availability | None; refactor manually while preserving behavior |

Metric policy `ifx-control-nesting-v1`: measure the maximum number of enclosing `if`, `switch` statement, `for`, either `foreach` syntax form, `while`, `do`, and `try` statements in an ordinary method body. The first construct has depth one; plain braces, using/lock blocks, catch/finally clauses, and else clauses add no depth. An else-if is another if and adds a level. A try's catch/finally contents remain inside that try level. Local function and anonymous function bodies are pruned and are not attributed to the enclosing method. Constructors, accessors, local functions as separate targets, and expression-bodied/abstract methods are outside this version's scope. Expression branching is not this statement-nesting metric.

`dotnet_code_quality.IFX1100.max_nesting_depth` accepts invariant integers 0 through 64; default 4, invalid values use 4. Report once on the method identifier only when measured depth exceeds the limit. Diagnostic properties are `MetricName=ControlFlowNestingDepth`, `MetricVersion=ifx-control-nesting-v1`, `MeasuredValue` and `Threshold` as invariant integer strings. These facts let reporting consume diagnostics without introducing speculative shared metric objects. The analyzer never persists reports or claims to measure test coverage.

Cyclomatic complexity remains upstream [CA1502](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1502); no IFX clone is introduced. Source method length and parameter-count policies are deferred. Existing CQ helpers remain compatibility code, not automatically registered analyzers.

### Repository Ownership Inventory

The source/configuration audit found recurring references to CA1822, CS0168, CA1062, CA1806, CA1823, CA1502, CA1801, CA2207, CA2007, CA1051, CA1303, CA1707, CA1825, CA1819, CA1824, CA1848, CA2000, and CS8981. These are source references/configuration, not a count of actual build violations. All remain upstream-owned; existing CA providers are code-fix-only, and their limitations remain in the remediation inventory above. CA1801 is obsolete in favor of IDE0060. Legacy CA1704/CA1413 support is not a new analyzer requirement. CA1502 remains upstream complexity policy; disposal CA2000, await CA2007, logging CA1848, nullable CS diagnostics, and using/style diagnostics receive no duplicate IFX emitter. Suppressed/ignored entries in consumer configuration do not transfer ownership or justify new rules.

The discoverable preexisting analyzers are DiagnosticDebtAnalyzer and Ifx001Signature. Other framework/VBD/SEC/CQ types retain tested Initialize helpers but no DiagnosticAnalyzer export. Compatibility retention does not justify activating upstream-overlapping rules. This work adds only MethodNestingAnalyzer as a discoverable analyzer; tests lock that exact export set.

## Endpoint Generator Diagnostics

Owned by the generator workstream; other diagnostic catalog sections are unchanged.
`vc.Ifx.Generators.MinimalEndpointGenerator` emits these generator-local descriptors
from `EndpointDiagnostics.All`. Category: Generation. Default severity: Error.
All are enabled by default; no code fix or Fix All support is supplied. They are
vc.Ifx-owned policy IDs and do not duplicate CS/CA diagnostics. Compiler-invalid
attribute applications are left to the C# compiler.

| ID | Title | Trigger and location |
| --- | --- | --- |
| IFX2000 | Invalid endpoint declaration | Invalid route template, method, or metadata; offending attribute argument. |
| IFX2001 | Unsupported endpoint handler | Unsupported signature or inaccessible container; method identifier. |
| IFX2002 | Duplicate endpoint route | Every equivalent method/route participant; other participants are additional locations. |
| IFX2003 | Duplicate endpoint name | Every duplicate explicit endpoint name; Name argument. |
| IFX2004 | Conflicting endpoint authorization | Contradictory anonymous/authorization metadata; endpoint attribute. |
| IFX2005 | Missing endpoint hosting reference | Missing ASP.NET endpoint hosting APIs; endpoint attribute. |
| IFX2006 | Unsupported endpoint payload | Declared delegate or delegate-array body/response payload, excluding service-bound delegate parameters; offending parameter or return type. |
| IFX2007 | Generated registry name collision | User type collides with the generated registry; endpoint attribute. |

Help link for all eight descriptors: [endpoint contract and diagnostic policy](minimal-api-generator-contract.md#duplicate-policy-and-diagnostics).
Release tracking is in `src/vc.Ifx.Generators/AnalyzerReleases.Unshipped.md`.
Tests are under `tests/unit/vc.Ifx.UnitTests/Generators/Implementation`.
The parser compares equivalent parameterized templates, including parameter-name
aliases; custom constraint overlap and collisions with external/manual registries
remain ASP.NET routing/integration-test responsibilities. Invalid handlers and all
members of duplicate groups are omitted; independent valid handlers can still emit.
Missing hosting APIs or a registry collision suppress the whole registry.

Gate 3 acceptance (2026-09-10): the frozen v1 policy rejects declared unsupported
signature shapes through IFX2001 and direct/array delegate payloads through
IFX2006. Arbitrary DTO members, custom converters and configured serializer
correctness remain consumer-tested. This is not a universal static serialization
proof; the plan's broader wording is reconciled to this accepted bounded scope.

## Consumer Handoff and Verification

Package composition reconciled 2026-09-10: CodeFixes does not reference or bundle
vc.Ifx.Analyzers; its only private Ifx dependency is vc.Ifx.Roslyn. Install the
separate Analyzers package for IFX emission. The accepted package-owner run
`b096e65d29104d539734b3e9247c8502` records 48 host checks, 19 validator checks,
six compiler probes and two fresh-cache NuGet consumers; combined installation
emits IFX1000 once. No provider source changed. See the
[support matrix](code-fix-support-matrix.md#package-composition) for the handoff
and host limits; Gate 3 did not rerun these package checks.

The analyzer and debt code fix consume the shared `DiagnosticPropertyNames.DiagnosticId` constant and `ReferenceMatcher`. The CodeFixes catalog handoff is reconciled to the accepted [support matrix](code-fix-support-matrix.md): three exported provider types cover IFX1000/1001, IFX005 and CA1062; 15 legacy providers are retired. Strict local evidence is 96/96 tests, 194/194 lines and 176/176 branches in `TestResults/coverage/vc.Ifx.CodeFixes/519d738e3ea74577b777bfa81c31bf07/summary.json`. IFX1100 remains analyzer-local because no generator or code fix needs a shared descriptor yet. This documentation reconciliation adds no diagnostic or provider and does not claim fresh package-host verification.

Tests in `tests/unit/vc.Ifx.UnitTests/Roslyn/SharedContracts` cover descriptor metadata, immutable ordered catalog, existing consumer compatibility, property/message transport, matcher boundaries and null handling. Syntax/semantic helpers and metrics are not applicable because none are exported. Package coverage applies to executable shared code only; compiler-inlined constants have no sequence points.

Analyzer policy tests live separately in `tests/unit/vc.Ifx.UnitTests/Roslyn/Analyzers`, with full legacy coverage in its `Legacy` subfolder. The 2026-09-09 strict serialized coverage run passed 506 tests, including 46 original debt/shared-contract regressions. Whole vc.Ifx.Analyzers measured 100% lines (2024/2024), branches (1090/1090), and methods without new coverage exclusions. Evidence: `TestResults/coverage/vc.Ifx.Analyzers/4803bd0e0a104e059b89cc2595043c1c/vc.Ifx.UnitTests/coverage.opencover.xml`. The analyzer-only serialized build passed with 0 warnings/errors. This scoped checkpoint is superseded by [final local acceptance](../planning/local-verification-20260910.md), which passed full solution, suite, strict coverage, reporting, and package checks. Hosted execution and hands-on IDE acceptance remain open. CodeFixes implementation/package handoff is accepted and its worker is closed; IFX2000-2007 remain the frozen generator-owned allocation. This bounded Gate 3 documentation handoff does not reopen those implementations.
