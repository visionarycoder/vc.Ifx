---
name: tax-accounting-patterns
title: Tax Accounting Patterns
description: Apply sales tax, use tax, sourcing, nexus, reporting, and liability-accounting patterns when indirect-tax calculation, accrual, filing, or reconciliation logic is in scope.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium-high
estimated_tokens: 1978
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - journal-entry-patterns
  - reconciliation-patterns
  - wa-state-saam
appliesTo: '**/*.{cs,csproj,sql,md,json,csv,xlsx}'
tags:
  - tax
  - sales-tax
  - use-tax
  - jurisdiction
  - 1099
  - washington-state
related_docs:
  - references/tax-implementation.md
---
# Tax Accounting Patterns

Agent applies indirect-tax accounting patterns for sales tax, use tax, nexus, sourcing, jurisdiction resolution, filing, payment, and reconciliation.

## When to Use

| Condition | Use |
|---|---|
| Customer invoices or receipts need sales-tax determination and liability tracking | Use this skill |
| Purchase flows need use-tax accrual or vendor-tax verification | Use this skill |
| Tax returns, remittance batches, or jurisdiction reconciliations are in scope | Use this skill |
| Washington State retail tax or B&O integration needs explicit tax treatment | Use this skill with `wa-state-saam` |

## When Not to Use

| Condition | Route |
|---|---|
| Work item covers journal drafting, approval, or posting without tax specialization | Use `journal-entry-patterns` |
| Work item covers tie-out workflow without tax-domain rules | Use `reconciliation-patterns` |
| Work item covers Washington State fiscal policy with no indirect-tax logic | Use `wa-state-saam` |
| Work item covers payroll withholding or income-tax accounting | Use a payroll or income-tax skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Tax registration footprint | Yes | Record registered jurisdictions and effective dates. |
| Transaction facts | Yes | Record ship-from, ship-to, bill-to, date, item type, and amount. |
| Product and service taxability | Yes | Record taxable, exempt, resale, freight, service, and bundle treatment. |
| Exemption evidence | Yes | Record certificate type, holder, expiration, and coverage. |
| GL mapping | Yes | Record tax codes, liability accounts, expense accounts, and clearing accounts. |
| Filing calendar | Yes | Record cadence by jurisdiction. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent classifies each line as taxable sale, exempt sale, taxable purchase, exempt purchase, or mixed-taxability activity. | Review taxability inputs and tax code assignment. | Every line carries one taxability outcome and one tax code. |
| 2 | Agent determines nexus, sourcing basis, filing party, jurisdiction stack, and effective-dated rates. | Compare footprint, marketplace role, address facts, and transaction date. | One governed jurisdiction stack applies to each line. |
| 3 | Agent calculates line tax, invoice tax, liability impact, and expense or asset impact. | Recalculate taxable base, rate stack, rounding, and exemption adjustments. | Stored tax equals deterministic recalculation and GL effect matches tax type. |
| 4 | Agent aggregates return populations, payments, credits, and adjustments by filing period. | Compare transaction subledger totals to filing workpapers. | Filing totals tie to governed transaction populations. |
| 5 | Agent reconciles tax subledger, remittance activity, and general-ledger balances. | Compare liability rollforward, return totals, and cash disbursements. | Opening balance plus activity minus remittance equals ending liability by jurisdiction. |

## Tax Type and Sourcing Decision Matrix

| Decision Topic | Use When | Jurisdiction Driver | Primary GL Effect | Pass |
|---|---|---|---|---|
| Sales tax collected from customers | Taxable sale occurs in a jurisdiction where the entity or marketplace filer collects tax. | Sales-tax nexus plus sale-date sourcing rule. | Credit sales-tax payable or clearing; keep revenue net of tax. | Customer tax posts to liability, not revenue. |
| Use tax accrued on purchases | Taxable purchase lacks full vendor-collected tax. | Purchase destination or place-of-use rule. | Debit tax expense or asset basis; credit use-tax payable. | Self-assessed tax posts with matching expense or asset effect. |
| Destination-based sourcing | Law sources tax to delivery, destination, service-benefit, or place-of-use location. | Ship-to, service-delivery, or usage location. | Liability posts to destination buckets. | Rate stack matches destination facts. |
| Origin-based sourcing | Law sources tax to seller location or order-acceptance location. | Ship-from or seller establishment. | Liability posts to origin buckets. | Rate stack matches origin facts. |
| Nexus determination present | Physical presence, threshold, inventory, employees, services, or marketplace rules create collection duty. | Effective-dated nexus evidence. | Record tax liability and return obligation. | Filing obligation exists for the period. |
| Nexus determination absent | No seller collection duty exists on the transaction date. | No effective nexus evidence for the transaction. | Omit customer sales-tax liability and keep purchase-side use-tax review open. | Seller collection stays off. |

## Tax Jurisdiction Hierarchy

| Layer | Role | Data Elements | Pass |
|---|---|---|---|
| State | Primary registration, return, and statewide rate layer | State code, registration id, cadence, statewide rate | Every taxable transaction resolves one state layer. |
| County | County surcharge or local layer | County code, county rate, effective dates | County layer applies only inside the county boundary. |
| City | City or municipal layer | City code, city rate, sourcing rule, effective dates | City layer aligns to the municipal boundary. |
| District | Special-purpose district layer | District code, type, rate, boundary source | District layer applies only to matching boundaries. |
| Composite stack | Ordered sum of the applicable layers | Composite rate, rounding rule, filing bucket ids | Stored composite rate equals the sum of active layers. |

## Tax Code to GL Mapping Matrix

| Tax Code Pattern | Use | Debit | Credit | Pass |
|---|---|---|---|---|
| `SALE-ST-<STATE>` | Customer state sales tax | Accounts receivable or cash | Sales-tax payable state | Collected tax bypasses revenue. |
| `SALE-LOCAL-<JUR>` | Customer local sales tax | Accounts receivable or cash | Sales-tax payable local | Local liability ties to jurisdiction detail. |
| `USE-EXP-<STATE>` | Use tax on expensed purchases | Tax expense or source expense | Use-tax payable | Self-assessed tax increases expense. |
| `USE-CAP-<STATE>` | Use tax on capitalizable purchases | Fixed asset or inventory basis | Use-tax payable | Asset basis includes the tax. |
| `TAX-CLEARING` | Filing and remittance staging | Sales-tax payable or use-tax payable | Tax clearing or cash | Remittance preserves a clearing trail. |
| `BO-WA` | Washington B&O tax | B&O tax expense | B&O payable or cash | B&O stays separate from sales-tax payable. |
| `1099-WITHHOLD` | Backup withholding linked to vendor disbursement | Vendor payable | Federal tax withholding payable | 1099 flows stay separate from indirect-tax liabilities. |

## Tax Liability Versus Tax Expense

| Pattern | Liability Treatment | Expense Treatment | Pass |
|---|---|---|---|
| Sales tax collected from customers | Record a liability until remittance. | No tax expense arises from pass-through customer collections. | Revenue remains net of collected tax. |
| Use tax on operating expense | Record a liability until remittance. | Record tax expense in the benefiting function or tax expense account. | Expense and liability arise in the same event. |
| Use tax on capital or inventory purchase | Record a liability until remittance. | Capitalize the tax into asset or inventory basis. | Basis and payable tie to the same purchase evidence. |
| Vendor-collected purchase tax | Purchaser liability stays absent when the vendor collected full tax. | Tax stays in purchase cost or tax expense. | No duplicate self-assessed liability appears. |
| B&O or gross-receipts tax | Record payable when the reporting period closes or the return accrues. | Record tax expense because the entity bears the tax. | B&O never enters sales-tax payable accounts. |

## Reference Files

| File | Purpose |
|---|---|
| [references/tax-implementation.md](references/tax-implementation.md) | Detailed jurisdiction lookup, rate management, exemption, use-tax accrual, reporting, .NET implementation, Washington State B&O, 1099 integration, and code examples. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter | Run `npm run frontmatter:validate`. | Validation returns zero errors for the new skill files. |
| STE wording | Run the repository modal-term scan on the new skill files. | Scan returns zero banned-term matches. |
| Tax type coverage | Review the decision matrix and tax-type table. | Sales tax, use tax, destination-based, origin-based, and nexus decisions appear with GL effect. |
| Hierarchy coverage | Review the jurisdiction hierarchy table. | State, county, city, district, and composite stack rows appear. |
| GL mapping coverage | Review the tax-code mapping matrix. | Sales-tax payable, use-tax payable, B&O, and 1099-related mapping rows appear. |
| Reference linkage | Open the relative reference link. | `references/tax-implementation.md` resolves from `SKILL.md`. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Sales tax posts as revenue | Agent credits liability accounts and keeps revenue net of collected tax. |
| Use tax accrual duplicates vendor-collected tax | Agent verifies invoice tax detail before self-assessment. |
| Composite tax rate ignores one local layer | Agent resolves the full jurisdiction stack and stores the component rows. |
| Expired exemption certificate suppresses tax | Agent validates certificate status and effective date before exemption treatment. |
| Washington B&O and retail sales tax share one liability bucket | Agent separates B&O expense and payable from customer sales-tax liabilities. |
| Marketplace-collected tax mixes with seller-collected tax | Agent stores the filing party and remittance owner on every taxable line. |

## Outputs

- Tax-type, nexus, and sourcing decision guidance
- Jurisdiction hierarchy and rate-resolution patterns
- Tax-code-to-GL mapping rules
- Liability, expense, and filing reconciliation controls
- Reference implementation path for .NET tax services
