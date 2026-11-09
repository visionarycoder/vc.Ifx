# Ifx

Core net10.0 runtime library: contracts, primitives, portable filtering and query
composition, pipelines, proxy pipeline, BCL archiving, local storage and secrets,
and in-memory channels. It has no project references or platform SDK dependencies.
Microsoft.Extensions supplies configuration, DI, logging, caching, and hosting
abstractions. Marker attributes are compiled into this library from their source
in the generator project without a compiler-tooling project reference.

External adapters are selected through Ifx.Integrations and its Azure/Windows
children. See [ownership and migration](../../docs/architecture/root-library-consolidation.md).
