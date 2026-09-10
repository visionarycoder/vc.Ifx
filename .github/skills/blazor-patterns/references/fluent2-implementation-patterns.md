---
title: Fluent 2 Blazor Implementation Patterns
doc_type: reference
status: active
last_updated: 2026-08-30
---

# Fluent 2 Blazor Implementation Patterns

Agent uses these patterns when implementing Fluent 2 design in Blazor applications using Microsoft Fluent UI Blazor components.

## Setup

### Install Fluent UI Blazor

```bash
dotnet add package Microsoft.FluentUI.AspNetCore.Components
```

### Register Services (Program.cs)

```csharp
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add Fluent UI services
builder.Services.AddFluentUIComponents();

var app = builder.Build();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();

app.Run();
```

### App.razor Setup

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    
    <!-- Fluent UI styles -->
    <link href="_content/Microsoft.FluentUI.AspNetCore.Components/css/reboot.css" rel="stylesheet" />
    <link href="_content/Microsoft.FluentUI.AspNetCore.Components/css/fluent.css" rel="stylesheet" />
    
    <HeadOutlet />
</head>
<body>
    <Routes />
    
    <!-- Fluent UI scripts -->
    <script src="_content/Microsoft.FluentUI.AspNetCore.Components/Microsoft.FluentUI.AspNetCore.Components.lib.module.js" type="module"></script>
</body>
</html>
```

## Layout with FluentDesignTheme

```razor
@inherits LayoutComponentBase
@using Microsoft.FluentUI.AspNetCore.Components

<FluentDesignTheme @ref="_theme" 
                    Mode="@_themeMode"
                    OfficeColor="@OfficeColor.Default"
                    StorageName="theme-preference" />

<FluentLayout>
    <FluentHeader>
        <FluentStack HorizontalAlignment="HorizontalAlignment.SpaceBetween" 
                     VerticalAlignment="VerticalAlignment.Center">
            <FluentStack Orientation="Orientation.Horizontal" VerticalAlignment="VerticalAlignment.Center">
                <FluentIcon Value="@(new Icons.Regular.Size24.AppFolder())" />
                <FluentLabel Typo="Typography.H3">My App</FluentLabel>
            </FluentStack>
            
            <FluentButton Appearance="Appearance.Lightweight"
                          IconStart="@(new Icons.Regular.Size20.WeatherMoon())"
                          OnClick="ToggleTheme">
                Toggle Theme
            </FluentButton>
        </FluentStack>
    </FluentHeader>
    
    <FluentBodyContent>
        <FluentStack Orientation="Orientation.Horizontal" Class="main-container">
            <FluentNavMenu Width="250px" Collapsible="true" Title="Navigation">
                <FluentNavLink Href="/" Match="NavLinkMatch.All" Icon="@(new Icons.Regular.Size20.Home())">
                    Home
                </FluentNavLink>
                <FluentNavLink Href="/data" Icon="@(new Icons.Regular.Size20.DataBarVertical())">
                    Data
                </FluentNavLink>
                <FluentNavLink Href="/settings" Icon="@(new Icons.Regular.Size20.Settings())">
                    Settings
                </FluentNavLink>
            </FluentNavMenu>
            
            <FluentMainLayout>
                @Body
            </FluentMainLayout>
        </FluentStack>
    </FluentBodyContent>
</FluentLayout>

@code {
    private FluentDesignTheme? _theme;
    private DesignThemeModes _themeMode = DesignThemeModes.System;

    private void ToggleTheme()
    {
        _themeMode = _themeMode switch
        {
            DesignThemeModes.Light => DesignThemeModes.Dark,
            DesignThemeModes.Dark => DesignThemeModes.System,
            _ => DesignThemeModes.Light
        };
    }
}
```

```css
/* wwwroot/css/app.css */
.main-container {
    height: calc(100vh - 50px);
}
```

## Typography

Agent uses Fluent UI Blazor typography components.

```razor
<!-- Display / Hero text -->
<FluentLabel Typo="Typography.H1" Weight="FontWeight.Bold">
    Welcome to My App
</FluentLabel>

<!-- Page title -->
<FluentLabel Typo="Typography.H2" Weight="FontWeight.Semibold">
    Dashboard
</FluentLabel>

<!-- Section headers -->
<FluentLabel Typo="Typography.H4" Weight="FontWeight.Semibold">
    Recent Activity
</FluentLabel>

<!-- Body text -->
<FluentLabel Typo="Typography.Body">
    This is body text with standard formatting.
</FluentLabel>

<!-- Caption text -->
<FluentLabel Typo="Typography.Caption" Color="Color.Neutral">
    Last updated: 2 hours ago
</FluentLabel>
```

### Typography Scale

| Typography | HTML Tag | Font Size | Weight | Use Case |
|------------|----------|-----------|--------|----------|
| H1 | h1 | 32px | Bold | Display, hero text |
| H2 | h2 | 24px | Semibold | Page titles |
| H3 | h3 | 20px | Semibold | Section titles |
| H4 | h4 | 16px | Semibold | Subsection headers |
| Body | p | 14px | Regular | Body text |
| Caption | span | 12px | Regular | Metadata, captions |

## Color and Theming

Fluent UI Blazor handles theming automatically through `FluentDesignTheme`.

```razor
<!-- Theme-aware components automatically adapt -->
<FluentCard>
    <FluentLabel>This card adapts to light/dark theme</FluentLabel>
</FluentCard>

<!-- Semantic colors -->
<FluentLabel Color="Color.Accent">Accent text</FluentLabel>
<FluentLabel Color="Color.Neutral">Neutral text</FluentLabel>
<FluentLabel Color="Color.Error">Error text</FluentLabel>
<FluentLabel Color="Color.Success">Success text</FluentLabel>
<FluentLabel Color="Color.Warning">Warning text</FluentLabel>
```

## Buttons

```razor
<!-- Primary action -->
<FluentButton Appearance="Appearance.Accent" OnClick="HandleSave">
    Save Changes
</FluentButton>

<!-- Standard button -->
<FluentButton Appearance="Appearance.Neutral" OnClick="HandleCancel">
    Cancel
</FluentButton>

<!-- Subtle button -->
<FluentButton Appearance="Appearance.Lightweight" OnClick="HandleHelp">
    Learn More
</FluentButton>

<!-- Icon button -->
<FluentButton Appearance="Appearance.Lightweight" 
              IconStart="@(new Icons.Regular.Size20.Delete())">
    Delete
</FluentButton>

<!-- Loading state -->
<FluentButton Appearance="Appearance.Accent" 
              Loading="@_isSaving"
              OnClick="HandleSave">
    @(_isSaving ? "Saving..." : "Save")
</FluentButton>

@code {
    private bool _isSaving;
    
    private async Task HandleSave()
    {
        _isSaving = true;
        await Task.Delay(2000); // Simulate save
        _isSaving = false;
    }
}
```

## Cards

```razor
<FluentCard Width="400px" Height="auto">
    <FluentStack Orientation="Orientation.Vertical" VerticalGap="12">
        <FluentLabel Typo="Typography.H4" Weight="FontWeight.Semibold">
            Card Title
        </FluentLabel>
        
        <FluentLabel Typo="Typography.Body">
            Card content with automatic theme support and Fluent styling.
        </FluentLabel>
        
        <FluentDivider />
        
        <FluentStack Orientation="Orientation.Horizontal" HorizontalGap="8">
            <FluentButton Appearance="Appearance.Accent">Action</FluentButton>
            <FluentButton Appearance="Appearance.Neutral">Cancel</FluentButton>
        </FluentStack>
    </FluentStack>
</FluentCard>
```

## Forms

```razor
<EditForm Model="@_formModel" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    
    <FluentStack Orientation="Orientation.Vertical" VerticalGap="16">
        <!-- Text input -->
        <FluentTextField @bind-Value="_formModel.Name"
                         Label="Name"
                         Placeholder="Enter your name"
                         Required />
        
        <!-- Email input -->
        <FluentTextField @bind-Value="_formModel.Email"
                         Label="Email"
                         Placeholder="Enter your email"
                         TextFieldType="TextFieldType.Email"
                         Required />
        
        <!-- Select dropdown -->
        <FluentSelect @bind-Value="_formModel.Category"
                      Label="Category"
                      Items="@_categories"
                      OptionText="@(c => c.Name)"
                      OptionValue="@(c => c.Id)" />
        
        <!-- Checkbox -->
        <FluentCheckbox @bind-Value="_formModel.AcceptTerms"
                        Label="I accept the terms and conditions"
                        Required />
        
        <!-- Switch -->
        <FluentSwitch @bind-Value="_formModel.EnableNotifications"
                      Label="Enable notifications" />
        
        <!-- Date picker -->
        <FluentDatePicker @bind-Value="_formModel.StartDate"
                          Label="Start Date" />
        
        <!-- Number input -->
        <FluentNumberField @bind-Value="_formModel.Quantity"
                           Label="Quantity"
                           Min="1"
                           Max="100"
                           Step="1" />
        
        <!-- Validation summary -->
        <FluentValidationSummary />
        
        <!-- Submit button -->
        <FluentButton Appearance="Appearance.Accent" Type="ButtonType.Submit">
            Submit
        </FluentButton>
    </FluentStack>
</EditForm>

@code {
    private FormModel _formModel = new();
    private List<Category> _categories = new();
    
    protected override void OnInitialized()
    {
        _categories = new List<Category>
        {
            new() { Id = "1", Name = "Category 1" },
            new() { Id = "2", Name = "Category 2" }
        };
    }
    
    private void HandleSubmit()
    {
        // Handle form submission
    }
    
    public class FormModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string Category { get; set; } = string.Empty;
        
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms")]
        public bool AcceptTerms { get; set; }
        
        public bool EnableNotifications { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        [Range(1, 100)]
        public int Quantity { get; set; } = 1;
    }
    
    public class Category
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
```

## Data Display

### FluentDataGrid

```razor
<FluentDataGrid Items="@_items" ResizableColumns="true">
    <PropertyColumn Property="@(p => p.Name)" Sortable="true" />
    <PropertyColumn Property="@(p => p.Email)" Sortable="true" />
    <PropertyColumn Property="@(p => p.Status)" Sortable="true" />
    <TemplateColumn Title="Actions">
        <FluentButton Appearance="Appearance.Lightweight"
                      IconStart="@(new Icons.Regular.Size16.Edit())"
                      OnClick="@(() => Edit(context))">
            Edit
        </FluentButton>
        <FluentButton Appearance="Appearance.Lightweight"
                      IconStart="@(new Icons.Regular.Size16.Delete())"
                      OnClick="@(() => Delete(context))">
            Delete
        </FluentButton>
    </TemplateColumn>
</FluentDataGrid>

@code {
    private IQueryable<Person> _items = Array.Empty<Person>().AsQueryable();
    
    protected override void OnInitialized()
    {
        _items = new List<Person>
        {
            new() { Id = 1, Name = "John Doe", Email = "john@example.com", Status = "Active" },
            new() { Id = 2, Name = "Jane Smith", Email = "jane@example.com", Status = "Active" }
        }.AsQueryable();
    }
    
    private void Edit(Person person) { /* ... */ }
    private void Delete(Person person) { /* ... */ }
    
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
```

## Dialogs and Message Boxes

```razor
@inject IDialogService DialogService

<FluentButton Appearance="Appearance.Accent" OnClick="ShowDialog">
    Open Dialog
</FluentButton>

@code {
    private async Task ShowDialog()
    {
        var dialog = await DialogService.ShowDialogAsync<CustomDialog>(
            new DialogParameters()
            {
                Title = "Confirm Action",
                Width = "500px",
                Height = "300px",
                PrimaryAction = "Confirm",
                SecondaryAction = "Cancel"
            });
        
        var result = await dialog.Result;
        
        if (!result.Cancelled)
        {
            // User clicked Confirm
        }
    }
}
```

### Custom Dialog Component

```razor
@* CustomDialog.razor *@
@inherits FluentDialogBase

<FluentDialogBody>
    <FluentStack Orientation="Orientation.Vertical" VerticalGap="16">
        <FluentLabel Typo="Typography.Body">
            Are you sure you want to proceed with this action?
        </FluentLabel>
    </FluentStack>
</FluentDialogBody>

<FluentDialogFooter>
    <FluentButton Appearance="Appearance.Accent" OnClick="ConfirmAsync">
        Confirm
    </FluentButton>
    <FluentButton Appearance="Appearance.Neutral" OnClick="CancelAsync">
        Cancel
    </FluentButton>
</FluentDialogFooter>

@code {
    private async Task ConfirmAsync()
    {
        await CloseAsync(DialogResult.Ok<bool>(true));
    }
    
    private async Task CancelAsync()
    {
        await CloseAsync(DialogResult.Cancel());
    }
}
```

## Message Bar (Status Messages)

```razor
<FluentMessageBar Intent="MessageIntent.Success" Visible="@_showSuccess">
    <FluentIcon Value="@(new Icons.Regular.Size20.CheckmarkCircle())" Slot="start" />
    Operation completed successfully!
</FluentMessageBar>

<FluentMessageBar Intent="MessageIntent.Error" Visible="@_showError">
    <FluentIcon Value="@(new Icons.Regular.Size20.ErrorCircle())" Slot="start" />
    An error occurred. Please try again.
</FluentMessageBar>

<FluentMessageBar Intent="MessageIntent.Warning" Visible="@_showWarning">
    <FluentIcon Value="@(new Icons.Regular.Size20.Warning())" Slot="start" />
    This action cannot be undone.
</FluentMessageBar>

<FluentMessageBar Intent="MessageIntent.Info" Visible="@_showInfo">
    <FluentIcon Value="@(new Icons.Regular.Size20.Info())" Slot="start" />
    New updates are available.
</FluentMessageBar>

@code {
    private bool _showSuccess;
    private bool _showError;
    private bool _showWarning;
    private bool _showInfo;
}
```

## Progress Indicators

```razor
<!-- Indeterminate progress -->
<FluentProgress Visible="@_isLoading" />

<!-- Determinate progress -->
<FluentProgress Value="@_progressValue" Max="100" Visible="true" />

<!-- Progress ring -->
<FluentProgressRing Visible="@_isLoading" />

@code {
    private bool _isLoading;
    private int _progressValue = 45;
}
```

## Icons

Fluent UI Blazor includes comprehensive icon support from Fluent UI System Icons.

```razor
@using Microsoft.FluentUI.AspNetCore.Components.Icons

<!-- Regular icons (default) -->
<FluentIcon Value="@(new Icons.Regular.Size20.Home())" />
<FluentIcon Value="@(new Icons.Regular.Size20.Settings())" />
<FluentIcon Value="@(new Icons.Regular.Size20.Person())" />

<!-- Filled icons -->
<FluentIcon Value="@(new Icons.Filled.Size20.Home())" />
<FluentIcon Value="@(new Icons.Filled.Size20.Star())" />

<!-- Different sizes -->
<FluentIcon Value="@(new Icons.Regular.Size16.Info())" />
<FluentIcon Value="@(new Icons.Regular.Size20.Info())" />
<FluentIcon Value="@(new Icons.Regular.Size24.Info())" />

<!-- With color -->
<FluentIcon Value="@(new Icons.Regular.Size20.CheckmarkCircle())" Color="Color.Success" />
<FluentIcon Value="@(new Icons.Regular.Size20.ErrorCircle())" Color="Color.Error" />
```

## Tabs

```razor
<FluentTabs>
    <FluentTab Label="Home" Icon="@(new Icons.Regular.Size20.Home())">
        <FluentLabel Typo="Typography.Body">Home tab content</FluentLabel>
    </FluentTab>
    
    <FluentTab Label="Profile" Icon="@(new Icons.Regular.Size20.Person())">
        <FluentLabel Typo="Typography.Body">Profile tab content</FluentLabel>
    </FluentTab>
    
    <FluentTab Label="Settings" Icon="@(new Icons.Regular.Size20.Settings())">
        <FluentLabel Typo="Typography.Body">Settings tab content</FluentLabel>
    </FluentTab>
</FluentTabs>
```

## Accordion

```razor
<FluentAccordion>
    <FluentAccordionItem Heading="Section 1" Expanded="true">
        <FluentLabel Typo="Typography.Body">
            Content for section 1
        </FluentLabel>
    </FluentAccordionItem>
    
    <FluentAccordionItem Heading="Section 2">
        <FluentLabel Typo="Typography.Body">
            Content for section 2
        </FluentLabel>
    </FluentAccordionItem>
    
    <FluentAccordionItem Heading="Section 3">
        <FluentLabel Typo="Typography.Body">
            Content for section 3
        </FluentLabel>
    </FluentAccordionItem>
</FluentAccordion>
```

## Tooltips

```razor
<FluentButton Appearance="Appearance.Neutral" Id="tooltip-target">
    Hover for info
</FluentButton>

<FluentTooltip Anchor="tooltip-target" Position="TooltipPosition.Top">
    This button performs an action
</FluentTooltip>
```

## Spacing with FluentStack

```razor
<!-- Vertical stack with consistent spacing -->
<FluentStack Orientation="Orientation.Vertical" VerticalGap="16">
    <FluentCard>Card 1</FluentCard>
    <FluentCard>Card 2</FluentCard>
    <FluentCard>Card 3</FluentCard>
</FluentStack>

<!-- Horizontal stack with spacing -->
<FluentStack Orientation="Orientation.Horizontal" HorizontalGap="12">
    <FluentButton>Button 1</FluentButton>
    <FluentButton>Button 2</FluentButton>
    <FluentButton>Button 3</FluentButton>
</FluentStack>

<!-- Centered content -->
<FluentStack Orientation="Orientation.Vertical"
             VerticalAlignment="VerticalAlignment.Center"
             HorizontalAlignment="HorizontalAlignment.Center"
             Style="height: 100vh;">
    <FluentLabel Typo="Typography.H2">Centered Content</FluentLabel>
</FluentStack>
```

## Responsive Design

```razor
<FluentGrid>
    <FluentGridItem xs="12" sm="6" md="4" lg="3">
        <FluentCard>Responsive card 1</FluentCard>
    </FluentGridItem>
    <FluentGridItem xs="12" sm="6" md="4" lg="3">
        <FluentCard>Responsive card 2</FluentCard>
    </FluentGridItem>
    <FluentGridItem xs="12" sm="6" md="4" lg="3">
        <FluentCard>Responsive card 3</FluentCard>
    </FluentGridItem>
    <FluentGridItem xs="12" sm="6" md="4" lg="3">
        <FluentCard>Responsive card 4</FluentCard>
    </FluentGridItem>
</FluentGrid>
```

## Common Patterns

| Pattern | Blazor Implementation |
|---------|----------------------|
| App shell | FluentLayout + FluentHeader + FluentNavMenu + FluentMainLayout |
| Theme switching | FluentDesignTheme with Mode binding |
| Navigation | FluentNavMenu + FluentNavLink |
| Cards | FluentCard with FluentStack for content |
| Forms | EditForm + Fluent input components + validation |
| Data display | FluentDataGrid with PropertyColumn/TemplateColumn |
| Dialogs | IDialogService.ShowDialogAsync |
| Status messages | FluentMessageBar with Intent |
| Loading states | FluentProgress or FluentProgressRing |
| Icons | FluentIcon with Icons.Regular/Filled |

## Verification Checklist

| Check | Test | Pass |
|-------|------|------|
| Theme support | Toggle theme, inspect colors. | Light/dark themes work, all components adapt. |
| Typography | Review one page. | Clear hierarchy with Fluent typography components. |
| Navigation | Test menu links. | Navigation works, active states highlight correctly. |
| Forms | Submit valid/invalid data. | Validation works, error messages appear. |
| Responsive | Test on mobile, tablet, desktop. | FluentGrid adapts correctly. |
| Icons | Review icon usage. | Icons render at correct sizes, align with text. |
| Accessibility | Tab through UI, use screen reader. | Focus visible, keyboard navigation works, ARIA labels present. |

## References

- [Fluent UI Blazor Documentation](https://www.fluentui-blazor.net/)
- [Fluent UI Blazor GitHub](https://github.com/microsoft/fluentui-blazor)
- [WinUI 3 Documentation](https://learn.microsoft.com/windows/apps/winui/winui3/)
- [WinUI Gallery](https://github.com/microsoft/WinUI-Gallery)
- [Blazor Documentation](https://learn.microsoft.com/aspnet/core/blazor)
- [Fluent 2 Design System](https://fluent2.microsoft.design/)
