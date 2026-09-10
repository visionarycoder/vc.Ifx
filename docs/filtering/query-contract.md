---
title: Query Contract
doc_type: reference
status: active
last_updated: 2026-09-10
---
# Query Contract

QuerySpec<T> is an immutable database-query description. Where composes FilterSpec
predicates; OrderBy/ThenBy preserve typed provider expressions; Page defines one
offset/limit window; Select preserves a provider projection. Apply does not execute,
compile a predicate, dispose a context or change the provider. Database access
code owns async enumeration, cancellation, tracking, includes and transactions.
Pagination requires explicit ordering and nonnegative offset/positive size.
Applications add a unique tie-breaker for stable pages. Repeated Page replaces the
window; repeated OrderBy replaces the ordering, matching a new specification.

QueryFilter<T> and its existing predicate extensions remain compatibility APIs.
Their IgnoreCase helpers preserve existing CLR behavior; provider support for
case normalization is not universal. Prefer provider collation configuration for
database-specific case-insensitive queries.

The legacy version-1 JSON shape retains operator/property/value/ignoreCase and
operator/children fields, with no runtime type names or $type discriminator.
Deserializer accepts canonical operator spelling only and rejects unknown fields,
duplicate properties, malformed node shapes, empty composites and Not with other
than one child. Value is a string or null; IN/NotIn encode a JSON array of string
or null values inside that field. Application-level field allowlists, size/depth
limits and authorization are still required for externally supplied filters.

The schema is embedded as VisionaryCoder.Framework.Schemas.queryfilter.schema.json.
Schema validation and deserialization share the structural reader; runtime member
types are validated during rehydration. Unknown schema versions are not silently
accepted: a future format requires an explicit converter/versioned entry point.
Rehydration adapts the legacy shape to the shared Filtering expression engine;
it never drops invalid children or returns an unfiltered query on failure.

See [application-stack-usage.md](./application-stack-usage.md) for layered
Client/Manager/Access usage guidance with FilterSpec and EF Core integration.
