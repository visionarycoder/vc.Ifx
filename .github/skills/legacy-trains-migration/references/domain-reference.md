---
title: Legacy Trains Migration Domain Reference
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
related_docs:
  - ../SKILL.md
---
# Legacy Trains Migration Domain Reference

## Trains 4 Terminology

| Team Term | Product Term | Usage Rule |
|---|---|---|
| Trains 4 | CGI Advantage 4 | Agent uses this term to distinguish the target platform from legacy Trains. |
| Legacy Trains | Source platform | Agent uses this term for extraction, mapping, and reconciliation inputs only. |
| Conversion wave | Planned migration batch | Agent ties each wave to one data scope, one validation gate, and one rollback rule. |
| Reconciliation pack | Validation evidence set | Agent records counts, totals, exceptions, and approval outcomes in one package. |

## CGI Advantage 4 Target Module Reference

| Target Domain | Migration Focus | Validation Focus | Pattern Source |
|---|---|---|---|
| General Accounting | Map journal, posting, period, and accounting segment behavior into Trains 4 posting structures. | Balance, period, and posting-line reconciliation. | GA User Guide transaction sections and Posting Line Inquiry |
| COA Crosswalk | Convert chart segments and legacy code values into governed target segments. | Zero unmapped required source codes at cutover. | GA User Guide COA Crosswalk |
| Vendor management | Convert supplier identity, status, remit, tax, and duplicate controls. | Vendor count, active-status parity, and duplicate suppression. | GA User Guide vendor sections plus tenant vendor model |
| Purchase orders and requisitions | Convert open commitments, document lineage, and status. | Open-item parity and downstream posting linkage. | GA User Guide common business tasks for orders and requisitions |
| AP and AR operational domains | Convert open items, settlement state, and ledger impact with explicit subledger lineage. | Document count, balance, and status reconciliation. | Tenant module inventory plus GA posting patterns |
| Batch processing and report jobs | Rebuild scheduled migration loads, validations, and evidence output. | Repeatable run logs, checkpoints, and exception packages. | GA User Guide batch processing and report jobs |

## Cutover Governance Matrix

| Concern | Agent Pattern | Pass |
|---|---|---|
| Conversion waves | Agent ties each wave to one data scope, one validation gate, and one rollback rule. | Waves remain isolated and auditable. |
| Reconciliation packs | Agent records counts, totals, exceptions, and approval outcomes in one evidence set. | Sign-off packages remain complete and repeatable. |
| Rollback boundary | Agent records reversible steps, forward-fix steps, and irreversible data points. | Decision gates appear before irreversible actions. |
| Training and support | Agent adds role-based training, support rosters, and post-go-live observation criteria. | Operational teams can validate and support cutover results. |

## Integration Hooks

| Surface | Agent Action | Output |
|---|---|---|
| `dotnet-webapi` | Agent exposes migration-run status, validation summary, or exception-review endpoints for controlled operations. | Internal migration-control API |
| `azure-service-bus-patterns` | Agent publishes conversion-complete, reconciliation-ready, or exception-review events for decoupled workflows. | Queue or topic orchestration for migration stages |
| Fabric | Agent lands reconciliation packs, archive history, and variance facts into analytical storage for audit and trend review. | Analytical history and migration evidence datasets |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| History migration starts with no retention rule | Agent classifies open items, operational history, and archive-only history before build work starts. |
| COA mapping freezes after data-load design | Agent locks the crosswalk before transformation code and validation thresholds are finalized. |
| Vendor masters convert with no duplicate or inactive-state rules | Agent normalizes status, identity, and duplicate keys in staging. |
| Parallel run becomes an informal comparison | Agent defines counts, totals, aging, and trial-balance evidence in a signed reconciliation pack. |
| Rollback planning omits irreversible-step tracking | Agent records each irreversible step and places a decision gate before it. |
| Migration ends at data load with no training or support window | Agent adds role-based training, support rosters, and post-go-live observation criteria. |

## Outputs

| Output | Description |
|---|---|
| Migration assessment and dependency inventory | Source-authority and dependency map |
| COA, vendor, document, and history mapping package | Conversion and retention guidance |
| API versus file conversion plan | Load-path decisions by entity |
| Parallel run and reconciliation design | Evidence thresholds and validation flows |
| Cutover, rollback, and support runbook | Freeze, go-live, validation, and reversal controls |
| Post-migration validation plan | Evidence thresholds for operational and analytical outputs |

## Reference Sources

| Source | Relevance |
|---|---|
| https://myadvantagecloud.cgi.com/GACCG40/PRDHelpService/fin/PDF_Files/CGI_Advantage_4_Financial_GA_User_Guide.pdf | Guide sections inform target posting behavior, COA crosswalk, batch jobs, inquiries, and common business tasks. |
| Legacy Trains data dictionary, report inventory, and close procedures | Source-system facts define extraction, mapping, reconciliation, and retention boundaries. |
