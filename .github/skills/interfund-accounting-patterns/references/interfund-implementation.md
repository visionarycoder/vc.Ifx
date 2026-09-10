---
title: Interfund Accounting Implementation Reference
doc_type: reference
status: active
last_updated: 2026-08-31
target_audience: ai
related_docs:
  - ../SKILL.md
related_skills:
  - fund-accounting-patterns
  - public-sector-accounting
  - double-entry-accounting
  - reconciliation-patterns
tags:
  - interfund
  - fund-accounting
  - gasb
  - governmental
  - transfers
  - eliminations
source_paths:
  - .github/skills/interfund-accounting-patterns/SKILL.md
---
# Interfund Accounting Implementation Reference

## Purpose

Agent uses this reference to implement interfund posting, settlement, reconciliation, and government-wide elimination logic. GASB Statement 34 governs internal balance and transfer presentation in government-wide statements. GASB Statement 54 reinforces fund identity and clear governmental fund reporting.

## Design Rules

| Rule | Implementation Result |
|---|---|
| One business event creates all fund legs. | Posting services persist one envelope and all related legs in one database transaction. |
| Classification occurs before account mapping. | Due to or due from, transfer, service, allocation, and reimbursement use separate rule paths. |
| Fund and counterparty are first-class dimensions. | Each leg stores `FundId`, `CounterpartyFundId`, and `InterfundTransactionId`. |
| Reporting adjustments stay outside operational ledgers. | Elimination entries live in a reporting layer or derived journal set. |
| Reconciliation uses posting keys. | Tie-out logic groups by fund pair, transaction id, and period. |

## Interfund Entity Model

```csharp
public enum InterfundTransactionType { DueToFrom, Transfer, ServicesProvidedUsed, SharedServiceAllocation, Reimbursement, Elimination }
public enum InterfundStatus { Draft, Approved, Posted, Settled, Cancelled }

public sealed class InterfundTransaction
{
    public Guid Id { get; private set; }
    public string TransactionNumber { get; private set; } = string.Empty;
    public InterfundTransactionType TransactionType { get; private set; }
    public InterfundStatus Status { get; private set; }
    public string SourceFundId { get; private set; } = string.Empty;
    public string DestinationFundId { get; private set; } = string.Empty;
    public string ReportingEntityId { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateOnly EffectiveDate { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public string ApprovalId { get; private set; } = string.Empty;
    public string? AllocationDriverCode { get; private set; }
    public List<InterfundLeg> Legs { get; } = [];
}

public sealed class InterfundLeg
{
    public Guid Id { get; private set; }
    public Guid InterfundTransactionId { get; private set; }
    public string FundId { get; private set; } = string.Empty;
    public string CounterpartyFundId { get; private set; } = string.Empty;
    public string AccountCode { get; private set; } = string.Empty;
    public decimal DebitAmount { get; private set; }
    public decimal CreditAmount { get; private set; }
    public string LegRole { get; private set; } = string.Empty;
}
```

## EF Core Pattern for Cross-Fund Posting

```csharp
public sealed class InterfundTransactionConfiguration : IEntityTypeConfiguration<InterfundTransaction>
{
    public void Configure(EntityTypeBuilder<InterfundTransaction> builder)
    {
        builder.ToTable("InterfundTransactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TransactionNumber).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.HasIndex(x => x.TransactionNumber).IsUnique();
        builder.HasIndex(x => new { x.ReportingEntityId, x.EffectiveDate, x.TransactionType });
        builder.HasMany(x => x.Legs).WithOne().HasForeignKey(x => x.InterfundTransactionId);
    }
}
```

Implementation notes:

| Topic | Guidance |
|---|---|
| Unit of work | Persist envelope, legs, journal batch id, and audit event in one transaction. |
| Idempotency | Use `TransactionNumber` plus source reference as the duplicate guard. |
| Precision | Apply one decimal policy to posting, allocation, reconciliation, and elimination. |
| Security | Restrict cross-fund posting to approved workflows and audited service identities. |

## Paired Entry Generation

Agent generates both sides from one classified envelope.

```csharp
public sealed record PostingLine(string FundId, string CounterpartyFundId, string AccountCode, decimal Debit, decimal Credit);

public static class InterfundPostingFactory
{
    public static IReadOnlyList<PostingLine> CreateDueToFrom(string lender, string borrower, decimal amount) =>
    [
        new(lender, borrower, "DueFromOtherFunds", amount, 0m),
        new(lender, borrower, "Cash", 0m, amount),
        new(borrower, lender, "Cash", amount, 0m),
        new(borrower, lender, "DueToOtherFunds", 0m, amount)
    ];

    public static IReadOnlyList<PostingLine> CreateTransfer(string source, string destination, decimal amount) =>
    [
        new(source, destination, "TransferOut", amount, 0m),
        new(source, destination, "Cash", 0m, amount),
        new(destination, source, "Cash", amount, 0m),
        new(destination, source, "TransferIn", 0m, amount)
    ];

    public static IReadOnlyList<PostingLine> CreateServiceBilling(string provider, string consumer, decimal amount) =>
    [
        new(provider, consumer, "DueFromOtherFunds", amount, 0m),
        new(provider, consumer, "ChargesForServicesRevenue", 0m, amount),
        new(consumer, provider, "SharedServiceExpense", amount, 0m),
        new(consumer, provider, "DueToOtherFunds", 0m, amount)
    ];

    public static IReadOnlyList<PostingLine> CreateSharedAllocation(string serviceFund, string participantFund, decimal amount) =>
        CreateServiceBilling(serviceFund, participantFund, amount);

    public static IReadOnlyList<PostingLine> CreateReimbursement(string originalPayer, string benefitingFund, decimal amount) =>
    [
        new(originalPayer, benefitingFund, "Cash", amount, 0m),
        new(originalPayer, benefitingFund, "OriginalExpense", 0m, amount),
        new(benefitingFund, originalPayer, "OriginalExpense", amount, 0m),
        new(benefitingFund, originalPayer, "Cash", 0m, amount)
    ];
}
```

## Due To and Due From Reconciliation

```csharp
public sealed record InterfundBalanceRow(string FundId, string CounterpartyFundId, decimal DueFrom, decimal DueTo);

public static class InterfundReconciliation
{
    public static bool IsBalanced(IReadOnlyCollection<InterfundBalanceRow> rows)
        => rows.Sum(x => x.DueFrom) == rows.Sum(x => x.DueTo);
}
```

```sql
SELECT
    CounterpartyFundId,
    SUM(CASE WHEN AccountCode = 'DueFromOtherFunds' THEN DebitAmount - CreditAmount ELSE 0 END) AS DueFromNet,
    SUM(CASE WHEN AccountCode = 'DueToOtherFunds' THEN CreditAmount - DebitAmount ELSE 0 END) AS DueToNet
FROM InterfundLegs
WHERE ReportingPeriod = @ReportingPeriod
GROUP BY CounterpartyFundId
HAVING SUM(CASE WHEN AccountCode = 'DueFromOtherFunds' THEN DebitAmount - CreditAmount ELSE 0 END)
    <> SUM(CASE WHEN AccountCode = 'DueToOtherFunds' THEN CreditAmount - DebitAmount ELSE 0 END);
```

Validation rules:

| Rule | Pass |
|---|---|
| Global due from equals global due to. | Net open interfund balance equals zero. |
| Fund-pair due from equals fund-pair due to. | Reciprocal schedules agree. |
| Closed loans have zero open balance. | Settlement cleared the reciprocal accounts. |

## Transfer Approval Workflow

| Stage | Stored Data | Pass |
|---|---|---|
| Request | Source fund, destination fund, amount, purpose, fiscal period | Request is complete. |
| Review | Legal authority, budget check, reporting impact | Review records approve or reject result. |
| Approval | Approval id, approver, approval date | Posted transfer references one approval id. |
| Posting | Envelope id, journal batch id, generated legs | Transfer out equals transfer in exactly. |
| Reporting | Statement mapping and elimination flag | Fund and government-wide views classify the transfer correctly. |

## Service Billing and Allocation

```csharp
public static class ServiceBillingCalculator
{
    public static decimal CalculateCharge(decimal units, decimal rate) => decimal.Round(units * rate, 2);
}

public sealed record AllocationParticipant(string FundId, decimal DriverQuantity);

public static class SharedServiceAllocator
{
    public static IReadOnlyDictionary<string, decimal> Allocate(decimal poolAmount, IReadOnlyCollection<AllocationParticipant> participants)
    {
        decimal totalDriver = participants.Sum(x => x.DriverQuantity);
        Dictionary<string, decimal> result = [];

        foreach (AllocationParticipant participant in participants.OrderBy(x => x.FundId))
        {
            result[participant.FundId] = decimal.Round(poolAmount * (participant.DriverQuantity / totalDriver), 2);
        }

        decimal remainder = poolAmount - result.Values.Sum();
        if (remainder != 0m) result[result.Keys.OrderBy(x => x).First()] += remainder;
        return result;
    }
}
```

| Pattern | Pass |
|---|---|
| Services provided and used | Charge equals units times rate and both funds post reciprocal legs. |
| Shared allocation | Allocated total equals the source pool after approved rounding. |
| Reimbursement | False revenue stays absent and the cost lands in the benefiting fund. |

## Elimination Entry Generation

```csharp
public sealed record EliminationEntry(string EntryType, string AccountCode, decimal Debit, decimal Credit, string SourceTransactionNumber);

public static class EliminationFactory
{
    public static IReadOnlyList<EliminationEntry> CreateTransferElimination(decimal amount, string sourceNumber) =>
    [
        new("TransferElimination", "TransferIn", amount, 0m, sourceNumber),
        new("TransferElimination", "TransferOut", 0m, amount, sourceNumber)
    ];

    public static IReadOnlyList<EliminationEntry> CreateBalanceElimination(decimal amount, string sourceNumber) =>
    [
        new("BalanceElimination", "DueToOtherFunds", amount, 0m, sourceNumber),
        new("BalanceElimination", "DueFromOtherFunds", 0m, amount, sourceNumber)
    ];
}
```

| Scenario | Action | Pass |
|---|---|---|
| Governmental to governmental | Eliminate same-activity balances and transfers. | Internal duplication disappears from governmental activities. |
| Business-type to business-type | Eliminate same-activity balances and transfers. | Internal duplication disappears from business-type activities. |
| Governmental to business-type | Present residual balance as `InternalBalances` and keep transfer presentation explicit. | Cross-activity relationship remains visible. |
| External or component unit outside scope | Skip elimination. | External balances remain. |

## Consolidated Financial Statement Preparation

1. Agent freezes fund-ledger balances for the reporting period.
2. Agent extracts interfund balances and activity by fund pair.
3. Agent classifies each item as eliminable, cross-activity residual, or external.
4. Agent generates elimination entries in a reporting layer.
5. Agent combines operational balances and reporting entries into government-wide statements.
6. Agent verifies net-zero eliminations and absence of same-entity duplication.

## Interfund Loan Tracking and Repayment

```csharp
public sealed record InterfundLoanStatus(
    string TransactionNumber,
    string LenderFundId,
    string BorrowerFundId,
    decimal OriginalAmount,
    decimal OpenAmount,
    DateOnly DueDate);
```

| Rule | Pass |
|---|---|
| Every open loan has a due date or current-cycle settlement target. | Aging is measurable. |
| Settlement entries reference the original transaction number. | Repayment reduces the matching open balance. |
| Aged loans route to review. | Old balances keep owner attention. |

## GASB Statement 34 and 54 Compliance

| Area | Check | Pass |
|---|---|---|
| GASB 34 | Review government-wide elimination logic. | Same-activity internal balances and transfers are eliminated. |
| GASB 34 | Review cross-activity presentation. | Residual balances appear in `InternalBalances` and transfers stay out of operating revenue. |
| GASB 54 | Review fund identity and governmental fund presentation. | Each interfund posting preserves clear fund context. |
| Reconciliation | Recalculate due to versus due from totals. | Net open reciprocal balance equals zero. |

## Test Matrix

| Scenario | Pass |
|---|---|
| Due to or due from | Lending fund records due from and borrowing fund records due to for the same amount. |
| Transfer | Transfer out equals transfer in and no repayable balance remains. |
| Service billing | Revenue and expense or expenditure entries tie to the billed amount. |
| Shared allocation | Distributed amount equals the pooled amount. |
| Reimbursement | False revenue stays absent and the correct fund carries the cost. |
| Government-wide elimination | Same-activity internal amounts disappear from consolidated totals. |
