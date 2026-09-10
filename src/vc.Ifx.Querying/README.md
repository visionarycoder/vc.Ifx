# Querying Helpers & Serialization

This module contains lightweight utilities for serializing filter trees, rehydrating query filters and small helper extensions for `QueryFilter<T>`.

Key files

- `QueryFilter<T>` — Lightweight wrapper for `Expression<Func<T,bool>>` used to compose and apply predicates
- `QueryFilterSerializer` / `QueryFilterRehydrator` — Serialization helpers for persisting filters
- `QueryFilterExtensions` — Convenience helpers for building and composing `QueryFilter<T>` instances

Splitting guidance

When extracting this module:
- Create `VisionaryCoder.Framework.Querying` project containing `QueryFilter` and serialization helpers
- Keep serialization schema stable (use ADR to track breaking changes)

