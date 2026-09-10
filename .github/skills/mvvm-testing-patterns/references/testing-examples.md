---
title: MVVM Testing Examples
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
related_docs:
  - ../SKILL.md
---
# MVVM Testing Examples

## Testing Pattern Examples

| Example | Purpose |
|---|---|
| Property change test | Verifies `PropertyChanged` notifications. |
| Async command and navigation test | Verifies `AsyncRelayCommand`, busy state, and route requests. |

```csharp
[TestClass]
public sealed class OrderEditorViewModelTests
{
    [TestMethod]
    public void CustomerName_WhenChanged_RaisesPropertyChanged()
    {
        var changes = new List<string?>();
        var viewModel = new OrderEditorViewModel();
        viewModel.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

        viewModel.CustomerName = "Ada";

        CollectionAssert.Contains(changes, nameof(OrderEditorViewModel.CustomerName));
    }

    [TestMethod]
    public async Task SaveCommand_WhenValid_NavigatesAfterSaveAsync()
    {
        var repository = new Mock<IOrderRepository>();
        var navigation = new FakeNavigationService();
        var viewModel = new OrderEditorViewModel(repository.Object, navigation);

        viewModel.CustomerName = "Ada";

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.IsFalse(viewModel.IsBusy);
        repository.Verify(x => x.SaveAsync(It.IsAny<OrderDraft>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.AreEqual("OrderSummary", navigation.LastRoute);
    }
}
```

## Validation and Coverage Matrix

| Concern | Agent action | Test | Pass |
|---|---|---|---|
| Validation tests | Agent asserts property errors, form state, and command gating together. | Run invalid and recovery paths. | Validation behavior stays observable. |
| Messenger cleanup | Agent resets messenger state or uses per-test instances. | Run tests in any order. | No test leaks cross-test messages. |
| Async cleanup | Agent awaits command completion and releases test doubles per test. | Run the full target class repeatedly. | No hanging tasks or order-dependent failures remain. |
| Coverage collection | Agent runs `dotnet test [test-project].csproj --collect:"XPlat Code Coverage"` when coverage already exists in the project workflow. | Inspect command results. | Coverage collection succeeds. |

## Integration Hooks

| Integration | Agent action | Pass |
|---|---|---|
| `CommunityToolkit.Mvvm` | Agent tests `ObservableObject`, `ObservableValidator`, `RelayCommand`, and `AsyncRelayCommand` through their public surface. | Toolkit abstractions stay observable and deterministic. |
| `writing-mstest-tests` | Agent aligns assertions, lifecycle, naming, and data-driven usage with repository MSTest v4 conventions. | Tests match repository MSTest patterns. |
| DI container | Agent uses `Microsoft.Extensions.DependencyInjection` only for composition checks that add value. | Container coverage stays focused and low-noise. |

## MCP Hooks

| Task | MCP Tool | Assistance |
|---|---|---|
| Find MSTest view-model tests, fakes, and messenger coverage | `github-mcp-server-search_code` | Locates command tests, `PropertyChanged` assertions, and isolation patterns. |
| Inspect versioned test fixtures and helper types | `github-mcp-server-get_file_contents` | Reads fake navigation services, test doubles, and target test classes across branches. |
| Review UI-test workflow history | `github-mcp-server-actions_list` | Lists CI runs tied to MVVM regressions, flaky tests, and coverage collection. |

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Tests call private helper methods directly. | Agent drives the public property or command surface. |
| Shared messenger instances leak state across tests. | Agent uses per-test instances or explicit reset paths. |
| Async command tests forget to await completion. | Agent awaits `ExecuteAsync` and verifies final state. |
| Mock setup dominates the test body. | Agent replaces low-value mocks with simple fakes or real helpers. |
| DI container tests appear in every class with no extra value. | Agent limits container tests to composition-sensitive scenarios. |

## Outputs

| Output | Description |
|---|---|
| Unit test set | MSTest v4 tests for notifications, commands, validation, messaging, and navigation |
| Isolation plan | Fakes, mocks, and real helpers chosen per collaborator |
| Integration guidance | Focused DI or real-service checks for composition-sensitive flows |
| Coverage guidance | Smallest existing coverage command for the changed view models |
