# vc.Ifx.Primitives

## Mission

Give framework consumers small, immutable values with explicit equality, validation,
and invariant text representation. Domain owner type parameters prevent accidental
mixing of otherwise identical identifiers, quantities, and codes.

## Contracts

- `Identity<TOwner>` validates nonempty GUIDs; `Code<TOwner>` trims and normalizes
  ASCII-style identifier text to uppercase. `NonEmptyText<TOwner>` trims text.
- `PositiveQuantity<TOwner>` requires values greater than zero;
  `NonNegativeAmount<TOwner>` requires zero or greater; `Percentage<TOwner>` uses
  the inclusive range 0 to 100, not a fractional range.
- `Month` represents a Gregorian year/month, including leap-year boundaries.
  `Money` keeps the amount and normalized three-letter currency together and
  rejects arithmetic across different currencies. It does not perform exchange
  rates, rounding policy, or validate membership in a live currency registry.
- Equality is value-based and owner-specific where the type has an owner. Text
  parsing/formatting uses invariant culture. Failed `TryParse` returns false and
  its documented default output; `Parse` throws `FormatException`.

## Compatibility Boundaries

`EntityId<TEntity,TKey>.Create` and implicit key conversion reject default keys
and whitespace strings. The public raw constructor, JSON conversion, EF conversion,
and text parsing intentionally retain zero/empty-GUID keys for persistence and
legacy compatibility. `TryParse` supports Guid, string, int, long, and short;
unsupported key types return false. Whitespace string parsing fails.

The JSON converter writes these supported keys as scalars and honors configured
scalar converters and number handling. Unsupported key types now fail with
`NotSupportedException` in both directions instead of writing unparseable strings.
Legacy JSON null-to-empty string reading remains supported. Struct defaults can
bypass constructor invariants; validate them at domain boundaries before use.

EF and ASP.NET Core integrations remain in this package to preserve existing
namespaces and consumers. This intentionally brings EF and ASP.NET Core references;
splitting them requires a separately versioned migration, not duplicate value types.
`UseEntityId` preserves raw keys. The model binder records attempted values and
adds safe model-state errors for invalid IDs; absent input is left unbound. It no
longer exposes reflection/format exceptions for malformed HTTP input.

## Verification

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -TestSourceScope Primitives -TestPackage vc.Ifx.Primitives -CoveragePackage vc.Ifx.Primitives -Filter FullyQualifiedName~Primitives
```

Tests cover value boundaries, formatting, serialization, EF conversion metadata,
and ASP.NET Core binding. The upgrade plan separately tracks solution-wide gates.
