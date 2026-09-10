---
name: interfund-accounting-patterns
title: Interfund Accounting Patterns
description: Apply GASB-aligned interfund classification, paired posting, reconciliation, and elimination patterns when governmental fund loans, transfers, reimbursements, or shared services are in scope.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium-high
estimated_tokens: 1680
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - fund-accounting-patterns
  - public-sector-accounting
  - double-entry-accounting
  - reconciliation-patterns
appliesTo: '**/*.{cs,csproj,sql,md,json,csv}'
tags:
  - interfund
  - fund-accounting
  - gasb
  - governmental
  - transfers
  - eliminations
---
# Interfund Accounting Patterns

Agent classifies interfund activity, creates reciprocal entries, enforces balancing, and prepares GASB-aligned reporting views.

## When to Use

| Condition | Use |
|---|---|
| Work item needs short-term borrowing between funds | Use this skill |
| Work item needs permanent resource movement between funds | Use this skill |
| Work item needs service billing, reimbursement, or shared cost allocation across funds | Use this skill |
| Work item needs government-wide elimination of internal balances or transfers | Use this skill |
| Posting design needs one transaction envelope that creates both fund sides | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work item focuses on broad fund setup or fund balance classification only | Use `fund-accounting-patterns` |
| Work item focuses on broad governmental accounting policy outside interfund scope | Use `public-sector-accounting` |
| Work item focuses on baseline debit-credit structure only | Use `double-entry-accounting` |
| Work item focuses on reconciliation workflow only | Use `reconciliation-patterns` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Source fund and destination fund | Yes | Record both fund ids and reporting entity. |
| Transaction purpose | Yes | Record loan, transfer, service, allocation, or reimbursement purpose. |
| Settlement expectation | Yes | Record repayment expectation, due date, or permanent movement basis. |
| Amount basis | Yes | Record direct amount, service units, or allocation driver. |
| Approval basis | Yes | Record authority, approver, and effective period. |
| Reporting scope | Yes | Record fund-statement, government-wide, or consolidated objective. |

## Interfund Transaction Type Matrix

| Type | GASB Class | Use When | Posting Pattern | Reporting Result |
|---|---|---|---|---|
| Due to or due from | Reciprocal interfund activity | One fund lends current resources to another fund and repayment expectation exists. | Lending fund records `DueFromOtherFunds`; borrowing fund records `DueToOtherFunds`. | Fund statements show reciprocal receivable and payable until settlement. |
| Transfer | Nonreciprocal interfund activity | Resource movement is permanent and no repayment expectation exists. | Source fund records transfer out; destination fund records transfer in. | Fund statements show other financing use and source or equivalent transfer captions. |
| Services provided and used | Reciprocal interfund activity | One fund delivers measurable goods or services for value received. | Provider records revenue; consumer records expenditure or expense. | Operating results stay in revenue and expenditure or expense classifications. |
| Shared service allocation | Reciprocal interfund activity | A pooled support cost is distributed by a documented driver such as FTE or square footage. | Service fund clears pooled cost; participating funds record expenditure or expense. | Statements show distributed support cost by consuming fund. |
| Reimbursement | Intra-entity correction pattern | One fund repays another for a cost paid on its behalf. | Original payer reduces expenditure or expense; benefiting fund records the cost. | Statements avoid false revenue and false transfer volume. |

## Transaction Choice Matrix

| Question | Yes | No |
|---|---|---|
| Repayment expectation exists. | Use due to or due from. | Evaluate transfer, service, or reimbursement. |
| Goods or services changed hands for value received. | Use services provided and used. | Evaluate transfer, reimbursement, or allocation. |
| One fund paid first for another fund and no service sale exists. | Use reimbursement. | Evaluate service or transfer. |
| Resource movement is permanent. | Use transfer. | Evaluate due to or due from, service, or reimbursement. |
| One cost pool needs distribution by formula. | Use shared service allocation. | Use direct service billing or another explicit pattern. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent classifies the event before account selection. | Review purpose, repayment expectation, and source support. | One transaction type fits and competing types stay rejected. |
| 2 | Agent generates both fund sides from one transaction envelope. | Sum debits and credits by fund and by transaction. | Each fund balances internally and the full event balances globally. |
| 3 | Agent applies type-specific controls. | Review due date, allocation driver, approval, and account map. | Required evidence exists for the selected type. |
| 4 | Agent stores reciprocal references and counterparty fund data. | Review transaction and leg records. | Both sides point to one interfund transaction id. |
| 5 | Agent reconciles open balances and open service settlements by fund pair and period. | Run interfund reconciliation. | Reciprocal balances tie exactly within configured precision. |
| 6 | Agent prepares reporting presentation or eliminations. | Review reporting entity and activity type. | Fund statements, government-wide statements, and consolidated views classify the activity correctly. |

## GASB Classification and Use Rules

| Rule Area | Rule | Test | Pass |
|---|---|---|---|
| Short-term financing | Agent uses due to or due from only when current resources move temporarily. | Review settlement date and borrowing purpose. | Repayment expectation is explicit. |
| Permanent support | Agent uses transfer only when repayment is absent and authority exists. | Review approval and policy basis. | Transfer support is permanent and approved. |
| Reciprocal service value | Agent uses services provided and used when value is received by the paying fund. | Review units, rate, or service catalog. | Charge ties to delivered service. |
| Allocation method | Agent uses shared allocation only with a reproducible driver and denominator. | Recalculate the allocation. | Distributed total equals the source pool. |
| Cost correction | Agent uses reimbursement to reclassify cost, not to create revenue. | Review original posting and correction posting. | False revenue stays absent. |

## Balancing Rules

| Rule | Test | Pass |
|---|---|---|
| Due to total equals due from total across all in-scope funds for each open period. | Sum open reciprocal balances by account family. | Net interfund balance equals zero. |
| Transfer out total equals transfer in total by approval document and period. | Compare transfer schedules by fund pair. | Net transfer effect equals zero inside the reporting entity. |
| Each interfund transaction has one source id and two or more fund legs. | Review stored leg records. | Every leg points to the same envelope id. |
| Allocation output equals the pooled cost after approved rounding. | Sum distributed amounts and compare to the source pool. | No orphan remainder remains. |

## Elimination Requirements for Consolidated Reporting

| Reporting View | Agent Action | Pass |
|---|---|---|
| Governmental fund statements | Agent presents interfund balances and transfers by fund as recorded. | Receivable, payable, transfer, service, and reimbursement effects remain visible by fund. |
| Government-wide statements | Agent eliminates internal balances and transfers within the same activity column. | Same-activity internal amounts do not inflate consolidated totals. |
| Cross-activity balances | Agent reports residual governmental-to-business-type balances as `InternalBalances` until settlement. | Cross-activity residuals remain visible. |
| Cross-activity transfers | Agent presents governmental-to-business-type transfers in transfer presentation, not as operating revenue. | Activity-to-activity movement remains explicit. |
| Consolidated reporting boundary | Agent eliminates balances and activity only inside the selected reporting entity. | External-party amounts remain in the statements. |
| Reimbursements | Agent reclassifies cost rather than reporting revenue or transfer volume. | Duplicated expense disappears and correct cost remains. |

## Reference Files

| File | Purpose |
|---|---|
| [references/interfund-implementation.md](references/interfund-implementation.md) | Detailed .NET entity model, paired-entry generation, EF Core mapping, loan tracking, allocation, elimination, and reconciliation patterns. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter | Run `npm run frontmatter:validate` or the repository validator directly when the `npm` script path is misconfigured. | Validation passes for the new files. |
| STE wording | Run the repository STE violation scan on the new files. | Scan returns zero matches. |
| Paired posting | Run targeted tests for due to or due from, transfer, service, allocation, and reimbursement scenarios. | Each scenario produces the expected balanced legs. |
| Reciprocal tie-out | Sum due to and due from balances by fund pair and period. | Open due to total equals open due from total exactly within configured precision. |
| Transfer tie-out | Compare transfer out to transfer in schedules. | Totals match by approval document and period. |
| Elimination correctness | Prepare same-activity and cross-activity statement samples. | Same-activity internal amounts are eliminated and residual cross-activity balances remain visible. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Repayable financing is posted as a transfer | Agent records a due to or due from pair and tracks repayment. |
| Reimbursement is posted as revenue | Agent reduces the original payer cost and records the cost in the benefiting fund. |
| Shared allocation uses an unsupported percentage | Agent stores the driver population and denominator for each run. |
| Import logic allows one-sided interfund posting | Agent generates both fund sides from one envelope and rejects partial persistence. |
| Consolidated reporting eliminates cross-activity residuals completely | Agent preserves `InternalBalances` between governmental and business-type activities. |

## Outputs

- Interfund transaction type matrix
- GASB classification and use guidance
- Balancing and reconciliation rules
- Elimination rules for government-wide and consolidated reporting
- Detailed implementation reference for .NET posting and reporting flows
