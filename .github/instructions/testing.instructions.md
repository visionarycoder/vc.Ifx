---
# 🔧 Copilot Instruction Metadata
version: 1.1.0
schema: 1
updated: 2026-09-09
owner: Platform/IDL
stability: stable
domain: testing
# version semantics: MAJOR / MINOR / PATCH
---

# Data-Driven Unit & Integration Test Instructions

## Scope
Applies to `.cs`, `.ts`, `.spec.ts`, and `.test.ts` files.

## Coverage & Complexity Policy (Long-Term)
- **Code coverage: 100% required.** Enforced by `Threshold=100` (`line,branch,method`, `total`) in `Directory.Build.props` for every test project. Add tests for new/changed code rather than lowering the threshold.
- **Cyclomatic complexity: follow best practices.** Enforced by `CA1502` at `dotnet_code_quality.CA1502.threshold = 10` (McCabe-recommended ceiling) in `GlobalAnalyzerConfig.globalconfig`. Refactor methods that exceed the threshold instead of suppressing the diagnostic.

## Unit Testing
- C#: Use MSTest 4.0.1 with `[TestMethod]` and `[DataRow(...)]`.
- Use FluentAssertions 8.8.0 for readable assertions (`Should().Be()`, `Should().BeTrue()`).
- TypeScript: Use Playwright's `test.each([...])` for parameterized UI tests.
- Separate test data from logic.
- Include edge cases, nulls, and boundaries.
- Use mocks/stubs for external dependencies.
- Follow Arrange-Act-Assert structure.
- Organize tests with `#region` blocks for grouping related tests.

## Test Organization Patterns
- **Extension class contract tests**: Validate static classes and public static methods
- **Threshold boundary tests**: Test exact boundaries (e.g., 84.99°C vs 85.0°C vs 85.01°C)
- **Scenario-based tests**: Gaming, server, workstation, laptop configurations
- **Hardware-specific tests**: Component-specific temperature thresholds and usage limits
- **Edge case tests**: Zero values, negative values, extreme values, null safety
- **Implementation drift prevention**: Property existence, method signature validation

## Orleans Grain Testing
- Use `Orleans.TestingHost 9.2.1` with TestCluster for grain tests.
- Test grain interfaces separately from implementations.
- Validate grain communication patterns using `GrainFactory.GetGrain<T>(id)`.
- Test grain lifecycle (activation, deactivation).
- Use `[TestInitialize]` and `[TestCleanup]` for TestCluster setup/teardown.

## Integration Testing
- C#: Use MSTest with real services or test doubles.
- TypeScript: Use Playwright for end-to-end flows.
- Clean up state between runs.
- Validate side effects (e.g., DB writes, API calls).
- Multi-target tests: Ensure tests run on both net8.0 and net10.0.

## 📝 Changelog
### 1.1.0 (2026-09-09)
- Added long-term coverage (100%) and cyclomatic complexity (CA1502 threshold 10) policy section.
### 1.0.0 (2025-10-03)
- Added metadata header (initial versioning schema).