---
title: MVVM Source Generator Examples
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
related_docs:
  - ../SKILL.md
---
# MVVM Source Generator Examples

## Manual Versus Generated Comparison

| Concern | Manual implementation | Generated implementation |
|---|---|---|
| Notification plumbing | Hand-authored event, equality check, setter body, and hook methods | Attribute or syntax trigger emits the full setter pattern |
| Command surface | Manual command fields and `CanExecute` invalidation | Generated command wrapper from a method contract |
| Validation repetition | Repeated error storage and rule calls | Generated validation bridge or attribute forwarding |
| Partial enforcement | Human review only | Compiler diagnostic for missing `partial` |
| Maintenance cost | Many duplicated lines across view models | One generator plus compact source annotations |

## Boilerplate Reduction Examples

### Before

```csharp
public sealed partial class CustomerEditorViewModel
{
    private string customerName = string.Empty;
    private bool isBusy;

    public string CustomerName
    {
        get => customerName;
        set
        {
            if (customerName == value)
            {
                return;
            }

            customerName = value;
            OnPropertyChanged(nameof(CustomerName));
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsBusy
    {
        get => isBusy;
        set
        {
            if (isBusy == value)
            {
                return;
            }

            isBusy = value;
            OnPropertyChanged(nameof(IsBusy));
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    public IAsyncRelayCommand SaveCommand { get; }
}
```

### After

```csharp
[GenerateObservableProperty(nameof(CustomerName), NotifyCommands = new[] { "SaveCommand" })]
[GenerateObservableProperty(nameof(IsBusy), NotifyCommands = new[] { "SaveCommand" })]
[GenerateCommand(nameof(SaveAsync), CommandName = "SaveCommand")]
public sealed partial class CustomerEditorViewModel : ObservableObject
{
    private Task SaveAsync() => Task.CompletedTask;
}
```

### Generated Output Shape

```csharp
partial class CustomerEditorViewModel
{
    private string customerName = string.Empty;
    private bool isBusy;

    public string CustomerName
    {
        get => customerName;
        set
        {
            if (EqualityComparer<string>.Default.Equals(customerName, value))
            {
                return;
            }

            customerName = value;
            OnPropertyChanged(nameof(CustomerName));
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsBusy
    {
        get => isBusy;
        set
        {
            if (EqualityComparer<bool>.Default.Equals(isBusy, value))
            {
                return;
            }

            isBusy = value;
            OnPropertyChanged(nameof(IsBusy));
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
}
```

## Diagnostic and Packaging Matrix

| Area | Pattern | Test | Pass |
|---|---|---|---|
| Missing partial | Agent reports one diagnostic with type location and a direct fix message. | Compile a non-partial target type. | Diagnostic points at the user-authored declaration. |
| Invalid method signature | Agent reports diagnostics for unsupported async return shapes, generic commands, or inaccessible methods. | Compile invalid signatures. | Each invalid shape yields a specific diagnostic. |
| GeneratorDriver tests | Agent validates positive, negative, compilation, and stability paths. | Run existing generator tests. | Tests cover emitted source and diagnostics. |
| Snapshot inspection | Agent inspects generated code text during test failures. | Compare actual and expected generated output. | Output drift becomes visible and actionable. |
| Packaging | Agent ships generator DLLs through analyzer assets. | Pack and consume the NuGet in a sample or existing test harness. | Consumer projects load the generator automatically. |

## Integration Hooks

| Platform | Hook | Route |
|---|---|---|
| WPF | Emit APIs that align with existing WPF binding, validation, and command patterns. | `wpf-mvvm-implementation` |
| .NET MAUI | Emit APIs that align with MAUI view models and platform navigation services. | `maui-patterns` |
| WinUI 3 | Emit APIs that align with WinUI 3 `x:Bind`, command, and observable state patterns. | `winui3-patterns` |

## MCP Hooks

| Task | MCP Tool | Assistance |
|---|---|---|
| Find generator attributes, diagnostics, tests, and packaging assets | `github-mcp-server-search_code` | Locates Roslyn pipelines, generated hint names, and analyzer pack targets. |
| Inspect generator projects and analyzer package files | `github-mcp-server-get_file_contents` | Reads `.csproj`, test baselines, and packaging metadata across branches. |
| Review generator build and test workflow history | `github-mcp-server-actions_list` | Lists CI runs tied to analyzer compilation, snapshot tests, and NuGet packing. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Generator code uses C# 9+ syntax inside a `netstandard2.0` Roslyn project. | Agent rewrites the implementation to C# 8-safe syntax. |
| Generator performs semantic work on every syntax node. | Agent restores syntax-first filtering. |
| Missing-partial contracts fail with an exception instead of a diagnostic. | Agent adds a dedicated diagnostic path. |
| Emitted member names drift between runs. | Agent centralizes naming and ordering rules. |
| NuGet packages omit analyzer assets. | Agent configures analyzer packaging and verifies the packed layout. |

## Outputs

| Output | Description |
|---|---|
| MVVM generator trigger and emission contract | Attribute and output rules for generated MVVM members |
| C# 8 constraint enforcement matrix | Roslyn project compatibility guidance |
| Diagnostic set | Invalid MVVM authoring shape coverage |
| GeneratorDriver verification plan | Positive, negative, compilation, and stability checks |
| Analyzer NuGet distribution checklist | Packaging and consumer validation guidance |
