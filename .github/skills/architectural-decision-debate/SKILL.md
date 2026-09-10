---
name: architectural-decision-debate
title: Architectural Decision Debate
description: Debate architectural and design decisions by presenting three positions (adopt, reject, defer) with at least 3 options each, challenging assumptions and existing patterns.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 3200
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - antipattern-detection-bundle
  - technology-selection
  - dotnet-architectural-layers
  - vbd-system-design
related_docs:
  - https://en.wikipedia.org/wiki/List_of_software_development_philosophies
  - https://learn.microsoft.com/en-us/azure/architecture/guide/
appliesTo: '**/*.{md,cs,csproj}'
tags:
  - architecture
  - debate
  - decision
  - trade-offs
  - philosophy
---

# Architectural Decision Debate

Agent uses this skill when developer asks about style, approach, pattern, or technology choice. Agent presents three debate positions—adopt, reject, defer—with at least 3 options per position, challenges assumptions, and examines trade-offs.

## When to Use

| Condition | Use |
|---|---|
| Developer asks "should we use X?" | Use this skill |
| Team debates architectural approach | Use this skill |
| Design pattern selection needed | Use this skill |
| Technology or framework choice required | Use this skill |
| Refactoring targets established pattern | Use this skill |
| Requirements expand beyond current architecture | Use this skill |

## When Not to Use

| Condition | Alternative |
|---|---|
| Anti-pattern already detected | Use `antipattern-detection-bundle` |
| Specific technology mandate exists | Use `technology-selection` for validation |
| No decision needed | Direct implementation guidance |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Decision topic | Yes | Pattern, technology, approach, or style |
| Current context | Yes | Existing architecture, constraints, requirements |
| Expansion or change driver | No | New requirements or growth pressures |

## Debate Framework

Agent presents three positions for every architectural decision:

### Position 1: Adopt (Use It)

Agent provides at least 3 distinct options for **why and how** to adopt the proposal.

**Structure:**
1. **Option A:** First adoption approach with rationale
2. **Option B:** Second adoption approach with different emphasis
3. **Option C:** Third adoption approach with alternative scope or timing

**Considerations:**
- Performance benefits
- Scalability improvements
- Maintainability gains
- Team skill alignment
- Industry standard adoption
- Ecosystem maturity

### Position 2: Reject (Don't Use It)

Agent provides at least 3 distinct options for **why and how** to reject the proposal.

**Structure:**
1. **Option A:** First rejection rationale with alternative
2. **Option B:** Second rejection rationale with different risk
3. **Option C:** Third rejection rationale with cost or complexity focus

**Considerations:**
- Complexity cost
- Team learning curve
- Maintenance burden
- Over-engineering risk
- Alternative simpler approaches
- Technical debt introduction

### Position 3: Defer (Make No Changes / Wait)

Agent provides at least 3 distinct options for **when and why** to defer the decision.

**Structure:**
1. **Option A:** First deferral scenario (timing-based)
2. **Option B:** Second deferral scenario (information-based)
3. **Option C:** Third deferral scenario (risk-based)

**Considerations:**
- Current solution adequacy
- Missing information or data
- Team capacity constraints
- Risk of premature optimization
- Uncertainty about future requirements
- Cost of reverting decision

## Challenge Framework

Agent challenges assumptions in each position:

### Assumption Challenges

**Current State:**
- "Is the existing pattern actually a problem?" (performance data, error rates, maintenance cost)
- "Are current constraints permanent or temporary?" (team size, budget, timeline)
- "Is the current architecture truly blocking expansion?" (evidence, metrics)

**Proposal:**
- "Does this solve the real problem or a perceived one?" (root cause analysis)
- "Are benefits measured or assumed?" (benchmarks, studies, references)
- "What breaks if we're wrong?" (reversal cost, blast radius)

**Requirements:**
- "Are expanding requirements stable?" (volatility, clarity, stakeholder alignment)
- "Do new requirements justify architectural change?" (incremental vs fundamental)
- "Can existing architecture absorb expansion with smaller changes?" (adapter, extension)

## Workflow

1. Agent clarifies decision topic and current context.
2. Agent identifies assumptions in proposal and current state.
3. Agent generates 3 options for Adopt position.
4. Agent generates 3 options for Reject position.
5. Agent generates 3 options for Defer position.
6. Agent challenges key assumptions across all positions.
7. Agent summarizes trade-offs and recommends decision-making criteria.

Test: Review all three positions and challenge questions.
Pass: Developer has clear view of trade-offs and can make informed decision.

## Output Format

Agent structures debate as follows:

```
Architectural Decision Debate
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Topic: [Decision Topic]
Context: [Current Architecture Summary]
Driver: [Change or Expansion Trigger]

━━━ Position 1: ADOPT ━━━

Option A: [First Adoption Approach]
  Rationale: [Why this works]
  Benefits: [Performance, scalability, maintainability]
  Risks: [What could go wrong]
  Effort: [Implementation cost]

Option B: [Second Adoption Approach]
  Rationale: [Alternative why]
  Benefits: [Different emphasis]
  Risks: [Different risks]
  Effort: [Different cost]

Option C: [Third Adoption Approach]
  Rationale: [Another angle]
  Benefits: [Yet another benefit set]
  Risks: [Yet another risk set]
  Effort: [Yet another cost]

━━━ Position 2: REJECT ━━━

Option A: [First Rejection Rationale]
  Alternative: [What to do instead]
  Why: [Core objection]
  Risk Avoided: [What bad outcome we prevent]

Option B: [Second Rejection Rationale]
  Alternative: [Different alternative]
  Why: [Different objection]
  Risk Avoided: [Different bad outcome]

Option C: [Third Rejection Rationale]
  Alternative: [Yet another alternative]
  Why: [Yet another objection]
  Risk Avoided: [Yet another bad outcome]

━━━ Position 3: DEFER ━━━

Option A: [First Deferral Scenario]
  When: [Trigger for revisiting]
  Why Wait: [What we gain by waiting]
  Interim Action: [What to do meanwhile]

Option B: [Second Deferral Scenario]
  When: [Different trigger]
  Why Wait: [Different gain]
  Interim Action: [Different meanwhile action]

Option C: [Third Deferral Scenario]
  When: [Yet another trigger]
  Why Wait: [Yet another gain]
  Interim Action: [Yet another meanwhile action]

━━━ ASSUMPTION CHALLENGES ━━━

Current State:
  ❓ [Challenge assumption about current architecture]
  ❓ [Challenge assumption about current constraints]
  ❓ [Challenge assumption about current pain points]

Proposal:
  ❓ [Challenge assumption about proposal benefits]
  ❓ [Challenge assumption about proposal cost]
  ❓ [Challenge assumption about proposal risks]

Requirements:
  ❓ [Challenge assumption about expanding requirements]
  ❓ [Challenge assumption about requirement stability]
  ❓ [Challenge assumption about necessity of change]

━━━ DECISION CRITERIA ━━━

Recommend deciding based on:
1. [First decision factor] (measurement, threshold, or condition)
2. [Second decision factor]
3. [Third decision factor]

If [condition], lean toward ADOPT.
If [condition], lean toward REJECT.
If [condition], lean toward DEFER.
```

## Design Philosophy Integration

Agent references relevant design philosophies and principles:

### Common Philosophies

- **YAGNI (You Aren't Gonna Need It)** — Defer complexity until proven necessary
- **KISS (Keep It Simple, Stupid)** — Favor simplest solution that works
- **DRY (Don't Repeat Yourself)** — Eliminate duplication
- **SOLID Principles** — Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **12-Factor App** — Cloud-native application patterns
- **CAP Theorem** — Consistency, Availability, Partition Tolerance trade-offs
- **Conway's Law** — System design reflects organizational structure
- **The Innovator's Dilemma** — Disruptive vs sustaining innovation trade-offs
- **Monolith First** — Start simple, extract services when boundaries clear

**Reference:** [List of Software Development Philosophies](https://en.wikipedia.org/wiki/List_of_software_development_philosophies)

### Architectural Pattern Philosophy

**Monolith → Modular Monolith → Deployment Domains → Microservices**

Each evolution adds complexity. Agent recommends moving right only when:
- Current pattern pain exceeds next pattern complexity cost
- Team has operational maturity for increased complexity
- Boundaries are stable and well-understood
- Metrics show tipping point reached

See `architectural-pattern-tipping-point` skill for detailed tipping point analysis.

## Example Debates

### Example 1: "Should we use microservices?"

**Context:** Modular monolith, 22 developers across 4 teams, 180K LOC

**Adopt Options:**
- A: Extract 2-3 high-value bounded contexts (Accounting, Payroll) as services; keep shared infrastructure monolithic
- B: Build strangler façade around existing monolith; migrate one domain at a time over 12 months
- C: New features as services; leave stable legacy monolith unchanged; hybrid architecture

**Reject Options:**
- A: Stay modular monolith; add deployment domains (independent deploys, shared database); lower complexity
- B: Refactor to cleaner module boundaries; use feature flags for team autonomy; avoid distributed system cost
- C: Wait until team >30 developers and operational excellence for distributed systems proven

**Defer Options:**
- A: When deployment coupling causes >5 production incidents/month (currently: 3/month)
- B: After measuring actual pain points for 6 months (deploy coordination time, merge conflicts, blast radius)
- C: When database contention becomes measurable bottleneck (currently: connection pool <50% saturated)

**Assumption Challenges:**
- ❓ Is deployment coupling the real problem or symptom? (Evidence: deploy coordination 6 hours, merge conflicts 28%)
- ❓ Do benefits (team autonomy, independent scaling) outweigh costs (operational complexity, distributed transactions)?
- ❓ Are service boundaries stable? (Evidence: bounded contexts unchanged for 8 months = stable)
- ❓ Does team have distributed systems expertise? (Monitoring, tracing, circuit breakers, saga patterns)

**Decision Criteria:**
- If deploy coordination >8 hours AND team has distributed ops expertise → lean ADOPT
- If operational maturity lacking OR boundaries unstable → lean REJECT (use deployment domains instead)
- If pain points unclear OR team capacity constrained → lean DEFER

### Example 2: "Should we use GraphQL instead of REST?"

**Adopt Options:**
- A: Use for mobile apps only, keep REST for server-to-server
- B: Add GraphQL layer over existing REST services
- C: New API surface only, migrate high-traffic endpoints later

**Reject Options:**
- A: Use REST with field projection (sparse fieldsets) and HATEOAS
- B: Use OData for flexible querying over REST
- C: Keep REST, add client-specific BFF (Backend for Frontend) services

**Defer Options:**
- A: When N+1 query problem causes measured performance issues
- B: After team completes GraphQL training and POC
- C: When mobile app requirements stabilize and need is clear

## Verification Checklist

Agent verifies:
- [ ] Three positions presented (Adopt, Reject, Defer)
- [ ] At least 3 options per position
- [ ] Assumptions challenged across current state, proposal, and requirements
- [ ] Trade-offs explicitly stated
- [ ] Decision criteria provided
- [ ] Relevant design philosophies referenced
- [ ] Evidence or metrics cited where available
