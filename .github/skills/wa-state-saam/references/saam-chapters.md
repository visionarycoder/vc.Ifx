---
title: Washington State SAAM Chapter Reference
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium-high
estimated_tokens: 1650
prerequisites:
  - ste-agent-writing-standard
  - wa-state-saam
related_skills:
  - wa-state-afrs
  - fund-accounting-patterns
  - budget-to-actual-tracking
related_docs:
  - ../SKILL.md
  - ./policy-reference.md
  - ./wa-state-fund-reference.md
  - https://ofm.wa.gov/accounting/saam/
tags:
  - saam
  - washington-state
  - accounting
  - reference
---
# Washington State SAAM Chapter Reference

This reference condenses the 2026-07-01 SAAM manual into implementation cues for coding, posting, budget control, and payment workflows. The current manual labels Chapter 30 as **Capital Assets**. Budget authority structure appears in Chapter 85.

## Core Source Cues

| Source anchor | SAAM cue | Use |
|---|---|---|
| 20.15.10 | Internal control supports operations, reporting, and compliance objectives with reasonable assurance. | Persist control evidence on high-risk fiscal events. |
| 75.30.10 | Fund types map to statewide codes such as `AA`, `BA`, `CA`, `DA`, `EA`, `FA`, `GA`, `HA`, `HB`, `HC`, `HD`. | Keep fund type as governed reference data, not free text. |
| 75.80.10 | Revenue categories align to state, federal, and private or local authority groupings. | Validate revenue source family against funding authority and fund purpose. |
| 80.30.82 | OFM adopted one statewide chart of accounts with optional agency extension codes. | Keep statewide codes stable and layer agency detail beside them. |
| 80.30.90 | Budgetary data is integrated in the uniform account code structure. | Store budget dimensions with the posting key. |
| 85.10.20 | Expenditure authority flows from Legislature, Governor, and OFM actions. | Validate authority source and effective period before posting. |
| 85.32.50 | Agencies pay 95 percent or more of obligations by the due date. | Expose AP timeliness metrics. |

## Uniform Account Code Structure

| Element | Size | SAAM meaning | Team pattern |
|---|---:|---|---|
| Agency code | 4 | Distinct operational unit assigned by OFM | Use as reporting owner. |
| Account code | 3 | Specific accounting entity | Validate with fund type and cash type. |
| Expenditure authority code | 3 | Spending authorization | Preserve period scope. |
| Appropriation index | 3 | AFRS key that expands to fund and appropriation coding | Resolve AI before edits and export. |
| Fund code | 3 | Accounting fund dimension | Keep one active row per posting date. |
| Program index | 5 | Functional activity key | Resolve PI to one active row. |
| Organization index | 4 | Organizational reduction key | Resolve OI before posting. |
| Object or subobject | 2 | Character of goods or services | Use statewide object meaning. |
| Sub-subobject | 4 | Detailed expenditure code | Separate statewide payroll detail from agency detail. |
| Revenue source | 4 | Revenue origin | Validate category family. |

## Object Code Catalog

| Object | Title | SAAM definition cue | Usage rule |
|---|---|---|---|
| `A` | Salaries and Wages | Employee compensation | Exclude independent-contractor payments. |
| `B` | Employee Benefits | State share of benefit costs | Pair with payroll employer costs. |
| `C` | Professional Service Contracts | Consulting or technical expertise | Keep contract-embedded travel in the contract subobject. |
| `E` | Goods and Services | Operating supplies and services | Use when no capital-outlay classification applies. |
| `G` | Travel | Authorized travel reimbursements | Route contract-embedded travel to the contract object. |
| `J` | Capital Outlays | Capital asset acquisition or addition | Apply Chapter 30 thresholds and useful-life rules. |
| `M` | Interfund Operating Transfers | Budgeted or legal fund transfers | Keep transfer direction explicit. |
| `N` | Grants, Benefits, and Client Services | Client payments and provider payments | Separate benefit delivery from procurement. |
| `P` | Debt Service | Principal, interest, and related costs | Preserve component detail. |
| `S` | Interagency Reimbursements | Reimbursements from another state agency | Break out reimbursed activity by subobject. |
| `T` | Intra-agency Reimbursements | Reallocations within one agency | Keep fiscal-year subobject totals at zero. |
| `W` | Other | Depreciation, amortization, bad debts, and similar items | Restrict to SAAM GL combinations. |

## Revenue Source Code Reference

| Family | Meaning | Authority alignment | Fund and posting constraint |
|---|---|---|---|
| `01XX` | Taxes | State authority | Keep separate from suspense activity. |
| `02XX` | Licenses, permits, and fees | State authority | Use for regulated activity and statutory fee collections. |
| `03XX` | Federal revenue | Federal authority | Tie to federal expenditure authority and grant restrictions. |
| `04XX` | State charges and miscellaneous revenue | State authority | Use for exchange or exchange-like state program charges. |
| `05XX` | Private or local charges and miscellaneous revenue | Private or local authority | Tie to nonfederal contracts, agreements, or reimbursements. |
| `06XX` | Trust revenues and Treasurer transfers | Trust or transfer activity | Exclude ordinary earned revenue. |
| `08XX` | Other revenues and financing sources | Residual statewide category | Use for financing sources and similar residual items. |
| `09XX` | Non-revenue activities | Suspense and clearing activity | Clear to zero at year-end. |

## Fund-Type Constraints

| Fund group | Revenue families that usually fit | Constraint |
|---|---|---|
| Governmental (`AA`, `BA`, `CA`, `DA`, `EA`) | `01XX`-`06XX`, `08XX` | Match revenue family to legal purpose and restriction. |
| Proprietary (`FA`, `GA`) | `04XX`, `05XX`, `08XX` | Preserve charge-for-service logic. |
| Fiduciary (`HA`, `HB`, `HC`, `HD`) | `06XX`, `08XX`, limited `09XX` clearing | Preserve trust or custodial separation. |

## Appropriation Index and Budget Coding Patterns

| Pattern | SAAM or AFRS cue | Pass target |
|---|---|---|
| AI resolves fund and appropriation | AFRS AI table stores a 3-character reduction key for fund and appropriation coding. | One active AI row resolves. |
| Budgetary data stays in the posting key | SAAM 80.30.90 integrates budgetary data into the uniform account code structure. | Authority and actuals reconcile without a side spreadsheet. |
| Authority starts before spending | SAAM 85.10.20 and 85.15.10 require expenditure authority before expenditure posting. | Posting fails when authority is missing, expired, or inactive. |
| Allotment gates encumbrance and spending | SAAM 85.15.15 records approved allotments through budgetary entries. | Available allotment stays nonnegative. |
| Reserve and reallotment stay explicit | SAAM 85.15.30 through 85.15.50 use reserve and reallotment entries. | Reserve transfers retain event lineage. |

## Account Code Posting Rules

| Rule | SAAM cue | Observable pass criteria |
|---|---|---|
| Fund or account stays self-balancing | 80.30.10 defines a self-balancing fiscal and accounting entity. | Trial balance nets to zero at fund or account grain. |
| Roll-up funds stay reporting-only | 80.30.10 combines accounts into roll-up funds for reporting. | Posting occurs at account level and reporting aggregates at roll-up level. |
| Source documents receive coding first | 80.30.84 starts accounting with source-document analysis and coding. | Receipt, voucher, payroll, and journal documents carry coding before posting. |
| Books of original entry preserve sequence | 80.30.84 posts similar transaction groups sequentially. | Journal and batch lineage remains reproducible. |
| Agency systems stay compatible with statewide systems | 80.30.88 assigns OFM statewide data ownership and approval controls. | New or modified systems emit AFRS-compatible fields. |
| Undistributed receipts use clearing logic | 85.24.70 records unknown account or revenue-source receipts in Account `01R`. | Clearing balance reaches zero. |
| Encumbrances reserve authority, not expense | 85.30.10 treats encumbrance as a commitment rather than an expenditure. | Encumbrance balances never inflate actual expenditure totals. |
| AP records include minimum payment metadata | 85.32.40 requires payee, identifier, voucher number, coding, received date, invoice data, and approvals. | Voucher audit record contains each minimum field. |

## Chapter Control Matrix

| Chapter | SAAM cue | Implementation rule |
|---|---|---|
| 20 | 20.15.40 principles 1-5 | Record approver role and accountability owner. |
| 20 | 20.22.10 and 20.22.20 | Classify fraud, improper payment, and information-security risks per workflow. |
| 20 | 20.15.30 | Retain evidence for agency head and CFO certification. |
| 30 | 30.20.20 | Apply capitalization thresholds and useful-life rules. |
| 30 | 30.40.20 | Drive extra controls from documented risk assessment. |
| 30 | 30.45.10 | Verify inventoriable assets at least once every other fiscal year. |
| 85 | 85.30.10 | Treat encumbrance as a commitment rather than an expenditure. |
| 85 | 85.32.50 | Measure invoice payment timeliness against due dates. |

## .NET Implementation Patterns

```csharp
public sealed record SaamPostingKey(
    string AgencyCode,
    string AccountCode,
    string AppropriationIndex,
    string FundCode,
    string ProgramIndex,
    string OrganizationIndex,
    string ObjectCode,
    string RevenueSourceCode,
    DateOnly PostingDate);

public sealed class SaamPostingValidator(
    IReferenceDataService referenceData,
    IBudgetAuthorityService budgetAuthority)
{
    public async Task ValidateAsync(SaamPostingKey key, CancellationToken cancellationToken)
    {
        await referenceData.RequireActiveAccountAsync(key.AccountCode, key.PostingDate, cancellationToken);
        await referenceData.RequireActiveAppropriationIndexAsync(key.AppropriationIndex, key.PostingDate, cancellationToken);
        await referenceData.RequireActiveProgramIndexAsync(key.ProgramIndex, key.PostingDate, cancellationToken);
        await referenceData.RequireActiveOrganizationIndexAsync(key.OrganizationIndex, key.PostingDate, cancellationToken);
        await referenceData.RequireObjectCodeAsync(key.ObjectCode, key.PostingDate, cancellationToken);
        await referenceData.RequireRevenueSourceAsync(key.RevenueSourceCode, key.PostingDate, cancellationToken);
        await budgetAuthority.RequireAvailableAuthorityAsync(key.AppropriationIndex, key.PostingDate, cancellationToken);
    }
}
```

## Verification Targets

| Check | Test | Pass |
|---|---|---|
| Object code validation | Validate posting date against active object catalog. | One active statewide object path resolves. |
| Revenue family validation | Compare revenue source family to authority type and fund purpose. | Source family aligns to funding support. |
| AI validation | Resolve AI against AFRS title data for the posting date. | Exactly one active AI row resolves. |
| AP timeliness | Measure vendor payments against due dates. | At least 95 percent of obligations pay by due date. |
| Internal control evidence | Review request, approval, receipt, and payment-release data. | Each controlled payment includes complete evidence. |
| Capital asset inventory | Review biennial or revolving verification records. | Each inventoriable asset shows current verification evidence. |

## Cross-References

| Skill | Use |
|---|---|
| `wa-state-afrs` | Use for AFRS file layouts, transaction codes, and title-table validation. |
| `fund-accounting-patterns` | Use for fund isolation, interfund rules, and balance classification design. |
| `budget-to-actual-tracking` | Use for authority, allotment, encumbrance, and actual variance reporting. |
