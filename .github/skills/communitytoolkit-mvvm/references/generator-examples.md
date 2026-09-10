---
title: CommunityToolkit.Mvvm Examples
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
related_docs:
  - ../SKILL.md
---
# CommunityToolkit.Mvvm Examples

## Manual Versus Generated Comparison

| Concern | Manual MVVM | CommunityToolkit.Mvvm |
|---|---|---|
| Property change code | Explicit backing field, equality check, event raise, and property name string | `[ObservableProperty]` plus generated setter logic |
| Command code | Explicit `ICommand` field or nested command type | `[RelayCommand]` on a method |
| Command invalidation | Manual `CanExecuteChanged` wiring | `NotifyCanExecuteChangedFor` or generated command hooks |
| Validation | Custom error dictionary and notification plumbing | `ObservableValidator` plus data annotation integration |
| Messaging | Static events or direct references | `WeakReferenceMessenger` or injected `IMessenger` |
| Inspection | Hand-authored code only | Source plus generated output in `obj\generated` |

## Boilerplate Reduction Examples

### Before

```csharp
public sealed class CustomerEditorViewModel : INotifyPropertyChanged
{
    private readonly ICustomerService customerService;
    private string customerName = string.Empty;
    private bool isBusy;
    private Customer? selectedCustomer;

    public CustomerEditorViewModel(ICustomerService customerService)
    {
        this.customerService = customerService;
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CustomerName)));
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    public Customer? SelectedCustomer
    {
        get => selectedCustomer;
        set
        {
            if (selectedCustomer == value)
            {
                return;
            }

            selectedCustomer = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCustomer)));
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    public IAsyncRelayCommand SaveCommand { get; }

    private bool CanSave() => !IsBusy && SelectedCustomer is not null && !string.IsNullOrWhiteSpace(CustomerName);

    private Task SaveAsync() => customerService.SaveAsync(SelectedCustomer!.Id, CustomerName);
}
```

## Package and Setup Matrix

| Area | Pattern | Test | Pass |
|---|---|---|---|
| Package install | Agent references `CommunityToolkit.Mvvm` from the target project. | Build the project. | Generators run and compile output succeeds. |
| Namespace use | Agent imports toolkit namespaces in view models, validators, and messenger consumers. | Inspect using directives and build output. | Types resolve without alias workarounds. |
| Generated files | Agent inspects `obj\generated` or `obj\Debug\**\generated` during troubleshooting. | Open generated files after build. | Generated members match attributes on source types. |
| Partial type setup | Agent marks generator-backed classes `partial`. | Build the project. | No missing-partial generator errors remain. |
| .NET 10 alignment | Agent uses partial properties only in projects that compile with the required language version and toolkit support. | Build the touched project. | Partial-property syntax compiles cleanly. |

## Integration Hooks

| Platform | Hook | Route |
|---|---|---|
| WPF | Bind toolkit view models into standard WPF bindings and validation flows. | `wpf-mvvm-implementation` |
| .NET MAUI | Pair toolkit view models with Shell navigation and handler customization. | `maui-patterns` |
| WinUI 3 | Pair toolkit view models with `x:Bind`, `NavigationView`, and WindowsAppSDK app structure. | `winui3-patterns` |

## MCP Hooks

| Task | MCP Tool | Assistance |
|---|---|---|
| Find `[ObservableProperty]`, `[RelayCommand]`, `ObservableRecipient`, and messenger usage | `github-mcp-server-search_code` | Locates generator-backed view models, validation hooks, and message contracts. |
| Inspect toolkit package references and generated-friendly source | `github-mcp-server-get_file_contents` | Reads project files, partial view models, and migration targets in versioned branches. |
| Review toolkit adoption history | `github-mcp-server-search_pull_requests` | Surfaces earlier boilerplate-reduction choices, generator fixes, and reviewer guidance. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Generator-backed classes lack `partial`. | Agent marks the type `partial` and rebuilds. |
| Messenger registration outlives screen lifetime. | Agent uses `ObservableRecipient` activation or explicit unregister flow. |
| Async command guards ignore busy state. | Agent ties guard logic to `IsBusy` and command invalidation attributes. |
| Manual code and generated code both raise notifications for the same property. | Agent removes duplicate manual notification plumbing. |
| Troubleshooting ignores generated output. | Agent inspects `obj\generated` before deeper refactoring. |

## Outputs

| Output | Description |
|---|---|
| Toolkit adoption matrix | Pattern selection for touched view models |
| Generator-backed property and command pattern | Attribute-driven observable state and actions |
| Validation and messaging contract map | Observable error and message-boundary guidance |
| Focused verification steps | Generated behavior checks for touched MVVM flows |

### After

```csharp
public sealed partial class CustomerEditorViewModel : ObservableValidator
{
    private readonly ICustomerService customerService;

    public CustomerEditorViewModel(ICustomerService customerService, IMessenger messenger)
    {
        this.customerService = customerService;
        Messenger = messenger;
    }

    protected IMessenger Messenger { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string customerName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private Customer? selectedCustomer;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        await customerService.SaveAsync(SelectedCustomer!.Id, CustomerName);
        Messenger.Send(new CustomerSavedMessage(SelectedCustomer.Id));
    }

    private bool CanSave() => !IsBusy && SelectedCustomer is not null && !string.IsNullOrWhiteSpace(CustomerName);
}
```
