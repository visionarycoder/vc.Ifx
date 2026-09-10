---
name: appropriation-lifecycle-patterns
title: Appropriation Lifecycle Patterns
description: Apply Washington State appropriation authorization, allotment, reservation, expenditure, and reversion controls when work needs SAAM-aligned budget availability enforcement.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1950
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - budget-to-actual-tracking
  - fund-accounting-patterns
  - wa-state-fiscal-policies
  - wa-state-saam
appliesTo: '**/*.{cs,sql,json,md,csv,xlsx,yml,yaml}'
tags:
  - appropriation
  - budget
  - washington-state
  - saam
  - fund-accounting
related_docs:
  - references/appropriation-control-patterns.md
---
# Appropriation Lifecycle Patterns

Agent designs Washington State budget-control flows with explicit appropriation authority, allotment limits, reservation states, expenditure liquidation, and reversion outcomes.

## When to Use

| Condition | Use |
|---|---|
| Prompt needs appropriation-aware budget control for Washington State agencies | Use this skill |
| Work item needs a full lifecycle from authorization through reversion | Use this skill |
| Procurement or payroll flow needs pre-encumbrance, encumbrance, and expenditure controls | Use this skill |
| Data model needs appropriation, biennium, fund, and appropriation-index relationships | Use this skill |
| Validation service needs funds-availability checks before spending events | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work item focuses on broad variance analytics and reporting with limited policy detail | Use `budget-to-actual-tracking` |
| Work item focuses on fund classification, interfund balances, or balance classifications | Use `fund-accounting-patterns` |
| Work item focuses on statewide fiscal calendars, coding domains, or payment policies beyond appropriation control | Use `wa-state-fiscal-policies` |
| Work item focuses on general SAAM mapping with no lifecycle design work | Use `wa-state-saam` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Authority source | Yes | Record appropriation code, title, legal source, authorized amount, and effective biennium. |
| Budget dimensions | Yes | Record fund, agency, organization, program, project, object code, and appropriation index scope. |
| Control events | Yes | Record authorization, allotment, requisition, purchase order, contract, invoice, payroll, journal, and close events. |
| Period model | Yes | Record fiscal year, biennium, open period, close period, lapse-year status, and reversion date. |
| Exception policy | Yes | Record override authority, approval evidence, negative-balance handling, and reappropriation rules. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent establishes appropriation authority as a distinct record tied to fund and biennium. | Review authority model. | Authorized amount, appropriation code, fund code, and biennium code remain explicit and queryable. |
| 2 | Agent models allotment as a narrower spending limit derived from authority without rewriting the original authorization. | Review budget facts and revisions. | Appropriation and allotment balances reconcile and remain separate layers. |
| 3 | Agent maps requisitions to pre-encumbrances and maps obligations to encumbrances with conversion and release links. | Review event-state matrix. | Each control event lands in one lifecycle stage with traceable source-document lineage. |
| 4 | Agent liquidates encumbrances into expenditures and records residual releases. | Test partial, full, and cancelled settlement cases. | Encumbrance balances, releases, and expenditures reconcile to source events. |
| 5 | Agent executes year-end and biennium-end reversion logic with reappropriation and carryforward checks. | Test close scenarios. | Expired availability exits spending authority unless an authorized continuation path exists. |

## Lifecycle Stage Matrix

| Stage | Trigger | Control Objective | Balance Effect | Exit Path |
|---|---|---|---|---|
| Budget Authorization | Legislative or delegated budget adoption | Establish legal spending ceiling | Sets authorized amount | Allotment or approved revision |
| Allotment | OFM, agency, or internal release of spending authority | Limit operational spending to a controlled subset of authority | Sets available allotment | Additional allotment, deallotment, or close |
| Pre-encumbrance | Requisition or planned-use approval | Reserve expected use before legal obligation | Soft-reserves availability | Release, expire, or convert to encumbrance |
| Encumbrance | Purchase order, contract, or other obligation event | Reserve committed authority against a legal obligation | Firm-reserves availability | Liquidate to expenditure, partially release, or cancel |
| Expenditure | Invoice, payroll, journal, or accrual recognition | Consume authority with recognized spending | Converts reservation to actual use | Adjustment, correction, or close |
| Reversion | Fiscal-year or biennium close event | Return unused or expired authority to the governing pool | Removes unspent balance from availability | Reappropriation, carryforward, or final lapse |

## Funds Availability Checking Algorithm

| Step | Calculation | Test | Pass |
|---|---|---|---|
| 1 | Resolve the candidate transaction dimensions: appropriation, fund, biennium, agency, organization, program, project, object code, and fiscal period. | Submit valid and invalid dimensional combinations. | Validator rejects unknown, expired, or mismatched dimension sets. |
| 2 | Derive current authority: authorized amount plus approved increases minus approved decreases. | Compare to revision history. | Derived authority equals the immutable event total. |
| 3 | Derive current allotment: allotted amount plus approved allotment revisions minus deallotments. | Compare to allotment event history. | Derived allotment equals the immutable event total. |
| 4 | Derive reserved amount: active pre-encumbrances plus active encumbrances minus released or liquidated portions. | Test conversion, cancellation, and partial liquidation paths. | Reserved amount matches open reservation detail. |
| 5 | Derive actual expenditures: posted expenditure events plus approved accruals minus reversals and corrections. | Compare to expenditure ledger. | Actual amount matches posted spending for the same dimensional grain. |
| 6 | Compute remaining authority and remaining allotment, then use the tighter balance as the control result. | Test over-authority, over-allotment, and exact-balance scenarios. | Control result blocks spending when either limit is negative. |
| 7 | Apply exception policy for authorized override, lapse-year treatment, or reappropriation status. | Test approved and unapproved exception cases. | Only approved exception paths pass the control point. |

### Availability Formula

| Measure | Formula |
|---|---|
| Remaining Authority | Revised Authority - Open Pre-encumbrances - Open Encumbrances - Actual Expenditures |
| Remaining Allotment | Revised Allotment - Open Pre-encumbrances - Open Encumbrances - Actual Expenditures |
| Control Balance | Lesser of Remaining Authority and Remaining Allotment |

## Control Level Decision Matrix

| Scenario | Control Level | Action |
|---|---|---|
| Legal spending ceiling changes by law, budget revision, or reappropriation | Appropriation | Update authority events and preserve prior history. |
| Operational release narrows agency spending capacity within existing authority | Allotment | Update allotment events and keep authority unchanged. |
| Planned use exists with no legal obligation yet | Pre-encumbrance | Reserve soft capacity and age or release the reservation. |
| Purchase order, contract, or equivalent obligation exists | Encumbrance | Reserve firm capacity and require liquidation linkage. |
| Invoice, payroll, or accrual posts recognized spending | Expenditure | Convert or post actual spending and release excess reservation. |
| Period close reaches unused or expired authority | Reversion | Remove unused availability unless an authorized continuation path exists. |

## Washington State Data Model Anchors

| Anchor | Pattern | Source Alignment |
|---|---|---|
| Appropriation | Store appropriation as the legal authorization record. | SAAM ontology defines appropriation as legislative authorization to expend from a fund for a designated purpose during a biennium. |
| Biennium | Store one appropriation-to-biennium relationship and preserve effective dates. | SAAM ontology defines biennium as the Washington two-year budget cycle. |
| Fund | Store one appropriation-to-fund relationship and validate fund compatibility. | SAAM ontology links appropriation to fund. |
| Appropriation Index | Preserve AI as a reduction key for fund and appropriation coding. | SAAM ontology defines AI as a three-character AFRS key. |
| Financial Transaction | Link spending events to appropriation and fiscal period. | SAAM semantic model links financial transactions to appropriation and fiscal period. |

## Integration with `budget-to-actual-tracking`

| Surface | Integration |
|---|---|
| Authority facts | This skill defines the control logic for appropriation and allotment layers that `budget-to-actual-tracking` reports as budget states. |
| Reservation facts | This skill defines pre-encumbrance and encumbrance transitions that feed the budget-state matrix and variance measures. |
| Actual facts | This skill defines expenditure liquidation rules that convert reserved balances into actual balances for analytics. |
| Close facts | This skill defines reversion, lapse, carryforward, and reappropriation outcomes that shape year-end available-balance reporting. |
| Verification | This skill supplies control-point tests that `budget-to-actual-tracking` reuses for variance, close, and reconciliation validation. |

## Reference Files

| File | Purpose |
|---|---|
| [references/appropriation-control-patterns.md](references/appropriation-control-patterns.md) | Detailed .NET patterns, EF Core queries, multi-year handling, capital and operating budget patterns, reappropriation logic, and code examples. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Authority layering | Compare appropriation, allotment, reservation, expenditure, and reversion facts. | Each lifecycle layer remains separate and reconcilable. |
| Funds availability | Test under-budget, exact-budget, over-budget, and expired-authority scenarios. | Validator returns allow or block results that match policy for each case. |
| Reservation lifecycle | Test requisition, conversion, partial liquidation, cancellation, and release paths. | Open reservations equal remaining obligations after each event. |
| Biennium alignment | Test cross-year and biennium-boundary transactions. | Transactions post only to valid fiscal periods and authority windows. |
| Reversion control | Test year-end and biennium-end close packages. | Unused authority exits availability unless a recorded continuation path exists. |
| Audit traceability | Review source-document, approval, and event-lineage fields. | Each balance change links to one authority or transaction event with approval evidence. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Allotment overwrites appropriation | Agent stores allotment as a narrower derived control layer with separate events. |
| Encumbrance liquidation loses the original reservation trail | Agent records liquidation links and residual release events. |
| Availability checks ignore open pre-encumbrances | Agent includes active soft reservations in the control formula where business policy uses them. |
| Reversion logic runs as a report-only adjustment | Agent records explicit close and reversion events with effective dates and approval metadata. |
| Cross-biennium transactions reuse stale authority | Agent validates fiscal period and authority window before posting. |

## Outputs

| Output | Description |
|---|---|
| Lifecycle model | Authority, allotment, reservation, expenditure, and reversion design |
| Availability algorithm | Deterministic funds-availability calculation and control-point logic |
| Control matrix | Decision logic for appropriation, allotment, reservation, spending, and close |
| Integration plan | Connection points for budget tracking, fund accounting, and SAAM policy skills |
| Verification plan | Scenario tests for lifecycle, close, and audit-trace behavior |
