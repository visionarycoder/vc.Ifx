---
name: vbd-architecture-conformance
title: VBD Architecture Conformance Testing
description: Generate and run automated conformance checks for proxy-only calls, contract dependencies, vault isolation, provider behavior, and naming rules.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1366
prerequisites:
  - vbd-system-design
  - ifx-component-communication
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vbd-boundary-contract-mapping
  - vbd-drift-analyzer-design
  - vbd-use-case-migration
appliesTo: '**/*.{cs,csproj,sln,slnx,json,md}'
tags:
  - vbd
  - architecture
  - testing
  - conformance
  - governance
---

# VBD Architecture Conformance Testing

Agent generates a runnable safety net that proves the selected architecture still holds.

## When to Use

Use when constructed components need continuous architecture proof.
Use when a migration wave needs pre-cutover and post-cutover conformance checks.
Use when a message-bus provider needs certification against a shared contract suite.
Use when analyzers, code fixes, or generators need reusable starter shapes.

## When Not to Use

Do not use when the prompt asks for new logical design. Use `vbd-system-design` or `vbd-advanced-design`.
Do not use when one DTO coupling finding is the only defect. Use `vbd-boundary-contract-mapping`.
Do not use when the prompt asks for communication substrate design. Use `ifx-component-communication`.

## Inputs

| Input | Required | Description |
|---|---|---|
| Selected design rules | Yes | Allowed dependencies, naming, proxy, bus, and vault rules |
| Project graph | Yes | Solution or project subset to test |
| Bus providers | No | In-memory and durable implementations |
| Existing analyzer assets | No | Analyzer, code-fix, or generator templates to extend |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Generate rule catalog | Agent generates one automated assertion per architecture rule. | Rule catalog | Agent reads the catalog. | Every rule has one test technique and one owner. |
| 2. Run graph checks | Agent runs project-graph and symbol-graph checks for dependency and caller rules. | Graph report | Agent reads the report. | Violations include file, symbol, and rule ID. |
| 3. Run provider suite | Agent runs the shared bus-provider conformance matrix. | Provider matrix | Agent reads the matrix. | Every configured provider has pass or fail per test row. |
| 4. Generate templates | Agent generates analyzer, code-fix, and generator skeletons when a rule lacks automation. | Template packet | Agent reads the packet. | Every new template has positive, negative, and fix-all test scope. |
| 5. Publish conformance report | Agent writes `CONF-*` records, waivers, and remediation links. | Conformance packet | Run `npm run vbd:artifacts -- validate <analysis-root> [--source-root <source-root>]` when governed artifacts exist. | Exit code = 0. Conformance references resolve. |

## Rule Catalog

| Rule ID | Assertion | Pass |
|---|---|---|
| ARCH-001 | Component calls use the generic proxy | Zero direct remote implementation calls |
| ARCH-002 | Managers do not call other managers directly | Zero direct manager-to-manager calls |
| ARCH-003 | Callers depend on contracts only | Zero caller dependencies on service or ORM projects |
| ARCH-004 | Vault isolation holds | Manager code touches declared activity vaults only |
| ARCH-005 | Ifx and utility code stay business-neutral | Zero business policy or workflow branching in shared infrastructure |
| ARCH-006 | Naming rules hold | Zero prohibited naming patterns |
| ARCH-007 | Bus provider passes contract suite | All provider matrix rows pass |

## Provider Conformance Matrix

| Test | Pass |
|---|---|
| Publish and subscribe round trip | Handler receives unchanged envelope |
| Ordering per key | Same-key messages stay in send order |
| Idempotent redelivery | Duplicate delivery does not duplicate business result |
| Poison or dead-letter path | Repeated failure routes to dead-letter |
| Envelope schema gate | Invalid envelope is rejected before dispatch |
| Startup fail-fast | Invalid provider config stops startup |

## Output Contract

| Output | Minimum content |
|---|---|
| Conformance report | Rule ID, status, evidence, and owner |
| Provider matrix | Provider name and test result per row |
| Template packet | Analyzer, code-fix, generator starters and paired tests |
| Waiver log | Rationale and expiration |
| Artifact packet | `CONF-*` records when governed artifacts exist |

## Verification

- [ ] Agent gives every rule an automated assertion.
- [ ] Agent runs the full provider suite for every configured provider.
- [ ] Agent gives templates paired positive, negative, and fix-all tests.
- [ ] Agent links violations to remediation skills.
- [ ] Agent time-boxes waivers.

Test: Run the architecture conformance suite for the selected solution scope.
Pass: Zero unwaived rule failures remain. Every provider row passes or is blocked from selection.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Code review is the only conformance gate | Agent writes automated assertions |
| One provider passes and others are assumed safe | Agent runs the same matrix for every provider |
| Templates fork per finding | Agent reuses shared skeletons |
| Waivers accumulate silently | Agent writes waiver owner and expiration |
