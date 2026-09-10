---
name: architectural-pattern-tipping-point
title: Architectural Pattern Tipping Point Analysis
description: Analyze existing codebase metrics to determine when architectural patterns (monolith, modular monolith, deployment domains, microservices) reach tipping points requiring change.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 3500
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - architectural-decision-debate
  - antipattern-detection-bundle
  - design-smell-detection
  - dotnet-architectural-layers
related_docs:
  - https://learn.microsoft.com/en-us/azure/architecture/guide/architecture-styles/
  - https://martinfowler.com/bliki/MonolithFirst.html
appliesTo: '**/*.{cs,csproj,sln,slnx,md}'
tags:
  - architecture
  - monolith
  - microservices
  - modular-monolith
  - tipping-point
  - metrics
---

# Architectural Pattern Tipping Point Analysis

Agent analyzes existing codebase metrics, team dynamics, and operational data to determine when current architectural pattern reaches tipping point requiring evolution to different pattern.

## When to Use

| Condition | Use |
|---|---|
| Deployment or scaling pain points exist | Use this skill |
| Team coordination friction increases | Use this skill |
| Performance degradation under growth | Use this skill |
| Evaluating architecture evolution path | Use this skill |
| Periodic architecture health assessment | Use this skill |

## When Not to Use

| Condition | Alternative |
|---|---|
| New greenfield project (no metrics yet) | Use `architectural-decision-debate` |
| Specific anti-pattern detected | Use `antipattern-detection-bundle` |

## Architectural Pattern Spectrum

Agent evaluates position on the architectural pattern spectrum:

### Pattern Definitions

| Pattern | Characteristics | Deployment Unit | Scaling Unit |
|---|---|---|---|
| **Monolith** | Single codebase, single deployment, shared data | Entire application | Entire application |
| **Modular Monolith** | Bounded contexts within monolith, internal module boundaries | Entire application | Entire application |
| **Deployment Domains** | Modules deployable independently, optional shared database | Module or domain | Module or domain |
| **Microservices** | Fully independent services, separate databases | Individual service | Individual service |

### Pattern Progression

```
Monolith
  ↓ (add internal boundaries)
Modular Monolith
  ↓ (add independent deployment)
Deployment Domains
  ↓ (add data independence)
Microservices
```

**Key Insight:** Each step adds complexity. Move only when current pattern's pain exceeds next pattern's complexity cost.

## Tipping Point Metrics Framework

Agent collects these metrics to determine architectural tipping points:

### 1. Deployment Metrics

| Metric | Monolith OK | Tipping Point | Action |
|---|---|---|---|
| **Deployment frequency** | Daily or better | <1/week due to coupling | Consider modular monolith |
| **Deployment duration** | <15 minutes | >30 minutes | Investigate build/test optimization first |
| **Deployment blast radius** | Acceptable risk | >3 incidents/month | Add module boundaries or deployment domains |
| **Rollback frequency** | <5% of deployments | >15% of deployments | Improve testing or reduce deployment scope |
| **Deploy coordination time** | <1 hour | >4 hours (multiple teams) | Consider deployment domains |

### 2. Team Metrics

| Metric | Monolith OK | Tipping Point | Action |
|---|---|---|---|
| **Team size** | <15 developers | >20 developers | Modular monolith with clear ownership |
| **Team count** | 1-2 teams | >3 teams with coordination friction | Deployment domains or microservices |
| **Merge conflict frequency** | <10% of PRs | >25% of PRs | Better module boundaries or code ownership |
| **PR review time** | <4 hours | >1 day (cross-team dependencies) | Reduce cross-module coupling |
| **Onboarding time** | <2 weeks | >1 month | Simplify or better modularization |

### 3. Codebase Metrics

| Metric | Monolith OK | Tipping Point | Action |
|---|---|---|---|
| **Lines of code** | <100K LOC | >250K LOC with poor modularity | Extract modules or services |
| **Project count** | <20 projects | >50 projects in single solution | Consider deployment domains |
| **Cyclomatic complexity (avg)** | <10 | >15 | Refactor before architectural change |
| **Circular dependencies** | 0 | Any | Fix before considering split |
| **Coupling metrics (Ce/Ca)** | Balanced | High efferent coupling in core | Invert dependencies or extract |

### 4. Runtime Metrics

| Metric | Monolith OK | Tipping Point | Action |
|---|---|---|---|
| **Startup time** | <30 seconds | >2 minutes | Lazy loading or split deployment |
| **Memory footprint** | <2 GB | >8 GB on smallest instance | Profile and optimize or split |
| **Scale-out efficiency** | Linear to 4-8 instances | Sublinear or resource contention | Identify bottleneck; consider split |
| **Database connection pool** | <50% saturation | >80% saturation | Connection optimization or data split |
| **Request latency P95** | <500ms | >2s under normal load | Performance optimization first |

### 5. Operational Metrics

| Metric | Monolith OK | Tipping Point | Action |
|---|---|---|---|
| **Incident blast radius** | Isolated to feature | Entire application down | Add fault isolation boundaries |
| **Mean Time to Recovery** | <1 hour | >4 hours | Improve observability or reduce scope |
| **On-call pages/month** | <10 | >30 | Identify root causes; consider isolation |
| **Production incidents** | <5/month | >15/month | Stability work or fault boundaries |

## Tipping Point Analysis Workflow

1. Agent collects metrics across all five dimensions.
2. Agent identifies metrics exceeding tipping point thresholds.
3. Agent determines root cause (complexity, coupling, team, or scale).
4. Agent evaluates whether current pattern can absorb issue with targeted fixes.
5. Agent recommends: (a) stay in current pattern with fixes, (b) evolve to next pattern, or (c) defer decision pending data.
6. Agent estimates migration effort and risk for evolution recommendation.

Test: Validate metrics collection is accurate and complete.
Pass: All five metric dimensions measured. Tipping points identified with evidence.

## Decision Matrix: When to Evolve

### From Monolith → Modular Monolith

**Trigger Signals:**
- Team size >10 developers
- Merge conflicts >15% of PRs
- Code ownership unclear
- LOC >100K with poor organization

**Stay Monolith When:**
- Team <10 developers
- Single product owner
- Deployment frequency acceptable (>weekly)
- Low operational complexity tolerance

**Evolve to Modular Monolith When:**
- Team growth planned
- Clear bounded contexts identified
- Internal module boundaries will improve clarity
- Deployment remains single unit (acceptable)

**Effort:** Medium (2-6 months for refactoring, no deployment changes)

### From Modular Monolith → Deployment Domains

**Trigger Signals:**
- Team count >3 teams
- Deploy coordination >4 hours
- Different modules have different deployment cadences needed
- Deployment duration >30 minutes

**Stay Modular Monolith When:**
- Single team or coordinated release acceptable
- Deployment complexity cost exceeds coordination pain
- Database transaction boundaries span modules
- Operational maturity for distributed systems lacking

**Evolve to Deployment Domains When:**
- Independent deployment frequency needed per module
- Teams want deployment autonomy
- Blast radius reduction critical
- Willing to accept distributed system complexity

**Effort:** High (6-12 months, requires deployment pipeline per domain, operational maturity)

### From Deployment Domains → Microservices

**Trigger Signals:**
- Database contention across domains
- Different domains need independent scaling characteristics
- Polyglot persistence or technology needs
- Regulatory or compliance isolation required

**Stay Deployment Domains When:**
- Shared database acceptable
- Consistent technology stack preferred
- Distributed transaction needs complex
- Operational complexity already high

**Evolve to Microservices When:**
- Data independence critical
- Independent scaling per service needed
- Clear service boundaries exist
- Team has distributed systems expertise

**Effort:** Very High (12-24+ months, requires data migration, distributed transactions, operational excellence)

## Reasons NOT to Apply Each Pattern

Agent always considers reasons to reject pattern evolution:

### Reasons NOT to Use Monolith

- ❌ Team >20 developers with coordination friction
- ❌ Different modules have vastly different scaling needs
- ❌ Deployment coupling causes >3 production incidents/month
- ❌ Single failure brings down entire system unacceptably
- ❌ Technology diversity needed (polyglot requirements)

### Reasons NOT to Use Modular Monolith

- ❌ Teams require deployment independence (different cadences)
- ❌ Regulatory isolation between modules required
- ❌ Database contention between modules unresolvable
- ❌ Module boundaries unclear or unstable
- ❌ Team lacks discipline to maintain boundaries

### Reasons NOT to Use Deployment Domains

- ❌ Database transactions span domain boundaries frequently
- ❌ Operational maturity for distributed deployment lacking
- ❌ Domain boundaries unclear or frequently changing
- ❌ Data consistency requirements span domains
- ❌ Deployment pipeline complexity exceeds team capacity

### Reasons NOT to Use Microservices

- ❌ Team <15 developers (insufficient capacity for operational overhead)
- ❌ Service boundaries unclear or frequently changing
- ❌ Distributed transactions required frequently
- ❌ Operational excellence for distributed systems lacking (monitoring, tracing, resilience)
- ❌ Network latency unacceptable for inter-service calls
- ❌ Data duplication and eventual consistency unacceptable
- ❌ Complexity cost exceeds coordination pain

## Code Analysis for Tipping Point Detection

Agent analyzes codebase for architectural pattern readiness.

See [references/code-analysis-patterns.md](references/code-analysis-patterns.md) for detailed code analysis examples.

## Verification Checklist

Agent verifies:
- [ ] All 5 metric dimensions collected
- [ ] Tipping points identified with thresholds
- [ ] Root cause analysis performed
- [ ] Multiple options provided (evolve, stay, defer)
- [ ] Effort and risk estimates included
- [ ] Reasons NOT to evolve considered
- [ ] Anti-pattern warnings included
- [ ] Decision criteria clear
