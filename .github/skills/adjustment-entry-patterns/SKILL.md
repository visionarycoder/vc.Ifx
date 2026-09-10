---
name: adjustment-entry-patterns
title: Adjustment Entry Patterns
description: Apply accounting adjustment-entry selection, approval, and period-control rules when posted balances need correction, accrual, reversal, reclassification, or prior-period treatment.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium-high
estimated_tokens: 1980
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - journal-entry-patterns
  - double-entry-accounting
  - closing-cycle-patterns
  - audit-trail-compliance
appliesTo: '**/*.{cs,csproj,sql,md,json,yml,yaml}'
tags:
  - adjustment
  - journal-entry
  - accrual
  - correction
  - accounting
related_docs:
  - references/adjustment-implementation.md
---
# Adjustment Entry Patterns

Agent applies adjustment-entry selection, approval, linkage, and period-control patterns when posted balances need controlled follow-up activity.

## When to Use

| Condition | Use |
|---|---|
| Prompt needs correcting, reversing, accrual, reclassification, or prior-period entry behavior | Use this skill |
| Work item changes posted balances without editing the source entry in place | Use this skill |
| Close flow needs adjustment approvals, linkage, and period-lock rules | Use this skill |
| .NET services need adjustment commands, handlers, and audit evidence | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work item covers entry drafting, review, and posting lifecycle without adjustment specialization | Use `journal-entry-patterns` |
| Work item covers debit-credit fundamentals, chart of accounts, or trial balance rules only | Use `double-entry-accounting` |
| Work item covers full close calendar sequencing or cutoff task orchestration | Use `closing-cycle-patterns` |
| Work item covers control evidence or retention without adjustment logic | Use `audit-trail-compliance` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Source entry policy | Yes | Record posted-entry immutability and source-link requirements. |
| Period status policy | Yes | Record open, soft-close, hard-close, and post-close rules. |
| Approval matrix | Yes | Record approver roles, thresholds, and segregation rules by type. |
| Reason and evidence policy | Yes | Record memo, ticket, source document, and support-package requirements. |
| Reversal policy | Yes | Record next-period date rule and reversal suppression conditions. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent classifies the request as correcting, reversing, accrual, reclassification, or prior-period. | Compare request facts to the decision matrix. | One entry type fits the request. |
| 2 | Agent validates source status, period state, support package, and balance. | Read source entry, period state, and requested effective date. | Invalid source references, closed-period violations, and unbalanced lines stop before draft creation. |
| 3 | Agent applies approval, segregation, and evidence rules for the selected type. | Compare entry header to the approval matrix. | Required approver roles, reason codes, and support references remain complete. |
| 4 | Agent posts the adjustment as a new linked entry. | Inspect persistence path and ledger rows. | Source entry remains immutable and follow-up entry stores source linkage. |
| 5 | Agent verifies period effect and reversal effect where the type uses one. | Run targeted scenario tests. | Period balances, next-period behavior, and linkage rules match policy. |

## Entry Type Decision Matrix

| Entry Type | Use When | Do Not Use When | Source Link | Period Target | Result |
|---|---|---|---|---|---|
| Correcting entry | A posted entry contains amount, account, dimension, date, or description error. | The work item records routine month-end matching activity. | Required to the original posted entry or batch. | Same period when open; current adjustment period when closed. | Net balances reflect intended results without mutating history. |
| Reversing entry | A valid accrual or estimate needs automatic offset in the next period. | The source entry is a permanent classification change or a discovered error. | Required to the source accrual or estimate. | Next open period. | Next period starts without duplicated accrual impact. |
| Accrual entry | Revenue or expense belongs to the current period even though settlement occurs later. | Activity already posted in the correct period. | Optional to source document; required to accrual support package. | Current reporting period. | Period activity matches earned or incurred activity. |
| Reclassification entry | Amount is valid but account or reporting classification is wrong. | The amount itself is wrong. | Required to the source posted entry or source balance evidence. | Same period when open; current adjustment period when locked. | Presentation changes without changing overall net position. |
| Prior period adjustment | A rare material condition affects previously issued results and policy routes the case through formal approval. | The issue fits normal current-period correction, accrual, or reclassification treatment. | Required to source entry, approval package, and disclosure package. | Controlled post-close or restatement period. | Prior-period effect stays explicit, approved, and auditable. |

## Approval Workflow Requirements by Type

| Entry Type | Minimum Roles | Segregation Rule | Escalation Trigger | Pass |
|---|---|---|---|---|
| Correcting entry | Preparer, approver | Preparer and approver stay different. | Materiality threshold, sensitive account, or cross-entity impact | Posted correction contains required approvals and actor separation. |
| Reversing entry | Source approver or scheduled reversal service plus reviewer | Reversal service identity stays separate from manual edit identity. | Manual override of reverse date or amount | Reverse entry links to approved source and logs scheduler or override actor. |
| Accrual entry | Preparer, accounting reviewer | Requestor stays outside final approval where policy separates duties. | Estimate exceeds threshold or prior estimate variance exceeds tolerance | Accrual stores reviewer decision, support calculation, and cutoff evidence. |
| Reclassification entry | Preparer, account owner or controller | Actor who benefits from classification move stays outside final approval. | Statement-line impact, grant restriction impact, or fund movement | Reclass entry stores approval and target classification rationale. |
| Prior period adjustment | Preparer, controller, finance lead, executive or audit reviewer per policy | No single actor performs preparation, approval, and posting. | All cases | Approval chain is complete before posting and disclosure reference exists. |

## Period Locking and Post-Close Adjustment Rules

| Period State | Correcting | Reversing | Accrual | Reclassification | Prior Period Adjustment |
|---|---|---|---|---|---|
| Open | Post to source period when policy approves. | Schedule next-period reverse date from the next open period. | Post to current reporting period. | Post to source period when correction belongs there. | Use only when policy classifies the case as prior-period. |
| Soft close | Route through close approver. | Keep reverse date unless finance approves change. | Allow close accruals with cutoff evidence. | Allow with controller review. | Route to post-close workflow. |
| Hard close | Post to current adjustment period with source linkage. | Execute only when the reverse period is open. | Post current-period accrual only. | Post current-period reclass only. | Use controlled post-close or restatement period only. |
| Issued statements | Preserve issued-period history and use current-period or restatement workflow. | Preserve scheduled reverse logic with audit note. | Use current-period accrual treatment only. | Use current-period reclass plus disclosure when material. | Use formal prior-period adjustment path only. |

## Post-Close Adjustment Decision Rules

| Scenario | Agent Action | Pass |
|---|---|---|
| Source period is locked and immaterial | Agent posts current-period correction or reclassification with source-period reference. | Current-period entry explains prior-period origin and reporting impact stays traceable. |
| Source period is locked and material | Agent routes the case to prior-period approval or restatement workflow. | Approval package and disclosure reference exist before posting. |
| Close package already issued | Agent preserves prior statements and records explicit post-close treatment. | No back-dated overwrite occurs in issued reporting history. |
| Auto-reversal date lands in a closed period | Agent shifts reversal to the next open period and records the shift reason. | Reversal date logic is deterministic and auditable. |

## Reference Files

| File | Purpose |
|---|---|
| [references/adjustment-implementation.md](references/adjustment-implementation.md) | Detailed .NET, EF Core, approval, audit, and period-control patterns for all five adjustment types. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter | Run `npm run frontmatter:validate`. | Validation returns zero errors for the new files. |
| STE wording | Run the repository modal-term scan on the new skill files. | Scan returns zero banned-term matches. |
| Decision matrix coverage | Review the entry-type matrix. | All five requested entry types appear with use case, non-use case, linkage, and period treatment. |
| Approval coverage | Review the approval workflow matrix. | Each entry type lists roles, segregation rule, escalation trigger, and pass condition. |
| Period control coverage | Review the period-lock matrix and post-close rules. | Open, soft-close, hard-close, and issued-statement cases remain explicit. |
| Reference linkage | Open the relative reference link. | `references/adjustment-implementation.md` resolves from `SKILL.md`. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Posted entry gets edited in place | Agent writes a new linked adjustment entry and preserves the source entry. |
| Reversal entry gets used to fix an error | Agent uses a correcting entry for errors and reserves reversing entries for temporary estimates or accruals. |
| Accrual lacks support package | Agent records calculation detail, cutoff evidence, and reviewer approval before posting. |
| Reclassification changes net activity unexpectedly | Agent uses equal and opposite dual posting so overall net balance stays unchanged. |
| Prior-period path becomes routine cleanup | Agent reserves prior-period adjustment flow for rare, policy-approved cases and routes normal cleanup through current-period adjustment paths. |
