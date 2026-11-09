# Ifx.Roslyn

Shared netstandard2.0 diagnostic identities, descriptors, and architecture models.
Analyzer, generator, and code-fix implementations are in their named child
projects. This library has no child-project or runtime-framework references.
Shared architecture models and diagnostic IDs now reside in Ifx.Roslyn, alongside
the other shared compiler contracts. Abstractions namespace segments are removed.

Physical file access and report persistence belong to the Ifx.Roslyn.Reporting
executable. See [ownership and migration](../../docs/architecture/root-library-consolidation.md).
