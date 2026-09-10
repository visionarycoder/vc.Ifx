---
title: Washington State Fund Reference
doc_type: reference
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 840
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - wa-state-saam
related_docs:
  - https://ofm.wa.gov/accounting/fund-reference-manual/
  - https://ofm.wa.gov/accounting/saam/
tags:
  - washington-state
  - saam
  - funds
  - fund-accounting
  - reference
---
# Washington State Fund Reference

Agent uses this reference with `wa-state-saam` when work needs Washington State fund classes, authorized account cues, or SAAM Chapter 75 alignment.

## Fund Decision Matrix

| Need | Use | Team Pattern |
|---|---|---|
| Identify the right statewide fund family | Fund reference manual | Classify the transaction into the governing fund group before schema or report design. |
| Validate account use inside one fund | Authorized account guidance | Store allowed account relationships beside the effective-dated fund record. |
| Map reporting or control rules | SAAM Chapter 75 and related policy sections | Link policy citations to the fund and account validation rules. |
| Build system crosswalks | Local-to-state mapping tables | Preserve local code, statewide fund, effective date, and owner. |

## Fund Families Reference

| Fund Family | Washington State Pattern | Implementation Focus |
|---|---|---|
| Governmental funds | Current financial resources and legal compliance dominate. | Keep budget, authority, and period controls explicit. |
| Proprietary funds | Business-type or internal-service activity drives operating measurement. | Keep operating revenue, expense, and full-cost reporting aligned. |
| Fiduciary funds | Assets held for others or restricted purposes require stewardship evidence. | Keep participant or beneficiary separation explicit. |
| Capital-project and debt-service patterns | Project financing or debt obligations drive the fund purpose. | Keep project, debt, and restricted-use reporting separate from operating funds. |

## Authorized Account Reference

| Topic | State Pattern | Team Rule |
|---|---|---|
| Fund-to-account compatibility | Funds use only authorized account structures for their purpose. | Validate fund and account pairs before posting or export. |
| Effective dating | Account authority changes over time. | Persist start and end dates on every authorized combination. |
| Restricted use | Some funds limit account use to a narrow legal purpose. | Store restriction text or code beside the validation rule. |
| Agency extension | Agencies layer detail without replacing statewide meaning. | Keep statewide base codes stable and place agency detail beside them. |

## SAAM Chapter 75 Control Cues

| Policy Cue | Accounting Effect | Validation Focus |
|---|---|---|
| Fund purpose | Transaction stays inside the legal and reporting purpose of the fund. | Reject postings that conflict with the stated fund purpose. |
| Authorized accounts | Posting uses the right statewide account family for that fund. | Validate account compatibility before approval or export. |
| Interfund activity | Due-to, due-from, transfer, and reimbursement behavior stays explicit. | Require reciprocal legs and traceable reason codes. |
| Reporting integrity | Fund reporting stays reproducible across agency and statewide views. | Tie reports to one effective-dated fund and account snapshot. |

## Crosswalk and Reporting Cues

| Area | Team Pattern | Pass Target |
|---|---|---|
| ERP or subledger crosswalk | Map local codes to statewide fund and account values through governed tables. | Zero unmapped required combinations remain. |
| Budget reporting | Link fund, appropriation, and authority views with the same fiscal period keys. | Budget and ledger views share one period vocabulary. |
| AFRS readiness | Carry statewide fund meaning into AFRS-facing validation. | Exported fund values are active and compatible. |
| Fabric or BI reporting | Publish fund class, legal restriction, and reporting group attributes in curated models. | Consumers see one canonical fund vocabulary. |

## Common Failure Patterns

| Pitfall | Agent Fix |
|---|---|
| Fund class drives reporting only after the schema is frozen | Agent classifies the fund first. |
| Authorized account rules live in spreadsheets only | Agent persists them as effective-dated validation data. |
| Agency extensions overwrite statewide codes | Agent keeps statewide codes stable and layers local detail beside them. |
| Interfund rules collapse into generic journal logic | Agent records reciprocal fund behavior explicitly. |

