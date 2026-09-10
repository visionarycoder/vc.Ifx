---
title: Fund Accounting State and Balance Reference
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
related_docs:
  - ../SKILL.md
---
# Fund Accounting State and Balance Reference

## Washington State Fund Reference

| Topic | Washington State Pattern | Implementation Focus |
|---|---|---|
| Fund Reference Manual role | OFM describes the manual as the complete inventory of legally authorized accounts for use by state agencies. | Treat the manual as the authoritative source catalog for state fund codes and fund metadata. |
| SAAM Chapter 75 relationship | OFM describes the manual as a supplement to SAAM Chapter 75 Uniform Chart of Accounts. | Align fund validation with chart-of-accounts validation and statement dimensions. |
| Active and inactive accounts | The manual includes active authorized accounts and inactive accounts eliminated by legislation or administrative action. | Store fund status, effective dates, retirement dates, and deactivation reason. |
| Legal authorization | Fund setup and continued use depend on legal or administrative authorization. | Require authorization evidence and effective-date validation during fund setup and posting. |
| Reference refresh | OFM updates the manual as legislation or administrative action changes the data. | Version fund-code reference data and preserve historical mappings for closed periods. |
| Fund type coding | SAAM 75.30.10 maps GAAP fund types to statewide codes such as `AA`, `BA`, `CA`, `FA`, and `HD`. | Persist fund type code, statement mapping, and source reference beside each governed fund record. |

## Detailed Fund Type Reference

| Category | Fund Type | WA Code | Purpose | Design Focus |
|---|---|---|---|---|
| Governmental | General Fund | `AA` | Default operating fund for unrestricted core activity | Broad posting access, budget control, and major-fund reporting |
| Governmental | Special Revenue Fund | `BA` | Restricted or committed revenue supports specific programs | Revenue-source tracing and legal restriction metadata |
| Governmental | Capital Projects Fund | `DA` | Resources support capital acquisition or construction | Project, funding source, and asset-ready state tracking |
| Governmental | Debt Service Fund | `CA` | Resources pay principal and interest | Debt calendar linkage and restricted cash tracking |
| Governmental | Permanent Fund | `EA` | Nonexpendable principal with spendable earnings | Corpus preservation and earnings-spend logic |
| Proprietary | Enterprise Fund | `FA` | Fee-supported external services | Full accrual operations, customer billing, and capital intensity |
| Proprietary | Internal Service Fund | `GA` | Cost recovery for internal service centers | Cost allocation, recharge logic, and internal customer dimensions |
| Fiduciary | Pension Trust Fund | `HC` | Resources support pension benefits | Participant, actuarial, and trust restriction data |
| Fiduciary | Investment Trust Fund | `HB` | External investment pool activity | Unit ownership, fair value, and custodian reconciliation |
| Fiduciary | Private-Purpose Trust Fund | `HA` | Trust resources support specified beneficiaries | Beneficiary restrictions and trust agreement attributes |
| Fiduciary | Custodial Fund | `HD` | Agency holds assets for other parties | Asset-by-owner custody and short-duration settlement flow |

## Fund Balance and Net Position Reference

| Classification | Use | Trigger | Output |
|---|---|---|---|
| Nonspendable | Resources stay unavailable for spending | Inventory, prepaid, long-term receivable, or corpus restriction | Balance flagged as unavailable for appropriation |
| Restricted | External or legal constraint drives use | Grant, statute, bond covenant, or constitutional rule | Restricted balance by source and purpose |
| Committed | Highest governing authority imposes formal constraint | Resolution or equivalent formal action before period end | Commitment record with authority date |
| Assigned | Intended use exists below committed level | Management designation or budgetary intent | Assignment metadata and responsible owner |
| Unassigned | Residual spendable balance in the general operating context | No stronger constraint applies | Residual available balance |
| Net position invested in capital assets | Proprietary or government-wide capital view | Capital assets net of related debt | Capital-asset-linked net position |
| Restricted net position | External or legal restrictions in accrual views | Debt, grant, or trust restrictions | Restricted accrual balance |
| Unrestricted net position | Residual accrual view | Remaining balance after invested and restricted components | Residual accrual balance |

## Washington State Fund Integration

| Integration Area | Agent Action | Pass |
|---|---|---|
| Fund Reference Manual link | Agent records `https://ofm.wa.gov/accounting/fund-reference-manual/` as the source reference for Washington State fund catalogs. | State-specific fund logic points to one authoritative catalog source. |
| SAAM Chapter 75 link | Agent records the manual as a supplement to SAAM Chapter 75 Uniform Chart of Accounts. | Fund validation aligns with broader state chart-of-accounts controls. |
| Authorized-account validation | Agent validates fund codes against active authorized accounts before posting, reporting, or interface export. | Inactive, unknown, or misdated fund codes fail validation. |
| Legal authorization checks | Agent stores approval and effective-date evidence for fund setup and use. | Each active fund code has traceable authorization metadata. |
| State-specific reporting | Agent carries Washington-specific fund attributes into extracts and reports. | State reporting outputs preserve fund code, status, and effective-period context. |
| .NET reference data management | Agent uses effective-dated reference data services, cache invalidation rules, and audit timestamps for fund codes. | Applications read one current fund catalog and retain historical lookup fidelity. |

## Public Sector Specific Patterns

| Pattern | Use | Implementation Shape |
|---|---|---|
| Fund key everywhere | Posting, approval, and reporting depend on fund identity | Require fund identifiers in commands, events, and tables |
| Reciprocal interfund entries | Each interfund event needs balanced dual-fund evidence | Generate one transaction envelope with two or more fund legs |
| Constraint-aware close | Balance classification changes only through approved events | Use classification rules driven by source restrictions and authority records |
| Multi-tenant fund isolation | Shared platforms serve multiple agencies or business units | Partition by tenant and fund and validate cross-tenant joins |
| Major-fund reporting overlay | Some funds need separate presentation while others aggregate | Store major-fund designation history per reporting period |
| Authorized fund-code registry | State-specific implementations need legally valid fund codes | Centralize fund master data with status, effective dates, and approval metadata |

## Integration Hooks

| Surface | Agent Action | Output |
|---|---|---|
| EF Core | Model fund, tenant, balance classification, and interfund instrument aggregates with composite indexes on tenant, fund, and fiscal period. | Transactional store with explicit isolation boundaries |
| Microsoft Fabric | Publish fund facts, interfund facts, and balance classifications into semantic models with tenant-aware security filters. | Fund dashboards, balance schedules, and interfund reconciliation reports |
| API and workflow layer | Enforce fund presence on commands, approvals, and close actions. | Stable service contracts with fund and tenant context |
| Reference data services | Publish active and inactive fund catalogs, approval state, and effective dates through one governed .NET lookup surface. | Authorized fund-code validation and historical code resolution |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| One chart of accounts substitutes for fund identity | Agent restores fund as a first-class dimension and key. |
| Transfer, loan, and reimbursement events share one generic code path | Agent creates explicit transaction types with distinct validations. |
| Fund balance classes store manual text only | Agent derives classes from governed rules and authority metadata. |
| Shared platform filters by tenant only | Agent filters by tenant and fund, then validates cross-fund permissions separately. |
| Major-fund designation stays hard-coded | Agent stores designation by reporting period and reporting package. |
| State fund codes load as static enums only | Agent uses effective-dated reference data with status and approval metadata. |
| Inactive state fund accounts stay selectable after retirement | Agent validates active status and effective dates before use. |

## Outputs

| Output | Description |
|---|---|
| Fund type map | Classified inventory of governmental, proprietary, and fiduciary funds |
| Interfund rules matrix | Paired-entry and settlement guidance for interfund activity |
| Balance classification model | Rules for fund balance and net position outcomes |
| Washington State fund reference model | Authorized-account, status, approval, and SAAM Chapter 75 integration guidance |
| Isolation design | EF Core, API, and Fabric boundaries for fund and tenant security |
| Verification plan | Reconciliation and reporting tests for fund behavior |

## Further Reading

| Resource | Type | Location |
|---|---|---|
| OFM Fund Reference Manual | URL | https://ofm.wa.gov/accounting/fund-reference-manual/ |
| SAAM Chapter 75 Uniform Chart of Accounts | PDF | `docs/references/Complete_SAAM_26A-05_2026_07_01-1.pdf` |
