---
name: chart-of-accounts-design
title: Chart of Accounts Design
description: Design chart-of-accounts structures, segment rules, and numbering strategies when work needs durable account coding, rollups, or governmental fund alignment.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium-high
estimated_tokens: 1900
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - double-entry-accounting
  - fund-accounting-patterns
  - public-sector-accounting
  - wa-state-saam
appliesTo: '**/*.{cs,sql,json,md,csv,xlsx}'
tags:
  - chart-of-accounts
  - account-structure
  - coa
  - accounting
  - fund-accounting
---
# Chart of Accounts Design

Agent designs chart-of-accounts structures with explicit segment purpose, numbering discipline, hierarchy rules, and growth capacity.

## When to Use

| Condition | Use |
|---|---|
| Work item needs a new chart of accounts or a major redesign | Use this skill |
| ERP, ledger, or reporting design needs account segments and numbering rules | Use this skill |
| Governmental accounting work needs fund-aware account structures | Use this skill |
| Migration planning needs crosswalks from a legacy chart to a governed target chart | Use this skill |
| .NET services need account validation, hierarchy navigation, or effective-dated activation rules | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work item focuses on balanced journal posting behavior only | Use `double-entry-accounting` |
| Work item focuses on fund classification and interfund flows only | Use `fund-accounting-patterns` |
| Work item focuses on broad GASB reporting treatment only | Use `public-sector-accounting` |
| Work item focuses on Washington State SAAM coding policy detail only | Use `wa-state-saam` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Entity type | Yes | Record commercial, nonprofit, governmental, or Washington State context. |
| Reporting objectives | Yes | Record statutory, management, budget, cost, grant, and consolidation views. |
| Organizational model | Yes | Record department, program, grant, project, and legal-entity boundaries. |
| Posting volume and growth horizon | Yes | Record expected account count and expansion areas. |
| Source-system constraints | No | Record ERP field limits, interface formats, and external reporting code requirements. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps reporting, control, and operational requirements into required dimensions. | Review the requirements matrix. | Each requirement maps to one or more explicit dimensions. |
| 2 | Agent selects the minimum viable account structure from the decision matrix. | Compare candidate structures to the mapped dimensions. | The chosen structure covers every required dimension without duplicate meaning. |
| 3 | Agent defines segment purpose, length, allowed values, and effective-dated governance. | Review the segment catalog. | Every segment has one clear business meaning and one rule set. |
| 4 | Agent designs numbering, reserved ranges, and parent-child rollups. | Review the numbering and hierarchy tables. | Growth space, rollups, and summary views stay explicit and non-overlapping. |
| 5 | Agent defines validation, activation, inactivation, and migration crosswalk rules. | Review the control model and crosswalk design. | Posting and migration paths resolve one valid target account for each approved source row. |
| 6 | Agent validates the design against reporting, consolidation, and sample posting scenarios. | Execute the verification checklist. | Each scenario resolves a valid account and expected rollup outcome. |

## Account Structure Decision Matrix

| Structure | Choose When | Strengths | Constraints | Example Shape |
|---|---|---|---|---|
| Single-segment | Entity scope is small, reporting is simple, and one natural account carries most meaning. | Setup stays fast and statement reporting stays direct. | Dimensional analysis and fund isolation move outside the account code. | `1000 Cash`, `4100 Sales`, `6100 Rent Expense` |
| Multi-segment | Entity needs independent dimensions such as fund, department, natural account, program, and project. | Validation stays precise and reporting stays flexible. | Governance load rises and segment ownership needs stewardship. | `001-120-5110-410-0000` |
| Hierarchical | Entity needs parent-child rollups, summary accounts, and controlled local detail below enterprise totals. | Consolidation and budgeting stay consistent across business units. | Hierarchy governance needs effective dates and leaf-posting controls. | Parent `5000 Personnel` -> child `5110 Salaries`, `5120 Overtime` |

## Segment Design Patterns

| Segment | Use When | Primary Meaning | Design Pattern | Example Values |
|---|---|---|---|---|
| Fund | Legal, restricted, or fiduciary resource pools drive accounting and reporting. | Source and use of resources within a governed fund boundary. | Use for governmental, grant, or restricted-resource contexts. Keep the segment first when fund is the primary reporting anchor. | `001 General`, `145 Federal Grant`, `410 Debt Service` |
| Department or Cost Center | Management needs organizational accountability, cost ownership, or approval routing. | Organizational unit that owns spend or operating results. | Use for manager-level performance views or cost allocation. Keep a stable parent-child map for reorganizations. | `120 Finance`, `340 Facilities`, `875 IT Operations` |
| Natural Account | Trial balance, statements, and debit-credit behavior need a stable account class. | Economic nature of the balance or transaction. | Use in every chart of accounts. Align ranges to asset, liability, equity, revenue, and expense families. | `1010 Cash`, `2110 Accounts Payable`, `5110 Salaries` |
| Program or Grant | Entity tracks statutory purpose, service line, or sponsored funding. | Outcome, service, or award dimension that cuts across departments. | Use when management, grantor, or public reporting needs costs and revenues by purpose. Separate grants from projects when award governance differs. | `410 Road Safety`, `F23 FHWA Grant`, `730 Public Outreach` |
| Project or Activity | Work is temporary, capital in nature, or needs milestone-level cost capture. | Time-bounded initiative, asset build, event, or activity. | Use when a start and end date, capitalization review, or project manager accountability exists. | `P10452 Bridge Rehab`, `A220 Payroll Conversion` |

## Segment Selection Rules

| Requirement Shape | Segment Rule | Pass Target |
|---|---|---|
| Legal restriction changes posting eligibility | Add `Fund`. | Each restricted transaction resolves one valid fund. |
| Manager accountability drives budget or spend review | Add `Department` or `Cost Center`. | Reports reconcile by accountable manager. |
| Statement classification drives the primary balance meaning | Add `Natural Account`. | Trial balance and statements derive from one governed natural account set. |
| Sponsored or statutory purpose changes allowability | Add `Program` or `Grant`. | Allowability and reporting stay traceable to the funding purpose. |
| Temporary work, capital build, or event cost needs lifecycle control | Add `Project` or `Activity`. | Open, close, and capitalization checks stay enforceable. |
| Requirement duplicates meaning already carried by another segment | Reuse the governed segment. | No two segments express the same business meaning. |

## Account Numbering Scheme Matrix

| Scheme | Choose When | Pattern | Benefits | Risks |
|---|---|---|---|---|
| Intelligent numbering | Users need visible meaning in the code itself. | Range logic such as `1xxx` assets and `5xxx` expenses. | Manual review stays faster. | Embedded meaning can block expansion when ranges fill unevenly. |
| Sequential numbering | System lookup drives meaning and human parsing is less important. | New codes issue from the next available governed block. | Expansion stays easy. | Users depend on metadata and reports rather than the code shape. |
| Hybrid numbering | Entity needs visible top-level meaning plus flexible growth inside ranges. | Family digit indicates class and trailing digits issue sequentially. | Readability and growth stay balanced. | Governance needs strict range ownership and exception control. |

## Reserved Range Planning

| Area | Planning Rule | Example |
|---|---|---|
| Natural account families | Reserve major blocks by statement class. | `1000-1999` assets, `2000-2999` liabilities, `3000-3999` equity, `4000-4999` revenue, `5000-7999` expense |
| Department growth | Reserve blocks per division and leave unused gaps between large groups. | `100-199` finance, `300-399` operations, `800-899` technology |
| Program and grant growth | Reserve sponsor or statutory bands for future awards. | `600-699` federal, `700-749` state, `750-799` local |
| Project growth | Reserve rolling year or portfolio bands for capital and operating projects. | `P10***` capital, `P20***` modernization |
| Retirements and mergers | Preserve retired codes and do not recycle them inside the audit horizon. | `5115` inactive with end date, replacement `5116` |

## Reference Files

| File | Purpose |
|---|---|
| [references/coa-implementation.md](references/coa-implementation.md) | Detailed hierarchy patterns, segment rules, reserved ranges, cross-segment validation, .NET implementation, migration guidance, and sample structures. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter | Run `npm run frontmatter:validate`. | Command returns zero validation errors. |
| STE wording | Run the repository STE violation grep on the new files. | Scan returns zero prohibited matches. |
| Segment uniqueness | Review the segment catalog. | Each segment carries one non-overlapping meaning. |
| Rollup integrity | Review summary-to-detail mappings. | Each leaf account rolls to one valid parent chain. |
| Reserved capacity | Count used and unused values by governed range. | Growth space remains available in each critical range. |
| Governmental alignment | Review fund and natural-account mapping for public-sector scenarios. | GASB and SAAM-oriented views map without duplicate coding meaning. |
| Migration readiness | Test legacy-to-target crosswalk samples. | Each approved legacy code resolves one active target account or one governed exception. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Natural account stores department or program meaning | Agent moves organizational or purpose meaning into a dedicated segment. |
| Chart design adds every requested dimension to the account string | Agent keeps only dimensions that drive control, posting, or reporting outcomes. |
| Reserved ranges disappear during urgent account creation | Agent keeps governed unused bands and assigns new codes from approved blocks only. |
| Parent accounts accept postings and summary roles at the same time | Agent marks summary nodes as non-posting and leaf nodes as posting-eligible. |
| Grant and project segments collapse into one field despite different lifecycle rules | Agent separates them when funding and delivery governance differ. |
| Retired accounts reactivate with a different meaning | Agent creates a new code and preserves the retired code history. |

## Outputs

- Account structure decision matrix
- Segment catalog with use rules
- Numbering and reserved-range plan
- Hierarchy and rollup design
- Validation and migration control model
- Commercial, governmental, and Washington State sample structures
