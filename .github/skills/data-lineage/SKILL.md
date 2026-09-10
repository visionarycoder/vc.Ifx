---
name: data-lineage
title: Data Lineage
description: Map upstream and downstream lineage paths and generate lineage artifacts with impact analysis.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 954
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - data-contract-frontmatter
  - data-quality-reporting
appliesTo: '**/*.{json,yml,yaml,csv,md}'
tags:
  - data
  - lineage
  - impact
  - governance
---
# Data Lineage

Agent maps source, transform, and sink relationships. Agent generates diagram, graph, and impact summaries without exposing secrets.

## When to Use

| User prompt | Use |
|---|---|
| User asks for source-to-target mapping | Use this skill |
| User asks for downstream impact of a table or column change | Use this skill |
| User uploads pipeline or notebook metadata for lineage | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for contract authoring only | Use `data-contract-frontmatter` |
| User asks for data rule scoring only | Use `data-quality-reporting` |
| User asks for file fix or parse diagnostics | Use an ingestion skill |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Lineage source | Yes | Pipeline JSON, notebook logic, SQL, or mapping table |
| Target scope | Yes | Table, column, report, or full lineage scope |
| Environment names | No | Workspace, lakehouse, model, or report names |
| Output format | No | Mermaid, JSON, Markdown, or all |

## Decision Table

| Condition | Agent step |
|---|---|
| Column mapping exists | Agent generates column lineage |
| Column mapping missing | Agent generates table lineage only |
| Cycles appear | Agent reports cycle path and flags graph risk |
| User asks for impact | Agent reads descendants and ancestors |
| Contract file exists | Agent aligns lineage terms with contract fields |

## Node Type Table

| Node type | Meaning |
|---|---|
| Source | External origin or raw file |
| Transform | Pipeline, notebook, SQL, or rule step |
| Sink | Table, model, report, or API target |

## Workflow

| Step | Agent step | Test | Pass |
|---|---|---|---|
| 1 | Agent reads lineage inputs and scope. | Read lineage definition or mapping table. | At least one source and one sink exist. |
| 2 | Agent verifies nodes and edges. | Count unique nodes and edges. | Each edge has one source and one sink. |
| 3 | Agent verifies graph structure. | Run cycle scan on edge list. | Cycle count equals zero or report lists each cycle. |
| 4 | Agent verifies optional column lineage. | Read mapped source and target columns. | Each mapped target column has at least one source column. |
| 5 | Agent verifies impact scope. | Traverse upstream and downstream nodes. | Impact list includes direct and indirect dependents. |
| 6 | Agent generates artifacts. | Read Mermaid or JSON output. | Output names match graph nodes exactly. |

## Impact Levels

| Level | Agent verifies | Pass |
|---|---|---|
| High | Changed node feeds report or semantic model | High-impact nodes are listed |
| Medium | Changed node feeds curated table or API | Medium-impact nodes are listed |
| Low | Changed node feeds staging scope only | Low-impact nodes are listed |

## Outputs

| Output | Format |
|---|---|
| Lineage summary | Markdown |
| Diagram source | Mermaid |
| Graph data | JSON |
| Impact table | Markdown or CSV |

## Verification Checklist

- [ ] Agent reads one authoritative lineage input.
- [ ] Agent reports node and edge counts.
- [ ] Agent reports cycles when present.
- [ ] Agent separates table lineage from column lineage.
- [ ] Agent reports direct and indirect impact.
