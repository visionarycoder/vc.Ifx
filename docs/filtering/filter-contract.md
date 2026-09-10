# Filter Contract

Filtering owns predicates and portable predicate trees. QuerySpec/database access
owns query shape, projection and pagination; EF integration owns database translation.

FilterSpec<T> composition is immutable and substitutes parameters without Invoke.
Captured values follow LINQ expression semantics until ToFilterNode snapshots them.
The legacy Filter.For<T>() builder remains supported; Build snapshots its children.
Groups snapshot supplied lists, including lists supplied through with expressions.

Portable nodes support comparisons, Boolean members/constants, groups, negation,
string Contains/StartsWith/EndsWith, Any/All/Contains and constant-list membership.
Empty AND is true; empty OR is false. `$` denotes a scalar collection element.
Nested lambda paths are relative to their own parameter. Cross-scope comparisons,
custom comparers/operators and lossy numeric member conversions are rejected.

Unsupported syntax, invalid paths and invalid values throw. Never discard a
condition. Translation reads captured fields/properties and literal arrays; it
does not compile arbitrary method calls. Captured values use invariant formatting.
DateTime, DateTimeOffset and TimeOnly values use round-trip formatting; TimeOnly
seconds and fractional ticks must survive membership translation and JSON storage.
POCO execution preserves CLR null semantics: guard dereferences explicitly.
EF collation/null/provider behavior requires separate integration verification.

Polymorphic JSON uses `$type`: condition, group, collection, constant, not.
Existing enum values and leaf field names stay unchanged. Applications must enforce
allowed fields, size/depth limits and authorization before accepting external ASTs.
