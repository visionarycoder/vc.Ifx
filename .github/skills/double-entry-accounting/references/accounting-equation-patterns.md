---
title: Accounting Equation Patterns
doc_type: reference
status: active
last_updated: 2026-08-30
summary: Reference patterns for accounting equation enforcement, normal balances, closing entries, and .NET ledger validation.
target_audience: ai
related_docs:
  - ../../journal-entry-patterns/SKILL.md
  - ../../ledger-balance-anomaly/SKILL.md
tags:
  - double-entry
  - accounting
  - validation
  - journal-entry
---
# Accounting Equation Patterns

This reference expands `double-entry-accounting` with equation checks, closing rules, and .NET validation examples.

## Cross References

- [Journal Entry Patterns](../../journal-entry-patterns/SKILL.md)
- [Ledger Balance Anomaly Detection](../../ledger-balance-anomaly/SKILL.md)

## Accounting Equation Enforcement

| Rule | Test | Pass |
|---|---|---|
| Entry balance | Sum debit and credit lines by entry. | Net amount equals `0.00`. |
| Trial balance | Sum debit and credit columns across all accounts. | Grand totals match within tolerance. |
| Equation review | Recalculate ending balances by account type. | `assets == liabilities + equity`. |
| Closing review | Close revenue and expense into retained earnings. | Temporary accounts equal zero and equity changes by net income. |

```text
Assets      = sum(asset ending balances)
Liabilities = sum(liability ending balances)
Equity      = sum(equity ending balances)
Variance    = Assets - (Liabilities + Equity)
Pass        = abs(Variance) <= tolerance
```

## Normal Balances

| Account Type | Normal Balance | Debit Effect | Credit Effect |
|---|---|---|---|
| Asset | Debit | Increase | Decrease |
| Liability | Credit | Decrease | Increase |
| Equity | Credit | Decrease | Increase |
| Revenue | Credit | Decrease | Increase |
| Expense | Debit | Increase | Decrease |
| Contra Asset | Credit | Decrease net asset value | Increase net reduction |

| Normal Balance | Debit Line | Credit Line |
|---|---|---|
| Debit | `+Amount` | `-Amount` |
| Credit | `-Amount` | `+Amount` |

## T-Account Examples

```text
Cash (Asset)
----------------------
Debit       | Credit
1,000 open  | 300 rent
----------------------
700 Dr end
```

```text
Rent Expense (Expense)
----------------------
Debit       | Credit
300 rent    |
----------------------
300 Dr end
```

```text
Accounts Receivable (Asset)
----------------------
Debit       | Credit
800 invoice |
----------------------
800 Dr end
```

```text
Service Revenue (Revenue)
----------------------
Debit       | Credit
            | 800 invoice
----------------------
800 Cr end
```

## Trial Balance Validation Patterns

| Pattern | Test | Pass |
|---|---|---|
| Carry-forward row | Include accounts with opening balance and zero activity. | Ending balance stays visible. |
| Account rollup | Sum debit and credit activity by account. | Ending balance matches opening plus net activity. |
| Type subtotal | Sum balances by account type. | Subtotals reconcile to statement intent. |
| Equation validation | Recalculate assets, liabilities, and equity from the trial balance. | Equation remains true for the period. |

| Account | Debit | Credit | Ending |
|---|---:|---:|---:|
| 1010 Cash | 1,000.00 | 300.00 | 700.00 Dr |
| 1200 Accounts Receivable | 800.00 | 0.00 | 800.00 Dr |
| 4000 Service Revenue | 0.00 | 800.00 | 800.00 Cr |
| 5100 Rent Expense | 300.00 | 0.00 | 300.00 Dr |

## Closing Entry Patterns

| Step | Entry Pattern | Pass |
|---|---|---|
| Close revenue | Debit revenue and credit retained earnings or income summary. | Revenue balances equal zero. |
| Close expense | Credit expense and debit retained earnings or income summary. | Expense balances equal zero. |
| Close net income | Move net income or loss into retained earnings. | Equity reflects period result. |
| Prevent duplicates | Search by period and close batch key. | One close batch exists per period. |

```text
Dr Service Revenue      800
   Cr Income Summary        800

Dr Income Summary      300
   Cr Rent Expense         300

Dr Income Summary      500
   Cr Retained Earnings    500
```

## .NET Validation Code Example

```csharp
namespace Accounting.Validation;

public enum AccountType { Asset, Liability, Equity, Revenue, Expense, ContraAsset }
public enum BalanceDirection { Debit, Credit }

public sealed record Account(Guid Id, string Number, AccountType Type, BalanceDirection NormalBalance, bool IsActive);
public sealed record JournalLine(Guid AccountId, BalanceDirection Direction, decimal Amount, string CurrencyCode);
public sealed record JournalEntry(Guid Id, IReadOnlyList<JournalLine> Lines);
public sealed record TrialBalanceRow(string AccountNumber, AccountType AccountType, decimal Debits, decimal Credits, decimal EndingBalance);
public sealed record ValidationError(string Code, string Message);

public sealed class JournalEntryValidator
{
    private static readonly IReadOnlyDictionary<AccountType, BalanceDirection> NormalBalances = new Dictionary<AccountType, BalanceDirection>
    {
        [AccountType.Asset] = BalanceDirection.Debit,
        [AccountType.Liability] = BalanceDirection.Credit,
        [AccountType.Equity] = BalanceDirection.Credit,
        [AccountType.Revenue] = BalanceDirection.Credit,
        [AccountType.Expense] = BalanceDirection.Debit,
        [AccountType.ContraAsset] = BalanceDirection.Credit,
    };

    public IReadOnlyList<ValidationError> Validate(JournalEntry entry, IReadOnlyDictionary<Guid, Account> accounts, decimal tolerance = 0.01m)
    {
        var errors = new List<ValidationError>();
        ValidateBalance(entry, errors, tolerance);
        ValidateAccountTypes(entry, accounts, errors);
        ValidateCurrency(entry, errors);
        return errors;
    }

    public static void ValidateBalance(JournalEntry entry, ICollection<ValidationError> errors, decimal tolerance)
    {
        var debits = entry.Lines.Where(x => x.Direction == BalanceDirection.Debit).Sum(x => x.Amount);
        var credits = entry.Lines.Where(x => x.Direction == BalanceDirection.Credit).Sum(x => x.Amount);
        if (entry.Lines.Count == 0 || Math.Abs(debits - credits) > tolerance)
            errors.Add(new ValidationError("entry.unbalanced", $"Debits {debits:F2} and credits {credits:F2} do not match."));
    }

    public static void ValidateAccountTypes(JournalEntry entry, IReadOnlyDictionary<Guid, Account> accounts, ICollection<ValidationError> errors)
    {
        foreach (var line in entry.Lines)
        {
            if (!accounts.TryGetValue(line.AccountId, out var account)) { errors.Add(new ValidationError("account.missing", $"Account {line.AccountId} is missing.")); continue; }
            if (!account.IsActive) errors.Add(new ValidationError("account.inactive", $"Account {account.Number} is inactive."));
            if (account.NormalBalance != NormalBalances[account.Type]) errors.Add(new ValidationError("account.normal-balance.invalid", $"Account {account.Number} has an invalid normal balance."));
        }
    }

    public static IReadOnlyList<TrialBalanceRow> ComputeTrialBalance(IEnumerable<JournalEntry> entries, IReadOnlyDictionary<Guid, Account> accounts) =>
        entries.SelectMany(entry => entry.Lines)
            .GroupBy(line => line.AccountId)
            .Select(group =>
            {
                var account = accounts[group.Key];
                var debits = group.Where(x => x.Direction == BalanceDirection.Debit).Sum(x => x.Amount);
                var credits = group.Where(x => x.Direction == BalanceDirection.Credit).Sum(x => x.Amount);
                var ending = account.NormalBalance == BalanceDirection.Debit ? debits - credits : credits - debits;
                return new TrialBalanceRow(account.Number, account.Type, debits, credits, ending);
            })
            .OrderBy(row => row.AccountNumber)
            .ToList();

    private static void ValidateCurrency(JournalEntry entry, ICollection<ValidationError> errors)
    {
        if (entry.Lines.Select(line => line.CurrencyCode).Distinct(StringComparer.OrdinalIgnoreCase).Count() != 1)
            errors.Add(new ValidationError("entry.currency.mixed", "Entry contains more than one currency."));
    }
}
```

## Validation Patterns

| Concern | Test | Pass |
|---|---|---|
| Balance validation | Run `ValidateBalance` on balanced and unbalanced entries. | Balanced entries pass and unbalanced entries return `entry.unbalanced`. |
| Account type validation | Run `ValidateAccountTypes` with inactive, missing, and broken-normal-balance accounts. | Validator returns the expected error code for each defect. |
| Trial balance computation | Run `ComputeTrialBalance` on posted entries. | Account totals and grand totals match ledger activity. |
| Equation rerun | Recompute equation after posting and after correction entries. | Equation stays true after each durable posting set. |

## EF Core Integration

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Persistence;

public sealed class JournalLineConfiguration : IEntityTypeConfiguration<JournalLineEntity>
{
    public void Configure(EntityTypeBuilder<JournalLineEntity> builder)
    {
        builder.Property(line => line.Amount).HasPrecision(18, 2);
        builder.Property(line => line.Direction).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(line => line.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.HasIndex(line => line.SourceReference).IsUnique();
        builder.HasOne(line => line.Account).WithMany().HasForeignKey(line => line.AccountId).OnDelete(DeleteBehavior.Restrict);
    }
}
```

| Hook | Test | Pass |
|---|---|---|
| Decimal precision | Save `123.45` and read it back. | Stored amount keeps scale `2`. |
| Unique source reference | Insert duplicate source reference. | Database rejects the duplicate row. |
| Restricted delete | Delete an account with posted lines. | Delete action fails or stays blocked by policy. |

## Common Validation Patterns

| Pattern | Pass |
|---|---|
| Zero-line entry rejection | Validator blocks the entry before persistence. |
| Mixed-currency rejection | Validator returns `entry.currency.mixed`. |
| Closed-period rejection | Posting service blocks the entry before ledger impact. |
| Duplicate close rejection | Close process detects an existing period close batch. |
| Temporary-account reset review | Revenue and expense balances equal zero after close. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Front matter | Run `node .scripts/validate-frontmatter.js --check`. | Command exits with code `0` for this file format. |
| STE wording | Run the repository modal-verb scan on this file. | Scan returns zero banned-term matches. |
| Link integrity | Open both relative skill links. | Both links resolve. |
| Code example coverage | Read the validator example. | Example includes balance, account-type, trial-balance, and EF Core patterns. |
