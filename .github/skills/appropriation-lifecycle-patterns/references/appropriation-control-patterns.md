---
title: Appropriation Control Patterns Reference
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
related_docs:
  - ../SKILL.md
source_paths:
  - docs/references/SAAM/Domain/Saam.Domain.txt
  - docs/references/SAAM/Ontology/saam-core-turtle.txt
  - docs/references/SAAM/Ontology/saam-fabric-semantic-model.json
tags:
  - appropriation
  - budget
  - washington-state
  - saam
  - fund-accounting
---
# Appropriation Control Patterns Reference

## Source Alignment Snapshot

| Source Anchor | Reference Value | Design Use |
|---|---|---|
| SAAM ontology `Appropriation` | Legislative authorization to expend from a fund for a designated purpose during a biennium | Treat appropriation as a legal authority record, not a derived reporting balance. |
| SAAM ontology `Biennium` | Washington two-year budget cycle with a two-digit AFRS code | Key appropriation authority to a biennium dimension and validate period membership. |
| SAAM ontology `AppropriationIndex` | Three-character AFRS key that expands to fund and appropriation coding | Preserve AI or equivalent reduction key for edit validation and reporting lineage. |
| SAAM ontology `Budget` | Planned resource allocation across programs, organizations, and accounts for a biennium | Keep budget structure available as planning context distinct from expenditure events. |
| SAAM semantic model `Appropriation.AuthorizedAmount` | Authorized amount at appropriation grain | Store authority facts at appropriation grain with revision lineage. |
| SAAM semantic model `FinancialTransaction.AppropriationCode` | Transaction-to-appropriation relationship | Require controlled transactions to carry appropriation context. |

## Architecture Pattern

| Layer | Responsibility | Persistence Shape |
|---|---|---|
| Reference layer | Stores agency, fund, appropriation, appropriation index, fiscal period, and biennium dimensions | Effective-dated master tables |
| Authority layer | Stores appropriation authorization, revision, allotment, deallotment, carryforward, and reappropriation events | Immutable event tables plus current-balance projections |
| Reservation layer | Stores pre-encumbrance, encumbrance, release, and liquidation events | Immutable event tables keyed to source documents |
| Expenditure layer | Stores invoices, payroll, accruals, journal adjustments, and reversals | Transaction ledger with posting and approval data |
| Close layer | Stores reversion, lapse, carryforward, and close snapshots | Period-close tables and immutable snapshot records |
| Query layer | Calculates availability, utilization, exception queues, and audit trails | EF Core projections, SQL views, or materialized read models |

## Domain Model Pattern

| Entity | Key Fields | Purpose |
|---|---|---|
| `AppropriationAuthority` | `AppropriationCode`, `FundCode`, `BienniumCode`, `AuthoritySource`, `AuthorizedAmount` | Legal authority baseline |
| `AppropriationRevision` | `RevisionId`, `AppropriationCode`, `RevisionType`, `AmountDelta`, `EffectiveDate` | Supplemental, transfer, reduction, or reappropriation history |
| `AllotmentEvent` | `AllotmentEventId`, `AppropriationCode`, `FiscalYear`, `AmountDelta`, `ApprovalId` | Spending release history |
| `ReservationDocument` | `ReservationId`, `ReservationType`, `SourceDocumentType`, `SourceDocumentNumber` | Header for pre-encumbrance or encumbrance lifecycle |
| `ReservationEvent` | `ReservationEventId`, `ReservationId`, `Stage`, `AmountDelta`, `EffectiveDate` | Reserve, release, convert, and liquidate actions |
| `ExpenditureEvent` | `ExpenditureEventId`, `AppropriationCode`, `Amount`, `TransactionType`, `FiscalPeriodId` | Actual spending facts |
| `ReversionEvent` | `ReversionEventId`, `AppropriationCode`, `Amount`, `CloseScope`, `EffectiveDate` | Year-end or biennium-end expiration outcome |
| `AppropriationSnapshot` | `SnapshotId`, `AppropriationCode`, `FiscalPeriodId`, balance columns | Deterministic close or audit replay record |

## Event Taxonomy

| Event Family | Event Type | Balance Impact | Notes |
|---|---|---|---|
| Authority | `AuthorizationLoaded` | Increases authority | Opening budget or legal authority load |
| Authority | `RevisionIncreased` | Increases authority | Supplemental, transfer-in, or reappropriation |
| Authority | `RevisionReduced` | Decreases authority | Reduction, transfer-out, or rescission |
| Allotment | `AllotmentReleased` | Increases allotment | Agency spending release |
| Allotment | `AllotmentReduced` | Decreases allotment | Deallotment or internal hold |
| Reservation | `PreEncumbranceCreated` | Increases soft reserve | Requisition approved |
| Reservation | `PreEncumbranceReleased` | Decreases soft reserve | Cancellation, expiry, or conversion |
| Reservation | `EncumbranceCreated` | Increases firm reserve | Contract or purchase order approved |
| Reservation | `EncumbranceReleased` | Decreases firm reserve | Cancellation or balance release |
| Reservation | `EncumbranceLiquidated` | Decreases firm reserve | Settlement into expenditure |
| Expenditure | `ExpenditurePosted` | Increases actual | Invoice, payroll, accrual, or journal |
| Expenditure | `ExpenditureReversed` | Decreases actual | Reversal or correction |
| Close | `ReversionPosted` | Removes residual availability | Expiration or lapse |
| Close | `ReappropriationPosted` | Increases next-period authority | Authorized continuation path |

## Funds Availability Query Shape

| Balance | Composition |
|---|---|
| Revised Authority | Authorization + authority increases - authority reductions |
| Revised Allotment | Allotment releases - allotment reductions |
| Open Pre-encumbrance | Active pre-encumbrance creates - active pre-encumbrance releases - conversions |
| Open Encumbrance | Active encumbrance creates - encumbrance releases - liquidations |
| Actual Expenditures | Posted expenditures - reversals |
| Remaining Authority | Revised Authority - Open Pre-encumbrance - Open Encumbrance - Actual Expenditures |
| Remaining Allotment | Revised Allotment - Open Pre-encumbrance - Open Encumbrance - Actual Expenditures |
| Control Balance | Lesser of remaining authority and remaining allotment |

## EF Core Entity Sketch

```csharp
public sealed class AppropriationAuthority
{
    public required string AppropriationCode { get; init; }
    public required string FundCode { get; init; }
    public required string BienniumCode { get; init; }
    public required string Title { get; init; }
    public required string AuthoritySource { get; init; }
    public decimal AuthorizedAmount { get; init; }
    public DateOnly EffectiveStartDate { get; init; }
    public DateOnly EffectiveEndDate { get; init; }

    public List<AppropriationRevision> Revisions { get; } = [];
    public List<AllotmentEvent> Allotments { get; } = [];
}

public sealed class AppropriationRevision
{
    public long RevisionId { get; init; }
    public required string AppropriationCode { get; init; }
    public required string RevisionType { get; init; }
    public decimal AmountDelta { get; init; }
    public DateOnly EffectiveDate { get; init; }
    public required string ApprovalReference { get; init; }
}

public sealed class ReservationEvent
{
    public long ReservationEventId { get; init; }
    public required string ReservationId { get; init; }
    public required string AppropriationCode { get; init; }
    public required string Stage { get; init; }
    public decimal AmountDelta { get; init; }
    public DateOnly EffectiveDate { get; init; }
    public required string SourceDocumentType { get; init; }
    public required string SourceDocumentNumber { get; init; }
}

public sealed class ExpenditureEvent
{
    public long ExpenditureEventId { get; init; }
    public required string AppropriationCode { get; init; }
    public required string FiscalPeriodId { get; init; }
    public required string TransactionType { get; init; }
    public decimal Amount { get; init; }
    public DateOnly PostingDate { get; init; }
}
```

## EF Core Mapping Pattern

```csharp
public sealed class BudgetControlDbContext(DbContextOptions<BudgetControlDbContext> options)
    : DbContext(options)
{
    public DbSet<AppropriationAuthority> Appropriations => Set<AppropriationAuthority>();
    public DbSet<AppropriationRevision> Revisions => Set<AppropriationRevision>();
    public DbSet<AllotmentEvent> Allotments => Set<AllotmentEvent>();
    public DbSet<ReservationEvent> ReservationEvents => Set<ReservationEvent>();
    public DbSet<ExpenditureEvent> ExpenditureEvents => Set<ExpenditureEvent>();
    public DbSet<ReversionEvent> Reversions => Set<ReversionEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppropriationAuthority>(entity =>
        {
            entity.HasKey(x => x.AppropriationCode);
            entity.Property(x => x.AppropriationCode).HasMaxLength(20);
            entity.Property(x => x.FundCode).HasMaxLength(20);
            entity.Property(x => x.BienniumCode).HasMaxLength(10);
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.Property(x => x.AuthorizedAmount).HasPrecision(18, 2);
            entity.HasMany(x => x.Revisions)
                .WithOne()
                .HasForeignKey(x => x.AppropriationCode);
            entity.HasMany(x => x.Allotments)
                .WithOne()
                .HasForeignKey(x => x.AppropriationCode);
            entity.HasIndex(x => new { x.FundCode, x.BienniumCode });
        });

        modelBuilder.Entity<AppropriationRevision>(entity =>
        {
            entity.HasKey(x => x.RevisionId);
            entity.Property(x => x.AmountDelta).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.AppropriationCode, x.EffectiveDate });
        });

        modelBuilder.Entity<ReservationEvent>(entity =>
        {
            entity.HasKey(x => x.ReservationEventId);
            entity.Property(x => x.AmountDelta).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.AppropriationCode, x.EffectiveDate, x.Stage });
            entity.HasIndex(x => new { x.SourceDocumentType, x.SourceDocumentNumber });
        });

        modelBuilder.Entity<ExpenditureEvent>(entity =>
        {
            entity.HasKey(x => x.ExpenditureEventId);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.AppropriationCode, x.FiscalPeriodId, x.PostingDate });
        });
    }
}
```

## Funds Availability Projection Pattern

```csharp
public sealed record AppropriationAvailabilityResult(
    string AppropriationCode,
    string FundCode,
    string BienniumCode,
    decimal RevisedAuthority,
    decimal RevisedAllotment,
    decimal OpenPreEncumbrance,
    decimal OpenEncumbrance,
    decimal ActualExpenditures,
    decimal RemainingAuthority,
    decimal RemainingAllotment,
    decimal ControlBalance);

public static class BudgetControlQueries
{
    public static IQueryable<AppropriationAvailabilityResult> BuildAvailabilityQuery(
        this BudgetControlDbContext db,
        string bienniumCode)
    {
        var authorityQuery =
            from appropriation in db.Appropriations
            where appropriation.BienniumCode == bienniumCode
            select new
            {
                appropriation.AppropriationCode,
                appropriation.FundCode,
                appropriation.BienniumCode,
                RevisedAuthority =
                    appropriation.AuthorizedAmount +
                    appropriation.Revisions.Sum(x => (decimal?)x.AmountDelta) ?? appropriation.AuthorizedAmount
            };

        var allotmentQuery =
            from allotment in db.Allotments
            group allotment by allotment.AppropriationCode into grouped
            select new
            {
                AppropriationCode = grouped.Key,
                RevisedAllotment = grouped.Sum(x => x.AmountDelta)
            };

        var preEncumbranceQuery =
            from reservation in db.ReservationEvents
            where reservation.Stage == "PreEncumbrance"
            group reservation by reservation.AppropriationCode into grouped
            select new
            {
                AppropriationCode = grouped.Key,
                OpenPreEncumbrance = grouped.Sum(x => x.AmountDelta)
            };

        var encumbranceQuery =
            from reservation in db.ReservationEvents
            where reservation.Stage == "Encumbrance"
            group reservation by reservation.AppropriationCode into grouped
            select new
            {
                AppropriationCode = grouped.Key,
                OpenEncumbrance = grouped.Sum(x => x.AmountDelta)
            };

        var actualQuery =
            from expenditure in db.ExpenditureEvents
            group expenditure by expenditure.AppropriationCode into grouped
            select new
            {
                AppropriationCode = grouped.Key,
                ActualExpenditures = grouped.Sum(x => x.Amount)
            };

        var query =
            from authority in authorityQuery
            join allotment in allotmentQuery
                on authority.AppropriationCode equals allotment.AppropriationCode into allotments
            from allotment in allotments.DefaultIfEmpty()
            join preEncumbrance in preEncumbranceQuery
                on authority.AppropriationCode equals preEncumbrance.AppropriationCode into preEncumbrances
            from preEncumbrance in preEncumbrances.DefaultIfEmpty()
            join encumbrance in encumbranceQuery
                on authority.AppropriationCode equals encumbrance.AppropriationCode into encumbrances
            from encumbrance in encumbrances.DefaultIfEmpty()
            join actual in actualQuery
                on authority.AppropriationCode equals actual.AppropriationCode into actuals
            from actual in actuals.DefaultIfEmpty()
            let revisedAllotment = allotment == null ? 0m : allotment.RevisedAllotment
            let openPreEncumbrance = preEncumbrance == null ? 0m : preEncumbrance.OpenPreEncumbrance
            let openEncumbrance = encumbrance == null ? 0m : encumbrance.OpenEncumbrance
            let actualExpenditures = actual == null ? 0m : actual.ActualExpenditures
            let remainingAuthority = authority.RevisedAuthority - openPreEncumbrance - openEncumbrance - actualExpenditures
            let remainingAllotment = revisedAllotment - openPreEncumbrance - openEncumbrance - actualExpenditures
            let controlBalance = remainingAuthority < remainingAllotment ? remainingAuthority : remainingAllotment
            select new AppropriationAvailabilityResult(
                authority.AppropriationCode,
                authority.FundCode,
                authority.BienniumCode,
                authority.RevisedAuthority,
                revisedAllotment,
                openPreEncumbrance,
                openEncumbrance,
                actualExpenditures,
                remainingAuthority,
                remainingAllotment,
                controlBalance);

        return query;
    }
}
```

## Corrected Authority Aggregation Pattern

The projection above uses a simple shape. Production code benefits from explicit null-safe aggregation before adding the baseline authority value. The pattern below avoids operator-precedence ambiguity and preserves SQL translation.

```csharp
var authorityQuery =
    from appropriation in db.Appropriations
    where appropriation.BienniumCode == bienniumCode
    let revisionTotal = appropriation.Revisions
        .Select(x => (decimal?)x.AmountDelta)
        .Sum() ?? 0m
    select new
    {
        appropriation.AppropriationCode,
        appropriation.FundCode,
        appropriation.BienniumCode,
        RevisedAuthority = appropriation.AuthorizedAmount + revisionTotal
    };
```

## Request-Time Validation Service Pattern

```csharp
public sealed class FundsAvailabilityService(BudgetControlDbContext db)
{
    public async Task<FundsAvailabilityDecision> CheckAsync(
        FundsAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await db.BuildAvailabilityQuery(request.BienniumCode)
            .Where(x => x.AppropriationCode == request.AppropriationCode)
            .SingleOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return FundsAvailabilityDecision.Blocked(
                "appropriation-not-found",
                "Appropriation authority does not exist for the requested biennium.");
        }

        if (result.ControlBalance < request.Amount)
        {
            return FundsAvailabilityDecision.Blocked(
                "insufficient-budget",
                $"Requested amount {request.Amount:0.00} exceeds available control balance {result.ControlBalance:0.00}.");
        }

        return FundsAvailabilityDecision.Allowed(result.ControlBalance - request.Amount);
    }
}
```

## Control-Point Service Boundary

| Control Point | Entry Trigger | Query Grain | Result |
|---|---|---|---|
| Requisition approval | Planned use request | Appropriation, fiscal period, fund, organization, program | Approve pre-encumbrance or route exception |
| Purchase order approval | Obligation event | Appropriation, document, fiscal period | Convert or create encumbrance |
| Contract amendment | Scope or amount change | Appropriation, document, revision | Increase or decrease encumbrance |
| Invoice posting | Settlement event | Appropriation, document, fiscal period | Liquidate encumbrance and post expenditure |
| Payroll posting | Period payroll close | Appropriation, payroll run, fiscal period | Post expenditure directly or liquidate payroll reservation |
| Journal correction | Manual adjustment | Appropriation, posting date, reason code | Reverse or correct actuals with audit evidence |
| Period close | Fiscal-year or biennium close | Appropriation, fiscal period, biennium | Revert or carry forward residual balances |

## SQL View Pattern

```sql
CREATE VIEW dbo.AppropriationAvailability AS
WITH Authority AS (
    SELECT
        a.AppropriationCode,
        a.FundCode,
        a.BienniumCode,
        a.AuthorizedAmount + COALESCE(SUM(r.AmountDelta), 0) AS RevisedAuthority
    FROM dbo.AppropriationAuthority AS a
    LEFT JOIN dbo.AppropriationRevision AS r
        ON r.AppropriationCode = a.AppropriationCode
    GROUP BY
        a.AppropriationCode,
        a.FundCode,
        a.BienniumCode,
        a.AuthorizedAmount
),
Allotment AS (
    SELECT
        AppropriationCode,
        COALESCE(SUM(AmountDelta), 0) AS RevisedAllotment
    FROM dbo.AllotmentEvent
    GROUP BY AppropriationCode
),
PreEncumbrance AS (
    SELECT
        AppropriationCode,
        COALESCE(SUM(AmountDelta), 0) AS OpenPreEncumbrance
    FROM dbo.ReservationEvent
    WHERE Stage = 'PreEncumbrance'
    GROUP BY AppropriationCode
),
Encumbrance AS (
    SELECT
        AppropriationCode,
        COALESCE(SUM(AmountDelta), 0) AS OpenEncumbrance
    FROM dbo.ReservationEvent
    WHERE Stage = 'Encumbrance'
    GROUP BY AppropriationCode
),
Actual AS (
    SELECT
        AppropriationCode,
        COALESCE(SUM(Amount), 0) AS ActualExpenditures
    FROM dbo.ExpenditureEvent
    GROUP BY AppropriationCode
)
SELECT
    authority.AppropriationCode,
    authority.FundCode,
    authority.BienniumCode,
    authority.RevisedAuthority,
    COALESCE(allotment.RevisedAllotment, 0) AS RevisedAllotment,
    COALESCE(preenc.OpenPreEncumbrance, 0) AS OpenPreEncumbrance,
    COALESCE(enc.OpenEncumbrance, 0) AS OpenEncumbrance,
    COALESCE(actual.ActualExpenditures, 0) AS ActualExpenditures,
    authority.RevisedAuthority
        - COALESCE(preenc.OpenPreEncumbrance, 0)
        - COALESCE(enc.OpenEncumbrance, 0)
        - COALESCE(actual.ActualExpenditures, 0) AS RemainingAuthority,
    COALESCE(allotment.RevisedAllotment, 0)
        - COALESCE(preenc.OpenPreEncumbrance, 0)
        - COALESCE(enc.OpenEncumbrance, 0)
        - COALESCE(actual.ActualExpenditures, 0) AS RemainingAllotment,
    CASE
        WHEN authority.RevisedAuthority
            - COALESCE(preenc.OpenPreEncumbrance, 0)
            - COALESCE(enc.OpenEncumbrance, 0)
            - COALESCE(actual.ActualExpenditures, 0)
            < COALESCE(allotment.RevisedAllotment, 0)
            - COALESCE(preenc.OpenPreEncumbrance, 0)
            - COALESCE(enc.OpenEncumbrance, 0)
            - COALESCE(actual.ActualExpenditures, 0)
        THEN authority.RevisedAuthority
            - COALESCE(preenc.OpenPreEncumbrance, 0)
            - COALESCE(enc.OpenEncumbrance, 0)
            - COALESCE(actual.ActualExpenditures, 0)
        ELSE COALESCE(allotment.RevisedAllotment, 0)
            - COALESCE(preenc.OpenPreEncumbrance, 0)
            - COALESCE(enc.OpenEncumbrance, 0)
            - COALESCE(actual.ActualExpenditures, 0)
    END AS ControlBalance
FROM Authority AS authority
LEFT JOIN Allotment AS allotment
    ON allotment.AppropriationCode = authority.AppropriationCode
LEFT JOIN PreEncumbrance AS preenc
    ON preenc.AppropriationCode = authority.AppropriationCode
LEFT JOIN Encumbrance AS enc
    ON enc.AppropriationCode = authority.AppropriationCode
LEFT JOIN Actual AS actual
    ON actual.AppropriationCode = authority.AppropriationCode;
```

## Multi-Year Appropriation Handling

| Case | Pattern | Test | Pass |
|---|---|---|---|
| Single-biennium authority | Key one appropriation to one biennium and reject out-of-window postings. | Post within and outside the effective dates. | Validator accepts only in-window postings. |
| Multi-year capital authority | Store one legal authority record with multiple allotment and fiscal-year schedule rows. | Run a cross-year capital project scenario. | Remaining authority rolls forward while annual allotment stays period-bound. |
| Reappropriated authority | Create a new authority event in the receiving fiscal year or biennium with provenance to the source appropriation. | Compare source and target authority chains. | Both chains remain traceable and balances reconcile. |
| Lapse-year adjustments | Store late adjustments in a separate period state tied to the original authority source. | Post current-year and lapse-year adjustments. | Reports separate current and lapse activity without double counting. |
| Federal or grant overlay | Add grant and award dates in parallel to appropriation dates. | Test grant end date before appropriation end date. | Stricter external date window blocks the transaction. |

### Multi-Year Design Notes

Washington State work benefits from separating legal authority life from fiscal-period release. A capital appropriation often remains active across several fiscal years while annual allotment remains narrower. The data model stays stable when authority and allotment stay separate entities. A query for current availability selects the active authority window, then applies the active allotment schedule for the transaction date. A close package records unused annual allotment separately from residual legal authority when the appropriation remains open.

## Capital Versus Operating Budget Patterns

| Dimension | Capital Pattern | Operating Pattern |
|---|---|---|
| Time horizon | Often spans multiple fiscal years or the full biennium | Usually aligns to annual operating cycle inside the biennium |
| Project linkage | Requires project and phase keys | Often uses program and organization keys without project detail |
| Reservation duration | Encumbrances stay open longer and support change orders | Encumbrances tend to settle faster |
| Allotment strategy | Scheduled around project milestones, grants, and construction phases | Scheduled around payroll, contracts, and recurring service demand |
| Close treatment | Residual authority continues when authorization remains active | Residual authority usually reverts at close unless a continuation path exists |

### Capital Budget Query Pattern

```csharp
var capitalAvailability =
    await db.BuildAvailabilityQuery(bienniumCode)
        .Join(
            db.ProjectPhases,
            availability => availability.AppropriationCode,
            phase => phase.AppropriationCode,
            (availability, phase) => new
            {
                availability.AppropriationCode,
                phase.ProjectCode,
                phase.PhaseCode,
                availability.ControlBalance,
                phase.NextMilestoneDate
            })
        .Where(x => x.ProjectCode == request.ProjectCode)
        .ToListAsync(cancellationToken);
```

### Operating Budget Query Pattern

```csharp
var payrollAvailability =
    await db.BuildAvailabilityQuery(bienniumCode)
        .Where(x => x.FundCode == request.FundCode)
        .Join(
            db.PayrollAllocationRules,
            availability => availability.AppropriationCode,
            rule => rule.AppropriationCode,
            (availability, rule) => new
            {
                availability.AppropriationCode,
                rule.OrganizationCode,
                rule.ProgramCode,
                availability.ControlBalance
            })
        .ToListAsync(cancellationToken);
```

## Reappropriation and Reversion Rules

| Rule Area | Pattern | Observable Outcome |
|---|---|---|
| Year-end residual operating authority | Post explicit reversion event when no continuation path exists | Available balance reaches zero after close |
| Biennium-end residual authority | Revert unused balance and preserve a close snapshot | Close rerun reproduces the same expired balance |
| Legislatively continued authority | Post reappropriation event into the receiving period with source reference | Source reversion and target authority both remain visible |
| Capital project continuation | Preserve remaining authority and reset annual allotment schedule when legal authority remains open | Project authority survives year-end while annual controls refresh |
| Partial reversion | Split residual amount between reverted balance and continued balance | Total of reverted plus continued equals pre-close residual |

### Reversion Service Pattern

```csharp
public sealed class ReversionService(BudgetControlDbContext db)
{
    public async Task<IReadOnlyList<ReversionResult>> CloseBienniumAsync(
        string bienniumCode,
        DateOnly effectiveDate,
        CancellationToken cancellationToken)
    {
        var availability = await db.BuildAvailabilityQuery(bienniumCode)
            .Where(x => x.ControlBalance > 0)
            .ToListAsync(cancellationToken);

        var results = new List<ReversionResult>(availability.Count);

        foreach (var item in availability)
        {
            if (await HasApprovedContinuationAsync(item.AppropriationCode, effectiveDate, cancellationToken))
            {
                results.Add(ReversionResult.Continued(item.AppropriationCode, item.ControlBalance));
                continue;
            }

            db.Reversions.Add(new ReversionEvent
            {
                ReversionEventId = 0,
                AppropriationCode = item.AppropriationCode,
                Amount = item.ControlBalance,
                CloseScope = "Biennium",
                EffectiveDate = effectiveDate
            });

            results.Add(ReversionResult.Reverted(item.AppropriationCode, item.ControlBalance));
        }

        await db.SaveChangesAsync(cancellationToken);
        return results;
    }

    private Task<bool> HasApprovedContinuationAsync(
        string appropriationCode,
        DateOnly effectiveDate,
        CancellationToken cancellationToken) =>
        db.ReappropriationApprovals
            .AnyAsync(
                x => x.AppropriationCode == appropriationCode &&
                     x.EffectiveDate <= effectiveDate &&
                     x.Status == "Approved",
                cancellationToken);
}
```

## Close Snapshot Pattern

```csharp
public sealed class AppropriationSnapshot
{
    public long SnapshotId { get; init; }
    public required string AppropriationCode { get; init; }
    public required string FiscalPeriodId { get; init; }
    public decimal RevisedAuthority { get; init; }
    public decimal RevisedAllotment { get; init; }
    public decimal OpenPreEncumbrance { get; init; }
    public decimal OpenEncumbrance { get; init; }
    public decimal ActualExpenditures { get; init; }
    public decimal ControlBalance { get; init; }
    public required string SnapshotType { get; init; }
    public DateTimeOffset CapturedAt { get; init; }
}
```

## Audit and Traceability Pattern

| Control Objective | Implementation Pattern | Query Signal |
|---|---|---|
| Source-document lineage | Persist requisition, purchase order, contract, invoice, payroll run, or journal identifiers on reservation and expenditure events | Query by document number returns the full lifecycle chain |
| Approval evidence | Persist approval reference, approver, approval timestamp, and workflow step on authority and allotment changes | Query by approval reference returns one governed change set |
| Replayability | Keep immutable events and create snapshots only as read-model optimizations | Rebuilt balance equals snapshot balance |
| Period accountability | Stamp fiscal period, fiscal year, and biennium on controlled events | Queries reconcile by period and by authority window |
| Exception visibility | Persist control failures and overrides as first-class events | Query identifies who approved each exception and why |

## Exception Handling Pattern

| Exception Type | Detection | Handling |
|---|---|---|
| Missing appropriation | No authority row for the requested code and biennium | Block transaction and record validation event |
| Closed period | Fiscal period status not open for the transaction date | Block posting and route to period-control workflow |
| Insufficient allotment | Remaining allotment below requested amount | Block or route override request |
| Insufficient authority | Remaining authority below requested amount | Block and require budget revision or reappropriation |
| Expired authority | Effective end date before posting date | Block and route close or continuation review |
| Residual encumbrance at close | Open encumbrance remains at reversion time | Require liquidation, release, or approved continuation |

## Performance Pattern

| Concern | Pattern |
|---|---|
| High-volume availability checks | Project current balances into a read model keyed by appropriation, fiscal period, and organization dimensions. |
| Large close runs | Process appropriations in deterministic batches ordered by appropriation code. |
| Query translation stability | Keep LINQ aggregations simple and verify generated SQL for joins and groupings. |
| Concurrency | Use optimistic concurrency on authority, allotment, and balance projections plus unique source-document constraints on event ingestion. |
| Audit retention | Partition large event tables by biennium or fiscal year while preserving consistent key shapes. |

## Testing Pattern

| Test Case | Input | Pass |
|---|---|---|
| Exact balance requisition | Request equals control balance | Validator allows the request and leaves zero residual control balance. |
| Over-budget invoice | Request exceeds control balance | Validator blocks the request with `insufficient-budget`. |
| Partial liquidation | Encumbrance exceeds invoice amount | Remaining encumbrance equals original amount minus liquidation amount. |
| Cancellation | Reserved document cancels before settlement | Open reservation reaches zero and actual remains unchanged. |
| Biennium boundary | Posting date crosses authority end date | Validator blocks expired-period transaction. |
| Reappropriation | Approved continuation exists at close | Close process posts continuation result and skips reversion for that balance. |

## Integration with Related Skills

| Related Skill | Connection |
|---|---|
| `budget-to-actual-tracking` | Reuses authority, reservation, and actual layers for variance and utilization reporting. |
| `fund-accounting-patterns` | Supplies fund isolation, fund classification, and interfund boundaries for appropriation posting. |
| `wa-state-fiscal-policies` | Supplies fiscal-year, biennium, code-governance, and lapse-year context. |
| `wa-state-saam` | Supplies statewide policy vocabulary and SAAM-oriented control mapping. |

## Implementation Checklist

| Step | Test | Pass |
|---|---|---|
| Model authority and revisions | Inspect schema and keys. | Appropriation authority, revision, and allotment entities exist with effective dates. |
| Model reservation lifecycle | Run document lifecycle tests. | Pre-encumbrance, encumbrance, release, and liquidation states reconcile. |
| Implement availability query | Compare LINQ and SQL outputs. | Results match across representative scenarios. |
| Implement validation service | Run allow and block cases. | Service returns deterministic decisions and reason codes. |
| Implement close processing | Run year-end and biennium-end close tests. | Reversion and continuation outcomes match approved policy paths. |
| Implement audit queries | Query by document, approval, and period. | Full lifecycle evidence remains queryable. |
