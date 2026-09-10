---
title: Fluent 2 WPF Implementation Patterns
doc_type: reference
status: active
last_updated: 2026-08-30
---

# Fluent 2 WPF Implementation Patterns

Agent uses these patterns when implementing or fixing Fluent 2 design in WPF applications using WPF-UI or ModernWpf.

## Window Chrome Pattern

### Problem
TitleBar nested inside NavigationView causes missing window controls (minimize, maximize, close).

### Solution
Agent positions TitleBar at window level in a separate grid row above NavigationView.

```xaml
<!-- ✓ Correct -->
<ui:FluentWindow>
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>
        
        <ui:TitleBar Grid.Row="0"
                     ShowMinimize="True"
                     ShowMaximize="True"
                     ShowClose="True" />
        
        <ui:NavigationView Grid.Row="1" />
    </Grid>
</ui:FluentWindow>

<!-- ✗ Avoid -->
<ui:FluentWindow>
    <Grid>
        <ui:NavigationView>
            <ui:NavigationView.TitleBar>  <!-- Wrong location -->
                <ui:TitleBar ... />
            </ui:NavigationView.TitleBar>
        </ui:NavigationView>
    </Grid>
</ui:FluentWindow>
```

## Typography Styles

Agent defines shared typography styles in `App.xaml` using the Fluent 2 type ramp.

```xaml
<Application.Resources>
    <!-- Page titles -->
    <Style x:Key="FluentPageTitleTextBlockStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="40" />
        <Setter Property="FontWeight" Value="SemiBold" />
        <Setter Property="Margin" Value="0,0,0,20" />
    </Style>

    <!-- Section headers -->
    <Style x:Key="FluentSectionHeaderTextBlockStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="20" />
        <Setter Property="FontWeight" Value="SemiBold" />
        <Setter Property="Margin" Value="0,0,0,12" />
    </Style>

    <!-- Labels -->
    <Style x:Key="FluentLabelTextBlockStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="14" />
        <Setter Property="FontWeight" Value="SemiBold" />
        <Setter Property="Margin" Value="0,0,0,4" />
    </Style>

    <!-- Body text -->
    <Style x:Key="FluentBodyTextBlockStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="14" />
        <Setter Property="FontWeight" Value="Normal" />
    </Style>

    <!-- Captions / metadata -->
    <Style x:Key="FluentCaptionTextBlockStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="12" />
        <Setter Property="FontWeight" Value="Normal" />
        <Setter Property="Foreground" Value="{DynamicResource TextFillColorSecondaryBrush}" />
    </Style>

    <!-- Statistics / large values -->
    <Style x:Key="FluentStatValueTextBlockStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="28" />
        <Setter Property="FontWeight" Value="SemiBold" />
    </Style>
</Application.Resources>
```

## Fluent 2 Type Ramp

| Style | Font Size | Weight | Use Case |
|-------|-----------|--------|----------|
| Page Title | 40px | SemiBold | Page headers |
| Section Header | 20px | SemiBold | Section headers |
| Label | 14px | SemiBold | Form labels |
| Body | 14px | Normal | Body text |
| Caption | 12px | Normal | Metadata, secondary text |
| Stat Value | 28px | SemiBold | Numeric displays |

## Spacing System

Agent applies 8px-grid spacing for consistent rhythm.

### Card Padding and Margins

```xaml
<!-- Standard card -->
<ui:Card Padding="24" Margin="0,0,0,24">
    <StackPanel Spacing="20">
        <!-- Content -->
    </StackPanel>
</ui:Card>

<!-- Compact card -->
<ui:Card Padding="16" Margin="0,0,0,16">
    <!-- Content -->
</ui:Card>
```

### Spacing Values

| Use Case | Value |
|----------|-------|
| Card padding | 24px (standard), 16px (compact) |
| Card margin | 0,0,0,24 (standard), 0,0,0,16 (compact) |
| Section spacing | 20-24px |
| Button spacing | 12px |
| Label-to-control spacing | 4-8px |
| Group spacing | 16px |

## Button Styles

Agent defines shared button styles for consistent sizing and padding.

```xaml
<!-- Standard button -->
<Style x:Key="FluentStandardButtonStyle" TargetType="Button">
    <Setter Property="Padding" Value="20,10" />
    <Setter Property="MinHeight" Value="40" />
</Style>

<!-- Large button -->
<Style x:Key="FluentLargeButtonStyle" TargetType="Button">
    <Setter Property="Padding" Value="24,12" />
    <Setter Property="MinHeight" Value="48" />
</Style>

<!-- Icon button -->
<Style x:Key="FluentIconButtonStyle" TargetType="Button">
    <Setter Property="Padding" Value="8" />
    <Setter Property="MinWidth" Value="40" />
    <Setter Property="MinHeight" Value="40" />
</Style>
```

### Usage

```xaml
<ui:Button Style="{StaticResource FluentStandardButtonStyle}"
           Content="Discover Packages" />

<ui:Button Style="{StaticResource FluentLargeButtonStyle}"
           Appearance="Primary"
           Content="Import Selected" />
```

## DataGrid Styling

Agent applies touch-friendly row heights and alternating backgrounds.

```xaml
<ui:DataGrid ItemsSource="{Binding Items}"
             RowHeight="48"
             AlternatingRowBackground="{DynamicResource SubtleFillColorSecondaryBrush}"
             HeadersVisibility="Column"
             AutoGenerateColumns="False">
    <ui:DataGrid.Columns>
        <!-- Columns -->
    </ui:DataGrid.Columns>
</ui:DataGrid>
```

### DataGrid Values

| Property | Value | Reason |
|----------|-------|--------|
| RowHeight | 48px | Touch-friendly minimum |
| AlternatingRowBackground | SubtleFillColorSecondaryBrush | Readability |
| HeadersVisibility | Column | Standard pattern |

## Status and Feedback

### InfoBar for Errors

```xaml
<ui:InfoBar IsOpen="{Binding HasError, Mode=TwoWay}"
            Severity="Error"
            Title="Import error"
            Message="{Binding ErrorMessage}"
            IsClosable="True"
            Margin="0,0,0,20" />
```

### ProgressRing for Operations

```xaml
<ui:ProgressRing IsIndeterminate="True"
                 Visibility="{Binding IsBusy, Converter={StaticResource BooleanToVisibilityConverter}}"
                 Width="40"
                 Height="40" />
```

### ViewModel Support

```csharp
public partial class ViewModel : ObservableObject
{
    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;
}
```

## Color Usage

Agent uses dynamic brushes from WPF-UI, never hard-coded colors.

### Common Brushes

| Brush | Use Case |
|-------|----------|
| `TextFillColorPrimaryBrush` | Primary text |
| `TextFillColorSecondaryBrush` | Secondary text, captions |
| `TextFillColorTertiaryBrush` | Disabled text |
| `SubtleFillColorSecondaryBrush` | Alternating backgrounds |
| `CardBackgroundFillColorDefaultBrush` | Card surfaces |
| `AccentFillColorDefaultBrush` | Accent elements |

```xaml
<!-- ✓ Correct -->
<TextBlock Foreground="{DynamicResource TextFillColorSecondaryBrush}"
           Text="{Binding Caption}" />

<!-- ✗ Avoid -->
<TextBlock Foreground="#666666"
           Text="{Binding Caption}" />
```

## Icon Usage

Agent uses Fluent symbols at 24px for UI chrome and 16px for inline content.

```xaml
<!-- Navigation icon -->
<ui:SymbolIcon Symbol="Home24" />

<!-- Inline content icon -->
<ui:SymbolIcon Symbol="Info16" />

<!-- Button icon -->
<ui:Button>
    <StackPanel Orientation="Horizontal" Spacing="8">
        <ui:SymbolIcon Symbol="Add24" />
        <TextBlock Text="Add Item" />
    </StackPanel>
</ui:Button>
```

## Verification Checklist

| Check | Test | Pass |
|-------|------|------|
| Window chrome | Run app, inspect title bar. | Minimize, maximize, close buttons visible and functional. |
| Typography | Review one page. | Clear hierarchy: 40px title → 20px headers → 14px body → 12px captions. |
| Spacing | Inspect cards and sections. | 24px card padding, 20-24px section spacing, 8px-aligned rhythm. |
| Buttons | Review action groups. | Consistent sizing, 40px minimum height, 20,10 padding. |
| DataGrid | Review grids. | 48px row height, alternating backgrounds. |
| Colors | Inspect in light and dark themes. | Dynamic brushes work, no hard-coded colors. |
| Feedback | Trigger error and busy states. | InfoBar appears for errors, ProgressRing shows for operations. |

## Common Fixes

| Issue | Agent Fix |
|-------|-----------|
| Missing window controls | Agent moves TitleBar to window-level grid row above NavigationView. |
| Inconsistent text sizes | Agent applies shared typography styles from App.xaml. |
| Cramped layout | Agent increases card padding to 24px, section spacing to 20-24px. |
| Small row heights | Agent sets DataGrid RowHeight to 48px. |
| Hard-coded colors | Agent replaces literals with dynamic brushes. |
| Missing error feedback | Agent adds InfoBar with ViewModel HasError and ErrorMessage properties. |
| Missing busy state | Agent adds ProgressRing with ViewModel IsBusy property. |

## References

- [Fluent 2 Design System](https://fluent2.microsoft.design/)
- [WPF-UI Library](https://wpfui.lepo.co/)
- [ModernWpf Library](https://github.com/Kinnara/ModernWpf)
- [Fluent Design System Skill](.github/skills/fluent-design-system/SKILL.md)
