---
title: Washington State Fiscal Policy Reference
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
related_docs:
  - ../SKILL.md
  - https://ofm.wa.gov/budget/how-it-works/
  - https://ofm.wa.gov/accounting/saam/
  - https://ofm.wa.gov/accounting/fund-reference-manual/
---
# Washington State Fiscal Policy Reference

## Fiscal Calendar Reference

| Topic | Washington State Pattern | Implementation Focus |
|---|---|---|
| Fiscal year | Fiscal year runs July 1 through June 30. | Build fiscal dimensions from July-based year boundaries, not calendar year boundaries. |
| Biennium | State budget operates on a two-year cycle beginning July 1 of each odd-numbered year. | Store biennium key, start date, end date, and supplemental revision lineage. |
| Supplemental updates | Biennial budgets receive annual or session-driven supplemental revisions. | Preserve original authority, revised authority, and revision source separately. |
| Monthly close | Agencies align accounting periods and close work to state fiscal periods. | Keep period-open, close, and post-close status on fiscal calendar rows. |
| Year-end and lapse | Fiscal-year close and lapse-year activity follow controlled timing windows. | Model close state and lapse-year eligibility explicitly on period and appropriation records. |

## Authority and Allotment Reference

| Topic | State Pattern | Design Focus |
|---|---|---|
| Appropriation structure | Spending authority ties to legislative or other authorized budget structures. | Store authority source, amount, effective period, and revision lineage. |
| Allotment process | Allotments distribute or refine broader authority into controlled spending availability. | Keep allotment as a separate control layer from appropriation and actuals. |
| Organization structure | Agency and organization segments route authority and reporting responsibility. | Model agency, program, and organization segments as governed dimensions. |
| Lapse-year appropriations | Prior-year authority remains visible for controlled closeout and adjustment handling where applicable. | Link appropriation records to fiscal year, lapse-year flag, and close status. |
| Budget closeout | Biennium and fiscal-year closeout require explicit carryforward, lapse, or finalization treatment. | Use close actions with immutable snapshots and approval metadata. |

## Code Family Reference

| Code Domain | Washington State Pattern | Implementation Focus |
|---|---|---|
| Object codes | State expenditure coding groups activity into standardized categories such as salaries, goods and services, capital, and grants or benefits. | Store object code family, subobject detail, effective dates, and reporting rollups. |
| Revenue source codes | Revenue tracking uses standardized source categories for statewide reporting and control. | Store source family, detail code, effective dates, and fund compatibility rules. |
| Fund codes | Fund structures align to the statewide fund reference inventory and legal authority. | Use governed fund master data with active and inactive states plus effective dates. |
| Agency codes | Agencies use statewide identifiers for budget, accounting, and reporting. | Separate agency code from organization and program segmentation. |
| Organization codes | Internal organizational hierarchy supports allocation, accountability, and reporting rollups. | Version organization hierarchies and retain historical parent-child mappings. |

## Payment and Closeout Reference

| Topic | State Pattern | Pass Target |
|---|---|---|
| Vendor payment process | Payment workflows align to statewide coding, approval, and disbursement expectations. | Payment records preserve coding, approval lineage, vendor identity, and remittance status. |
| Allotment tracking | Spending activity compares against controlled availability, not broad authority only. | Available authority remains derivable from appropriation, allotment, and actual layers. |
| Lapse-year handling | Prior-year adjustments stay visible and segregated from current-year operating activity. | Reports separate current-year and lapse-year activity without ambiguity. |
| Budget closeout | Final close processes preserve immutable fiscal snapshots and approved exceptions. | Close packages reproduce the same balances and code sets on re-run. |
| Reconciliation | Agency systems reconcile to statewide fiscal codes and authority records. | Difference sets resolve to zero or documented approved exceptions. |

## File Format Specifications

| Format Area | State Pattern | Validation Focus | .NET Integration Shape |
|---|---|---|---|
| Reference-data extracts | Statewide code tables and agency crosswalks commonly move as controlled flat files, spreadsheets, or governed delimited extracts. | Validate headers, code length, active dates, duplicate keys, and retired-code markers. | Use import schemas with deterministic column maps and effective-date parsing. |
| Fiscal calendar tables | Fiscal year, biennium, period, and close-state data move as reference sets, not hard-coded constants only. | Validate non-overlapping date ranges, one current period state, and consistent biennium boundaries. | Seed EF Core tables from versioned source files and track provenance. |
| Crosswalk files | Agency-to-state crosswalks map local dimensions to statewide agency, fund, object, and revenue codes. | Validate one active mapping per local key and date window. | Store crosswalk rows as effective-dated entities with uniqueness constraints. |
| Export snapshots | Budget, allotment, and closeout snapshots move as immutable extracts for review and audit support. | Validate snapshot ID uniqueness, source timestamp, and row-count integrity. | Generate immutable file names and snapshot manifests tied to batch or close IDs. |

## Integration Hooks

| Surface | Agent Action | Output |
|---|---|---|
| `wa-state-saam` | Agent links fiscal controls, code semantics, and closeout rules to the governing SAAM policy area where one exists. | Policy-traceable fiscal rule set |
| AFRS documentation | Agent aligns fiscal code domains and period logic to the statewide accounting interface vocabulary used by AFRS-facing work. | Shared statewide code vocabulary |
| EF Core reference data | Agent models fiscal calendar, biennium, appropriation, allotment, object, revenue, fund, agency, and organization tables with effective dates and audit columns. | Governed reference-data store |
| Validation services | Agent exposes period, authority, and code validators as reusable services with cache refresh and provenance checks. | Consistent runtime validation layer |
| Reporting layer | Agent publishes fiscal dimensions and authority facts for variance, closeout, and compliance reporting. | Queryable budget and fiscal policy analytics |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Calendar-year logic substitutes for Washington fiscal-year logic | Agent builds fiscal dimensions from July-based boundaries and biennium keys. |
| Appropriation and allotment collapse into one number | Agent stores each authority layer separately with reconciliation logic. |
| Statewide codes become static enums in application code | Agent moves code domains into effective-dated reference tables and crosswalk services. |
| Reorganizations rewrite historical reports | Agent versions organization hierarchies and resolves reports by effective date. |
| Closeout overwrites the prior state | Agent publishes immutable close snapshots with approved-exception lineage. |

## Outputs

| Output | Description |
|---|---|
| Fiscal calendar model | Fiscal year, biennium, period, and close-state design |
| Authority control model | Appropriation, allotment, lapse-year, and closeout rule set |
| Statewide code inventory | Object, revenue, fund, agency, and organization reference-data design |
| EF Core and validation hooks | Reference-data persistence, cache, and validator plan |
| Verification plan | Fiscal-boundary, code-validity, closeout, and audit-trace tests |

## Further Reading

| Resource | Type | Location |
|---|---|---|
| FHWA Federal Highway Policy and Guidance | URL | https://www.fhwa.dot.gov/guidance/ |
| FTA Financial Management Oversight | URL | https://www.transit.dot.gov/regulations-and-guidance/program-oversight/financial-management-oversight |
| FTA State Management Oversight | URL | https://www.transit.dot.gov/regulations-and-guidance/program-oversight/state-management-oversight |
| US DOT Program Oversight | URL | https://www.transit.dot.gov/regulations-and-guidance/program-oversight/program-oversight |
