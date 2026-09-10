---
title: "Architecture Decision Records"
doc_type: "reference"
status: "active"
last_updated: "2026-09-10"
summary: "Overview of the repository ADR process, navigation, and decision catalog."
owner: "Platform/IDL"
tags:
  - adr
  - architecture
  - documentation
  - decision-records
target_audience: "both"
---

# Architecture Decision Records

This folder captures the significant architectural decisions that shape the repository.

## Purpose

The ADR set records the context, decision, trade-offs, and consequences behind important technical choices so future contributors understand why the system is designed as it is.

## ADR lifecycle

- Proposed: under active discussion
- Accepted: in effect and considered the current direction
- Superseded: replaced by a newer decision
- Deprecated: retained for historical reference only

## ADR navigation

- [Index](./index.md)
- [Table of contents](./toc.md)
- [Template](./adr-template.md)

## Current ADRs

| ADR | Title | Status | Date |
| --- | --- | --- | --- |
| [ADR-0001](./adr-0001.md) | Establish Solution Architect Radar and Best Practice Capsules | Accepted | 2025-10-04 |
| [ADR-0002](./adr-0002.md) | Adopt GitOps for CI/CD | Accepted | 2025-10-04 |
| [ADR-0003](./adr-0003.md) | XML Documentation Generation and Unit Testing Strategy | Accepted | 2025-10-16 |
| [ADR-0004](./adr-0004.md) | Modular Copilot Instruction Architecture | Accepted | 2025-11-14 |
| [ADR-0005](./adr-0005.md) | Add `IN` Membership Operator to Filtering Model and EF Core Translation | Accepted | 2025-11-18 |
| [ADR-0006](./adr-0006.md) | Standardize ADR structure and navigation | Accepted | 2026-09-09 |
| [ADR-0007](./adr-0007.md) | Versioning strategy | Accepted | 2026-09-10 |

## How to add a new ADR

1. Copy [adr-template.md](./adr-template.md) to a new file named `adr-XXXX.md`.
2. Capture the decision context and alternatives.
3. Add the ADR to [index.md](./index.md).
4. Update [toc.md](./toc.md) so the new record is discoverable.

## References

- [Architecture Decision Records (Joel Parker Henderson)](https://github.com/joelparkerhenderson/architecture_decision_record)
- [ThoughtWorks Tech Radar](https://www.thoughtworks.com/radar)
