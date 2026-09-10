---
title: Roslyn Code Generation Examples
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
related_docs:
  - ../SKILL.md
---
# Roslyn Code Generation Examples

## Incremental Generator Pipeline

| Concern | Pattern |
|---|---|
| Entry point | `CreateSyntaxProvider` plus `RegisterSourceOutput` |
| Candidate filter | Cheap syntax predicate only |
| Semantic projection | `GeneratorSyntaxContext` transform |
| Deduplication | `Collect()` plus stable distinct key |
| Emission | One projection item maps to one or more stable files |

```csharp
[Generator]
public sealed class ContractGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, ct) => TryCreateTarget(ctx, ct))
            .Where(static target => target is not null);

        var compilationAndTargets = context.CompilationProvider.Combine(targets.Collect());

        context.RegisterSourceOutput(compilationAndTargets, static (spc, pair) =>
        {
            foreach (var target in pair.Right.OrderBy(static x => x!.HintSuffix, StringComparer.Ordinal))
            {
                if (target is null)
                {
                    continue;
                }

                spc.AddSource(target.HintSuffix + ".g.cs", Render(target));
            }
        });
    }
}
```

## Receiver Selection Matrix

| Need | Preferred pattern |
|---|---|
| Production generator with cache-aware recomputation | `IIncrementalGenerator` |
| Legacy `ISourceGenerator` with semantic projection during discovery | `ISyntaxContextReceiver` |
| Legacy `ISourceGenerator` with syntax-only prefilter | `ISyntaxReceiver` |
| Migration from classic generator to incremental pipeline | Keep receiver logic small, then move discovery into `CreateSyntaxProvider` |

```csharp
public sealed class AttributedClassReceiver : ISyntaxContextReceiver
{
    public List<INamedTypeSymbol> Candidates { get; } = new List<INamedTypeSymbol>();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } declaration &&
            context.SemanticModel.GetDeclaredSymbol(declaration) is INamedTypeSymbol symbol &&
            symbol.GetAttributes().Any(static x => x.AttributeClass?.Name == "GenerateClientAttribute"))
        {
            Candidates.Add(symbol);
        }
    }
}
```

## Attributed Class Discovery

| Concern | Pattern |
|---|---|
| Attribute lookup | Resolve `INamedTypeSymbol` for the attribute once per compilation |
| Candidate match | Compare symbols with `SymbolEqualityComparer.Default` |
| Partial requirement | Report a diagnostic when the target type lacks `partial` |
| Contract model | Store namespace, type name, accessibility, members, and attribute arguments |

```csharp
private static GeneratorTarget? TryCreateTarget(GeneratorSyntaxContext context, CancellationToken cancellationToken)
{
    var declaration = (ClassDeclarationSyntax)context.Node;
    var symbol = context.SemanticModel.GetDeclaredSymbol(declaration, cancellationToken);
    if (symbol is null)
    {
        return null;
    }

    var attribute = symbol.GetAttributes()
        .FirstOrDefault(static attributeData => attributeData.AttributeClass?.ToDisplayString() == "Demo.GenerateClientAttribute");

    return attribute is null
        ? null
        : GeneratorTarget.Create(symbol, declaration);
}
```

## Multi-File Generation

| Concern | Pattern |
|---|---|
| One feature, many artifacts | Emit separate files for contracts, extensions, and registration helpers |
| Hint naming | Include feature and type identity in every hint name |
| Partial types | Split generated members by concern, not by random chunk size |
| Shared helpers | Emit one helper file per compilation only when at least one target needs it |

```csharp
foreach (var target in targets.OrderBy(static x => x.TypeName, StringComparer.Ordinal))
{
    context.AddSource(target.TypeName + ".Contracts.g.cs", RenderContracts(target));
    context.AddSource(target.TypeName + ".Extensions.g.cs", RenderExtensions(target));
}

if (targets.Count > 0)
{
    context.AddSource("GeneratorRegistration.g.cs", RenderRegistration(targets));
}
```

## Debugging Generators

| Concern | Pattern |
|---|---|
| Local debug entry | Gate `Debugger.Launch()` behind a named environment variable |
| Repository safety | Keep debug hooks off by default |
| Failure triage | Emit diagnostics before attaching the debugger when contract input is invalid |

```csharp
if (Environment.GetEnvironmentVariable("ROSLYN_GENERATOR_DEBUG") == "1" && !Debugger.IsAttached)
{
    Debugger.Launch();
}
```

## MSTest Generator Testing Patterns

| Test case | Assertion focus |
|---|---|
| Valid target | Generated file exists and compilation succeeds |
| Invalid target | Expected diagnostic ID and message appear |
| Negative path | Non-target source emits no generated files |
| Stability | Two runs produce identical generated text |
| Multi-file | Every expected hint name appears exactly once |

```csharp
[TestMethod]
public void Emits_multiple_files_for_valid_target()
{
    var compilation = TestCompilationFactory.Create(
        "using Demo;" + Environment.NewLine +
        "[GenerateClient]" + Environment.NewLine +
        "public partial class OrderClient {}");

    GeneratorDriver driver = CSharpGeneratorDriver.Create(new ContractGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

    Assert.AreEqual(0, diagnostics.Length);
    Assert.IsTrue(outputCompilation.SyntaxTrees.Any(static tree => tree.FilePath.EndsWith("OrderClient.Contracts.g.cs", StringComparison.Ordinal)));
    Assert.IsTrue(outputCompilation.SyntaxTrees.Any(static tree => tree.FilePath.EndsWith("OrderClient.Extensions.g.cs", StringComparison.Ordinal)));
}
```

## Caching and Performance

| Hot path | Agent action |
|---|---|
| Attribute symbol lookup | Cache symbols in compilation-level projection. |
| Semantic model usage | Resolve symbols only after syntax predicates pass. |
| String generation | Reuse builders per emission path and keep stable append order. |
| Equality | Use compact value objects with deterministic equality for incremental cache reuse. |
| Cancellation | Pass `CancellationToken` through every transform. |

## Generated Code Conventions

| Concern | Pattern |
|---|---|
| Header | Emit `// <auto-generated />` |
| Namespace style | Emit file-scoped namespace in generated code when consumer target framework and style accept it |
| Nullability | Match consumer nullable context explicitly |
| Accessibility | Mirror target type contract |
| Formatting | Use stable indentation and trailing newline |

```csharp
// <auto-generated />
#nullable enable
namespace Demo.Generated;

internal static partial class OrderClientGenerated
{
    internal const string FeatureName = "Contracts";
}
```

## MCP Hooks

| Need | GitHub MCP hook | Agent action |
|---|---|---|
| Find existing generator patterns | `search_code` | Agent searches for `IIncrementalGenerator`, `CreateSyntaxProvider`, hint-name conventions, and generator tests before editing. |
| Review generator PR changes | `pull_request_read` | Agent reads changed generators, snapshots, and diagnostic tests before extending code-generation behavior. |
| Verify Roslyn test and performance coverage | `search_code` | Agent searches for generator driver tests, debug hooks, and deterministic output assertions so generator changes stay production-safe. |

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Generator keeps full syntax trees in long-lived models | Agent projects into compact immutable data and drops tree references. |
| Hint names depend on enumeration order | Agent sorts inputs and builds deterministic hint names. |
| Receiver performs semantic work on every visited node | Agent narrows syntax shape first or migrates to incremental providers. |
| Multi-file generators emit shared helper files once per target | Agent emits shared helper files once per compilation. |
| Debug hooks run in CI | Agent gates hooks behind environment variables or debug symbols. |
| Tests assert file count only | Agent asserts hint names, source content, diagnostics, and updated compilation. |

## Outputs

| Output | Description |
|---|---|
| Incremental pipeline design | Discovery, projection, and emission plan for a production generator |
| Discovery contract | Attribute and syntax rules with exclusion logic |
| Generated file map | Stable hint names and artifact boundaries |
| Debug strategy | Safe local debugging hook and triage path |
| MSTest suite | Positive, negative, diagnostic, stability, and compilation tests |
| Performance review | Cache, allocation, and recomputation guidance |
