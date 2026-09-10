---
title: Adjustment Implementation Reference
doc_type: reference
status: active
last_updated: 2026-08-31
summary: Detailed .NET and EF Core patterns for accounting adjustment entries, approvals, period control, and audit evidence.
target_audience: ai
related_docs:
  - ../SKILL.md
tags:
  - adjustment
  - journal-entry
  - accrual
  - correction
  - accounting
---
# Adjustment Implementation Reference

## Implementation Scope

This reference expands `adjustment-entry-patterns` with .NET implementation patterns for correcting entries, reversing entries, accrual entries, reclassification entries, and prior-period adjustments. The design keeps posted history immutable, approvals explicit, and period controls deterministic.

## Cross References

- [Adjustment Entry Patterns](../SKILL.md)
- [Journal Entry Patterns](../../journal-entry-patterns/SKILL.md)
- [Double-Entry Accounting](../../double-entry-accounting/SKILL.md)
- [Audit Trail Compliance](../../audit-trail-compliance/SKILL.md)

## Core Adjustment Invariants

| Invariant | Test | Pass |
|---|---|---|
| Posted entries stay immutable. | Attempt to update posted header or line content in place. | Persistence blocks the update path and a linked adjustment path remains available. |
| Every adjustment balances. | Sum debits and credits on each adjustment entry. | Difference equals `0.00` within configured precision. |
| Every adjustment stores reason, support, and source context. | Review adjustment header data. | Reason code, support reference, and source reference remain populated where policy requires them. |
| Period state drives posting path. | Submit the same request against open, soft-close, hard-close, and issued periods. | Handler result changes by period state according to policy. |
| Approval state precedes posting state. | Attempt direct post without approval on approval-bound types. | Handler blocks posting and returns policy failure. |
| Auto-reversal executes once. | Re-run reversal executor for one due entry. | Exactly one reversing entry exists for the scheduled source. |

## Shared Domain Model

```csharp
namespace Accounting.Adjustments;

public enum AdjustmentEntryType
{
    Correcting,
    Reversing,
    Accrual,
    Reclassification,
    PriorPeriodAdjustment
}

public enum AdjustmentStatus
{
    Draft,
    PendingApproval,
    Approved,
    Posted,
    Reversed,
    Rejected
}

public enum PeriodGateStatus
{
    Open,
    SoftClosed,
    HardClosed,
    Issued
}

public enum CorrectionMethod
{
    ReverseAndRepost,
    DeltaOnly
}

public sealed record FiscalPeriod(string FiscalYear, string FiscalMonth, DateOnly StartDate, DateOnly EndDate);

public sealed record AdjustmentLineInput(
    Guid AccountId,
    string FundCode,
    string DepartmentCode,
    string? ProgramCode,
    string? ProjectCode,
    decimal Debit,
    decimal Credit,
    string Description);

public sealed record AdjustmentDraft(
    string EntryPrefix,
    AdjustmentEntryType EntryType,
    string EntityCode,
    FiscalPeriod Period,
    string ReasonCode,
    string Explanation,
    string SupportReference,
    string PreparedByUserId,
    Guid? SourceEntryId,
    Guid? ReversesEntryId,
    DateOnly? AutoReverseOn,
    string? ApprovalPackageId = null,
    string? DisclosureReference = null);

public sealed class AdjustmentEntry
{
    private readonly List<AdjustmentLine> lines = [];

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string EntryNumber { get; private set; } = string.Empty;
    public AdjustmentEntryType EntryType { get; private set; }
    public AdjustmentStatus Status { get; private set; } = AdjustmentStatus.Draft;
    public Guid? SourceEntryId { get; private set; }
    public Guid? ReversesEntryId { get; private set; }
    public string EntityCode { get; private set; } = string.Empty;
    public FiscalPeriod EffectivePeriod { get; private set; } = new(string.Empty, string.Empty, DateOnly.MinValue, DateOnly.MinValue);
    public string ReasonCode { get; private set; } = string.Empty;
    public string Explanation { get; private set; } = string.Empty;
    public string SupportReference { get; private set; } = string.Empty;
    public string PreparedByUserId { get; private set; } = string.Empty;
    public DateTimeOffset PreparedAtUtc { get; private set; }
    public DateOnly? AutoReverseOn { get; private set; }
    public string? ApprovalPackageId { get; private set; }
    public string? DisclosureReference { get; private set; }
    public IReadOnlyList<AdjustmentLine> Lines => lines;

    private AdjustmentEntry() { }

    public static AdjustmentEntry Create(AdjustmentDraft draft, string entryNumber, DateTimeOffset preparedAtUtc)
    {
        return new AdjustmentEntry
        {
            EntryNumber = entryNumber,
            EntryType = draft.EntryType,
            EntityCode = draft.EntityCode,
            EffectivePeriod = draft.Period,
            ReasonCode = draft.ReasonCode,
            Explanation = draft.Explanation,
            SupportReference = draft.SupportReference,
            PreparedByUserId = draft.PreparedByUserId,
            PreparedAtUtc = preparedAtUtc,
            SourceEntryId = draft.SourceEntryId,
            ReversesEntryId = draft.ReversesEntryId,
            AutoReverseOn = draft.AutoReverseOn,
            ApprovalPackageId = draft.ApprovalPackageId,
            DisclosureReference = draft.DisclosureReference
        };
    }

    public void AddLine(AdjustmentLine line) => lines.Add(line);
    public void SubmitForApproval() => Status = AdjustmentStatus.PendingApproval;
    public void MarkApproved() => Status = AdjustmentStatus.Approved;
    public void MarkPosted() => Status = AdjustmentStatus.Posted;
}

public sealed class AdjustmentLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; private set; }
    public string FundCode { get; private set; } = string.Empty;
    public string DepartmentCode { get; private set; } = string.Empty;
    public string? ProgramCode { get; private set; }
    public string? ProjectCode { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public string Description { get; private set; } = string.Empty;

    private AdjustmentLine() { }

    public static AdjustmentLine Create(AdjustmentLineInput input) => new()
    {
        AccountId = input.AccountId,
        FundCode = input.FundCode,
        DepartmentCode = input.DepartmentCode,
        ProgramCode = input.ProgramCode,
        ProjectCode = input.ProjectCode,
        Debit = input.Debit,
        Credit = input.Credit,
        Description = input.Description
    };
}
```

## EF Core Persistence Shape

```csharp
namespace Accounting.Adjustments.Persistence;

public sealed class AdjustmentEntryEntity
{
    public Guid Id { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public string EntityCode { get; set; } = string.Empty;
    public string EntryType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? SourceEntryId { get; set; }
    public Guid? ReversesEntryId { get; set; }
    public string FiscalYear { get; set; } = string.Empty;
    public string FiscalMonth { get; set; } = string.Empty;
    public string ReasonCode { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string SupportReference { get; set; } = string.Empty;
    public string PreparedByUserId { get; set; } = string.Empty;
    public DateTimeOffset PreparedAtUtc { get; set; }
    public DateOnly? AutoReverseOn { get; set; }
    public string? ApprovalPackageId { get; set; }
    public string? DisclosureReference { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = [];
    public ICollection<AdjustmentLineEntity> Lines { get; set; } = [];
    public ICollection<AdjustmentApprovalEntity> Approvals { get; set; } = [];
    public ICollection<AdjustmentAuditEventEntity> AuditEvents { get; set; } = [];
}

public sealed class AdjustmentLineEntity
{
    public Guid Id { get; set; }
    public Guid AdjustmentEntryId { get; set; }
    public Guid AccountId { get; set; }
    public string FundCode { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public string? ProgramCode { get; set; }
    public string? ProjectCode { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Description { get; set; } = string.Empty;
}

public sealed class AdjustmentApprovalEntity
{
    public Guid Id { get; set; }
    public Guid AdjustmentEntryId { get; set; }
    public string StepCode { get; set; } = string.Empty;
    public string ActorUserId { get; set; } = string.Empty;
    public string ActorRole { get; set; } = string.Empty;
    public DateTimeOffset ActionedAtUtc { get; set; }
    public string Outcome { get; set; } = string.Empty;
    public string Memo { get; set; } = string.Empty;
}

public sealed class AdjustmentAuditEventEntity
{
    public Guid Id { get; set; }
    public Guid AdjustmentEntryId { get; set; }
    public string EventCode { get; set; } = string.Empty;
    public string ActorUserId { get; set; } = string.Empty;
    public string ActorRole { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public string Narrative { get; set; } = string.Empty;
    public string SourceChannel { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string BeforeSnapshotJson { get; set; } = string.Empty;
    public string AfterSnapshotJson { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
}
```

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Adjustments.Persistence;

public sealed class AdjustmentEntryConfiguration : IEntityTypeConfiguration<AdjustmentEntryEntity>
{
    public void Configure(EntityTypeBuilder<AdjustmentEntryEntity> builder)
    {
        builder.ToTable("AdjustmentEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntryNumber).HasMaxLength(40).IsRequired();
        builder.Property(x => x.EntityCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.EntryType).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ReasonCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SupportReference).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => x.EntryNumber).IsUnique();
        builder.HasIndex(x => x.SourceEntryId);
        builder.HasIndex(x => x.ReversesEntryId);
        builder.HasIndex(x => x.AutoReverseOn);
    }
}
```

## Shared Services and Helpers

```csharp
namespace Accounting.Adjustments.Application;

public sealed record PeriodGateResult(
    PeriodGateStatus Status,
    FiscalPeriod PostingPeriod,
    string RuleCode,
    string Explanation);

public interface IPeriodGateService
{
    Task<PeriodGateResult> ResolveAsync(string entityCode, AdjustmentEntryType entryType, FiscalPeriod requestedPeriod, Guid? sourceEntryId, CancellationToken cancellationToken);
}

public interface IApprovalPolicyService
{
    Task<IReadOnlyList<string>> ResolveRolesAsync(AdjustmentEntryType entryType, string entityCode, decimal amount, FiscalPeriod postingPeriod, CancellationToken cancellationToken);
}

public interface IEntryNumberService
{
    Task<string> NextAsync(string entityCode, string prefix, CancellationToken cancellationToken);
}

public interface IAdjustmentRepository
{
    Task<AdjustmentEntryProjection> GetPostedSourceAsync(Guid entryId, CancellationToken cancellationToken);
    Task AddAsync(AdjustmentEntry entry, string correlationId, CancellationToken cancellationToken);
}

public sealed record AdjustmentEntryProjection(
    Guid Id,
    string EntityCode,
    string Status,
    string FiscalYear,
    string FiscalMonth,
    IReadOnlyList<AdjustmentLineProjection> Lines);

public sealed record AdjustmentLineProjection(
    Guid AccountId,
    string FundCode,
    string DepartmentCode,
    string? ProgramCode,
    string? ProjectCode,
    decimal Debit,
    decimal Credit);

public interface IAuditTrailWriter
{
    Task WriteAsync(Guid adjustmentEntryId, string eventCode, string actorUserId, string actorRole, string reasonCode, string narrative, string sourceChannel, string correlationId, object beforeSnapshot, object afterSnapshot, string outcome, CancellationToken cancellationToken);
}

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
```

```csharp
namespace Accounting.Adjustments.Domain;

public static class AdjustmentValidators
{
    public static void EnsureBalanced(IEnumerable<AdjustmentLineInput> lines)
    {
        var totalDebit = lines.Sum(x => x.Debit);
        var totalCredit = lines.Sum(x => x.Credit);
        if (lines.Any() is false || decimal.Round(totalDebit - totalCredit, 2) != 0m)
            throw new InvalidOperationException("Adjustment lines are not balanced.");
    }

    public static void EnsureReason(string reasonCode, string explanation, string supportReference)
    {
        if (string.IsNullOrWhiteSpace(reasonCode) || string.IsNullOrWhiteSpace(explanation) || string.IsNullOrWhiteSpace(supportReference))
            throw new InvalidOperationException("Reason, explanation, and support reference are required.");
    }

    public static void EnsurePostedSource(string sourceStatus)
    {
        if (!string.Equals(sourceStatus, "Posted", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Source entry is not posted.");
    }
}

public static class AdjustmentFactory
{
    public static AdjustmentEntry Create(AdjustmentDraft draft, string entryNumber, IEnumerable<AdjustmentLineInput> lines, DateTimeOffset preparedAtUtc)
    {
        AdjustmentValidators.EnsureReason(draft.ReasonCode, draft.Explanation, draft.SupportReference);
        AdjustmentValidators.EnsureBalanced(lines);

        var entry = AdjustmentEntry.Create(draft, entryNumber, preparedAtUtc);
        foreach (var line in lines.Select(AdjustmentLine.Create))
            entry.AddLine(line);

        entry.SubmitForApproval();
        return entry;
    }

    public static IReadOnlyList<AdjustmentLineInput> Reverse(IEnumerable<AdjustmentLineProjection> lines, string description)
    {
        return lines.Select(line => new AdjustmentLineInput(
            line.AccountId,
            line.FundCode,
            line.DepartmentCode,
            line.ProgramCode,
            line.ProjectCode,
            line.Credit,
            line.Debit,
            description)).ToList();
    }
}
```

## Correcting Entry Pattern

| Concern | Pattern |
|---|---|
| Use | Fix amount, account, dimension, date, or description errors in posted activity. |
| Header fields | `SourceEntryId`, `CorrectionMethod`, `ReasonCode`, `SupportReference` |
| Line treatment | `ReverseAndRepost` reverses source lines and adds corrected lines. `DeltaOnly` posts net difference only. |
| Period treatment | Same period when open; current adjustment period when source period is locked. |
| Approval treatment | Separate approver from preparer. Sensitive or material corrections escalate. |

```csharp
using MediatR;

namespace Accounting.Adjustments.Application.CorrectingEntries;

public sealed record CreateCorrectingEntryCommand(
    Guid SourceEntryId,
    CorrectionMethod Method,
    FiscalPeriod RequestedPeriod,
    IReadOnlyList<AdjustmentLineInput> ReplacementLines,
    string ReasonCode,
    string Explanation,
    string SupportReference,
    string PreparedByUserId,
    string PreparedByRole,
    string CorrelationId) : IRequest<Guid>;

public sealed class CreateCorrectingEntryHandler(
    IAdjustmentRepository repository,
    IPeriodGateService periodGateService,
    IApprovalPolicyService approvalPolicyService,
    IEntryNumberService entryNumberService,
    IAuditTrailWriter auditTrailWriter,
    IClock clock) : IRequestHandler<CreateCorrectingEntryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCorrectingEntryCommand request, CancellationToken cancellationToken)
    {
        var source = await repository.GetPostedSourceAsync(request.SourceEntryId, cancellationToken);
        AdjustmentValidators.EnsurePostedSource(source.Status);

        var gate = await periodGateService.ResolveAsync(source.EntityCode, AdjustmentEntryType.Correcting, request.RequestedPeriod, request.SourceEntryId, cancellationToken);
        var entryNumber = await entryNumberService.NextAsync(source.EntityCode, "COR", cancellationToken);
        var lines = request.Method == CorrectionMethod.DeltaOnly
            ? request.ReplacementLines
            : AdjustmentFactory.Reverse(source.Lines, "Correction reversal").Concat(request.ReplacementLines).ToList();

        var entry = AdjustmentFactory.Create(
            new AdjustmentDraft("COR", AdjustmentEntryType.Correcting, source.EntityCode, gate.PostingPeriod, request.ReasonCode, request.Explanation, request.SupportReference, request.PreparedByUserId, request.SourceEntryId, null, null),
            entryNumber,
            lines,
            clock.UtcNow);

        var amount = entry.Lines.Sum(x => Math.Max(x.Debit, x.Credit));
        var roles = await approvalPolicyService.ResolveRolesAsync(AdjustmentEntryType.Correcting, source.EntityCode, amount, gate.PostingPeriod, cancellationToken);
        await repository.AddAsync(entry, request.CorrelationId, cancellationToken);
        await auditTrailWriter.WriteAsync(entry.Id, "adjustment.correcting.created", request.PreparedByUserId, request.PreparedByRole, request.ReasonCode, request.Explanation, "application", request.CorrelationId, new { source.Id }, new { entry.Id, entry.EntryNumber, Roles = roles }, "success", cancellationToken);
        return entry.Id;
    }
}
```

| Scenario | Test | Pass |
|---|---|---|
| Open-period correction | Submit `ReverseAndRepost` for an open source period. | New entry posts in the source period and source entry remains unchanged. |
| Locked-period correction | Submit the same request against a hard-closed source period. | Handler routes posting to the approved current adjustment period. |
| Delta correction | Submit `DeltaOnly` for one under-accrued expense line. | Entry balances and net effect equals the requested delta only. |

## Reversing Entry Pattern

| Concern | Pattern |
|---|---|
| Use | Reverse temporary accruals and estimates in the next period. |
| Header fields | `SourceEntryId`, `ReversesEntryId`, `AutoReverseOn`, `ReasonCode` |
| Line treatment | Reverse source lines by account, dimension, and amount. |
| Period treatment | Next open period from the period gate service. |
| Approval treatment | Source approval usually satisfies policy. Manual override routes through reviewer approval. |

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Adjustments.Application.ReversingEntries;

public sealed record ScheduleReversingEntryCommand(
    Guid SourceEntryId,
    DateOnly RequestedReverseDate,
    bool AutoReverse,
    string ReasonCode,
    string Explanation,
    string SupportReference,
    string PreparedByUserId,
    string PreparedByRole,
    string CorrelationId) : IRequest<Guid>;

public sealed class ScheduleReversingEntryHandler(
    IAdjustmentRepository repository,
    IPeriodGateService periodGateService,
    IEntryNumberService entryNumberService,
    IAuditTrailWriter auditTrailWriter,
    IClock clock) : IRequestHandler<ScheduleReversingEntryCommand, Guid>
{
    public async Task<Guid> Handle(ScheduleReversingEntryCommand request, CancellationToken cancellationToken)
    {
        var source = await repository.GetPostedSourceAsync(request.SourceEntryId, cancellationToken);
        AdjustmentValidators.EnsurePostedSource(source.Status);

        var requestedPeriod = new FiscalPeriod(source.FiscalYear, source.FiscalMonth, request.RequestedReverseDate, request.RequestedReverseDate);
        var gate = await periodGateService.ResolveAsync(source.EntityCode, AdjustmentEntryType.Reversing, requestedPeriod, source.Id, cancellationToken);
        var entryNumber = await entryNumberService.NextAsync(source.EntityCode, "REV", cancellationToken);

        var entry = AdjustmentFactory.Create(
            new AdjustmentDraft("REV", AdjustmentEntryType.Reversing, source.EntityCode, gate.PostingPeriod, request.ReasonCode, request.Explanation, request.SupportReference, request.PreparedByUserId, source.Id, source.Id, request.AutoReverse ? request.RequestedReverseDate : null),
            entryNumber,
            AdjustmentFactory.Reverse(source.Lines, "Automatic reversal"),
            clock.UtcNow);

        await repository.AddAsync(entry, request.CorrelationId, cancellationToken);
        await auditTrailWriter.WriteAsync(entry.Id, "adjustment.reversing.scheduled", request.PreparedByUserId, request.PreparedByRole, request.ReasonCode, request.Explanation, "application", request.CorrelationId, new { source.Id }, new { entry.Id, entry.EntryNumber, entry.AutoReverseOn }, "success", cancellationToken);
        return entry.Id;
    }
}

public sealed class AutoReversalProcessor(
    AccountingDbContext dbContext,
    IJournalPostingService postingService,
    IAuditTrailWriter auditTrailWriter,
    IClock clock)
{
    public async Task<int> ExecuteAsync(DateOnly businessDate, CancellationToken cancellationToken)
    {
        var dueEntries = await dbContext.AdjustmentEntries
            .Where(x => x.EntryType == "Reversing" && x.AutoReverseOn == businessDate && x.Status == "Approved")
            .ToListAsync(cancellationToken);

        foreach (var entry in dueEntries)
        {
            await postingService.PostAsync(entry.Id, cancellationToken);
            await auditTrailWriter.WriteAsync(entry.Id, "adjustment.reversing.executed", "system:auto-reversal", "scheduler", entry.ReasonCode, "Automatic reversal execution", "scheduler", entry.CorrelationId, new { Status = "Approved" }, new { Status = "Posted", ExecutedAtUtc = clock.UtcNow }, "success", cancellationToken);
        }

        return dueEntries.Count;
    }
}
```

| Scenario | Test | Pass |
|---|---|---|
| Due reversal | Execute processor on the scheduled date. | One posted reverse entry exists and scheduler audit event exists. |
| Duplicate execution | Execute processor twice on the same date. | Second run posts zero additional entries. |
| Closed reverse period | Resolve reverse date into a closed period. | Period gate shifts to the next open period and logs the rule code. |

## Accrual Entry Pattern

| Concern | Pattern |
|---|---|
| Use | Recognize revenue or expense in the earning or incurrence period. |
| Header fields | `RecognitionPeriod`, `AccrualBasis`, `AutoReverseOn`, `ReasonCode` |
| Line treatment | Balance estimated receivable, payable, revenue, or expense accounts. |
| Period treatment | Current recognition period drives posting. |
| Approval treatment | Reviewer checks estimate basis, cutoff, and materiality. |

```csharp
using MediatR;

namespace Accounting.Adjustments.Application.AccrualEntries;

public interface IAccrualMatchingService
{
    Task<bool> BelongsToPeriodAsync(string entityCode, FiscalPeriod recognitionPeriod, DateOnly supportingEventDate, string accrualBasis, CancellationToken cancellationToken);
}

public sealed record CreateAccrualEntryCommand(
    string EntityCode,
    FiscalPeriod RecognitionPeriod,
    DateOnly SupportingEventDate,
    bool AutoReverseNextPeriod,
    string AccrualBasis,
    IReadOnlyList<AdjustmentLineInput> Lines,
    string ReasonCode,
    string Explanation,
    string SupportReference,
    string PreparedByUserId,
    string PreparedByRole,
    string CorrelationId) : IRequest<Guid>;

public sealed class CreateAccrualEntryHandler(
    IAdjustmentRepository repository,
    IAccrualMatchingService accrualMatchingService,
    IPeriodGateService periodGateService,
    IApprovalPolicyService approvalPolicyService,
    IEntryNumberService entryNumberService,
    IAuditTrailWriter auditTrailWriter,
    IClock clock) : IRequestHandler<CreateAccrualEntryCommand, Guid>
{
    public async Task<Guid> Handle(CreateAccrualEntryCommand request, CancellationToken cancellationToken)
    {
        var belongs = await accrualMatchingService.BelongsToPeriodAsync(request.EntityCode, request.RecognitionPeriod, request.SupportingEventDate, request.AccrualBasis, cancellationToken);
        if (!belongs) throw new InvalidOperationException("Supporting event does not align with the requested recognition period.");

        var gate = await periodGateService.ResolveAsync(request.EntityCode, AdjustmentEntryType.Accrual, request.RecognitionPeriod, null, cancellationToken);
        var entryNumber = await entryNumberService.NextAsync(request.EntityCode, "ACC", cancellationToken);
        var autoReverseOn = request.AutoReverseNextPeriod ? gate.PostingPeriod.EndDate.AddDays(1) : (DateOnly?)null;

        var entry = AdjustmentFactory.Create(
            new AdjustmentDraft("ACC", AdjustmentEntryType.Accrual, request.EntityCode, gate.PostingPeriod, request.ReasonCode, request.Explanation, request.SupportReference, request.PreparedByUserId, null, null, autoReverseOn),
            entryNumber,
            request.Lines,
            clock.UtcNow);

        var amount = entry.Lines.Sum(x => Math.Max(x.Debit, x.Credit));
        var roles = await approvalPolicyService.ResolveRolesAsync(AdjustmentEntryType.Accrual, request.EntityCode, amount, gate.PostingPeriod, cancellationToken);
        await repository.AddAsync(entry, request.CorrelationId, cancellationToken);
        await auditTrailWriter.WriteAsync(entry.Id, "adjustment.accrual.created", request.PreparedByUserId, request.PreparedByRole, request.ReasonCode, request.Explanation, "application", request.CorrelationId, new { request.SupportingEventDate, request.AccrualBasis }, new { entry.Id, entry.EntryNumber, entry.AutoReverseOn, Roles = roles }, "success", cancellationToken);
        return entry.Id;
    }
}
```

| Scenario | Test | Pass |
|---|---|---|
| Matched period | Create accrual with support event inside the reporting period. | Entry persists with the requested recognition period. |
| Mismatched period | Create accrual with support event outside policy bounds. | Handler rejects the request before persistence. |
| Auto-reverse next period | Set `AutoReverseNextPeriod` to `true`. | Entry stores `AutoReverseOn` at the next-period start date. |

## Reclassification Entry Pattern

| Concern | Pattern |
|---|---|
| Use | Move valid amounts from one classification to another without changing total balance. |
| Header fields | `SourceEntryId`, `ReasonCode`, `SupportReference` |
| Line treatment | Use equal and opposite dual posting. |
| Period treatment | Same period when open; current adjustment period when the source period is locked. |
| Approval treatment | Account owner or controller verifies statement impact. |

```csharp
using MediatR;

namespace Accounting.Adjustments.Application.ReclassificationEntries;

public sealed record ReclassificationMove(
    Guid FromAccountId,
    Guid ToAccountId,
    string FundCode,
    string DepartmentCode,
    string? ProgramCode,
    string? ProjectCode,
    decimal Amount,
    string Description);

public sealed record CreateReclassificationEntryCommand(
    Guid SourceEntryId,
    FiscalPeriod RequestedPeriod,
    IReadOnlyList<ReclassificationMove> Moves,
    string ReasonCode,
    string Explanation,
    string SupportReference,
    string PreparedByUserId,
    string PreparedByRole,
    string CorrelationId) : IRequest<Guid>;

public static class ReclassificationLineBuilder
{
    public static IReadOnlyList<AdjustmentLineInput> Build(IReadOnlyList<ReclassificationMove> moves) =>
        moves.SelectMany(move => new[]
        {
            new AdjustmentLineInput(move.FromAccountId, move.FundCode, move.DepartmentCode, move.ProgramCode, move.ProjectCode, 0m, move.Amount, move.Description + " source"),
            new AdjustmentLineInput(move.ToAccountId, move.FundCode, move.DepartmentCode, move.ProgramCode, move.ProjectCode, move.Amount, 0m, move.Description + " target")
        }).ToList();
}

public sealed class CreateReclassificationEntryHandler(
    IAdjustmentRepository repository,
    IPeriodGateService periodGateService,
    IApprovalPolicyService approvalPolicyService,
    IEntryNumberService entryNumberService,
    IAuditTrailWriter auditTrailWriter,
    IClock clock) : IRequestHandler<CreateReclassificationEntryCommand, Guid>
{
    public async Task<Guid> Handle(CreateReclassificationEntryCommand request, CancellationToken cancellationToken)
    {
        var source = await repository.GetPostedSourceAsync(request.SourceEntryId, cancellationToken);
        AdjustmentValidators.EnsurePostedSource(source.Status);

        var lines = ReclassificationLineBuilder.Build(request.Moves);
        var gate = await periodGateService.ResolveAsync(source.EntityCode, AdjustmentEntryType.Reclassification, request.RequestedPeriod, source.Id, cancellationToken);
        var entryNumber = await entryNumberService.NextAsync(source.EntityCode, "RCL", cancellationToken);

        var entry = AdjustmentFactory.Create(
            new AdjustmentDraft("RCL", AdjustmentEntryType.Reclassification, source.EntityCode, gate.PostingPeriod, request.ReasonCode, request.Explanation, request.SupportReference, request.PreparedByUserId, source.Id, null, null),
            entryNumber,
            lines,
            clock.UtcNow);

        var amount = request.Moves.Sum(x => x.Amount);
        var roles = await approvalPolicyService.ResolveRolesAsync(AdjustmentEntryType.Reclassification, source.EntityCode, amount, gate.PostingPeriod, cancellationToken);
        await repository.AddAsync(entry, request.CorrelationId, cancellationToken);
        await auditTrailWriter.WriteAsync(entry.Id, "adjustment.reclassification.created", request.PreparedByUserId, request.PreparedByRole, request.ReasonCode, request.Explanation, "application", request.CorrelationId, new { source.Id, MoveCount = request.Moves.Count }, new { entry.Id, entry.EntryNumber, Roles = roles }, "success", cancellationToken);
        return entry.Id;
    }
}
```

| Scenario | Test | Pass |
|---|---|---|
| Same-period reclass | Move expense between departments in an open period. | Entry posts in the same period and total expense stays unchanged. |
| Cross-fund reclass | Submit fund change that policy flags as sensitive. | Approval policy escalates to controller or fund owner. |
| Locked-period reclass | Submit reclass after hard close. | Handler routes to the approved current adjustment period and keeps source linkage. |

## Prior Period Adjustment Pattern

| Concern | Pattern |
|---|---|
| Use | Record a rare material effect on previously issued results. |
| Header fields | `SourceEntryId`, `ApprovalPackageId`, `DisclosureReference`, `ReasonCode` |
| Line treatment | Post through approved restatement or retained-earnings pattern for the jurisdiction and policy set. |
| Period treatment | Special adjustment period, controlled reopening, or restatement period. |
| Approval treatment | Enhanced approval chain includes controller, finance lead, and policy-defined executive reviewer. |

```csharp
using MediatR;

namespace Accounting.Adjustments.Application.PriorPeriodAdjustments;

public interface IPriorPeriodApprovalService
{
    Task<bool> ValidatePackageAsync(string approvalPackageId, string disclosureReference, CancellationToken cancellationToken);
}

public sealed record CreatePriorPeriodAdjustmentCommand(
    Guid SourceEntryId,
    FiscalPeriod PostingPeriod,
    string ApprovalPackageId,
    string DisclosureReference,
    IReadOnlyList<AdjustmentLineInput> Lines,
    string ReasonCode,
    string Explanation,
    string SupportReference,
    string PreparedByUserId,
    string PreparedByRole,
    string CorrelationId) : IRequest<Guid>;

public sealed class CreatePriorPeriodAdjustmentHandler(
    IAdjustmentRepository repository,
    IPeriodGateService periodGateService,
    IApprovalPolicyService approvalPolicyService,
    IPriorPeriodApprovalService priorPeriodApprovalService,
    IEntryNumberService entryNumberService,
    IAuditTrailWriter auditTrailWriter,
    IClock clock) : IRequestHandler<CreatePriorPeriodAdjustmentCommand, Guid>
{
    public async Task<Guid> Handle(CreatePriorPeriodAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var packageApproved = await priorPeriodApprovalService.ValidatePackageAsync(request.ApprovalPackageId, request.DisclosureReference, cancellationToken);
        if (!packageApproved) throw new InvalidOperationException("Prior-period approval package is incomplete.");

        var source = await repository.GetPostedSourceAsync(request.SourceEntryId, cancellationToken);
        AdjustmentValidators.EnsurePostedSource(source.Status);

        var gate = await periodGateService.ResolveAsync(source.EntityCode, AdjustmentEntryType.PriorPeriodAdjustment, request.PostingPeriod, source.Id, cancellationToken);
        var entryNumber = await entryNumberService.NextAsync(source.EntityCode, "PPA", cancellationToken);
        var entry = AdjustmentFactory.Create(
            new AdjustmentDraft("PPA", AdjustmentEntryType.PriorPeriodAdjustment, source.EntityCode, gate.PostingPeriod, request.ReasonCode, request.Explanation, request.SupportReference, request.PreparedByUserId, source.Id, null, null, request.ApprovalPackageId, request.DisclosureReference),
            entryNumber,
            request.Lines,
            clock.UtcNow);

        var amount = entry.Lines.Sum(x => Math.Max(x.Debit, x.Credit));
        var roles = await approvalPolicyService.ResolveRolesAsync(AdjustmentEntryType.PriorPeriodAdjustment, source.EntityCode, amount, gate.PostingPeriod, cancellationToken);
        if (!roles.Contains("ExecutiveReviewer", StringComparer.Ordinal)) throw new InvalidOperationException("Enhanced prior-period approval chain is incomplete.");

        await repository.AddAsync(entry, request.CorrelationId, cancellationToken);
        await auditTrailWriter.WriteAsync(entry.Id, "adjustment.prior-period.created", request.PreparedByUserId, request.PreparedByRole, request.ReasonCode, request.Explanation, "application", request.CorrelationId, new { source.Id, request.ApprovalPackageId, request.DisclosureReference }, new { entry.Id, entry.EntryNumber, Roles = roles }, "success", cancellationToken);
        return entry.Id;
    }
}
```

| Scenario | Test | Pass |
|---|---|---|
| Complete approval package | Submit a prior-period request with approval and disclosure references. | Entry persists in pending approval state. |
| Missing executive reviewer | Resolve approvals without the executive step. | Handler rejects the request. |
| Issued-period source | Submit source entry from an issued period. | Source remains immutable and adjustment path uses the approved posting period only. |

## Approval Workflow Integration

| Entry Type | Required Roles | Extra Control | Pass |
|---|---|---|---|
| Correcting | Preparer, approver, controller by threshold | Preparer cannot self-approve material correction. | Stored approvals match policy. |
| Reversing | Reviewer, scheduler identity for auto path | Date-change override audit event | Override history remains visible. |
| Accrual | Preparer, accounting reviewer, controller by threshold | Support calculation review | Estimate basis remains attached. |
| Reclassification | Preparer, account owner or controller | Statement-impact review | Target classification rationale remains attached. |
| Prior period adjustment | Preparer, controller, finance lead, executive reviewer | Disclosure package validation | Approval chain is complete before posting. |

```csharp
namespace Accounting.Adjustments.Application.Approvals;

public sealed class AdjustmentApprovalOrchestrator(
    AccountingDbContext dbContext,
    IAuditTrailWriter auditTrailWriter,
    IClock clock)
{
    public async Task ApproveAsync(Guid adjustmentEntryId, string stepCode, string actorUserId, string actorRole, string memo, CancellationToken cancellationToken)
    {
        var entry = await dbContext.AdjustmentEntries.SingleAsync(x => x.Id == adjustmentEntryId, cancellationToken);
        dbContext.AdjustmentApprovals.Add(new AdjustmentApprovalEntity
        {
            Id = Guid.NewGuid(),
            AdjustmentEntryId = adjustmentEntryId,
            StepCode = stepCode,
            ActorUserId = actorUserId,
            ActorRole = actorRole,
            ActionedAtUtc = clock.UtcNow,
            Outcome = "Approved",
            Memo = memo
        });

        entry.Status = "Approved";
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditTrailWriter.WriteAsync(adjustmentEntryId, "adjustment.approved", actorUserId, actorRole, "approval", memo, "approval-workflow", entry.CorrelationId, new { Status = "PendingApproval" }, new { Status = "Approved", StepCode = stepCode }, "success", cancellationToken);
    }
}
```

## Period Control Integration

| Period State | Correcting | Reversing | Accrual | Reclassification | Prior Period Adjustment |
|---|---|---|---|---|---|
| Open | Post to source or requested period | Post to next open period | Post to recognition period | Post to requested period | Use only when policy classifies the event as prior-period |
| SoftClosed | Route through close-role approval | Keep reverse logic and log any date shift | Allow close accruals with cutoff evidence | Allow controlled reclass | Route through formal approval package |
| HardClosed | Redirect to current adjustment period | Use the next open period | Reject back-post and use current period only | Redirect to current adjustment period | Use special adjustment or restatement period |
| Issued | Preserve statement history | Preserve statement history | Preserve statement history | Preserve statement history | Preserve statement history and disclosure package |

```csharp
namespace Accounting.Adjustments.Application.Periods;

public sealed class PeriodGateService(IFinancialPeriodRepository repository) : IPeriodGateService
{
    public async Task<PeriodGateResult> ResolveAsync(string entityCode, AdjustmentEntryType entryType, FiscalPeriod requestedPeriod, Guid? sourceEntryId, CancellationToken cancellationToken)
    {
        var status = await repository.GetStatusAsync(entityCode, requestedPeriod.FiscalYear, requestedPeriod.FiscalMonth, cancellationToken);
        if (status == PeriodGateStatus.Open) return new(status, requestedPeriod, "period.open", "Requested period is open.");
        if (status == PeriodGateStatus.SoftClosed) return new(status, requestedPeriod, "period.soft-close", "Soft-close routing applies.");

        if (status == PeriodGateStatus.HardClosed)
        {
            var routed = entryType == AdjustmentEntryType.Reversing
                ? await repository.GetNextOpenPeriodAsync(entityCode, requestedPeriod.EndDate, cancellationToken)
                : await repository.GetCurrentAdjustmentPeriodAsync(entityCode, cancellationToken);
            return new(status, routed, "period.hard-close", "Source period is locked.");
        }

        var issuedPeriod = entryType == AdjustmentEntryType.PriorPeriodAdjustment
            ? await repository.GetRestatementPeriodAsync(entityCode, requestedPeriod, cancellationToken)
            : await repository.GetCurrentAdjustmentPeriodAsync(entityCode, cancellationToken);
        return new(status, issuedPeriod, "period.issued", "Issued statement history remains preserved.");
    }
}
```

## Audit Trail Requirements

| Category | Required Fields | Pass |
|---|---|---|
| Who | Actor user id, actor role, delegated identity when present | Every create, approve, reject, post, reverse, and override event stores actor identity. |
| What | Entry id, entry number, entry type, line counts, amount, source entry id | Every event identifies the affected adjustment artifact and source linkage. |
| When | Event timestamp in UTC, business date, approval-step timestamp | Event order remains deterministic and queryable. |
| Why | Reason code, explanation, support reference, policy rule code | Every sensitive event includes business purpose and policy path. |
| Where | Source channel, service name, batch id, correlation id | Traceability back to the initiating channel remains available. |
| Outcome | Success, rejection, failure, override, posting result | Every event records final disposition. |

```csharp
using System.Text.Json;

namespace Accounting.Adjustments.Infrastructure.Audit;

public sealed class AuditTrailWriter(AccountingDbContext dbContext, IClock clock) : IAuditTrailWriter
{
    public async Task WriteAsync(Guid adjustmentEntryId, string eventCode, string actorUserId, string actorRole, string reasonCode, string narrative, string sourceChannel, string correlationId, object beforeSnapshot, object afterSnapshot, string outcome, CancellationToken cancellationToken)
    {
        dbContext.AdjustmentAuditEvents.Add(new AdjustmentAuditEventEntity
        {
            Id = Guid.NewGuid(),
            AdjustmentEntryId = adjustmentEntryId,
            EventCode = eventCode,
            ActorUserId = actorUserId,
            ActorRole = actorRole,
            OccurredAtUtc = clock.UtcNow,
            ReasonCode = reasonCode,
            Narrative = narrative,
            SourceChannel = sourceChannel,
            CorrelationId = correlationId,
            BeforeSnapshotJson = JsonSerializer.Serialize(beforeSnapshot),
            AfterSnapshotJson = JsonSerializer.Serialize(afterSnapshot),
            Outcome = outcome
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

## Reporting and Posting Integration

| Surface | Requirement | Pass |
|---|---|---|
| Trial balance | Include posted adjustment lines in period totals and preserve source reference. | Trial balance reflects adjustment impact with traceable origin. |
| Adjustment register | Show type, source entry id, period, amount, reason, preparer, approver, and status. | Filtering by type and period remains available. |
| Close package | Show accruals, reversals due next period, and post-close adjustments. | Close package contains current-period and next-period follow-up evidence. |
| Audit package | Show source snapshots, approval steps, and audit events. | Full lifecycle reconstruction remains available. |

```csharp
namespace Accounting.Adjustments.Application.Posting;

public interface IJournalPostingService
{
    Task PostAsync(Guid adjustmentEntryId, CancellationToken cancellationToken);
}

public sealed class AdjustmentPostingService(AccountingDbContext dbContext, IAuditTrailWriter auditTrailWriter, IClock clock) : IJournalPostingService
{
    public async Task PostAsync(Guid adjustmentEntryId, CancellationToken cancellationToken)
    {
        var entry = await dbContext.AdjustmentEntries.Include(x => x.Lines).SingleAsync(x => x.Id == adjustmentEntryId, cancellationToken);
        if (!string.Equals(entry.Status, "Approved", StringComparison.Ordinal)) throw new InvalidOperationException("Only approved adjustments post.");
        if (decimal.Round(entry.Lines.Sum(x => x.Debit) - entry.Lines.Sum(x => x.Credit), 2) != 0m) throw new InvalidOperationException("Adjustment is not balanced.");

        foreach (var line in entry.Lines)
            dbContext.GeneralLedgerLines.Add(new GeneralLedgerLineEntity { Id = Guid.NewGuid(), JournalEntryId = entry.Id, AccountId = line.AccountId, FundCode = line.FundCode, DepartmentCode = line.DepartmentCode, ProgramCode = line.ProgramCode, ProjectCode = line.ProjectCode, Debit = line.Debit, Credit = line.Credit, FiscalYear = entry.FiscalYear, FiscalMonth = entry.FiscalMonth, PostedAtUtc = clock.UtcNow, SourceType = entry.EntryType, SourceReference = entry.EntryNumber });

        entry.Status = "Posted";
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditTrailWriter.WriteAsync(entry.Id, "adjustment.posted", "system:posting", "posting-service", entry.ReasonCode, "Adjustment posted to general ledger.", "posting-service", entry.CorrelationId, new { Status = "Approved" }, new { Status = "Posted", entry.FiscalYear, entry.FiscalMonth }, "success", cancellationToken);
    }
}
```

## End-to-End Verification Matrix

| Flow | Test | Pass |
|---|---|---|
| Correcting | Create, approve, and post a correction for an open-period source entry. | Source entry remains unchanged and corrected balances land in the proper period. |
| Reversing | Create accrual, approve, post, schedule reverse, and execute reverse. | Due period receives exactly one offsetting posted reverse entry. |
| Accrual | Create accrual with support event, approve, post, and inspect next-period reverse date. | Recognition period matches the business event and reverse date remains explicit. |
| Reclassification | Create reclass that moves expense between departments. | Net financial activity stays constant while classification changes. |
| Prior period adjustment | Create material prior-period adjustment with approval and disclosure package. | Adjustment remains pending until enhanced approvals complete and reporting evidence remains attached. |

## Common Failure Modes

| Failure Mode | Root Cause | Correction |
|---|---|---|
| Source history mutates | Handler updates source rows directly | Preserve source entry and write linked follow-up entry only |
| Duplicate auto-reversal | Scheduler lacks idempotency or status check | Filter on due date plus unposted approved status and enforce unique reverse link |
| Back-post after hard close | Period gate ignores close state | Route to current adjustment or restatement period only |
| Reclass changes net income | Builder produces one-sided movement | Build equal debit and credit movement pairs |
| Prior-period path lacks disclosure reference | Approval package validation is incomplete | Enforce disclosure reference before persistence |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Relative links | Open every relative link in this file. | All links resolve. |
| Front matter | Run `npm run frontmatter:validate`. | Validation returns zero errors for the reference file. |
| Correcting example | Review command, handler, and verification rows. | Source linkage, method handling, and period gating remain explicit. |
| Reversing example | Review schedule and processor examples. | `AutoReverseOn` logic and one-time execution remain explicit. |
| Accrual example | Review matching and handler examples. | Recognition-period matching and next-period reversal logic remain explicit. |
| Reclassification example | Review move model and dual-post builder. | Offset lines preserve equal and opposite movement. |
| Prior-period example | Review approval service and handler example. | Enhanced approval package and disclosure reference remain explicit. |
| Audit coverage | Review audit writer and audit matrix. | Who, what, when, why, where, and outcome fields remain explicit. |
