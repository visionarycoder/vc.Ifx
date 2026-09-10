---
name: terminology-normalization
title: Terminology Normalization
description: Normalize terms, synonyms, abbreviations, and cross-system labels when a prompt needs one canonical vocabulary across data, code, analytics, and documentation.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1655
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - business-glossary-metadata
  - naming-standards
  - ontology-design-patterns
  - semantic-layer-fabric
appliesTo: '**/*.{md,json,csv,yml,yaml,sql,dax,cs,ts}'
tags:
  - terminology
  - normalization
  - synonym
  - naming
  - governance
---
# Terminology Normalization

Agent aligns synonyms, abbreviations, acronyms, and legacy labels into one canonical terminology set.

## When to Use

| Condition | Use |
|---|---|
| Prompt needs one canonical term across multiple systems or teams | Use this skill |
| Prompt needs synonym, acronym, or abbreviation mapping | Use this skill |
| Prompt needs report, API, warehouse, and code labels aligned | Use this skill |
| Prompt needs crosswalks between old and new terminology versions | Use this skill |
| Prompt needs search terms and aliases preserved during renames | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Prompt needs ownership, stewardship, or glossary publication flow | Use `business-glossary-metadata` |
| Prompt needs classes, properties, and semantic reasoning rules | Use `ontology-design-patterns` |
| Prompt needs casing and code-style conventions only | Use `naming-standards` |
| Prompt needs Fabric model storage or DAX design | Use `semantic-layer-fabric` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Source term set | Yes | Agent records every observed variant and source location. |
| Canonical scope | Yes | Agent records business, data, code, analytics, or enterprise scope. |
| Conflict rules | Yes | Agent records tie-breakers for preferred wording. |
| Change history | No | Agent records prior labels and redirect needs. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent inventories variants, acronyms, abbreviations, and near-duplicates. | Review intake inventory. | Each observed label appears once in the source set. |
| 2 | Agent groups variants by concept and selects one canonical term per concept. | Review canonical map. | Each concept has one preferred label. |
| 3 | Agent classifies every non-canonical term as synonym, abbreviation, legacy label, or rejected term. | Review variant taxonomy. | Every alternate label has one disposition. |
| 4 | Agent maps canonical terms across schemas, reports, APIs, and semantic models. | Review cross-system crosswalk. | Systems resolve back to the same canonical vocabulary. |
| 5 | Agent selects rollout handling from the decision matrix. | Compare rename scope to matrix rows. | Rename plan preserves traceability and searchability. |
| 6 | Agent verifies sampled assets and search paths after normalization. | Review asset sample. | Sampled labels match the approved vocabulary. |

## Decision Matrix

| Conflict Shape | Preferred Rule | Example |
|---|---|---|
| Full synonym set | Keep one business-preferred label | `Customer` over `Client` |
| Abbreviation and long form | Keep full term as canonical and map abbreviation as alias | `Purchase Order` with alias `PO` |
| Vendor jargon and domain term | Keep domain term as canonical | `Employee` over vendor label `Assoc` |
| Overloaded acronym | Add domain qualifier or reject the short form | `AR (Accounting)` vs `AR (Accounts Receivable)` |
| Versioned rename | Keep new canonical label and preserve legacy redirect | `Site -> Location` |
| Ontology label and report caption mismatch | Keep the governed term and map local captions | `Fund` mapped to `saam:Fund` |

## Pattern Catalog

| Pattern | Use | Example |
|---|---|---|
| Canonical mapping row | One concept with many variants | `canonical: Customer; aliases: Client, Account Holder` |
| Acronym register | Controlled short forms | `RLS -> Row-Level Security` |
| Rejected-term list | Blocks ambiguous or retired words | `User` rejected where employee and customer diverge |
| Domain qualifier | Separates overloaded labels | `Account (Billing)` and `Account (Identity)` |
| Version record | Tracks vocabulary changes over time | `v2026-08-29: Site -> Location` |
| Search alias table | Preserves findability after renames | `old_label`, `canonical_label` |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Canonical uniqueness | Review mapping table for duplicate preferred labels. | One canonical label exists per concept. |
| Alias coverage | Sample source systems against alias map. | Observed variants resolve to canonical terms. |
| Abbreviation clarity | Review acronym register. | Each short form resolves clearly within its domain. |
| Version traceability | Review rename log and alias redirects. | Historic labels remain searchable. |
| Fabric consistency | Compare captions, measure names, and glossary labels. | User-facing surfaces use approved terms. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Canonical terms copy vendor jargon | Agent promotes the domain-owned label. |
| Same acronym resolves to unrelated meanings | Agent adds domain qualifiers or rejects the acronym. |
| Deprecated labels vanish from search | Agent preserves alias redirects. |
| Label changes land in reports but not model measures or docs | Agent validates all dependent layers. |
| Normalization ignores ontology or glossary anchors | Agent ties canonical terms back to the governing source. |

## Reference Assets

| Resource | Role | Location |
|---|---|---|
| Guvi ontology article | Background for shared semantics and term alignment | https://www.guvi.in/blog/ontologies-in-ai/ |
| SAAM JSON-LD | Real-world labels, definitions, and namespaces | `docs/references/SAAM/Ontology/saam-core-jsonld.json` |
| SAAM Turtle | Real-world prefLabel and hierarchy context | `docs/references/SAAM/Ontology/saam-core-turtle.txt` |
