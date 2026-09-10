---
title: System Map: Architecture Governance Artifacts
doc_type: reference
status: active
last_updated: 2026-09-10
---

# System Map: Architecture Governance Artifacts

This document provides a **meta‑level view** of how our architecture governance artifacts interconnect.  
It shows how **decisions flow into practices**, how practices roll up into the **radar**, and how governance cycles (branching + quarterly reviews) keep everything alive.

---

## Visual System Map

```mermaid
flowchart TD

    subgraph Decisions
      ADRs[Architecture Decision Records]
    end

    subgraph Practices
      Capsules[Best Practice Capsules]
      Radar[Solution Architect Radar]
    end

    subgraph Governance
      Branching[Branching Strategy Playbook]
      Quarterly[Quarterly Review Checklist]
      Timeline[Quarterly Timeline Diagram]
    end

    %% Relationships
    ADRs --> Capsules
    Capsules --> Radar
    Radar --> Quarterly
    Quarterly --> Timeline
    Branching --> Quarterly
    Branching --> Radar
    Quarterly --> ADRs

    %% Styling
    classDef decision fill=#ffe0b2,stroke=#f90,stroke-width=1px,color=#000;
    classDef practice fill=#c8e6c9,stroke=#2e7d32,stroke-width=1px,color=#000;
    classDef governance fill=#bbdefb,stroke=#1565c0,stroke-width=1px,color=#000;

    class ADRs decision;
    class Capsules,Radar practice;
    class Branching,Quarterly,Timeline governance;
