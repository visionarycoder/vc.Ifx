---
name: multi-currency-accounting
title: Multi-Currency Accounting
description: Apply functional-currency, foreign-exchange, translation, and remeasurement patterns when accounting workflows span transaction, entity, and reporting currencies.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: high
estimated_tokens: 1980
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - double-entry-accounting
  - journal-entry-patterns
  - financial-reporting-gaap
related_docs:
  - references/fx-implementation.md
appliesTo: '**/*.{cs,csproj,sql,json,md}'
tags:
  - multi-currency
  - foreign-exchange
  - fx
  - translation
  - remeasurement
  - gaap
---
# Multi-Currency Accounting

Agent applies ASC 830 and IAS 21 aligned patterns for entities that record, remeasure, translate, and consolidate balances across more than one currency.

## When to Use

| Prompt or Code Shape | Use |
|---|---|
| Transaction currency differs from the entity operating currency | Agent uses this skill. |
| Consolidation package needs local currency, functional currency, and reporting currency flows | Agent uses this skill. |
| Work item needs exchange-rate storage, foreign currency journal logic, or FX gain-loss recognition | Agent uses this skill. |
| .NET services need deterministic translation, remeasurement, or CTA calculation | Agent uses this skill. |
| Reporting package needs multi-currency trial balance or reporting-currency consolidation | Agent uses this skill. |

## When Not to Use

| Prompt or Code Shape | Route |
|---|---|
| Work item covers only balanced journals and ledger fundamentals | Agent uses `double-entry-accounting`. |
| Work item covers only entry approval, reversal, or adjustment workflow | Agent uses `journal-entry-patterns`. |
| Work item covers only statement assembly and disclosure packaging | Agent uses `financial-reporting-gaap`. |
| Work item covers only hedge accounting or derivative valuation | Agent uses a treasury or risk-management skill. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Entity and ledger scope | Yes | Agent records legal entity, branch, business unit, and ledger identifiers. |
| Functional currency policy | Yes | Agent records the primary economic environment and supporting indicators. |
| Transaction currency policy | Yes | Agent records which source documents arrive in foreign currency and at what grain. |
| Reporting currency set | Yes | Agent records parent reporting currency and comparative periods. |
| Exchange-rate catalog | Yes | Agent records spot, average, closing, and historical rate series plus source ownership. |
| Gain-loss policy | Yes | Agent records realized, unrealized, CTA, and remeasurement destinations. |
| Precision and rounding rule | Yes | Agent records currency decimal scale, rate scale, and posting tolerance. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent determines each entity functional currency from cash-flow, sales-price, cost, financing, and autonomy evidence. | Review entity policy matrix. | Every entity has one approved functional currency and one evidence set. |
| 2 | Agent records foreign currency transactions in the transaction currency and functional-currency equivalent at the transaction-date spot rate or approved approximation. | Recalculate sample transactions. | Stored functional amounts match the approved rate source within tolerance. |
| 3 | Agent remeasures non-functional-currency balances into functional currency when books are maintained in a different currency. | Review remeasurement output by account type. | Monetary items use current rates, non-monetary items use historical rates, and gain-loss lines land in income. |
| 4 | Agent translates functional-currency financial statements into reporting currency for consolidation. | Review translation output and CTA rollforward. | Assets and liabilities use closing rates, income and expenses use average or transaction-date rates, equity uses historical rates, and CTA lands in equity. |
| 5 | Agent consolidates translated balances, intercompany eliminations, and reporting packages in one reporting currency. | Compare child totals, eliminations, and parent totals. | Consolidated balances reconcile to translated trial balances and elimination journals. |
| 6 | Agent validates realized and unrealized FX recognition, CTA movement, and disclosure support. | Run targeted reconciliation and report checks. | Income-statement FX lines, equity CTA, and rate-source evidence agree with policy. |

## Multi-Currency Decision Matrix

| Situation | Primary Question | Approach | Rate Basis | Recognition Destination | When to Use |
|---|---|---|---|---|---|
| Functional currency determination | Which currency drives cash inflows, pricing, costs, and financing? | Select one functional currency per entity. | Policy evidence, not rate math. | No journal entry at determination time. | Use at entity onboarding, restructure, or operating-environment change. |
| Foreign currency transaction | Did the source transaction occur in a currency other than the functional currency? | Record the transaction in source currency and convert to functional currency. | Spot rate on the transaction date or approved daily rate. | Settlement differences flow to realized gain or loss; open-item revaluation flows to unrealized gain or loss. | Use for invoices, receipts, payables, loans, and cash activity in foreign currency. |
| Remeasurement | Are the books kept in a currency other than the functional currency? | Remeasure ledger balances into functional currency. | Current rate for monetary items, historical rate for non-monetary items, rate by recognition pattern for income and expense. | Net remeasurement gain or loss flows to income. | Use for branches or ledgers that post in local currency while functional currency differs. |
| Translation | Does the entity functional currency differ from the parent reporting currency? | Translate the full financial statements into reporting currency. | Closing, average, and historical rates by statement area. | Translation adjustment flows to cumulative translation adjustment in equity. | Use for consolidation and external reporting when the subsidiary functional currency remains its local currency. |
| Reporting currency consolidation | Does the group publish statements in one parent currency? | Aggregate translated child balances and elimination entries in the parent currency. | Reporting-currency balances from translated children plus reporting-currency eliminations. | Consolidated FX effects stay in translated income or CTA based on origin. | Use for monthly close, quarter-end, year-end, and management reporting packages. |

## Functional Currency Determination Matrix

| Indicator | Evidence | Strong Signal |
|---|---|---|
| Sales price currency | Customer invoices, contracts, and price lists | Revenue prices remain anchored to one currency. |
| Cash receipt currency | Collections, settlement bank accounts, and treasury sweeps | Cash inflows concentrate in one currency. |
| Cost currency | Payroll, materials, overhead, and local operating spend | Operating costs concentrate in one currency. |
| Financing currency | Debt service, capital injections, and working-capital support | Financing and retained cash management center on one currency. |
| Intercompany dependency | Degree of local autonomy and intercompany funding dependence | Autonomous operations support local functional currency; dependent operations support parent-centered functional currency. |

## Exchange Rate Type Matrix

| Rate Type | Use | Test | Pass |
|---|---|---|---|
| Spot rate | Measure transaction-date activity and direct settlements. | Compare stored rate to approved rate table on the transaction date. | Rate matches the approved source and effective date. |
| Daily rate | Approximate high-volume same-day transactions when policy allows. | Compare daily rate to intraday spot samples. | Variance stays inside policy tolerance. |
| Average rate | Translate recurring income and expense when period volatility stays acceptable. | Compare average-rate translation to transaction-level sample. | Sample variance stays inside policy tolerance. |
| Closing rate | Translate ending assets and liabilities and revalue monetary balances. | Compare balance translation date to close calendar. | Rate date equals the reporting or valuation date. |
| Historical rate | Translate equity and non-monetary items tied to original recognition. | Trace the stored rate back to the originating transaction or event. | Historical rate remains immutable after posting. |

## Gain and Loss Recognition Matrix

| Scenario | Recognition | Destination | Pass |
|---|---|---|---|
| Foreign currency receivable or payable settles at a rate different from original recognition | Realized gain or loss | Income statement FX gain-loss accounts | Settlement journal clears the open item and records the rate difference. |
| Monetary balance stays open at period end | Unrealized gain or loss | Income statement FX revaluation accounts | Revaluation journal reverses or refreshes in the next valuation cycle per policy. |
| Ledger currency differs from functional currency | Remeasurement gain or loss | Income statement remeasurement accounts | Net remeasurement impact equals the plug required to balance the functional-currency trial balance. |
| Functional currency differs from reporting currency | Translation adjustment | CTA in equity or other comprehensive income based on policy framework | Consolidated equity rollforward includes the CTA movement. |

## .NET Implementation Guidance

| Concern | Agent Action |
|---|---|
| Domain model | Agent separates transaction currency, functional currency, and reporting currency on journals, subledgers, and reporting snapshots. |
| Value objects | Agent uses value objects for `CurrencyCode`, `Money`, `ExchangeRateKey`, and `RateType`. |
| Rate governance | Agent stores effective dates, source system, publication timestamp, inversion policy, and superseded flags for each rate. |
| Posting flow | Agent posts base journal lines in functional currency and preserves source-currency amounts for open-item revaluation and settlement. |
| Consolidation | Agent snapshots translated trial balances before elimination and statement assembly. |
| Auditability | Agent preserves the selected rate, rate date, rate type, calculation path, and prior valuation link on every FX-sensitive posting. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter | Run `npm run frontmatter:validate`. | Validation returns zero errors. |
| STE wording | Run the repository STE scan on the new files. | Scan returns zero modal-verb matches. |
| Functional currency assignment | Review entity matrices against policy evidence. | Every entity has one assigned functional currency with traceable support. |
| Transaction measurement | Recalculate sampled transactions at stored spot or daily rates. | Sampled functional amounts reconcile within configured tolerance. |
| Remeasurement | Reperform one period-end remeasurement by account type. | Monetary and non-monetary balances use the expected rate class and the plug lands in income. |
| Translation | Reperform one subsidiary translation and CTA rollforward. | Statement areas use the expected rate class and CTA equals the balancing difference. |
| Consolidation | Compare translated child balances, eliminations, and parent totals. | Consolidated reporting currency totals reconcile without unexplained FX variance. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Entity uses one currency for bookkeeping and another for policy without explicit functional-currency evidence | Agent records a determination memo and applies remeasurement before translation. |
| Historical rates overwrite prior equity or non-monetary rates | Agent stores immutable originating rates and links them to the source event. |
| Unrealized revaluation journals remain in place after settlement | Agent links valuation, reversal, and settlement journals by open-item identifier. |
| CTA lands in income | Agent routes translation differences to equity and keeps remeasurement differences in income. |
| Consolidation mixes local, functional, and reporting balances in one layer | Agent snapshots each layer separately and reconciles between layers. |

## Outputs

- Functional-currency decision matrix
- Transaction, remeasurement, and translation workflow guidance
- Exchange-rate governance matrix
- Gain-loss recognition guidance
- Reporting-currency consolidation validation checklist
