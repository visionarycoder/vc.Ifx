# Filtering Subsystem

This directory contains the filtering model, expression translation and execution strategies used to build and apply portable filters across POCO collections and EF Core queryables.

Key components

- `VisionaryCoder.Framework.Filtering.Abstractions` — Filter node model and enums (`FilterNode`, `FilterCondition`, `FilterGroup`, `FilterOperation`, etc.)
- `ExpressionToFilterNode` — Translate LINQ `Expression<Func<T,bool>>` into `FilterNode` trees
- `Poco` execution strategy — Apply `FilterNode` to in-memory `IEnumerable<T>`
- `EFCore` execution strategy — Translate `FilterNode` into EF Core expressions (optimized translation for `IN`, Any/All and string ops)

Samples

A sample demo demonstrates building filters, applying to POCO lists and EF queryables. See `src/VisionaryCoder.Framework/Filtering/Sample`.

Splitting guidance

When extracting the filtering subsystem into a standalone package:

1. Create `VisionaryCoder.Framework.Filtering.Abstractions` project with the model types and interfaces.
2. Create `VisionaryCoder.Framework.Filtering.Poco` and `VisionaryCoder.Framework.Filtering.EFCore` projects for execution strategies.
3. Keep `ExpressionToFilterNode` close to the Abstractions or in a lightweight `Filtering.Helpers` package if you want to share it between strategies.

