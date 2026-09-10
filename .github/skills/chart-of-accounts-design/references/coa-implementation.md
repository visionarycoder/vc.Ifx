---
title: Chart of Accounts Implementation Reference
doc_type: reference
status: active
last_updated: 2026-08-31
summary: Compact reference for hierarchy design, segment rules, .NET implementation, governmental alignment, and migration planning.
target_audience: ai
related_docs:
  - ../SKILL.md
  - ../../double-entry-accounting/SKILL.md
  - ../../fund-accounting-patterns/SKILL.md
  - ../../public-sector-accounting/SKILL.md
  - ../../wa-state-saam/SKILL.md
tags:
  - chart-of-accounts
  - account-structure
  - coa
  - accounting
  - fund-accounting
---
# Chart of Accounts Implementation Reference

This reference expands `chart-of-accounts-design` with implementation detail.

## Account Hierarchy Design

| Topic | Rule | Pass |
|---|---|---|
| Posting level | Leaf accounts post and parent accounts summarize. | No summary node accepts direct posting. |
| Parent chain | Each leaf resolves one ordered root-to-leaf path. | Rollup path is unique. |
| Effective dating | Parent-child links carry start and end dates. | Historical reports preserve the valid prior hierarchy. |

| Level | Role | Example |
|---|---|---|
| 0 | Root chart | `ROOT` |
| 1 | Statement family | `1000 Assets`, `5000 Expenses` |
| 2 | Category | `5100 Payroll Costs` |
| 3 | Posting or local detail | `5110 Salaries`, `5112 Salaries Overtime` |

## Segment Length and Format Rules

| Segment | Length | Format | Example | Use |
|---|---:|---|---|---|
| Fund | 3-5 | Fixed numeric or alphanumeric | `001`, `145`, `AA01` | Governmental or restricted resource boundary |
| Department | 3-6 | Fixed numeric | `120`, `0340`, `875100` | Manager accountability and cost center control |
| Natural Account | 4-6 | Fixed numeric with family digit first | `1010`, `2110`, `5110` | Statement classification and normal balance |
| Program or Grant | 3-8 | Alphanumeric | `410`, `F23A` | Purpose or sponsor reporting |
| Project or Activity | 5-10 | Alphanumeric with stable prefix | `P10452`, `A22001` | Temporary work and lifecycle tracking |

| Format Rule | Recommendation | Pass |
|---|---|---|
| Storage | Persist segments in discrete fields and format display strings separately. | Queries and integrations stay stable. |
| Delimiters | Use delimiters in UI and reports. | Validation stays independent of display format. |
| Metadata | Keep dates and status outside the visible code. | Code meaning stays stable over time. |

## Reserved Ranges by Account Type

| Range | Type | Typical Use | Growth Rule |
|---|---|---|---|
| `1000-1999` | Assets | Cash, receivables, inventory, capital assets | Leave gaps between liquid, receivable, and capital groups. |
| `2000-2999` | Liabilities | Payables, accruals, debt, deferred inflows | Reserve subranges for current and long-term balances. |
| `3000-3999` | Equity or Fund Balance | Retained earnings, net position, fund balance | Separate unrestricted and restricted bands. |
| `4000-4999` | Revenue | Sales, taxes, grants, charges, transfers in | Reserve sponsor or source bands where tracing matters. |
| `5000-7999` | Expense or Expenditure | Payroll, services, supplies, debt service, capital outlay | Keep wide blocks for detailed cost reporting. |
| `8000-8999` | Other Financing | Transfers, proceeds, special reporting families | Use separate governmental presentation bands. |
| `9000-9999` | Statistical or Elimination | Consolidation-only or memorandum accounts | Block from routine operational posting. |

## Roll-Up and Consolidation Rules

| Rule | Design Pattern | Pass |
|---|---|---|
| Natural-account rollup | Aggregate leaf balances through the parent tree only. | Parent totals equal descendant leaf totals. |
| Cross-fund rollup | Roll up inside each fund first, then consolidate across funds. | Fund totals reconcile to consolidated totals. |
| Alternate reporting tree | Keep management trees separate from statutory trees. | One posting account supports many governed report views. |
| Elimination layer | Use dedicated consolidation members outside normal posting paths. | Elimination activity stays outside operational reports. |

## Cross-Segment Validation Rules

| Rule | Trigger | Outcome |
|---|---|---|
| Fund to natural-account compatibility | Governmental posting uses an unauthorized pair. | Validator rejects the row. |
| Department to program compatibility | Program ownership limits department usage. | Validator rejects the row. |
| Grant to project period alignment | Project activity falls outside the award window. | Validator rejects the row. |
| Project status | Closed or archived project receives new activity. | Validator blocks posting. |
| Effective dates | Segment value is inactive on the transaction date. | Validator returns an inactive-date error. |

## .NET Implementation

```csharp
namespace Accounting.ChartOfAccounts;

public sealed record AccountSegment(string Name, string Value, int Sequence);

public sealed class Account
{
    public Guid Id { get; init; }
    public string AccountCode { get; init; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string NormalBalance { get; init; } = string.Empty;
    public Guid? ParentAccountId { get; set; }
    public bool IsPostingAllowed { get; set; }
    public DateOnly ActiveFrom { get; set; }
    public DateOnly? ActiveTo { get; set; }
    public List<AccountSegment> Segments { get; } = [];

    public bool IsActiveOn(DateOnly accountingDate) =>
        accountingDate >= ActiveFrom && (ActiveTo is null || accountingDate <= ActiveTo.Value);
}

public sealed class AccountValidator
{
    public IReadOnlyList<string> Validate(Account account, DateOnly accountingDate)
    {
        var errors = new List<string>();
        if (!account.IsActiveOn(accountingDate)) errors.Add("account.inactive");
        if (account.Segments.Count == 0) errors.Add("segment.required");
        return errors;
    }
}

public sealed class HierarchyService
{
    public IReadOnlyList<Account> GetAncestors(Account leaf, IReadOnlyDictionary<Guid, Account> accounts)
    {
        var result = new List<Account>();
        var current = leaf;
        while (current.ParentAccountId is Guid id && accounts.TryGetValue(id, out var parent))
        {
            result.Add(parent);
            current = parent;
        }
        return result;
    }
}
```

| Concern | Implementation Rule | Pass |
|---|---|---|
| Entity model | Keep category, normal balance, parent id, posting flag, effective dates, and ordered segments on the account entity. | Account metadata supports posting and reporting. |
| Segment validation | Validate required segments, format, compatibility, and effective dates before persistence. | Invalid combinations fail before posting. |
| Hierarchy navigation | Use ancestor traversal for rollups | Rollups stay deterministic. |
| Activation and inactivation | Inactivate with an end date and block new postings. | Historical reports keep retired accounts visible. |

## GASB and SAAM Alignment

| Topic | Alignment |
|---|---|
| Fund segment | Governmental ledgers keep fund as a first-class segment. |
| Natural accounts | Natural-account ranges support assets, liabilities, fund balance, revenues, expenditures, and other financing sources or uses. |
| Basis separation | Fund-level modified accrual and government-wide full accrual views stay separable. |
| SAAM orientation | Washington State designs align statewide chart categories, fixed lengths, and effective-dated authorization. |
| AFRS readiness | State-facing interfaces use governed values, valid dates, and explicit mapping metadata. |

## Migration Patterns from Legacy COA

| Pattern | Use | Control |
|---|---|---|
| One-to-one | Legacy meaning already matches the target. | Preserve direct crosswalk and effective dates. |
| One-to-many split | Legacy code combines meanings such as department and natural account. | Split with steward-approved rules and transaction history. |
| Many-to-one collapse | Legacy detail exceeds future reporting need. | Preserve source history and archive the old map. |
| Parallel run | Cutover risk is high. | Reconcile balances and sample postings across both charts. |
| Phased activation | New segments enter in stages. | Time-box defaults and remove them after stabilization. |

## Sample COA Structures

| Entity | Structure | Example |
|---|---|---|
| Commercial | Single-segment | `1010 Cash`, `4100 Sales Revenue`, `6100 Rent Expense` |
| Commercial with hierarchy | Parent-child | `5000 Operating Expenses` -> `6100 Rent` |
| Governmental | Multi-segment | `145-120-7310-620-P23045` |
| Washington State | Multi-segment | `001-340-5110-410-P10452` |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter | Run `node .scripts/validate-frontmatter.js --check`. | Command returns code `0`. |
| Link integrity | Open `../SKILL.md` and related skill links. | All local references resolve. |
| Validation coverage | Review inactive, missing-segment, and incompatible-pair cases. | Each case maps to one explicit validator outcome. |
| Rollup integrity | Run ancestor traversal and parent-total samples. | Parent totals match governed leaf totals. |
