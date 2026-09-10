---
title: Controller Pattern Instruction
description: Controller versus router pattern selection and implementation guidance
doc_type: instruction
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 980
prerequisites:
  - .github/instructions/ste-agent-writing-standard.instructions.md
  - .github/instructions/frontmatter-standard.instructions.md
related_skills:
  - skill-lifecycle-controller
  - create-skill
  - skill-suite-optimization
appliesTo: '.github/skills/**/*.md'
tags:
  - controller
  - architecture
  - patterns
  - skills
---
# Controller Pattern Instruction

Agent applies this guidance when deciding between controller and router patterns for skill design.

## Objectives

Agent distinguishes execution-capable controllers from routing-only bundles.
Agent selects the pattern that minimizes token overhead for the workflow frequency.
Agent creates controllers when cross-cutting analysis is required.
Agent creates routers when specialist coverage is mature and discovery is the primary value.

## Pattern Definitions

| Pattern | Purpose | Token Characteristic | Delegation Behavior |
|---------|---------|---------------------|---------------------|
| Router (Bundle) | Decision + Delegation | Low overhead (~1,000-2,000 tokens) | Always delegates to specialists |
| Controller | Execution + Delegation | Medium overhead (~2,000-3,500 tokens) | May execute directly OR delegate |

## Decision Matrix

| Condition | Use Controller | Use Router (Bundle) |
|-----------|----------------|---------------------|
| Work requires cross-cutting analysis (ecosystem audit, SAST, compliance reporting) | Yes | No |
| Work requires mode-based execution (scan → remediate → verify → report) | Yes | No |
| Specialists exist for all cases and no new capability is needed | No | Yes |
| Domain is stable and users need discovery | No | Yes |
| High frequency (weekly) with token burn concern | Yes | Consider (analyze token math) |
| Low frequency (quarterly) with mature specialists | No | Yes |
| Workflow is new and no specialist exists yet | Yes | No (build specialist first, controller later) |

## Token Efficiency Analysis

Agent calculates token overhead for both patterns before selection.

### Router (Bundle) Token Cost

```
Total = Bundle + Specialist(s)

Example (Security):
  Bundle: 1,150 tokens
  + Specialist 1: 1,500 tokens
  + Specialist 2: 1,500 tokens
  = 4,150 tokens per invocation
```

### Controller Token Cost

```
Total = Controller only (if executing directly)
     OR Controller + Specialist (if delegating)

Example (Security):
  Controller: 2,800 tokens
  (executes scan + remediate + verify directly)
  = 2,800 tokens per invocation
```

### Breakeven Calculation

```
Frequency × (Router Cost - Controller Cost) = Monthly Savings

Example (Weekly security sweeps):
  4 weeks × (4,150 - 2,800) = 5,400 tokens/month saved
```

**Threshold:** If monthly savings >2,000 tokens AND frequency ≥weekly, controller justifies overhead.

## Mode Activation Pattern

Controllers use mode activation tables instead of pure routing:

| User Intent | Mode | What Controller Does |
|-------------|------|---------------------|
| "Scan for issues" | `scan` | Executes scanning workflow directly |
| "Fix issues" | `remediate` | Delegates to specialists in priority order |
| "Verify changes" | `verify` | Executes verification workflow directly |
| "Generate report" | `report` | Executes compliance reporting directly |

**Router equivalent:**
- Routes "scan" → specialist-scan skill
- Routes "fix" → specialist-fix skill
- Routes "verify" → specialist-verify skill
- Routes "report" → ??? (skill doesn't exist)

## Workflow Structure

| Step | Router Action | Controller Action |
|------|---------------|-------------------|
| 1. Detect intent | Read activation table | Read mode table |
| 2. Select route | Pick specialist skill | Pick mode (create/review/optimize/audit/migrate/deprecate) |
| 3. Execute work | Load specialist skill | Execute mode OR delegate to specialist |
| 4. Generate output | Specialist output | Mode-specific output |

## Implementation Checklist

When creating a controller:

- [ ] Agent defines at least 3 modes (create/fix/verify pattern)
- [ ] Agent writes mode activation table (User Intent → Mode → Action)
- [ ] Agent implements direct execution for at least 1 mode (not just delegation)
- [ ] Agent adds capability that no specialist provides (audit, drift detection, compliance reporting)
- [ ] Agent keeps main skill <3,500 tokens
- [ ] Agent moves verbose detail to `references/`
- [ ] Agent validates token savings justify overhead (if converting from bundle)

When creating a router:

- [ ] Agent writes activation table (Signal → Specialist)
- [ ] Agent verifies all routes point to existing specialists
- [ ] Agent keeps routing logic <2,000 tokens
- [ ] Agent documents when NOT to use the router (direct specialist cases)

## Conversion Criteria

Agent converts router → controller when:

1. **Cross-cutting analysis needed:** Work requires scanning all targets and generating unified report
2. **Mode progression exists:** Workflow has natural phases (scan → fix → verify → report)
3. **Token burn measured:** Router + specialists consistently >3,000 tokens per use
4. **Frequency justifies:** Weekly or more frequent usage
5. **Capability gap exists:** Work requires something no specialist provides

Agent keeps router when:

1. **Stable domain:** Accounting, WA state systems, MVVM patterns have mature specialist coverage
2. **Discovery value high:** Users need "which skill do I use for X?" routing
3. **Low frequency:** Monthly or less frequent usage
4. **Specialists sufficient:** All workflow needs covered by existing skills

## Anti-Patterns

| Anti-Pattern | Fix |
|--------------|-----|
| Controller with only 1 mode | Use specialist skill instead |
| Controller that only routes (no direct execution) | Use router bundle instead |
| Router with >10 routes and complex decision trees | Split into multiple focused routers or create controller |
| Controller with >4,000 token main file | Move detail to `references/` |
| Converting stable domain routers (accounting, MVVM) | Keep as router; discovery is the value |

## Verification Matrix

| Check | Test | Pass |
|-------|------|------|
| Pattern selection | Compare workflow to decision matrix | One pattern clearly wins |
| Token efficiency | Calculate router vs controller cost | Controller saves tokens OR provides new capability |
| Mode completeness | Review mode activation table | All user intents map to modes |
| Direct execution | Check workflow steps | Controller executes at least one mode directly |
| Specialist delegation | Verify delegation logic | Controller delegates appropriately for complex specialist work |

## Example Conversions

| Old Bundle | New Controller | Justification |
|------------|----------------|---------------|
| `cwe-fixes-bundle` | `security-controller` | Cross-cutting SAST, compliance reporting, 32% token savings |
| `mstest-fixes-bundle` | `test-modernization-controller` | Already executed workflow, formalized modes, added v2→v4 migration |
| `webapi-fixes-bundle` | `webapi-hardening-controller` | Multi-track orchestration, added endpoint audit + OpenAPI generation |

| Preserved Router | Justification |
|------------------|---------------|
| `accounting-systems-bundle` | Stable domain, mature specialists, discovery value high |
| `wa-state-systems-bundle` | State regulations stable, routing IS the value |
| `mvvm-toolkit-bundle` | MVVM patterns stable, routing to platform specialists correct |

## Common Mistakes

| Mistake | Agent Fix |
|---------|-----------|
| Creating controller for one-time workflow | Agent creates specialist skill instead |
| Converting stable domain router "because controllers are new" | Agent preserves router, documents why |
| Adding modes without direct execution | Agent either adds execution logic or converts to router |
| Token estimate ignores reference files | Agent includes reference token counts in total overhead calculation |
| Naming pattern inconsistent | Agent uses `*-controller` suffix for controllers, `*-bundle` for routers |
