---
title: Roslyn Analyzer Patterns Reference
description: Compact Roslyn analyzer descriptor, registration, testing, and packaging patterns.
doc_type: reference
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 980
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - roslyn-analyzer-authoring
appliesTo: '**/*.{cs,csproj,props,targets,md}'
tags:
  - roslyn
  - analyzers
  - reference
---
# Roslyn Analyzer Patterns Reference

## Descriptor Pattern

| Element | Use |
|---|---|
| ID | Use stable `IFX####` or repository pattern IDs. |
| Title | Use a short actionable title. |
| Message | Use offender placeholders such as `{0}`. |
| Category | Match sibling analyzers. |
| Severity | Use warning or error only when build enforcement fits. |
| Help link | Use a stable docs link when available. |

```csharp
internal static readonly DiagnosticDescriptor Rule = new(
    id: "IFX1001",
    title: "Use typed problem details",
    messageFormat: "Member '{0}' returns an untyped error contract",
    category: "Reliability",
    defaultSeverity: DiagnosticSeverity.Warning,
    isEnabledByDefault: true);
```

## Registration Pattern

| Rule shape | Registration |
|---|---|
| Syntax-only structure | `RegisterSyntaxNodeAction` |
| Symbol contract | `RegisterSymbolAction` |
| Expression semantics | `RegisterOperationAction` |
| Shared metadata lookup | `RegisterCompilationStartAction` |

```csharp
public override void Initialize(AnalysisContext context)
{
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
}
```

## Performance Pattern

| Risk | Agent move |
|---|---|
| Repeated metadata lookups | Cache type symbols in compilation start. |
| Per-node allocations | Use static lambdas and avoid LINQ in hot paths. |
| Full semantic scans | Filter with syntax first. |
| Ignored cancellation | Pass `CancellationToken` through every Roslyn lookup. |

## Test Pattern

| Case | Minimal assertion |
|---|---|
| Positive | One expected diagnostic with location. |
| Negative | No diagnostics. |
| Message | Exact message arguments when wording matters. |
| Multi-hit | Multiple diagnostics in stable order only when order is deterministic. |

```csharp
[TestMethod]
public async Task Reports_rule_for_invalid_code_async()
{
    var test = new CSharpAnalyzerTest<MyAnalyzer, MSTestVerifier>
    {
        TestCode = "class C { void {|#0:M|}() { } }"
    };

    test.ExpectedDiagnostics.Add(
        new DiagnosticResult("IFX1001", DiagnosticSeverity.Warning).WithLocation(0));

    await test.RunAsync();
}
```

## Packaging Pattern

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <IncludeBuildOutput>false</IncludeBuildOutput>
  </PropertyGroup>
  <ItemGroup>
    <None Include="$(OutputPath)\$(AssemblyName).dll" Pack="true" PackagePath="analyzers/dotnet/cs" />
  </ItemGroup>
</Project>
```
