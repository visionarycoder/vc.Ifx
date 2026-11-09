# Shared Roslyn Contracts

DiagnosticIds, ProjectType, Layer, and VolatilityLevel reside in the Ifx.Roslyn
namespace and project root. Analyzers, generators, and code fixes consume this
shared parent without referencing sibling implementation projects.
