---
name: skill-discovery-guide
title: Skill Discovery Guide
description: Route agents to the correct skill, bundle, or learning path when the entry point is unclear.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: low
estimated_tokens: 1353
prerequisites:
  - skill-suite-optimization
related_skills:
  - skill-lifecycle-controller
  - create-skill
  - technology-selection
  - documentation-fixes
  - test-fixes
  - test-modernization-controller
  - security-controller
  - webapi-hardening-controller
  - rfc-fixes-bundle
  - skill-suite-optimization
tags:
  - discovery
  - navigation
  - routing
  - skills
appliesTo: '**/*.{md,cs,csproj,json,yml,yaml,xml,ts,tsx,js,jsx,ps1,sql}'
---
# Skill Discovery Guide

Agent routes ambiguous prompts to the smallest skill set that covers the work.

## When to Use

| Condition | Use |
|---|---|
| Entry point is unclear | Use this guide |
| Many skills fit the same prompt | Use this guide |
| A bundle versus specialist choice is unclear | Use this guide |
| A role-based learning path is requested | Use this guide |

## When Not to Use

| Condition | Route |
|---|---|
| The exact skill is already named | Use the named skill |
| One specialist is already obvious | Use the direct specialist |
| Suite cleanup is requested | Use `skill-suite-optimization` |
| A new skill scaffold is requested | Use `create-skill` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Entry clue | Yes | Agent records technology, problem, pattern, or business system. |
| Scope | Yes | Agent records repo, app, layer, or document set. |
| Delivery goal | Yes | Agent records build, review, migration, design, or fix. |
| Constraints | No | Agent records runtime, compliance, or security limits. |
| Depth | No | Agent records router only, specialist only, or learning path. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent classifies the prompt as technology, problem, pattern, or system. | Review intake row. | One primary lens is selected. |
| 2 | Agent checks the route table and selects one bundle or one specialist family. | Review route decision. | One route is selected. |
| 3 | Agent narrows the choice to one specialist or one ordered learning path. | Review final route. | The route is directly actionable. |
| 4 | Agent opens the selected `SKILL.md` and follows it. | Review execution log. | Work continues from the selected skill. |

## Rule Matrix

| Prompt Shape | First Route | Second Route |
|---|---|---|
| Skill creation, review, optimization, or ecosystem audit | `skill-lifecycle-controller` | `create-skill` |
| Markdown, links, front matter, guidance drift | `documentation-fixes` | `documentation-governance` |
| Test failures, MSTest warnings, or test modernization | `test-modernization-controller` | `test-fixes` |
| API contracts, routes, uploads, compatibility, or endpoint audit | `webapi-hardening-controller` | `dotnet-webapi` |
| Security, auth, injection, or CWE families | `security-controller` | `webapi-authz-hardening` |
| Problem Details, JSON, JWT, or JSON-seq | `rfc-fixes-bundle` | `rfc-9457-compliance` |
| Cross-stack platform or architecture choice | `technology-selection` | `critical-infrastructure-bundle` |
| Washington State finance systems | `wa-state-systems-bundle` | `public-sector-accounting` |
| Desktop MVVM, WPF, WinUI, or MAUI | `mvvm-toolkit-bundle` | `wpf-mvvm-implementation` |
| Fabric, semantic models, or analytics integration | `fabric-integration-bundle` | `semantic-data-bundle` |
| Mainframe copybooks or fixed-width migration | `mainframe-integration-bundle` | `copybook-to-dotnet` |

## Pattern Matrix

| Need | Pattern | Reference |
|---|---|---|
| Full inventory by category | Use the category catalog instead of expanding this file. | `references/inventory-by-category.md` |
| Bundle lookup | Use the router index. | `references/bundle-router-index.md` |
| Complexity sizing | Use the complexity index. | `references/complexity-index.md` |
| Cross-stack combinations | Use the dependency and problem matrices. | `references/cross-reference-matrix.md` |
| Role onboarding | Use an ordered learning path instead of freeform exploration. | `references/cross-reference-matrix.md` |

## Verification Matrix

| Check | Test | Pass |
|---|---|---|
| Route clarity | Replay the prompt against the route table. | One primary route wins. |
| Bundle coverage | Compare bundle names to the router index. | Every bundle route exists. |
| Specialist coverage | Compare specialist names to the skill inventory. | Every cited specialist exists. |
| Token budget | Recalculate the file token count. | The file stays below the specialist-skill limit. |
| STE wording | Scan the file for modal verbs. | Zero prohibited modal matches exist. |

## Verification Checklist

| Checkpoint | Pass Condition |
|---|---|
| Core routers listed | Common entry doors map to one route each. |
| Reference files linked | Inventory, router, complexity, and cross-reference catalogs remain linked. |
| Counts decoupled | The main file avoids stale inventory totals. |
| Learning path guidance preserved | Role-based routing still exists through references. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Jumping to a specialist before checking a router | Agent checks the route table first. |
| Treating learning paths as immediate task routing | Agent uses learning paths for onboarding only. |
| Copying inventory counts into the main file | Agent keeps counts in reference files. |
| Using aliases as primary route entries | Agent routes through bundles or specialists first. |
