---
title: Subledger Reconciliation Implementation Reference
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
related_docs:
  - ../SKILL.md
tags:
  - reconciliation
  - subledger
  - gl-control
  - audit
  - accounting
---
# Subledger Reconciliation Implementation Reference

## Implementation Scope

This reference describes one .NET implementation shape for subsidiary-ledger to general-ledger control reconciliation. The design keeps reconciliation runs deterministic, reproducible, and auditable across AR, AP, payroll, and fixed-assets domains.

## Architecture Overview

| Component | Responsibility | Output |
|---|---|---|
| `ISubledgerBalanceExtractor` | Reads one subledger population and emits normalized balance rows. | `SubledgerBalanceRow` sequence |
| `IGlControlBalanceExtractor` | Reads GL balances for mapped control accounts at the same cutoff. | `GlControlBalanceRow` sequence |
| `IReconciliationComparator` | Groups rows, compares totals, applies tolerance, and emits breaks. | `ReconciliationResult` |
| `IBreakClassifier` | Assigns one primary break category per unmatched row group. | `BreakCategory` and investigation notes |
| `IReconciliationEvidenceStore` | Persists query definition, balance snapshots, and reviewer notes. | Evidence package |
| `IAlertPublisher` | Sends summary or exception notifications. | Email or queue message |
| `ISignOffService` | Records preparer and reviewer disposition. | `SignOffRecord` |

## Normalized Data Contracts

```csharp
public sealed record ReconciliationScope(
    string ScopeCode,
    string SubledgerType,
    string FiscalYear,
    string FiscalPeriod,
    DateTimeOffset CutoffUtc,
    string FunctionalCurrency);

public sealed record BalanceKey(
    string EntityCode,
    string ControlAccount,
    string CurrencyCode,
    string? FundCode,
    string? DepartmentCode);

public sealed record SubledgerBalanceRow(
    BalanceKey Key,
    decimal EndingBalance,
    int DocumentCount,
    string SourceReference,
    DateTimeOffset ExtractedAtUtc);

public sealed record GlControlBalanceRow(
    BalanceKey Key,
    decimal EndingBalance,
    int JournalLineCount,
    string LedgerReference,
    DateTimeOffset ExtractedAtUtc);

public sealed record ToleranceRule(
    decimal AbsoluteAmount,
    decimal Percentage,
    int DocumentCountVariance);

public sealed record BreakCaseRow(
    BalanceKey Key,
    decimal SubledgerBalance,
    decimal GlBalance,
    decimal Difference,
    decimal DifferencePercent,
    int CountDifference,
    string Status,
    string BreakCategory);
```

## EF Core Entity Shape

```csharp
public sealed class ReconciliationRun
{
    public Guid Id { get; set; }
    public string ScopeCode { get; set; } = string.Empty;
    public string SubledgerType { get; set; } = string.Empty;
    public string FiscalYear { get; set; } = string.Empty;
    public string FiscalPeriod { get; set; } = string.Empty;
    public DateTimeOffset CutoffUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ConfigurationHash { get; set; } = string.Empty;
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public ICollection<ReconciliationBalanceSnapshot> BalanceSnapshots { get; set; } = [];
    public ICollection<ReconciliationBreakCase> BreakCases { get; set; } = [];
}

public sealed class ReconciliationBalanceSnapshot
{
    public Guid Id { get; set; }
    public Guid ReconciliationRunId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public string EntityCode { get; set; } = string.Empty;
    public string ControlAccount { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string? FundCode { get; set; }
    public string? DepartmentCode { get; set; }
    public decimal EndingBalance { get; set; }
    public int DocumentCount { get; set; }
    public string SourceReference { get; set; } = string.Empty;
}

public sealed class ReconciliationBreakCase
{
    public Guid Id { get; set; }
    public Guid ReconciliationRunId { get; set; }
    public string EntityCode { get; set; } = string.Empty;
    public string ControlAccount { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string? FundCode { get; set; }
    public string? DepartmentCode { get; set; }
    public decimal SubledgerBalance { get; set; }
    public decimal GlBalance { get; set; }
    public decimal Difference { get; set; }
    public decimal DifferencePercent { get; set; }
    public int CountDifference { get; set; }
    public string BreakCategory { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string OwnerUserId { get; set; } = string.Empty;
    public string EvidencePath { get; set; } = string.Empty;
}
```

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<ReconciliationRun>(entity =>
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ScopeCode).HasMaxLength(100);
        entity.Property(x => x.SubledgerType).HasMaxLength(50);
        entity.Property(x => x.ConfigurationHash).HasMaxLength(128);
        entity.HasIndex(x => new { x.ScopeCode, x.FiscalYear, x.FiscalPeriod, x.CutoffUtc }).IsUnique();
    });

    modelBuilder.Entity<ReconciliationBalanceSnapshot>(entity =>
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.EndingBalance).HasPrecision(18, 2);
        entity.HasIndex(x => new
        {
            x.ReconciliationRunId,
            x.SourceType,
            x.EntityCode,
            x.ControlAccount,
            x.CurrencyCode,
            x.FundCode,
            x.DepartmentCode
        });
    });

    modelBuilder.Entity<ReconciliationBreakCase>(entity =>
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.SubledgerBalance).HasPrecision(18, 2);
        entity.Property(x => x.GlBalance).HasPrecision(18, 2);
        entity.Property(x => x.Difference).HasPrecision(18, 2);
        entity.Property(x => x.DifferencePercent).HasPrecision(9, 4);
        entity.HasIndex(x => new { x.ReconciliationRunId, x.Status, x.BreakCategory });
    });
}
```

## Control-Account Configuration

```csharp
public sealed class SubledgerReconciliationOptions
{
    public Dictionary<string, ControlAccountMapping[]> Mappings { get; set; } = [];
    public Dictionary<string, ToleranceSettings> Tolerances { get; set; } = [];
    public AlertSettings Alerts { get; set; } = new();
}

public sealed class ControlAccountMapping
{
    public string ScopeCode { get; set; } = string.Empty;
    public string SubledgerType { get; set; } = string.Empty;
    public string EntityCode { get; set; } = string.Empty;
    public string ControlAccount { get; set; } = string.Empty;
    public string? FundCode { get; set; }
    public string? DepartmentCode { get; set; }
}

public sealed class ToleranceSettings
{
    public decimal AbsoluteAmount { get; set; }
    public decimal Percentage { get; set; }
    public int DocumentCountVariance { get; set; }
}

public sealed class AlertSettings
{
    public string SummaryRecipient { get; set; } = string.Empty;
    public string EscalationRecipient { get; set; } = string.Empty;
    public int AgingThresholdDays { get; set; }
}
```

## Reconciliation Run Flow

| Step | Implementation Detail | Pass |
|---|---|---|
| Run creation | Persist one `ReconciliationRun` row before extracts begin. | Run id exists before snapshot or break rows exist. |
| Extract phase | Execute subledger and GL extract queries with the same `CutoffUtc` and dimension filters. | Snapshot rows preserve one shared cutoff. |
| Compare phase | Group by `BalanceKey`, calculate difference metrics, and classify status. | Every balance group receives one status. |
| Persist phase | Store snapshots, break rows, and summary totals in one transaction. | Rerun after failure does not create partial evidence. |
| Notify phase | Send summary notification with break counts and severity. | Alerts reference persisted run id. |
| Review phase | Persist reviewer sign-off or rejection with note and timestamp. | Sign-off record points to one run id and one actor. |

## Shared Compare Algorithm

```csharp
public static IReadOnlyList<BreakCaseRow> CompareBalances(
    IEnumerable<SubledgerBalanceRow> subledgerRows,
    IEnumerable<GlControlBalanceRow> glRows,
    ToleranceRule tolerance)
{
    var subledgerLookup = subledgerRows
        .GroupBy(x => x.Key)
        .ToDictionary(
            x => x.Key,
            x => new
            {
                Balance = x.Sum(y => y.EndingBalance),
                Count = x.Sum(y => y.DocumentCount)
            });

    var glLookup = glRows
        .GroupBy(x => x.Key)
        .ToDictionary(
            x => x.Key,
            x => new
            {
                Balance = x.Sum(y => y.EndingBalance),
                Count = x.Sum(y => y.JournalLineCount)
            });

    var keys = subledgerLookup.Keys.Union(glLookup.Keys).OrderBy(x => x.EntityCode).ThenBy(x => x.ControlAccount).ToArray();
    var results = new List<BreakCaseRow>(keys.Length);

    foreach (var key in keys)
    {
        var subledger = subledgerLookup.GetValueOrDefault(key);
        var gl = glLookup.GetValueOrDefault(key);

        var subledgerBalance = subledger?.Balance ?? 0m;
        var glBalance = gl?.Balance ?? 0m;
        var difference = subledgerBalance - glBalance;
        var baseAmount = Math.Max(Math.Abs(subledgerBalance), Math.Abs(glBalance));
        var differencePercent = baseAmount == 0m ? 0m : Math.Abs(difference) / baseAmount;
        var countDifference = (subledger?.Count ?? 0) - (gl?.Count ?? 0);

        var amountWithinTolerance = Math.Abs(difference) <= tolerance.AbsoluteAmount;
        var percentageWithinTolerance = differencePercent <= tolerance.Percentage;
        var countWithinTolerance = Math.Abs(countDifference) <= tolerance.DocumentCountVariance;

        var status = amountWithinTolerance && percentageWithinTolerance && countWithinTolerance
            ? "Matched"
            : "Break";

        results.Add(new BreakCaseRow(
            key,
            subledgerBalance,
            glBalance,
            difference,
            differencePercent,
            countDifference,
            status,
            string.Empty));
    }

    return results;
}
```

## Break Classification Algorithm

```csharp
public static string ClassifyBreak(
    BreakCaseRow row,
    DateTimeOffset cutoffUtc,
    bool subsequentPeriodOffsetExists,
    bool duplicateSourceReferenceExists,
    bool mappingGapExists,
    bool foreignCurrencyPopulationExists,
    bool valuationAdjustmentPopulationExists)
{
    if (row.Status == "Matched")
    {
        return "Matched";
    }

    if (duplicateSourceReferenceExists)
    {
        return "DuplicateImpact";
    }

    if (mappingGapExists)
    {
        return "Mapping";
    }

    if (foreignCurrencyPopulationExists)
    {
        return "Currency";
    }

    if (valuationAdjustmentPopulationExists)
    {
        return "Valuation";
    }

    if (subsequentPeriodOffsetExists && cutoffUtc != DateTimeOffset.MinValue)
    {
        return "Timing";
    }

    if (row.SubledgerBalance == 0m || row.GlBalance == 0m)
    {
        return "MissingPosting";
    }

    return "Cutoff";
}
```

## Tolerance Strategy

| Threshold Type | Use | Example |
|---|---|---|
| Absolute amount | Catches direct dollar differences | `0.01` for strict control accounts, `5.00` for low-volume fringe items |
| Percentage | Catches materiality relative to balance size | `0.001` for 0.10% |
| Count variance | Catches hidden population defects even when amounts net to zero | `0` for strict doc count tie-out |
| Aging threshold | Catches stale unresolved items | `5` close days or `30` calendar days |

```json
{
  "SubledgerReconciliation": {
    "Tolerances": {
      "AR": {
        "AbsoluteAmount": 0.01,
        "Percentage": 0.0001,
        "DocumentCountVariance": 0
      },
      "AP": {
        "AbsoluteAmount": 0.01,
        "Percentage": 0.0001,
        "DocumentCountVariance": 0
      },
      "Payroll": {
        "AbsoluteAmount": 1.00,
        "Percentage": 0.0005,
        "DocumentCountVariance": 0
      },
      "FixedAssets": {
        "AbsoluteAmount": 0.01,
        "Percentage": 0.0001,
        "DocumentCountVariance": 0
      }
    }
  }
}
```

## Accounts Receivable Sample

### AR Extract Query

```csharp
public sealed class AccountsReceivableExtractor(AppDbContext dbContext) : ISubledgerBalanceExtractor
{
    public async Task<IReadOnlyList<SubledgerBalanceRow>> ExtractAsync(
        ReconciliationScope scope,
        CancellationToken cancellationToken)
    {
        return await dbContext.ArOpenItems
            .Where(x => x.PostingDate <= scope.CutoffUtc.UtcDateTime)
            .Where(x => x.Status != "Voided")
            .GroupBy(x => new
            {
                x.EntityCode,
                x.ControlAccount,
                x.CurrencyCode,
                x.FundCode,
                x.DepartmentCode
            })
            .Select(x => new SubledgerBalanceRow(
                new BalanceKey(
                    x.Key.EntityCode,
                    x.Key.ControlAccount,
                    x.Key.CurrencyCode,
                    x.Key.FundCode,
                    x.Key.DepartmentCode),
                x.Sum(y => y.OpenAmount),
                x.Count(),
                "AR-OPEN-ITEMS",
                scope.CutoffUtc))
            .ToListAsync(cancellationToken);
    }
}
```

### AR Break Patterns

| Pattern | Query Focus | Resolution |
|---|---|---|
| Unapplied cash not posted to control | Find cash receipts with `AppliedAtUtc` after cutoff and GL batch before cutoff mismatch. | Post or reclass unapplied cash. |
| Customer-account map defect | Compare customer class to mapped control account. | Correct master-data map and reclass. |
| Duplicate invoice import | Group by external invoice number, customer, and amount. | Reverse duplicate invoice or import batch. |
| Allowance mismatch | Recalculate aging reserve and compare to allowance control. | Post allowance adjustment. |

## Accounts Payable Sample

### AP Extract Query

```csharp
public sealed class AccountsPayableExtractor(AppDbContext dbContext) : ISubledgerBalanceExtractor
{
    public async Task<IReadOnlyList<SubledgerBalanceRow>> ExtractAsync(
        ReconciliationScope scope,
        CancellationToken cancellationToken)
    {
        return await dbContext.ApVouchers
            .Where(x => x.AccountingDate <= scope.CutoffUtc.UtcDateTime)
            .Where(x => x.Status != "Cancelled")
            .GroupBy(x => new
            {
                x.EntityCode,
                x.ControlAccount,
                x.CurrencyCode,
                x.FundCode,
                x.DepartmentCode
            })
            .Select(x => new SubledgerBalanceRow(
                new BalanceKey(
                    x.Key.EntityCode,
                    x.Key.ControlAccount,
                    x.Key.CurrencyCode,
                    x.Key.FundCode,
                    x.Key.DepartmentCode),
                x.Sum(y => y.OpenLiabilityAmount),
                x.Count(),
                "AP-VOUCHERS",
                scope.CutoffUtc))
            .ToListAsync(cancellationToken);
    }
}
```

### AP Break Patterns

| Pattern | Query Focus | Resolution |
|---|---|---|
| Invoice hold population omitted | Filter held vouchers and compare to subledger report criteria. | Align extract filter or release hold with approval. |
| GRNI lag | Compare receipts before cutoff to AP accrual journal status. | Post accrual or reverse stale receipt accrual. |
| Vendor merge defect | Compare merged vendor ids to control-account map. | Repair vendor master relationship and reclass. |
| Duplicate voucher load | Group by vendor invoice number, vendor id, and amount. | Reverse duplicate voucher. |

## Payroll Sample

### Payroll Extract Query

```csharp
public sealed class PayrollExtractor(AppDbContext dbContext) : ISubledgerBalanceExtractor
{
    public async Task<IReadOnlyList<SubledgerBalanceRow>> ExtractAsync(
        ReconciliationScope scope,
        CancellationToken cancellationToken)
    {
        return await dbContext.PayrollLiabilityLines
            .Where(x => x.CheckDate <= scope.CutoffUtc.UtcDateTime)
            .Where(x => x.VoidFlag == false)
            .GroupBy(x => new
            {
                x.EntityCode,
                x.ControlAccount,
                x.CurrencyCode,
                x.FundCode,
                x.DepartmentCode
            })
            .Select(x => new SubledgerBalanceRow(
                new BalanceKey(
                    x.Key.EntityCode,
                    x.Key.ControlAccount,
                    x.Key.CurrencyCode,
                    x.Key.FundCode,
                    x.Key.DepartmentCode),
                x.Sum(y => y.LiabilityAmount),
                x.Count(),
                "PAYROLL-LIABILITIES",
                scope.CutoffUtc))
            .ToListAsync(cancellationToken);
    }
}
```

### Payroll Break Patterns

| Pattern | Query Focus | Resolution |
|---|---|---|
| Off-cycle run not posted | Compare payroll register id to GL batch id. | Post delayed batch or hold reconciliation open with evidence. |
| Tax remittance lag | Compare payroll tax liability to remittance clearing activity after cutoff. | Clear remittance or age unresolved tax liability. |
| Labor distribution defect | Compare earning code and cost center maps to liability control. | Correct labor distribution and post adjustment. |
| Stale payroll clearing item | List checks or ACH items older than aging threshold. | Void, reissue, escheat, or escalate. |

## Fixed Assets Sample

### Fixed-Assets Extract Query

```csharp
public sealed class FixedAssetsExtractor(AppDbContext dbContext) : ISubledgerBalanceExtractor
{
    public async Task<IReadOnlyList<SubledgerBalanceRow>> ExtractAsync(
        ReconciliationScope scope,
        CancellationToken cancellationToken)
    {
        var costRows = dbContext.AssetBooks
            .Where(x => x.InServiceDate <= scope.CutoffUtc.UtcDateTime)
            .GroupBy(x => new
            {
                x.EntityCode,
                x.AssetCostControlAccount,
                x.CurrencyCode,
                x.FundCode,
                x.DepartmentCode
            })
            .Select(x => new SubledgerBalanceRow(
                new BalanceKey(
                    x.Key.EntityCode,
                    x.Key.AssetCostControlAccount,
                    x.Key.CurrencyCode,
                    x.Key.FundCode,
                    x.Key.DepartmentCode),
                x.Sum(y => y.HistoricalCost),
                x.Count(),
                "FA-COST",
                scope.CutoffUtc));

        var depreciationRows = dbContext.AssetBooks
            .Where(x => x.InServiceDate <= scope.CutoffUtc.UtcDateTime)
            .GroupBy(x => new
            {
                x.EntityCode,
                x.AccumulatedDepreciationControlAccount,
                x.CurrencyCode,
                x.FundCode,
                x.DepartmentCode
            })
            .Select(x => new SubledgerBalanceRow(
                new BalanceKey(
                    x.Key.EntityCode,
                    x.Key.AccumulatedDepreciationControlAccount,
                    x.Key.CurrencyCode,
                    x.Key.FundCode,
                    x.Key.DepartmentCode),
                x.Sum(y => y.AccumulatedDepreciation),
                x.Count(),
                "FA-DEPRECIATION",
                scope.CutoffUtc));

        return await costRows.Concat(depreciationRows).ToListAsync(cancellationToken);
    }
}
```

### Fixed-Assets Break Patterns

| Pattern | Query Focus | Resolution |
|---|---|---|
| Depreciation run gap | Compare expected depreciation periods to posted run table. | Re-run depreciation or correct asset state. |
| Disposal not posted | Compare disposed assets to GL disposal journals. | Post disposal or restore asset state pending review. |
| CIP transfer lag | Compare capital project completion date to in-service date and CIP clearing journals. | Transfer CIP and capitalize asset. |
| Asset-class map defect | Compare asset category to cost and depreciation controls. | Correct category mapping and reclass. |

## GL Control Extract Query

```csharp
public sealed class GlControlExtractor(AppDbContext dbContext) : IGlControlBalanceExtractor
{
    public async Task<IReadOnlyList<GlControlBalanceRow>> ExtractAsync(
        ReconciliationScope scope,
        IReadOnlyCollection<ControlAccountMapping> mappings,
        CancellationToken cancellationToken)
    {
        var mappedAccounts = mappings.Select(x => x.ControlAccount).Distinct().ToArray();

        return await dbContext.GeneralLedgerLines
            .Where(x => x.PostingDate <= scope.CutoffUtc.UtcDateTime)
            .Where(x => mappedAccounts.Contains(x.AccountNumber))
            .GroupBy(x => new
            {
                x.EntityCode,
                ControlAccount = x.AccountNumber,
                x.CurrencyCode,
                x.FundCode,
                x.DepartmentCode
            })
            .Select(x => new GlControlBalanceRow(
                new BalanceKey(
                    x.Key.EntityCode,
                    x.Key.ControlAccount,
                    x.Key.CurrencyCode,
                    x.Key.FundCode,
                    x.Key.DepartmentCode),
                x.Sum(y => y.DebitAmount - y.CreditAmount),
                x.Count(),
                "GL-LINES",
                scope.CutoffUtc))
            .ToListAsync(cancellationToken);
    }
}
```

## Investigation Queries

| Question | Query Pattern | Pass |
|---|---|---|
| Does a next-period offset exist? | Search the same source reference or grouped balance in the first subsequent period. | Query returns exact offset evidence or none. |
| Does a duplicate source reference exist? | Group by source reference, amount, and entity. | Duplicate groups show count greater than one. |
| Does a mapping gap exist? | Left join source segment values to control-account map. | Unmapped or multi-mapped rows appear explicitly. |
| Does a stale clearing item exist? | Filter unresolved clearing rows older than configured aging threshold. | Output shows item age, owner, and amount. |

## Email Alert Patterns

| Alert Type | Trigger | Recipient | Payload |
|---|---|---|---|
| Summary alert | Every completed run | Close mailbox or preparer | Run id, scope, matched count, break count, total difference |
| Break escalation | Break amount or count exceeds threshold | Reviewer or controller | Top breaks, owners, aging, evidence links |
| Stale-item escalation | Break stays open beyond aging threshold | Resolver and reviewer | Open days, last note, unresolved amount |
| Rejected sign-off | Reviewer rejects certification | Preparer and manager | Rejection note, resubmission due date |

```csharp
public sealed class ReconciliationEmailAlertPublisher(IEmailClient emailClient) : IAlertPublisher
{
    public async Task PublishSummaryAsync(
        ReconciliationRun run,
        IReadOnlyCollection<ReconciliationBreakCase> breakCases,
        AlertSettings alertSettings,
        CancellationToken cancellationToken)
    {
        var subject = $"Reconciliation {run.ScopeCode} {run.FiscalYear}-{run.FiscalPeriod} {run.Status}";
        var body = string.Join(Environment.NewLine,
        [
            $"RunId: {run.Id}",
            $"Scope: {run.ScopeCode}",
            $"SubledgerType: {run.SubledgerType}",
            $"BreakCount: {breakCases.Count}",
            $"OpenBreakAmount: {breakCases.Sum(x => Math.Abs(x.Difference)):N2}"
        ]);

        await emailClient.SendAsync(
            alertSettings.SummaryRecipient,
            subject,
            body,
            cancellationToken);
    }
}
```

## Audit Trail Requirements

| Audit Element | Requirement | Pass |
|---|---|---|
| Run identity | Persist immutable run id and configuration hash. | Same input and configuration reproduce the same result. |
| Cutoff evidence | Persist extract timestamp, fiscal period, and filter definition. | Reviewer sees the exact cutoff used. |
| Source trace | Persist source references, grouped counts, and source-system identifiers. | Each balance traces back to detailed populations. |
| User trace | Persist preparer, reviewer, resolver, and timestamp data. | Every disposition event has actor identity. |
| Adjustment trace | Link each correction journal or source fix to one break case. | Open-to-close chain remains visible. |
| Change trace | Version tolerance and mapping configuration. | Historical runs preserve their original rule set. |

## Sign-Off Pattern

```csharp
public sealed class SignOffRecord
{
    public Guid Id { get; set; }
    public Guid ReconciliationRunId { get; set; }
    public string ActionCode { get; set; } = string.Empty;
    public string ActorUserId { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTimeOffset ActionedAtUtc { get; set; }
}
```

| Action Code | Meaning | Pass |
|---|---|---|
| `Prepared` | Preparer completed extracts and break classification. | Evidence package exists. |
| `Reviewed` | Reviewer validated evidence and open-item treatment. | Reviewer note exists. |
| `Approved` | Certification accepted for the period. | No open material breaks remain without approved disposition. |
| `Rejected` | Reviewer returned the run for rework. | Rejection note and owner exist. |

## Validation Plan

| Test Area | Test | Pass |
|---|---|---|
| Exact match | Run compare logic with equal balances and counts. | Status equals `Matched`. |
| Amount break | Run compare logic with balance variance beyond threshold. | Status equals `Break` and difference equals expected amount. |
| Count break | Run compare logic with count variance and net-zero amount. | Break still opens. |
| Timing classification | Inject next-period offset evidence. | Classifier returns `Timing`. |
| Duplicate classification | Inject duplicate source evidence. | Classifier returns `DuplicateImpact`. |
| Mapping classification | Inject unmapped segment evidence. | Classifier returns `Mapping`. |
| Persistence | Save one run with snapshots and break rows. | Transaction commits one consistent evidence package. |
| Alerting | Publish summary with open breaks. | Email payload includes run id and open-break metrics. |

## Operational Notes

| Area | Pattern |
|---|---|
| Idempotency | Use one unique key on scope, fiscal period, and cutoff. |
| Performance | Pre-aggregate subledger and GL populations in SQL before materialization. |
| Partitioning | Partition evidence tables by fiscal year or run date for long-term history. |
| Security | Restrict evidence access by entity, fund, and finance role. |
| Retention | Keep evidence, sign-off notes, and configuration snapshots per policy. |

