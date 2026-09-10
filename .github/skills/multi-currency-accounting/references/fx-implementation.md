---
title: FX Implementation Reference
doc_type: reference
status: active
last_updated: 2026-08-31
summary: Detailed rate-governance, translation, remeasurement, consolidation, and .NET implementation patterns for multi-currency accounting.
target_audience: ai
related_docs:
  - ../SKILL.md
  - ../../double-entry-accounting/SKILL.md
  - ../../journal-entry-patterns/SKILL.md
  - ../../financial-reporting-gaap/SKILL.md
tags:
  - multi-currency
  - foreign-exchange
  - fx
  - translation
  - remeasurement
  - gaap
---
# FX Implementation Reference

## Implementation Scope

This reference expands `multi-currency-accounting` with rate management, translation, remeasurement, consolidation, and .NET implementation detail. The patterns align to ASC 830 and IAS 21 style outcomes: foreign currency transactions use functional-currency measurement, remeasurement differences flow through income, and translation differences flow through cumulative translation adjustment in equity.

## Cross References

- [Multi-Currency Accounting](../SKILL.md)
- [Double-Entry Accounting](../../double-entry-accounting/SKILL.md)
- [Journal Entry Patterns](../../journal-entry-patterns/SKILL.md)
- [Financial Reporting GAAP](../../financial-reporting-gaap/SKILL.md)

## Rate Sourcing and Storage

| Area | Pattern | Test | Pass |
|---|---|---|---|
| Source ownership | Treasury, ERP, market-data provider, or approved manual desk owns each rate family. | Review rate-source catalog. | Each rate family has one owner and one backup owner. |
| Source traceability | Persist provider name, feed name, as-of date, published timestamp, and ingest batch id. | Read rate metadata. | Every active rate row contains the source trace fields. |
| Effective dating | Store `ValidFromUtc`, `ValidToUtc`, and `RateDate` separately. | Review overlapping rows for one currency pair and rate type. | No active overlap exists for the same key. |
| Direct and inverse handling | Store the published direction and compute inverses through one deterministic rule. | Recalculate inverse samples. | Inverse rates reconcile inside configured precision. |
| Supersession | Preserve prior rates when corrections arrive and mark the latest approved version active. | Compare version history for one corrected rate. | Historical rows remain queryable and only one active row remains. |
| Precision | Store rates at scale 6 or higher and currency amounts at their configured currency scale. | Inspect schema or EF configuration. | Rate and amount precision satisfy policy. |

### Daily Rates vs. Period-End Rates

| Use Case | Rate Class | Pass |
|---|---|---|
| Foreign currency transaction recognition | Spot rate or approved daily rate | Stored functional amount ties to the transaction date. |
| Open-item valuation at month end | Closing rate on the valuation date | Valuation journal uses the close calendar date. |
| Balance-sheet translation | Closing rate on the reporting date | Ending assets and liabilities reconcile to translated trial balance. |
| Income-statement translation | Period average or transaction-date rate by policy | Variance between sampled transaction translation and average translation stays inside tolerance. |
| Equity translation | Historical rate | Equity movement traces to original recognition or ownership event. |

### Historical Rate Tracking

| Historical Event | Stored Rate Anchor | Pass |
|---|---|---|
| Initial capital contribution | Contribution settlement date | Equity translation keeps the original rate. |
| Fixed asset acquisition | Asset recognition date | Non-monetary remeasurement uses the acquisition rate. |
| Prepaid expense recognition | Original payment or recognition date | Carrying amount remains tied to historical rate until recognition changes. |
| Inventory capitalization | Inventory receipt or capitalization date | Remeasurement keeps inventory at historical functional amount until sale or impairment. |
| Prior-period retained earnings rollforward | Prior issued reporting package | Opening equity stays stable apart from approved restatement events. |

## Translation Methodology: CTA to Equity

Translation applies when the subsidiary functional currency differs from the parent reporting currency and the local functional currency remains valid.

| Statement Area | Rate Basis | Journal or Report Result | Pass |
|---|---|---|---|
| Assets | Closing rate | Reporting-currency ending balance | All asset accounts use the same reporting-date close. |
| Liabilities | Closing rate | Reporting-currency ending balance | All liability accounts use the same reporting-date close. |
| Revenue | Average rate or transaction-date rate | Reporting-currency period activity | Policy rate series matches the reporting period. |
| Expenses | Average rate or transaction-date rate | Reporting-currency period activity | Shared cost accounts follow the same policy class. |
| Common stock and paid-in capital | Historical rate | Reporting-currency equity balance | Each equity layer traces to its origin date. |
| Beginning retained earnings | Prior-period translated ending balance | Reporting-currency opening balance | Opening balance matches prior issued package. |
| Current-period income | Derived from translated revenue and expense | Reporting-currency net income | Translated net income matches translated income statement. |
| CTA | Balancing difference | Equity reserve | CTA equals translated net assets minus translated equity excluding CTA. |

### Translation Rollforward

```text
Translated net assets
- Translated common stock and paid-in capital
- Opening translated retained earnings
- Translated current-period income
- Dividends at historical or declaration-date rate
= Ending CTA
```

### Translation Validation Pattern

| Check | Test | Pass |
|---|---|---|
| Current-rate balance sheet | Translate one local trial balance with the reporting close. | Assets and liabilities match the translated balance sheet. |
| Average-rate income statement | Translate one month of revenue and expense with the approved average series. | Income statement agrees to the published translation output. |
| CTA bridge | Recompute CTA from translated net assets and translated equity. | CTA equals the stored equity reserve without unexplained difference. |

## Remeasurement Methodology: Gain or Loss to Income

Remeasurement applies when the ledger recording currency differs from the functional currency. The objective is a functional-currency trial balance before translation.

| Account Class | Rate Basis | Result | Pass |
|---|---|---|---|
| Cash, receivables, payables, debt, accrued liabilities | Current rate | Functional-currency carrying amount at valuation date | Monetary balances revalue at the current rate. |
| Inventory carried at historical cost | Historical rate | Functional-currency carrying amount from capitalization date | Historical layers remain stable until derecognition or write-down event. |
| Property, plant, and equipment | Historical rate | Functional-currency carrying amount from acquisition date | Asset basis ties to acquisition support. |
| Prepaids and deferred charges at historical basis | Historical rate | Functional-currency carrying amount from original recognition | Remaining balances keep their historical basis. |
| Revenue related to non-monetary items | Rate linked to the underlying historical item when appropriate | Functional-currency activity | Cost of sales and depreciation follow the related historical basis. |
| Other revenue and expense | Transaction-date, weighted average, or current rate based on policy | Functional-currency activity | Rate class matches the economic event and policy. |
| Remeasurement gain or loss | Balancing difference | Income statement FX gain-loss account | Functional-currency trial balance balances exactly. |

### Remeasurement Logic Pattern

```text
For each ending balance:
1. Identify the functional currency.
2. Classify the balance as monetary or non-monetary.
3. Resolve the rate basis from policy and source event.
4. Convert carrying amount into functional currency.
5. Sum the converted debits and credits.
6. Post the net plug to remeasurement gain or loss.
```

### Realized vs. Unrealized Gain-Loss Pattern

| Scenario | Recognition Pattern | Destination | Pass |
|---|---|---|---|
| Foreign-currency invoice settles after rate movement | Realized gain or loss | Income statement | Settlement journal clears receivable or payable and records only the rate difference. |
| Open foreign-currency receivable or payable at period end | Unrealized gain or loss | Income statement | Valuation journal revalues the open item and links to the prior valuation. |
| Ledger in local currency, functional currency equals parent currency | Remeasurement gain or loss | Income statement | Net plug equals the variance produced by current and historical conversions. |
| Functional currency differs from reporting currency | Translation adjustment | CTA in equity | Income statement excludes CTA movement. |

## Reporting Currency Consolidation

| Layer | Inputs | Output | Pass |
|---|---|---|---|
| Entity source ledger | Transaction-currency and local-book balances | Local ledger trial balance | Local books agree to subledger detail. |
| Functional-currency layer | Remeasured or directly measured functional balances | Functional trial balance | Functional trial balance balances exactly. |
| Reporting-currency translation layer | Functional trial balance plus approved reporting rates | Translated trial balance | CTA and translated net income reconcile. |
| Consolidation layer | Parent balances, translated child balances, and elimination journals | Consolidated reporting package | Consolidated totals agree to entity packages plus eliminations. |

### Multi-Currency Trial Balance Pattern

| Column | Purpose |
|---|---|
| EntityCode | Keeps legal-entity ownership explicit. |
| AccountNumber | Preserves chart-of-accounts mapping. |
| TransactionCurrencyCode | Preserves source-currency trace when account detail exists at item level. |
| LocalBookAmount | Stores the amount in the ledger-entry currency. |
| FunctionalCurrencyCode | States the entity accounting basis. |
| FunctionalAmount | Stores the remeasured or directly measured balance. |
| ReportingCurrencyCode | States the parent reporting basis. |
| ReportingAmount | Stores the translated consolidation amount. |
| RateType | States spot, average, closing, or historical selection. |
| RateDate | States the effective date of the applied rate. |
| CalculationReference | Links the balance to valuation, translation, or consolidation batch. |

## .NET Entity Model

```csharp
namespace Accounting.MultiCurrency;

public enum ExchangeRateType
{
    Spot,
    Daily,
    Average,
    Closing,
    Historical
}

public enum FxBalanceNature
{
    Monetary,
    NonMonetary,
    Equity,
    Revenue,
    Expense
}

public sealed record Currency(
    string Code,
    string Name,
    int AmountScale,
    int RateScale,
    bool IsReportingCurrency,
    bool IsActive);

public sealed record ExchangeRateKey(
    string FromCurrency,
    string ToCurrency,
    ExchangeRateType RateType,
    DateOnly RateDate);

public sealed class ExchangeRate
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string FromCurrency { get; init; } = string.Empty;
    public string ToCurrency { get; init; } = string.Empty;
    public ExchangeRateType RateType { get; init; }
    public DateOnly RateDate { get; init; }
    public decimal Rate { get; init; }
    public string SourceSystem { get; init; } = string.Empty;
    public DateTimeOffset PublishedAtUtc { get; init; }
    public DateTimeOffset ValidFromUtc { get; init; }
    public DateTimeOffset? ValidToUtc { get; init; }
    public bool IsActive { get; init; }
}

public sealed record ForeignCurrencyTransaction(
    Guid Id,
    string EntityCode,
    string AccountNumber,
    string TransactionCurrencyCode,
    string FunctionalCurrencyCode,
    DateOnly TransactionDate,
    decimal SourceAmount,
    decimal FunctionalAmount,
    decimal AppliedRate,
    string OpenItemReference);

public sealed record TrialBalanceRow(
    string EntityCode,
    string AccountNumber,
    FxBalanceNature BalanceNature,
    string CurrencyCode,
    decimal Amount);
```

## Decimal Precision and Rounding Patterns

| Concern | Pattern | Pass |
|---|---|---|
| Rate precision | Store rates with at least 6 decimal places; store more when thinly traded currencies require it. | Recomputed conversions match the persisted amount after configured rounding. |
| Amount precision | Configure per-currency amount scale and keep internal calculations at higher precision before final rounding. | Aggregated journals balance after rounding adjustments. |
| Rounding boundary | Round at posting or report-output boundaries, not on every intermediate multiplication. | Recalculation does not drift from persisted totals. |
| Tolerance | Store tolerance per currency pair and report. | Validation explains every accepted variance. |

### EF Core Precision Example

```csharp
using Microsoft.EntityFrameworkCore;

namespace Accounting.MultiCurrency.Persistence;

public sealed class MultiCurrencyDbContext(DbContextOptions<MultiCurrencyDbContext> options) : DbContext(options)
{
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExchangeRate>(builder =>
        {
            builder.Property(x => x.FromCurrency).HasMaxLength(3);
            builder.Property(x => x.ToCurrency).HasMaxLength(3);
            builder.Property(x => x.Rate).HasPrecision(18, 8);
            builder.HasIndex(x => new { x.FromCurrency, x.ToCurrency, x.RateType, x.RateDate, x.IsActive });
        });
    }
}
```

## Exchange Rate Resolution Service

```csharp
namespace Accounting.MultiCurrency;

public interface IExchangeRateProvider
{
    ExchangeRate GetRate(ExchangeRateKey key);
}

public sealed class ExchangeRateResolver(IExchangeRateProvider provider)
{
    public decimal Convert(decimal amount, string fromCurrency, string toCurrency, ExchangeRateType rateType, DateOnly rateDate, int resultScale)
    {
        if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
            return Math.Round(amount, resultScale, MidpointRounding.ToEven);

        var rate = provider.GetRate(new ExchangeRateKey(fromCurrency, toCurrency, rateType, rateDate));
        return Math.Round(amount * rate.Rate, resultScale, MidpointRounding.ToEven);
    }
}
```

## Foreign Currency Transaction Processing

```csharp
namespace Accounting.MultiCurrency;

public sealed class ForeignCurrencyTransactionProcessor(ExchangeRateResolver resolver)
{
    public ForeignCurrencyTransaction Measure(
        Guid id,
        string entityCode,
        string accountNumber,
        string transactionCurrencyCode,
        string functionalCurrencyCode,
        DateOnly transactionDate,
        decimal sourceAmount,
        string openItemReference,
        int amountScale)
    {
        var functionalAmount = resolver.Convert(
            sourceAmount,
            transactionCurrencyCode,
            functionalCurrencyCode,
            ExchangeRateType.Spot,
            transactionDate,
            amountScale);

        return new ForeignCurrencyTransaction(
            id,
            entityCode,
            accountNumber,
            transactionCurrencyCode,
            functionalCurrencyCode,
            transactionDate,
            sourceAmount,
            functionalAmount,
            functionalAmount == 0m ? 0m : decimal.Round(functionalAmount / sourceAmount, 8, MidpointRounding.ToEven),
            openItemReference);
    }
}
```

## Translation Calculation Example

```csharp
namespace Accounting.MultiCurrency;

public sealed record TranslationRow(
    string AccountNumber,
    FxBalanceNature Nature,
    decimal FunctionalAmount,
    decimal ReportingAmount,
    ExchangeRateType AppliedRateType,
    DateOnly RateDate);

public sealed class TranslationCalculator(ExchangeRateResolver resolver)
{
    public IReadOnlyList<TranslationRow> Translate(
        IEnumerable<TrialBalanceRow> rows,
        string functionalCurrency,
        string reportingCurrency,
        DateOnly closingDate,
        DateOnly averageRateDate,
        IReadOnlyDictionary<string, DateOnly> historicalRateDates,
        int amountScale)
    {
        return rows.Select(row =>
        {
            var rateType = row.BalanceNature switch
            {
                FxBalanceNature.Monetary => ExchangeRateType.Closing,
                FxBalanceNature.NonMonetary => ExchangeRateType.Closing,
                FxBalanceNature.Equity => ExchangeRateType.Historical,
                FxBalanceNature.Revenue => ExchangeRateType.Average,
                FxBalanceNature.Expense => ExchangeRateType.Average,
                _ => ExchangeRateType.Closing
            };

            var rateDate = rateType switch
            {
                ExchangeRateType.Average => averageRateDate,
                ExchangeRateType.Historical => historicalRateDates[row.AccountNumber],
                _ => closingDate
            };

            var reportingAmount = resolver.Convert(row.Amount, functionalCurrency, reportingCurrency, rateType, rateDate, amountScale);
            return new TranslationRow(row.AccountNumber, row.BalanceNature, row.Amount, reportingAmount, rateType, rateDate);
        }).ToList();
    }
}
```

### CTA Calculation Example

```csharp
namespace Accounting.MultiCurrency;

public static class CtaCalculator
{
    public static decimal ComputeEndingCta(
        decimal translatedAssets,
        decimal translatedLiabilities,
        decimal translatedEquityExcludingCta)
    {
        var translatedNetAssets = translatedAssets - translatedLiabilities;
        return decimal.Round(translatedNetAssets - translatedEquityExcludingCta, 2, MidpointRounding.ToEven);
    }
}
```

## Remeasurement Calculation Example

```csharp
namespace Accounting.MultiCurrency;

public sealed record RemeasurementRow(
    string AccountNumber,
    FxBalanceNature Nature,
    decimal LocalAmount,
    decimal FunctionalAmount,
    ExchangeRateType AppliedRateType,
    DateOnly RateDate);

public sealed class RemeasurementCalculator(ExchangeRateResolver resolver)
{
    public IReadOnlyList<RemeasurementRow> Remeasure(
        IEnumerable<TrialBalanceRow> rows,
        string localCurrency,
        string functionalCurrency,
        DateOnly closingDate,
        IReadOnlyDictionary<string, DateOnly> historicalRateDates,
        int amountScale)
    {
        return rows.Select(row =>
        {
            var rateType = row.Nature switch
            {
                FxBalanceNature.Monetary => ExchangeRateType.Closing,
                FxBalanceNature.NonMonetary => ExchangeRateType.Historical,
                FxBalanceNature.Equity => ExchangeRateType.Historical,
                FxBalanceNature.Revenue => ExchangeRateType.Average,
                FxBalanceNature.Expense => ExchangeRateType.Average,
                _ => ExchangeRateType.Closing
            };

            var rateDate = rateType == ExchangeRateType.Historical
                ? historicalRateDates[row.AccountNumber]
                : closingDate;

            var functionalAmount = resolver.Convert(row.Amount, localCurrency, functionalCurrency, rateType, rateDate, amountScale);
            return new RemeasurementRow(row.AccountNumber, row.Nature, row.Amount, functionalAmount, rateType, rateDate);
        }).ToList();
    }

    public decimal ComputeGainLoss(IEnumerable<RemeasurementRow> debitRows, IEnumerable<RemeasurementRow> creditRows, int amountScale)
    {
        var debits = debitRows.Sum(x => x.FunctionalAmount);
        var credits = creditRows.Sum(x => x.FunctionalAmount);
        return decimal.Round(debits - credits, amountScale, MidpointRounding.ToEven) * -1m;
    }
}
```

## Consolidation Pattern

| Step | Action | Pass |
|---|---|---|
| 1 | Freeze functional trial balances for every child entity. | One approved balance set exists per entity and period. |
| 2 | Translate each child into the reporting currency and snapshot CTA. | Each child package contains translated balances and CTA support. |
| 3 | Prepare reporting-currency elimination entries for intercompany balances, revenue, cost, and investment elimination. | Elimination journals balance in the reporting currency. |
| 4 | Aggregate parent, child, and elimination balances into a consolidated trial balance. | Consolidated debits equal consolidated credits. |
| 5 | Publish statement-ready balances and CTA rollforward support. | Consolidated statements trace back to translated entity packages. |

## Validation Example

```csharp
namespace Accounting.MultiCurrency;

public static class MultiCurrencyValidation
{
    public static bool TrialBalanceBalances(IEnumerable<decimal> debits, IEnumerable<decimal> credits, decimal tolerance)
    {
        var debitTotal = debits.Sum();
        var creditTotal = credits.Sum();
        return Math.Abs(debitTotal - creditTotal) <= tolerance;
    }

    public static decimal ComputeRealizedGainLoss(decimal settlementFunctionalAmount, decimal originalFunctionalAmount, int scale)
        => decimal.Round(settlementFunctionalAmount - originalFunctionalAmount, scale, MidpointRounding.ToEven);
}
```

## Implementation Checklist

| Check | Test | Pass |
|---|---|---|
| Rate completeness | Query all required currency pairs and rate types for the close calendar. | No required rate is missing. |
| Historical anchors | Trace fixed assets, inventory layers, and equity balances to stored historical rates. | Every sampled balance has an immutable anchor rate. |
| Remeasurement | Reperform one branch remeasurement independently. | Net gain-loss plug matches the system result. |
| Translation | Reperform one subsidiary translation independently. | CTA and translated statements match the system result. |
| Consolidation | Compare translated balances plus eliminations to the consolidated package. | No unexplained reporting-currency variance remains. |
