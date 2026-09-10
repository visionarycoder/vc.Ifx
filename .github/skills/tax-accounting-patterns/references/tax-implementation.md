---
title: Tax Implementation Reference
doc_type: reference
status: active
last_updated: 2026-08-31
summary: Detailed sales-tax, use-tax, reporting, .NET implementation, Washington State B&O, and 1099-integration patterns for indirect-tax accounting.
target_audience: ai
related_docs:
  - ../SKILL.md
  - ../../journal-entry-patterns/SKILL.md
  - ../../reconciliation-patterns/SKILL.md
  - ../../wa-state-saam/SKILL.md
tags:
  - tax
  - sales-tax
  - use-tax
  - jurisdiction
  - 1099
  - washington-state
---
# Tax Implementation Reference

## Implementation Scope

This reference expands `tax-accounting-patterns` with implementation detail for jurisdiction lookup, rate maintenance, taxability rules, exemptions, use-tax accrual, filing, remittance, reconciliation, .NET entities, and Washington State tax integration.

## Cross References

- [Tax Accounting Patterns](../SKILL.md)
- [Journal Entry Patterns](../../journal-entry-patterns/SKILL.md)
- [Reconciliation Patterns](../../reconciliation-patterns/SKILL.md)
- [Washington State SAAM](../../wa-state-saam/SKILL.md)

## Core Tax Invariants

| Invariant | Test | Pass |
|---|---|---|
| Collected customer sales tax never inflates revenue. | Post a taxable invoice and inspect revenue and liability rows. | Revenue excludes tax and liability equals collected tax. |
| Self-assessed use tax never duplicates vendor-collected tax. | Process one taxed invoice and one untaxed invoice for the same category. | Only the untaxed or under-taxed invoice creates use-tax payable. |
| Jurisdiction resolution remains effective-dated. | Recalculate one historical transaction after a rate change. | Historical transaction keeps the original valid rate set. |
| Exemption logic stays evidence-based. | Submit valid, expired, and missing certificate cases. | Only the valid certificate suppresses tax. |
| Filing buckets tie to the transaction subledger. | Aggregate one filing period by jurisdiction and compare to the return workpaper. | Period totals equal governed transaction populations. |
| Liability rollforward ties to cash remittance. | Compare opening liability, current activity, adjustments, and payment activity. | Opening plus activity minus payment equals ending liability. |

## Jurisdiction Model and Hierarchy

| Layer | Purpose | Core Fields | Control |
|---|---|---|---|
| State | Registration, statewide rate, filing cadence, and return ownership | `StateCode`, `RegistrationId`, `FilingFrequency`, `ActiveFrom`, `ActiveTo` | Every taxable line resolves one active state row. |
| County | Local county add-on rate and return bucket | `CountyCode`, `CountyName`, `BoundaryKey`, `ActiveFrom`, `ActiveTo` | County row applies only when the address falls inside the boundary. |
| City | City or municipality layer | `CityCode`, `CityName`, `BoundaryKey`, `SourcingRule`, `ActiveFrom`, `ActiveTo` | City row aligns to municipal boundary and sourcing rule. |
| District | Special-purpose district layer | `DistrictCode`, `DistrictType`, `BoundaryKey`, `Priority`, `ActiveFrom`, `ActiveTo` | District rows respect boundary overlap priority. |
| Composite filing bucket | Summarized return bucket for one registration and period | `TaxAuthorityCode`, `ReturnLineCode`, `LiabilityAccount`, `Frequency` | Every computed component maps to one filing bucket. |

## Sales Tax Calculation

### Jurisdiction Lookup by Address

| Step | Input | Action | Pass |
|---|---|---|---|
| 1 | Ship-to, bill-to, ship-from, order date, and service-delivery facts | Normalize address, geocode or boundary-match, and store the resolved location fingerprint. | One normalized address fingerprint exists for each taxable line. |
| 2 | Tax law sourcing rule | Select destination, origin, place-of-use, or service-benefit sourcing. | One sourcing basis applies to the line. |
| 3 | Boundary data | Resolve state, county, city, and district rows in priority order. | One ordered jurisdiction stack exists for the line. |
| 4 | Effective date | Filter to active rate rows for the transaction date. | Every layer uses a rate active on the transaction date. |
| 5 | Filing profile | Map each component to a filing bucket and liability account. | Every component contains filing and GL metadata. |

Practical lookup flow:

1. Address normalization service standardizes street, city, postal code, and country.
2. Boundary lookup service resolves local layers from governed GIS data, postal tables, or approved tax content.
3. Tax engine stores the resolved jurisdiction fingerprint beside the transaction line so reruns stay deterministic.
4. Rate selection filters by `EffectiveFrom` and `EffectiveTo`.
5. Composite rate equals the sum of the active jurisdiction layers after exclusions and exemptions.

### Rate Table Management

| Topic | Pattern | Pass |
|---|---|---|
| Effective dating | Store `EffectiveFrom` and optional `EffectiveTo` on every rate row. | Historical transactions recalculate to the same rate set. |
| Layer separation | Store one row per state, county, city, or district rate component. | Composite rate remains explainable. |
| Change ingestion | Load new rates into future-dated rows and end-date superseded rows. | No in-place overwrite erases prior history. |
| Governance | Version rate source, ingestion batch, and approval evidence. | Audit trail identifies who loaded the rate and when. |
| Filing mapping | Attach liability account and return-line metadata to each effective row or linked tax code. | Calculated tax flows directly into returns and GL. |

### Taxable Versus Non-Taxable Items

| Item Pattern | Taxability Rule | Example Result |
|---|---|---|
| Tangible goods | Default taxable unless explicit exemption or exclusion exists. | Standard merchandise line produces sales tax. |
| Resale goods | Exempt when valid resale certificate exists and jurisdiction accepts the certificate. | No tax on the sale line and certificate id persists. |
| Freight and shipping | Taxability follows jurisdiction rule and invoice structure. | Shipping line stays taxable or exempt based on local rule. |
| Services | Taxability varies by service type and jurisdiction. | Installation, SaaS, repair, or professional service lines evaluate separate codes. |
| Digital products | Taxability follows jurisdiction digital-goods rule. | Downloaded software and subscriptions map to dedicated tax codes. |
| Government or nonprofit sales | Exemption applies only with supported legal basis and evidence. | Exempt line carries entity and certificate evidence. |

### Tax Exemption Handling

| Exemption Type | Required Evidence | Validation Rule | Pass |
|---|---|---|---|
| Resale | Resale certificate number, holder, and jurisdiction coverage | Certificate exists, remains active, and covers the jurisdiction. | Tax suppresses only while the certificate remains valid. |
| Government | Government customer identity and exemption authority | Customer entity and legal basis match the governed rule. | Tax suppresses and evidence remains queryable. |
| Product exclusion | Product or service tax code with governed exclusion | Tax code maps to non-taxable treatment for the jurisdiction. | Exclusion logic follows the maintained code table. |
| Temporary holiday or incentive | Effective-dated event rule | Transaction date falls inside the active exemption window. | Holiday treatment starts and ends on the governed dates. |

Exemption control pattern:

- Transaction lines store `ExemptionReasonCode`, `CertificateId`, and `CertificateEffectiveDate`.
- Certificate repository stores holder, jurisdiction coverage, and expiration.
- Tax calculation runs the certificate check before rate application.
- Manual override path records actor, reason, and evidence package.

## Use Tax Accrual

### Purchase Monitoring

| Signal | Source | Action | Pass |
|---|---|---|---|
| Taxable purchase category | AP invoice line, PO line, receipt line, or expense report | Route line to vendor-tax verification. | Every taxable purchase line enters the review population. |
| Missing tax amount | Vendor invoice omits tax or shows zero tax | Compare expected tax to actual tax. | Review identifies tax under-collection. |
| Partial tax amount | Vendor collected tax for some jurisdictions only | Calculate deficiency against the governed rate stack. | Deficiency amount equals expected tax minus vendor-collected tax. |
| Capital purchase | Fixed-asset or inventory acquisition | Capitalize self-assessed tax into the asset or inventory basis. | Basis and payable tie to the same source document. |

### Vendor Tax Collection Verification

| Verification Step | Test | Pass |
|---|---|---|
| Match vendor invoice jurisdiction to purchase destination or use location. | Compare vendor tax detail to resolved purchase jurisdiction. | Jurisdiction stack matches or variance stays explicit. |
| Compare vendor tax rate to governed effective-dated rate. | Recalculate expected rate by transaction date. | Rate variance stays zero or deficiency amount persists. |
| Compare taxable base to governed taxability logic. | Review item tax code and exempt evidence. | Tax base matches product and exemption rules. |
| Suppress duplicate accrual when vendor already collected full tax. | Process fully taxed purchase. | No use-tax payable entry posts. |

### Self-Assessment and Accrual

| Scenario | Entry Pattern | Pass |
|---|---|---|
| Expense purchase with missing tax | Debit tax expense or source expense; credit use-tax payable. | Expense and liability equal the deficiency amount. |
| Capital asset purchase with missing tax | Debit asset basis; credit use-tax payable. | Asset basis and liability equal the deficiency amount. |
| Inventory purchase with missing tax | Debit inventory; credit use-tax payable. | Inventory basis carries the tax until sale or issue. |
| Prior-period discovery | Post current-period accrual with source-period reference and governed approval evidence. | Historical source stays traceable and liability enters the current controlled period. |

## Tax Reporting

### Monthly and Quarterly Filing Requirements

| Frequency | Typical Driver | Control |
|---|---|---|
| Monthly | High-volume or high-liability jurisdictions | Close calendar includes monthly cut-off, return preparation, review, and remittance. |
| Quarterly | Medium-volume jurisdictions | Quarter-end calendar aggregates three months and preserves prior-period adjustments. |
| Annual | Low-volume or registration-maintenance filings | Annual workflow still preserves monthly subledger detail for audit support. |
| Event-driven | Registration closeout, amendment, or special return | Event record stores legal trigger, deadline, and approval. |

### Jurisdiction-Specific Returns

| Return Concern | Pattern | Pass |
|---|---|---|
| State return | Aggregate state and supported local components to the state filing profile. | State return totals tie to state filing buckets. |
| Home-rule or separate local return | File local taxes outside the state return when the jurisdiction requires direct filing. | Local liabilities stay separable from state-collected locals. |
| Amended return | Preserve original filed values, delta, reason, and approval. | Amendment history remains queryable. |
| Credit carryforward | Track credit origin, period, jurisdiction, and application order. | Credits reduce future payment without erasing source history. |

### Payment Processing

| Step | Action | Pass |
|---|---|---|
| Payment staging | Move approved return liability into tax clearing. | Clearing amount equals approved remittance amount. |
| Disbursement | Pay authority by ACH, EFT, check, or portal batch. | Cash activity ties to approved remittance batch. |
| Acknowledgment | Store confirmation number, payment date, amount, and filer. | Payment evidence remains attached to the filing period. |
| Exception handling | Record reject, reversal, returned payment, or adjustment separately. | Failed payments never disappear from the audit trail. |

### Reconciliation to GL

| Reconciliation Layer | Compare | Pass |
|---|---|---|
| Tax subledger to GL | Jurisdiction liability buckets to liability accounts | Balance difference equals zero after approved timing items. |
| Filed return to subledger | Return line totals to transaction populations | Filing workbook ties to the governed source data. |
| Payment register to GL cash | Approved remittance batches to cash disbursements | Payment amount, date, and authority reference match. |
| Carryforward credits to GL | Credits and offsets to liability rollforward | Credit balances remain explicit and non-negative by rule set. |

Sample rollforward:

```text
Opening liability
+ Current-period sales tax collected
+ Current-period use tax accrued
+ Prior-period adjustments
- Current-period remittances
- Approved credit applications
= Ending liability
```

## .NET Implementation

### TaxJurisdiction Entity Model

```csharp
namespace Accounting.Tax;

public enum JurisdictionLevel
{
    State,
    County,
    City,
    District
}

public sealed class TaxJurisdiction
{
    public Guid Id { get; init; }
    public string AuthorityCode { get; init; } = string.Empty;
    public JurisdictionLevel Level { get; init; }
    public string StateCode { get; init; } = string.Empty;
    public string JurisdictionCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string BoundaryKey { get; init; } = string.Empty;
    public int Priority { get; init; }
    public DateOnly ActiveFrom { get; init; }
    public DateOnly? ActiveTo { get; init; }
    public string FilingFrequency { get; init; } = string.Empty;
    public string LiabilityAccountCode { get; init; } = string.Empty;

    public bool IsActiveOn(DateOnly transactionDate) =>
        transactionDate >= ActiveFrom && (ActiveTo is null || transactionDate <= ActiveTo.Value);
}
```

### TaxRate with Effective Dates

```csharp
namespace Accounting.Tax;

public sealed class TaxRate
{
    public Guid Id { get; init; }
    public Guid TaxJurisdictionId { get; init; }
    public string TaxCode { get; init; } = string.Empty;
    public decimal RatePercent { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public string ReturnLineCode { get; init; } = string.Empty;
    public string SourceVersion { get; init; } = string.Empty;

    public bool AppliesTo(DateOnly transactionDate) =>
        transactionDate >= EffectiveFrom && (EffectiveTo is null || transactionDate <= EffectiveTo.Value);
}

public sealed record TaxCalculationLine(
    string LineId,
    string ProductTaxCode,
    decimal TaxableAmount,
    bool IsExempt,
    string? ExemptionCertificateId);

public sealed record AddressContext(
    string CountryCode,
    string StateCode,
    string PostalCode,
    string City,
    string Street1);

public sealed record TaxComponent(
    string AuthorityCode,
    string JurisdictionCode,
    decimal RatePercent,
    decimal TaxAmount,
    string LiabilityAccountCode,
    string ReturnLineCode);
```

### TaxCalculation Service

```csharp
namespace Accounting.Tax;

public interface IJurisdictionResolver
{
    Task<IReadOnlyList<TaxJurisdiction>> ResolveAsync(
        AddressContext address,
        DateOnly transactionDate,
        CancellationToken cancellationToken);
}

public interface IRateRepository
{
    Task<IReadOnlyList<TaxRate>> GetActiveRatesAsync(
        IReadOnlyList<Guid> jurisdictionIds,
        string productTaxCode,
        DateOnly transactionDate,
        CancellationToken cancellationToken);
}

public sealed record TaxCalculationResult(
    decimal TaxableBase,
    decimal TotalTax,
    IReadOnlyList<TaxComponent> Components);

public sealed class TaxCalculationService(
    IJurisdictionResolver jurisdictionResolver,
    IRateRepository rateRepository)
{
    public async Task<TaxCalculationResult> CalculateAsync(
        AddressContext address,
        TaxCalculationLine line,
        DateOnly transactionDate,
        CancellationToken cancellationToken)
    {
        if (line.IsExempt)
        {
            return new TaxCalculationResult(line.TaxableAmount, 0m, []);
        }

        var jurisdictions = await jurisdictionResolver.ResolveAsync(address, transactionDate, cancellationToken);
        var rates = await rateRepository.GetActiveRatesAsync(
            jurisdictions.Select(j => j.Id).ToArray(),
            line.ProductTaxCode,
            transactionDate,
            cancellationToken);

        var components =
            (from jurisdiction in jurisdictions
             join rate in rates on jurisdiction.Id equals rate.TaxJurisdictionId
             let taxAmount = decimal.Round(line.TaxableAmount * rate.RatePercent / 100m, 2, MidpointRounding.AwayFromZero)
             orderby jurisdiction.Level, jurisdiction.Priority
             select new TaxComponent(
                 jurisdiction.AuthorityCode,
                 jurisdiction.JurisdictionCode,
                 rate.RatePercent,
                 taxAmount,
                 jurisdiction.LiabilityAccountCode,
                 rate.ReturnLineCode))
            .ToArray();

        return new TaxCalculationResult(
            line.TaxableAmount,
            components.Sum(component => component.TaxAmount),
            components);
    }
}
```

### Tax Liability Reconciliation

```csharp
namespace Accounting.Tax.Reconciliation;

public sealed record TaxLedgerBalance(string LiabilityAccountCode, decimal Amount);
public sealed record TaxSubledgerBalance(string LiabilityAccountCode, decimal Amount);

public sealed class TaxLiabilityReconciliationService
{
    public IReadOnlyList<string> Reconcile(
        IReadOnlyList<TaxLedgerBalance> ledgerBalances,
        IReadOnlyList<TaxSubledgerBalance> subledgerBalances)
    {
        var differences =
            (from subledger in subledgerBalances
             join ledger in ledgerBalances
                 on subledger.LiabilityAccountCode equals ledger.LiabilityAccountCode
                 into ledgerJoin
             from ledger in ledgerJoin.DefaultIfEmpty(new TaxLedgerBalance(subledger.LiabilityAccountCode, 0m))
             let difference = decimal.Round(subledger.Amount - ledger.Amount, 2, MidpointRounding.AwayFromZero)
             where difference != 0m
             select $"{subledger.LiabilityAccountCode}:{difference:F2}")
            .ToArray();

        return differences;
    }
}
```

### Persistence and Query Patterns

| Concern | Pattern | Pass |
|---|---|---|
| Jurisdiction history | Keep jurisdiction and rate rows effective-dated and immutable after activation. | Historical recalculation stays deterministic. |
| Line detail | Persist transaction-line tax components, not the composite total only. | Filing and reconciliation explain every component. |
| Filing buckets | Persist return-line code and authority code on every tax component. | Return assembly uses governed metadata, not inferred joins only. |
| Exemption evidence | Persist certificate id, reason, and validation result on the line snapshot. | Audit review sees why tax suppressed. |
| Reconciliation runs | Persist run id, period, population hash, differences, and resolver notes. | Repeated runs stay comparable. |

## Tax Liability Rollforward and Journal Integration

| Event | Debit | Credit | Pass |
|---|---|---|---|
| Taxable customer sale | Accounts receivable or cash | Revenue and sales-tax payable split by line | Collected tax bypasses revenue. |
| Use-tax accrual on expense purchase | Tax expense or source expense | Use-tax payable | Liability and expense appear in the same period. |
| Use-tax accrual on capital purchase | Asset basis | Use-tax payable | Asset basis carries the tax. |
| Return payment | Sales-tax payable or use-tax payable | Cash or tax clearing | Payment reduces liability and preserves payment evidence. |
| Period-end B&O accrual | B&O tax expense | B&O payable | B&O liability remains separate from sales-tax payable. |

## 1099 Integration and SAAM 85 Cross-Reference

1099 reporting and indirect-tax accounting touch the same vendor and payment ecosystem, but they solve different tax obligations. Integration stays strongest when AP, vendor master, and tax services share validated master data and effective dates.

| Integration Point | Pattern | Pass |
|---|---|---|
| Vendor master | Store legal name, TIN status, address, 1099 classification, and indirect-tax vendor-collection status in one governed profile. | AP, 1099, and use-tax logic read the same mastered vendor facts. |
| Invoice capture | Preserve vendor tax detail and payment-reporting attributes on the invoice snapshot. | Later 1099 and use-tax review use the same evidence package. |
| Withholding or backup withholding | Post to dedicated withholding payable accounts, not to sales-tax or use-tax liability. | Federal withholding never pollutes indirect-tax balances. |
| SAAM 85 alignment | Cross-reference vendor-reporting and information-return obligations during AP design review. | Vendor tax-reporting fields remain explicit in AP and reporting models. |
| Address governance | Share normalized remit-to and legal addresses across 1099 and tax filing services. | Address-driven filing and information-reporting flows stay consistent. |

## Washington State B&O Tax Considerations

Washington State B&O tax operates as a gross-receipts tax borne by the entity. B&O tax stays distinct from retail sales tax collected from customers.

| Topic | Pattern | Pass |
|---|---|---|
| Tax base | Calculate B&O on gross receipts after governed deductions and exemptions. | B&O workpaper ties to gross-receipts population and deduction detail. |
| Classification | Map receipts into the governed B&O classification table. | Every taxable receipt line carries one B&O class. |
| Separation from sales tax | Keep B&O expense and payable separate from retail sales-tax payable. | Sales-tax liability rollforward excludes B&O. |
| Filing cadence | Align B&O period close and remittance to assigned Washington filing cadence. | B&O return and payment tie to the close calendar. |
| Apportionment | Preserve service or multi-state apportionment factors where the revenue type uses them. | Apportionment inputs remain auditable and effective-dated. |

Washington-specific operational pattern:

1. Sales-tax engine calculates retail sales tax on taxable customer transactions.
2. Revenue engine aggregates gross receipts for B&O classification.
3. Filing engine prepares retail sales tax and B&O as separate return components even when the state portal accepts both in one filing session.
4. GL mapping keeps `SalesTaxPayable` and `BOPayable` in separate accounts and separate reconciliation populations.

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Jurisdiction lookup | Run sample destination, origin, exempt, and no-nexus cases. | Each case resolves the correct jurisdiction stack and sourcing basis. |
| Rate versioning | Re-run a historical transaction after future rate load. | Historical calculation stays unchanged. |
| Use-tax accrual | Test taxed, untaxed, partially taxed, expense, and capital cases. | Only deficiency amounts accrue and classification matches policy. |
| Filing tie-out | Compare one filing-period return to the subledger and GL. | Return totals, liability rollforward, and cash remittance agree. |
| B&O separation | Compare Washington sales-tax and B&O postings. | Sales-tax liability and B&O expense or payable stay separate. |
| 1099 separation | Compare vendor withholding, 1099 fields, and indirect-tax liabilities. | Information-reporting balances never appear in indirect-tax payable accounts. |
