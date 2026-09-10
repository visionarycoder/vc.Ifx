---
title: "ADR Index"
doc_type: "reference"
status: "active"
last_updated: "2026-09-10"
summary: "Catalog of architecture decision records for the repository."
owner: "Platform/IDL"
tags:
  - adr
  - architecture
  - documentation
  - decision-records
target_audience: "both"
---

# Architecture Decision Records (ADR) Index

This index provides a chronological overview of all ADRs in this repository.
Each ADR captures a significant architectural decision, its context, and consequences.
ADRs are immutable: once accepted, they remain as historical records. If a decision changes, a new ADR supersedes the old one.

## Navigation

- [README](./README.md)
- [Table of contents](./toc.md)
- [Template](./adr-template.md)

---

## ADRs

| ADR ID | Title | Status | Date | Supersedes |
| --- | --- | --- | --- | --- |
| [ADR-0001](./adr-0001.md) | Establish Solution Architect Radar and Best Practice Capsules | Accepted | 2025-10-04 | – |
| [ADR-0002](./adr-0002.md) | Adopt GitOps for CI/CD | Accepted | 2025-10-04 | – |
| [ADR-0003](./adr-0003.md) | XML Documentation Generation and Unit Testing Strategy | Accepted | 2025-10-16 | – |
| [ADR-0004](./adr-0004.md) | Modular Copilot Instruction Architecture | Accepted | 2025-11-14 | – |
| [ADR-0005](./adr-0005.md) | Add `IN` Membership Operator to Filtering Model and EF Core Translation | Accepted | 2025-11-18 | – |
| [ADR-0006](./adr-0006.md) | Standardize ADR structure and navigation | Accepted | 2026-09-09 | – |
| [ADR-0007](./adr-0007.md) | Versioning strategy | Accepted | 2026-09-10 | – |

---

## Status legend

- Proposed → under discussion, not yet accepted.
- Accepted → decision is in effect.
- Superseded → replaced by a newer ADR.
- Deprecated → no longer relevant, but retained for historical context.

---

## How to add a new ADR

1. Copy the [ADR template](./adr-template.md) into a new file named `adr-XXXX.md`.
2. Fill in the details (context, decision, consequences, and alternatives).
3. Update this index with the new ADR entry.
4. If the new ADR supersedes an old one, update the Supersedes column.

---

## References

- [Architecture Decision Records (Joel Parker Henderson)](https://github.com/joelparkerhenderson/architecture_decision_record)
- [ThoughtWorks Tech Radar](https://www.thoughtworks.com/radar)
