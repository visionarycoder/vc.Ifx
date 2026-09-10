---
title: Fiscal Period Close Checklist
doc_type: reference
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: high
estimated_tokens: 7200
prerequisites:
  - closing-cycle-patterns
  - wa-state-saam
  - wa-state-afrs
related_skills:
  - closing-cycle-patterns
  - wa-state-saam
  - wa-state-afrs
  - reconciliation-patterns
  - audit-trail-compliance
related_docs:
  - ../SKILL.md
  - ../../wa-state-saam/SKILL.md
  - ../../wa-state-afrs/SKILL.md
tags:
  - closing
  - month-end
  - year-end
  - fiscal-period
  - washington-state
---
# Fiscal Period Close Checklist

This reference gives the `closing-cycle-patterns` skill one comprehensive checklist for month-end, quarter-end, and year-end close work in governmental accounting environments with Washington State AFRS and SAAM touchpoints.

## Reference Alignment

| Source | Reference Use | Observable Pass |
|---|---|---|
| `../SKILL.md` | Establishes close-run ownership, dependency, evidence, and reopen governance patterns. | The close workflow records one owner, one dependency state, and one evidence path for every task. |
| `../../wa-state-saam/SKILL.md` | Establishes statewide policy alignment, fiscal-period control, appropriation context, and internal-control evidence. | The close workflow stores period state, statewide coding context, and sign-off evidence. |
| `../../wa-state-afrs/SKILL.md` | Establishes AFRS submission, reconciliation, reject handling, and batch trace rules. | The close workflow links every AFRS batch to one run, one acknowledgement, and one correction path. |
| `reconciliation-patterns` | Establishes exception routing and tie-out discipline across subledgers, bank activity, and statewide reports. | Every unresolved difference carries one owner, one reason, and one due point. |
| `audit-trail-compliance` | Establishes evidence retention, sign-off trace, and controlled reopen handling. | Every close event stores timestamp, actor, source artifact, and approval state. |

## Close Calendar and State Model

| State | Entry Condition | Exit Condition | Observable Pass |
|---|---|---|---|
| Open | Routine posting, interface processing, and correction work remain active. | Controller moves the period to soft close. | The system accepts routine posting and stamps the current period as open. |
| Soft close | Team limits discretionary late entries and starts final tie-out review. | Blocking reconciliations and core adjustments finish. | The unresolved-item list shrinks to approved exceptions only. |
| Hard close | Routine posting stops and final statements freeze for the period. | Approver signs the close package or authorizes a controlled reopen. | No routine posting enters the closed period and final statements tie to final ledger balances. |
| Post-close adjustment window | Only approved corrections with explicit evidence enter the closed period. | Window end arrives or approver closes the window early. | Every correction links to one approval, one reason, and one replacement statement package. |
| Reopened | Controller reopens the period for one defined correction scope. | Controller re-hard-closes the period after the correction package clears review. | Reopen reason, scope, actor, timestamp, and replacement outputs remain queryable. |

## Ownership and Sign-Off Roles

| Role | Core Responsibility | Evidence Produced | Observable Pass |
|---|---|---|---|
| Preparer | Completes assigned checklist steps, attaches source schedules, and explains differences. | Reconciliations, schedules, journal support, and checklist completion marks. | Every completed task includes one preparer identity and one support package. |
| Reviewer | Recomputes critical balances, challenges unsupported entries, and clears or returns exceptions. | Review notes, recalculations, and accepted or returned status. | Every critical task includes one reviewer action after preparer completion. |
| Approver | Accepts the close package, authorizes post-close entries, and authorizes controlled reopens. | Final sign-off record, approval timestamp, and scope statement. | One final approval record closes the run and no later change appears without a new approval trail. |
| AFRS Interface Owner | Builds, validates, submits, and reconciles AFRS-facing batches. | Batch snapshot, validation report, acknowledgement, and resubmission lineage. | Every submitted batch links to one close run and one outcome state. |
| Budget Owner | Verifies appropriation, allotment, and budget-to-actual positions. | Budget variance pack and authority exception log. | Material budget variances include documented cause and follow-up action. |
| Treasury or Cash Owner | Verifies cash activity, bank reconciliation, and investment positions. | Bank recs, cash proof, and investment support. | Cash balances tie across bank, book, and statewide reporting views. |

## Month-End Close Checklist

The month-end checklist uses a standard progression from intake, reconciliation, accruals, reporting, and sign-off. Agencies adjust relative due points to match their published close calendar.

| # | Workstream | Team Action | Evidence | Observable Pass |
|---:|---|---|---|---|
| 1 | Close launch | Controller publishes the period close calendar, owners, due points, and blocker escalation path. | Approved close calendar and ownership matrix. | Every checklist item has one owner and one due point before task work starts. |
| 2 | Source intake | Operations team confirms that feeder systems, import jobs, and file drops completed for the target period. | Interface completion log and intake dashboard snapshot. | No required source feed remains missing at close start. |
| 3 | Change control | Application owner freezes nonessential master-data and interface changes for the close window. | Freeze notice and deployment log. | No unapproved structural change occurs during close execution. |
| 4 | Bank statements | Treasury team loads all bank statements and lockbox reports for the closing date. | Bank statement archive and receipt log. | Every active bank account shows one statement set for the closing period. |
| 5 | Bank reconciliation completion | Treasury team reconciles every bank account and cash clearing account to the general ledger. | Reconciliation workbook, unmatched-item log, and reviewer initials. | Every bank account difference equals zero or one approved timing item list. |
| 6 | Outstanding cash items | Treasury team reviews deposits in transit, outstanding warrants, ACH returns, and stale checks. | Aged outstanding-item schedule. | Every outstanding item carries age, reason, and follow-up owner. |
| 7 | Cash suspense and clearing | Accounting team clears cash suspense, returned-payment, and transit-clearing balances. | Clearing account rollforward and support entries. | Clearing balances resolve to zero or one approved carryforward explanation. |
| 8 | Accounts payable subledger | Accounts payable team reconciles vouchers, unpaid liabilities, and payment wraps to the general ledger. | AP subledger-to-GL reconciliation and exception list. | AP control accounts tie to subledger totals at period end. |
| 9 | Accounts receivable subledger | Accounts receivable team reconciles billings, collections, adjustments, and unapplied cash to the general ledger. | AR reconciliation and unapplied-cash detail. | AR control accounts tie to subledger totals and unapplied cash carries owner assignment. |
| 10 | Payroll subledger | Payroll team reconciles payroll expense, payroll payable, benefit payable, and tax withholdings. | Payroll register summary and GL tie-out. | Payroll totals tie to the GL and pending remittances appear in liability schedules. |
| 11 | Grant and project subledgers | Grant accounting team reconciles grant receivables, deferred revenue, reimbursement claims, and project coding. | Grant schedule by award and reimbursement status. | Grant balances tie by award, project, and fund. |
| 12 | Inventory and usage | Program team records material usage, inventory adjustments, and obsolete-stock review when inventory exists. | Inventory rollforward and adjustment support. | Inventory movement explains the ending inventory balance without unexplained variance. |
| 13 | Interfund and interagency balances | Accounting team matches due-to, due-from, transfer, and reimbursement balances with counterparties. | Counterparty confirmation log and balancing schedule. | Every interfund and interagency balance ties or carries one documented exception. |
| 14 | Debt and investment activity | Treasury team records interest earned, fees, fair-value activity, and debt service accruals. | Investment statement pack and debt amortization support. | Debt and investment balances tie to custodial records and book balances. |
| 15 | Revenue accrual entries | Accounting team records earned but unbilled or uncollected revenue for the period. | Accrual journal support with source calculations. | Revenue recognition lands in the correct period and the support recomputes to the posted amount. |
| 16 | Expense accrual entries | Accounting team records incurred but unpaid expenses, including utilities, contracted services, and goods received not invoiced. | Expense accrual schedule with invoice or receiving support. | Significant incurred costs appear in the closing period even when cash or invoice timing lags. |
| 17 | Payroll accrual | Payroll team records earned payroll, related taxes, and employee benefits that cross the period boundary. | Payroll accrual computation and liability breakdown. | Payroll expense and related liabilities tie to the payroll calendar and headcount records. |
| 18 | Prepaid review | Accounting team reviews prepaid assets and recognizes the expired portion. | Prepaid rollforward by asset class. | Unexpired balances remain on the balance sheet and expired amounts move to expense. |
| 19 | Deferred revenue review | Revenue owner reviews cash received before earning activity and updates deferred balances. | Deferred revenue rollforward by source. | Unearned amounts remain liabilities and earned amounts move to revenue. |
| 20 | Allowance and reserve review | Accounting team updates allowance, reserve, or estimate balances from the latest aging or claim data. | Aging analysis and reserve calculation memo. | Estimate changes trace to a current analytic basis. |
| 21 | Cutoff verification | Controller verifies shipment, receiving, billing, payroll, and cash cutoff treatment around period end. | Cutoff test sample log and exception summary. | Sampled transactions land in the correct accounting period. |
| 22 | Journal entry review | Reviewer verifies manual journals for support quality, coding accuracy, and duplication risk. | Journal review checklist and returned-entry log. | Every material manual journal has support, reviewer action, and duplicate-prevention evidence. |
| 23 | Subledger reconciliation verification | Controller verifies that all material subledgers tie to general ledger control accounts after adjustments. | Consolidated subledger tie-out report. | Every listed subledger balance equals the corresponding GL control balance. |
| 24 | AFRS staging validation | AFRS interface owner validates statewide codes, period mapping, batch counts, and control totals before submission. | Validation report and immutable batch snapshot. | Structural, fiscal, code, and balancing validation pass with zero unresolved hard-stop errors. |
| 25 | AFRS submission and acknowledgement | AFRS interface owner submits required batches, captures acknowledgements, and routes rejects immediately. | Submission record, acknowledgement, and reject log. | Every batch ends in accepted, warning, or reject status with one owner and one next action. |
| 26 | Budget vs. actual review | Budget owner reviews expenditure, revenue, allotment, and appropriation variances for the closed period and year-to-date totals. | Budget variance pack and exception commentary. | Material variances carry documented cause, action, and accountable owner. |
| 27 | Management reports | Accounting team produces management statements, cash reports, and variance summaries from final balances. | Report package and distribution log. | Report totals tie to the final ledger and distribution reaches the defined audience. |
| 28 | Close evidence archive | Close coordinator stores reconciliations, reports, journal support, approvals, and system snapshots in the evidence store. | Archived close package with index. | A reviewer retrieves every critical artifact from one indexed package. |
| 29 | Final sign-off | Preparer, reviewer, and approver complete final sign-off for the period close. | Signed checklist and approval trail. | The period carries one final approval state and one locked package version. |

## Quarter-End Additional Checklist

Quarter-end close includes all month-end tasks plus deeper analysis and external-reporting preparation.

| # | Additional Workstream | Team Action | Evidence | Observable Pass |
|---:|---|---|---|---|
| 1 | Detailed variance analysis | Budget owner and accounting team expand variance review to program, fund, grant, object, and statewide coding dimensions. | Detailed variance workbook and management commentary. | Material quarter-to-date and year-to-date variances carry documented root cause and action. |
| 2 | Grant accounting updates | Grant team updates reimbursement status, match calculations, indirect-cost allocations, and deferred or unavailable revenue balances. | Grant quarter-end schedule by award. | Grant balances tie to award terms, billing status, and ledger balances. |
| 3 | Investment income accrual | Treasury team accrues quarter-end investment earnings, fees, and fair-value changes when policy requires recognition. | Investment accrual memo and statement support. | Investment income and related receivable or payable balances tie to custodial data. |
| 4 | Capital project review | Capital accounting team reviews project-in-progress balances, capitalization triggers, retainage, and funding alignment. | CIP schedule and capitalization decision log. | Capitalizable items move to the correct asset class and unsupported CIP balances do not remain open. |
| 5 | Debt compliance review | Treasury team validates covenant reporting, debt service schedules, and related restricted-balance classifications. | Debt compliance packet. | Scheduled debt amounts and compliance indicators match source agreements. |
| 6 | Fund balance and net position scan | Accounting team reviews restricted, committed, assigned, and unassigned positions for large shifts. | Classification review memo. | Significant classification movement carries documented cause and approval. |
| 7 | AFRS quarter reporting package | AFRS interface owner confirms that quarter-specific statewide reports and batch activity reconcile to agency books. | AFRS quarter reconciliation report. | Agency totals and AFRS-facing totals reconcile or carry approved exceptions only. |
| 8 | Quarterly financial statements | Accounting team prepares quarter-end statements, notes support, and management discussion inputs. | Statement package and tie-out workbook. | Statement lines tie to supporting schedules and final ledger balances. |
| 9 | Control self-assessment refresh | Controller reviews segregation, approval completeness, and unresolved aged exceptions. | Quarter-end control assessment. | Open control exceptions carry remediation owner and target date. |

## Year-End Comprehensive Checklist

Year-end close includes every month-end and quarter-end task plus annual reporting, fixed-asset, compliance, and tax-processing work.

| # | Additional Workstream | Team Action | Evidence | Observable Pass |
|---:|---|---|---|---|
| 1 | All quarter-end items | Close coordinator carries forward every quarter-end requirement into the year-end plan. | Consolidated year-end checklist. | No quarter-end control drops from the year-end package. |
| 2 | Final AFRS period mapping | AFRS interface owner validates final fiscal-month, biennium, and continuation-period coding for year-end activity. | Fiscal-period validation report. | Final-year activity posts to the intended fiscal period without mapping exceptions. |
| 3 | Receivable confirmation and collectibility | Revenue owner reviews major receivable balances, confirms collectibility, and updates allowance estimates. | Confirmation summary and allowance memo. | Receivable support explains gross, allowance, and net balances. |
| 4 | Payable search for unrecorded liabilities | Accounting team performs extended search procedures for unrecorded invoices, receiving activity, and contractual obligations. | Search log, vendor response support, and resulting journal entries. | Significant unpaid obligations appear in the correct fiscal year. |
| 5 | Depreciation calculation | Capital accounting team runs annual depreciation and amortization for capital assets and intangible assets. | Depreciation run output and review log. | Current-year expense and accumulated balances recompute from the approved asset register. |
| 6 | Fixed asset inventory | Capital accounting team completes physical inventory, tagging review, impairment screening, and asset retirement review. | Inventory certification, exception log, and disposal support. | Asset existence, condition, and retirement status reconcile to the asset ledger. |
| 7 | Construction-in-progress closeout | Capital accounting team transfers completed projects from CIP into depreciable asset classes. | CIP closeout memo and capitalization entries. | Completed projects do not remain indefinitely in CIP without documented reason. |
| 8 | Lease and subscription review | Accounting team updates lease liability, right-of-use asset, and subscription asset schedules for new or changed agreements. | Lease and subscription rollforward. | Annual expense, liability, and asset balances tie to the contract schedule. |
| 9 | Compensated absence and long-term liability review | Payroll and accounting teams refresh annual estimates for leave, claims, and long-term obligations. | Liability actuarial or estimate support. | Long-term liability balances reflect current-year usage and estimate data. |
| 10 | Fund balance classification | Controller classifies ending governmental fund balance into nonspendable, restricted, committed, assigned, and unassigned categories. | Fund balance classification memo. | Classifications tie to legal constraints, governing actions, and ledger balances. |
| 11 | Net position classification | Accounting team classifies proprietary and government-wide net position into investment in capital assets, restricted, and unrestricted components. | Net position schedule. | Classification totals tie to statement balances and capital or debt schedules. |
| 12 | GASB compliance checks | Technical accounting owner reviews new and existing GASB presentation, disclosure, recognition, and measurement requirements. | GASB compliance checklist and conclusion memo. | Annual statements and note support reflect the applicable GASB rule set for the reporting year. |
| 13 | Interfund elimination and government-wide conversion | Reporting team records consolidation, elimination, and government-wide conversion entries when required. | Conversion workbook and elimination support. | Consolidated statements tie to fund statements and conversion entries reconcile cleanly. |
| 14 | CAFR or ACFR preparation | Reporting team assembles the annual report, note disclosures, statistical schedules, and required supplementary information. | Draft ACFR package and assembly checklist. | Every statement and schedule ties to the final trial balance and supporting schedules. |
| 15 | SEFA and grant reporting alignment | Grant accounting team ties federal and state grant expenditure reporting to the final ledger and grant schedules. | SEFA tie-out workbook and grant reporting package. | Expenditure totals tie across the ledger, grant schedules, and external reports. |
| 16 | 1099 processing | Accounts payable team validates vendor tax classifications, accumulates reportable payments, and prepares annual information-return files or reports. | 1099 workpapers, exception log, and vendor validation report. | Reportable payments tie to AP history and unresolved TIN or classification exceptions carry owner action. |
| 17 | Subsequent-event review | Controller reviews major events after year end that affect recognition or disclosure. | Subsequent-event memo and disclosure decisions. | Material subsequent events carry conclusion, approver, and disclosure linkage. |
| 18 | Audit package and PBC support | Close coordinator prepares prepared-by-client schedules, lead sheets, and evidence indexes for audit support. | PBC tracker and evidence index. | Auditors receive one indexed package that ties back to the final closed ledger. |

## AFRS Submission Deadlines and Windows

The statewide source material establishes critical operating windows and interface controls. Agency close calendars add the period-specific due dates from OFM close notices and internal management deadlines.

| Topic | Required Operating Pattern | Evidence | Observable Pass |
|---|---|---|---|
| Deadline source | Controller records each OFM-issued AFRS close deadline in the agency close calendar for month-end, quarter-end, and year-end runs. | Close calendar revision log. | Every run references one published statewide deadline source and one agency internal deadline set. |
| Internal lead time | AFRS interface owner sets agency submission deadlines ahead of the statewide due point to preserve correction time. | Submission plan by batch group. | At least one correction cycle fits between the internal deadline and the statewide due point. |
| Processing blackout window | AFRS interface owner excludes the documented `8:00 p.m.` to `10:00 p.m.` batch-processing window from submit schedules. | Job schedule and scheduler exclusions. | No scheduled batch submission starts inside the blocked window. |
| Reference-data refresh window | Reference-data jobs exclude the documented Tuesday-through-Saturday `1:00 a.m.` refresh window that lasts about `15` minutes. | Extraction schedule and retry configuration. | No scheduled AFRSTitles extract starts inside the blocked refresh window. |
| Patch and reboot awareness | Operations team aligns close monitoring with the documented monthly patch and reboot window on the outbound server. | Maintenance calendar and alert routing. | Extraction jobs retry cleanly and alert on extended outage conditions. |
| Same-day reruns | AFRS interface owner increments `BATCH-NO` for each same-day rerun and preserves lineage to the rejected batch. | Batch lineage table and immutable payload snapshots. | Replacement batches never reuse prior batch identifiers. |
| Reject turnaround | Interface owner routes D51 rejects on the same business day and records correction ownership immediately. | Reject log with owner and due point. | Every reject has one owner, one reason, and one next transmission target. |
| Period lock alignment | Controller confirms that AFRS submissions complete before agency hard close enters final lock state. | Close-state transition log. | Hard close occurs after required submissions and initial reconciliation complete. |

## Post-Close Adjustment Entry Window

Post-close adjustments preserve financial accuracy without erasing the original close package. The pattern below keeps one controlled correction path per period state.

| Run Type | Standard Window Pattern | Approval Path | Observable Pass |
|---|---|---|---|
| Month-end | Agency uses a one-business-day correction window after hard close for material errors only. | Preparer, reviewer, and approver all sign the adjustment request. | Every post-close month-end entry carries one approved request and one replacement report package. |
| Quarter-end | Agency uses a two-business-day correction window because statement and grant impacts expand. | Controller or delegate signs the request after reviewer clearance. | Quarter-end corrections update statements and variance packs in the same release set. |
| Year-end | Agency uses a five-business-day correction window or the published audit-preparation window, whichever closes first. | Controller and financial-reporting approver sign the request. | Year-end corrections preserve original and replacement statement packages with full lineage. |

### Adjustment Entry Control Steps

| Step | Team Action | Evidence | Observable Pass |
|---|---|---|---|
| 1 | Preparer opens one adjustment request with period, reason, amount, accounts, and source support. | Adjustment request record and support attachments. | The request contains complete coding and support before review starts. |
| 2 | Reviewer validates materiality, accounting treatment, and period selection. | Reviewer notes and recalculation support. | Reviewer accepts or rejects the request with explicit rationale. |
| 3 | Approver authorizes posting scope and report reissue scope. | Approval record with timestamp. | The approval identifies the exact journals and outputs in scope. |
| 4 | Accounting team posts the approved journal with one correction tag linked to the original close run. | Posted journal and linkage record. | The journal links to the close run, request, and approval trail. |
| 5 | Reporting team reruns affected statements, schedules, and AFRS reconciliations. | Replacement statement package and delta report. | Replacement outputs tie to the revised ledger and show the correction impact. |
| 6 | Close coordinator archives original and replacement artifacts without overwrite. | Versioned archive index. | The archive preserves both versions and one clear supersession link. |

## Reversing Entry Processing Schedule

Reversing entries reduce duplicate manual work on the first day of the next period while preserving correct period recognition.

| Entry Type | Reverse Timing | Owner | Observable Pass |
|---|---|---|---|
| Routine expense accrual | First business day of the next open period | General accounting | The reversal posts automatically and the next invoice clears against the reversal path without duplication. |
| Routine revenue accrual | First business day of the next open period | Revenue accounting | Cash receipt or billing activity offsets the reversal cleanly. |
| Payroll accrual | First payroll processing date or first business day of the next period, based on payroll-calendar design | Payroll accounting | Payroll posting replaces the accrual without leaving residual expense distortion. |
| Utility or recurring service accrual | First business day of the next open period | General accounting | Vendor invoice settlement clears the accrued liability path. |
| Grant reimbursement accrual | First reimbursement posting date in the next period | Grant accounting | Subsequent claim or cash activity offsets the accrued receivable without double counting. |
| Year-end accrual with audit carryforward | Manual reversal only after reviewer release in the new fiscal year | Controller office | The reversal waits for audit-sensitive balances and leaves a full approval trail. |
| Deferred revenue or prepaid rollforward | No automatic reversal; recognition follows the schedule engine | Revenue or asset owner | Recognition follows the underlying schedule rather than a blanket reversal. |

### Reversal Schedule Controls

| Control | Team Action | Observable Pass |
|---|---|---|
| Reversal date control | Scheduler posts reversals into the first valid open period only. | No reversal lands in a closed or wrong fiscal period. |
| Linkage control | Every reversal points to one originating journal line or journal group. | Reviewers trace reversal lineage without manual lookup. |
| Duplicate prevention | Posting service blocks a second reversal for the same origin. | One origin produces one reversal set only. |
| Exception handling | Team routes blocked reversals to one queue with owner and due point. | Every failed reversal appears on the exception dashboard within the same day. |

## Sign-Off Workflow

| Stage | Preparer Action | Reviewer Action | Approver Action | Observable Pass |
|---|---|---|---|---|
| Reconciliation completion | Preparer attaches reconciliations, clears items, and records remaining exceptions. | Reviewer recomputes material totals and clears or returns the task. | Approver reviews exception population for readiness. | Material reconciliations show completed preparer and reviewer actions before approval. |
| Adjustment review | Preparer attaches journal support and period rationale. | Reviewer validates accounting treatment and coding. | Approver authorizes final posting scope for material entries. | Material manual journals show all three role actions. |
| Reporting package | Preparer publishes statements and support schedules from final balances. | Reviewer ties statements to supporting schedules. | Approver accepts final package version. | Final statements and schedules tie across every material line. |
| AFRS package | Preparer or interface owner attaches batch validation, submission, and acknowledgement artifacts. | Reviewer verifies control totals and reject resolution. | Approver accepts statewide reporting completion. | Every required batch has one accepted disposition or one approved exception memo. |
| Final close | Preparer completes the checklist and archive index. | Reviewer clears the close package. | Approver sets the run status to closed. | The system changes period state only after all required approvals exist. |

## .NET Task Orchestration Patterns

The close process benefits from deterministic orchestration, explicit state transitions, and durable evidence capture.

### Domain Model Pattern

| Type | Responsibility | Observable Pass |
|---|---|---|
| `CloseRun` | Stores period, run type, calendar, hard-close target, and final status. | One run record represents one period-close execution. |
| `CloseTask` | Stores task name, owner, due point, blocker state, and completion evidence. | One task record exists for every checklist item in scope. |
| `CloseDependency` | Stores predecessor and successor task linkage. | Blocked tasks remain blocked until predecessors finish. |
| `CloseEvidence` | Stores artifact path, checksum, source system, and retention class. | Reviewers retrieve every artifact from one indexed evidence set. |
| `CloseApproval` | Stores preparer, reviewer, approver, timestamps, and scope. | Every approval action remains queryable by role and time. |
| `AdjustmentRequest` | Stores post-close correction request, rationale, and impact scope. | Every correction path begins with one request record. |
| `AfrsBatchSubmission` | Stores batch identity, control totals, submission status, and lineage to rejects or replacements. | Every statewide submission remains traceable from source to acknowledgement. |

### Command and Workflow Pattern

| Command | Use | Observable Pass |
|---|---|---|
| `StartCloseRun` | Opens the run, instantiates tasks, and stamps the initial state. | The run starts with the published checklist and ownership matrix. |
| `CompleteCloseTask` | Marks one task complete after evidence upload and validation. | Completion fails when required evidence or dependencies remain missing. |
| `RecordAdjustmentEntry` | Posts one approved close or post-close entry and links support. | The system records one journal-to-approval lineage path. |
| `SubmitAfrsBatch` | Builds, validates, and submits one AFRS batch snapshot. | Submission stores immutable payload, counts, amounts, and outcome state. |
| `AcknowledgeAfrsBatch` | Records accept, warning, or reject results from AFRS. | Every submitted batch reaches one terminal or corrective state. |
| `ApproveCloseRun` | Advances the run from review complete to approved. | Approval fails when required tasks remain incomplete. |
| `ReopenCloseRun` | Reopens one scoped period for one approved correction set. | Reopen records reason, actor, and reclose target. |

### Scheduling Pattern

| Scheduler Concern | Pattern | Observable Pass |
|---|---|---|
| Reminder cadence | Quartz schedules reminders from the close calendar and escalates overdue blockers. | Owners receive reminders before due points and escalations after misses. |
| AFRS blackout handling | Scheduler excludes the `8:00 p.m.` to `10:00 p.m.` submission window and the Tuesday-through-Saturday `1:00 a.m.` refresh window. | Jobs do not start inside excluded windows. |
| Reversal automation | Scheduler runs reversal jobs on the first valid open-period business date. | Reversal jobs post once and stamp success or failure state. |
| Retry and idempotency | Commands use idempotency keys per run, task, journal, and batch. | Reruns do not duplicate postings, evidence, or submissions. |

### Verification Pattern

| Verification Area | Test | Observable Pass |
|---|---|---|
| Task completeness | Query all required tasks for the run and verify completed status and evidence count. | Required tasks show completed status with complete evidence. |
| Reconciliation integrity | Recompute tied balances from source schedules and compare to stored totals. | Recomputed totals equal stored final totals. |
| Approval trace | Query approval chain by run and by material journal. | Preparer, reviewer, and approver actions remain complete and ordered. |
| AFRS lineage | Follow one batch from source snapshot through acknowledgement and any reject replay. | One complete lineage chain exists for every batch. |
| Reopen control | Attempt a reopen without approval metadata. | The system blocks the reopen and records the rejection reason. |

## Close Metrics and Dashboard Cues

| Metric | Use | Observable Pass |
|---|---|---|
| Reconciliation completion rate | Tracks readiness for hard close. | Completion reaches `100%` for blocking reconciliations before hard close. |
| Open exception count | Tracks unresolved items by age and owner. | Aged exceptions trend downward through the close window. |
| Manual journal count | Tracks concentration of late or high-risk entries. | Manual journals stay within the agency risk tolerance and show reviewer coverage. |
| AFRS reject rate | Tracks submission quality by batch family and cause. | Reject rate trends downward and every reject carries correction lineage. |
| Post-close adjustment count | Tracks quality of the original close package. | Post-close corrections remain rare and fully approved. |
| Days to close | Tracks calendar efficiency across periods. | Actual close duration meets the published close calendar target. |

## Verification Checklist

| Checkpoint | Test | Observable Pass |
|---|---|---|
| Front matter | Review required front matter properties and allowed values. | `title`, `doc_type`, `status`, and `last_updated` are present and valid. |
| Month-end coverage | Count checklist rows and confirm required topics appear. | The month-end table contains more than `20` items and includes bank reconciliation, subledger reconciliation, accruals, prepaid or deferred review, cutoff, budget review, and management reports. |
| Quarter-end coverage | Review additional quarter-end table content. | Quarter-end content includes variance analysis, grant updates, investment income accrual, and quarterly statements. |
| Year-end coverage | Review additional year-end table content. | Year-end content includes depreciation, fixed assets, fund balance classification, GASB review, ACFR preparation, and `1099` processing. |
| Washington State alignment | Review AFRS and SAAM operating windows and policy references. | The reference includes AFRS windows, batch lineage rules, fiscal-period control, and control-evidence expectations. |
| Workflow coverage | Review adjustment, reversal, sign-off, and orchestration sections. | The reference includes post-close, reversal, sign-off, and `.NET` orchestration guidance. |

## Cross References

| Skill or Reference | Use |
|---|---|
| [Closing Cycle Patterns](../SKILL.md) | Use for the core close-run workflow and implementation framing. |
| [Washington State SAAM](../../wa-state-saam/SKILL.md) | Use for statewide policy, period control, and internal-control evidence alignment. |
| [Washington State AFRS](../../wa-state-afrs/SKILL.md) | Use for statewide interface, validation, submission, and reject-handling patterns. |
| [AFRS File Specifications](../../wa-state-afrs/references/afrs-file-specifications.md) | Use for batch identity, processing windows, and fixed-width control totals. |
| [AFRS Transaction Codes Reference](../../wa-state-afrs/references/afrs-transaction-codes.md) | Use for refresh windows, reject families, and replay lineage. |
| [Washington State SAAM Policy Reference](../../wa-state-saam/references/policy-reference.md) | Use for policy mapping, period snapshots, and internal-control alignment. |

