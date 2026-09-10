---
title: Quarterly Radar & Capsules Review
doc_type: report
status: active
last_updated: 2026-09-10
---

# Quarterly Radar & Capsules Review

## Purpose

Ensure the Solution Architect Radar and Best Practice Capsules remain accurate, actionable, and aligned with current strategy.

## Cadence

- **Schedule:** First week of each quarter (Q1–Q4).
- **Duration:** 90 minutes for the review + 2 hours for follow-ups.

## Roles

- **Facilitator:** Solution Architect (owner of the playbook).
- **Contributors:** Leads for Security, Cloud, DevOps, Data, Integration, Observability.
- **Recorder:** Notes decisions and actions; opens PRs/ADRs.

## Inputs

- **Evidence:** Incident trends, performance reports, cost dashboards (FinOps), compliance findings.
- **Benchmarks:** Internal KPIs, SLIs/SLOs, deployment velocity, MTTR, change fail rate.
- **Roadmaps:** Product and platform roadmaps; cloud provider updates.

## Agenda

1. **Status check:** Review last quarter’s changes and outcomes.
2. **Maturity scan:** Validate Adopt/Trial/Assess/Hold per specialty.
3. **Capsule updates:** Adjust principles, patterns, anti-patterns, tooling.
4. **Decisions:** Identify items requiring ADRs; assign owners.
5. **Backlog:** Create issues for deferred improvements.

## Review heuristics

- **Adopt:** Is it default practice in critical paths? Evidence shows stability and value.
- **Trial:** Is a time-boxed pilot active with clear success criteria?
- **Assess:** Do we have a research note and a decision deadline?
- **Hold:** Are risks documented with examples of harm or overhead?

## Actions

- **Radar update:** Edit best-practices/radar.md (quadrant lists + Mermaid diagram).
- **Capsule edits:** Update READMEs (principles, tooling, patterns) with examples.
- **ADRs:** Draft new ADRs (ADR-XXXX) for significant shifts; link from radar and capsules.
- **Issues:** Open GitHub issues for tasks; tag with `radar`, `capsule`, `adr`.

## Outputs

- **Updated radar:** Adopt/Trial/Assess/Hold refined.
- **Revised capsules:** Concrete guidance kept fresh.
- **ADR records:** Decisions captured and linked.
- **Change log:** Commit messages referencing ADR IDs.

## Automation

- **ADR index:** GitHub Action regenerates docs/adr/index.md.
- **Lint & links:** Validate internal links to capsules and ADRs in CI.
- **Docs build:** MkDocs preview for PRs (if enabled).

## Governance

- **Approval:** Changes to radar require at least two specialty leads’ review.
- **Traceability:** Radar bullets link to capsules; capsules link to ADRs when decisions apply.
- **Versioning:** Keep quarterly tags (e.g., `radar-2025-Q4`) for historical comparison.

## Template PR checklist

- **Scope:** Which specialties changed?
- **Evidence:** What data supports the change?
- **Docs:** Radar updated; capsules updated; ADR created or referenced.
- **CI:** ADR index updated; docs build passes; links validated.

## Diagrams

```Mermaid
timeline
    title Quarterly Architecture Governance Cycle

    section Q1
      January : Kickoff review meeting
      February : Radar & capsule updates
      March : ADR drafting & approvals

    section Q2
      April : Radar validation
      May : Capsule refresh
      June : Tag quarterly snapshot (radar-2025-Q2)

    section Q3
      July : Mid-year review
      August : ADR consolidation
      September : Radar + capsule sync

    section Q4
      October : Annual strategy alignment
      November : Final capsule updates
      December : Tag yearly snapshot (radar-2025-Q4) + publish changelog
```

---
## Related Visuals

- [Branching Strategy Diagram](branching-strategy.md#branching-strategy-playbook)
- [Radar Quadrants](../best-practices/radar.md#visual-radar-mermaid)
