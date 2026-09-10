---
title: Fluent 2 WinUI 3 Implementation Patterns
doc_type: reference
status: active
last_updated: 2026-08-30
---

# Fluent 2 WinUI 3 Implementation Patterns

Agent uses these patterns when implementing Fluent 2 design in WinUI 3 applications with Windows App SDK.

## App Shell Pattern

Agent uses the modern Windows 11 app shell with Mica backdrop, custom title bar, and NavigationView.

```xml
<Window
    x:Class="MyApp.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Mica backdrop -->
    <Window.SystemBackdrop>
        <MicaBackdrop Kind="Base" />
    </Window.SystemBackdrop>
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>
        
        <!-- Custom title bar -->
        <Grid x:Name="AppTitleBar" Grid.Row="0" Height="48">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto" />
                <ColumnDefinition Width="*" />
            </Grid.ColumnDefinitions>
            
            <Image Source="Assets/AppIcon.png" 
                   Width="16" Height="16" 
                   Margin="16,0,0,0" />
            <TextBlock Grid.Column="1" 
                       Text="My App" 
                       Margin="12,0,0,0"
                       VerticalAlignment="Center"
                       Style="{StaticResource CaptionTextBlockStyle}" />
        </Grid>
        
        <!-- Main navigation -->
        <NavigationView Grid.Row="1"
                        PaneDisplayMode="Left"
                        IsBackButtonVisible="Auto"
                        IsSettingsVisible="True">
            <NavigationView.MenuItems>
                <NavigationViewItem Content="Home" Icon="Home" Tag="home" />
                <NavigationViewItem Content="Data" Icon="Library" Tag="data" />
            </NavigationView.MenuItems>
            
            <Frame x:Name="ContentFrame" />
        </NavigationView>
    </Grid>
</Window>
```

### Code-Behind Setup

```csharp
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Set custom title bar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
    }
}
```

## Mica Material Usage

Agent applies Mica for window-level backgrounds, never sets solid backgrounds that cover it.

```xml
<!-- ✓ Correct - Transparent to show Mica -->
<Page Background="Transparent">
    <Grid>
        <!-- Content -->
    </Grid>
</Page>

<!-- ✗ Avoid - Solid background covers Mica -->
<Page Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
    <Grid>
        <!-- Content -->
    </Grid>
</Page>
```

## Acrylic Material

Agent uses Acrylic for transient surfaces (menus, flyouts, teaching tips).

```xml
<Border Background="{ThemeResource AcrylicBackgroundFillColorDefaultBrush}"
        CornerRadius="8"
        Padding="16">
    <StackPanel Spacing="12">
        <TextBlock Text="Quick Actions" Style="{StaticResource SubtitleTextBlockStyle}" />
        <Button Content="Action 1" />
        <Button Content="Action 2" />
    </StackPanel>
</Border>
```

## Typography

Agent uses WinUI 3 text styles that match Fluent 2 type ramp.

```xml
<!-- Display text -->
<TextBlock Text="Welcome" Style="{StaticResource TitleLargeTextBlockStyle}" />

<!-- Page title -->
<TextBlock Text="Dashboard" Style="{StaticResource TitleTextBlockStyle}" />

<!-- Section headers -->
<TextBlock Text="Recent Items" Style="{StaticResource SubtitleTextBlockStyle}" />

<!-- Body text -->
<TextBlock Text="Description..." Style="{StaticResource BodyTextBlockStyle}" />

<!-- Captions -->
<TextBlock Text="Last updated: 2 hours ago" 
           Style="{StaticResource CaptionTextBlockStyle}"
           Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
```

### Typography Scale

| Style | Font Size | Weight | Use Case |
|-------|-----------|--------|----------|
| TitleLargeTextBlockStyle | 40px | SemiBold | Display text |
| TitleTextBlockStyle | 28px | SemiBold | Page titles |
| SubtitleTextBlockStyle | 20px | SemiBold | Section headers |
| BodyStrongTextBlockStyle | 14px | SemiBold | Emphasized body |
| BodyTextBlockStyle | 14px | Regular | Body text |
| CaptionTextBlockStyle | 12px | Regular | Metadata, captions |

## Spacing and Layout

Agent uses 8px grid spacing with WinUI 3 spacing properties.

```xml
<!-- Card with standard padding -->
<Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
        CornerRadius="8"
        Padding="16"
        Margin="0,0,0,16">
    <StackPanel Spacing="12">
        <TextBlock Text="Card Title" Style="{StaticResource SubtitleTextBlockStyle}" />
        <TextBlock Text="Card content..." Style="{StaticResource BodyTextBlockStyle}" />
    </StackPanel>
</Border>

<!-- Grid with consistent spacing -->
<Grid RowSpacing="16" ColumnSpacing="16">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>
    <!-- Content -->
</Grid>
```

### Spacing Values

| Use Case | Value |
|----------|-------|
| Card padding | 16px (standard), 12px (compact) |
| Card margin | 0,0,0,16 |
| Stack/Grid spacing | 12-16px (content), 8px (tight) |
| Button spacing | 8-12px |
| Section spacing | 24px |

## Controls and Components

### InfoBar for Status

```xml
<InfoBar x:Name="StatusBar"
         IsOpen="False"
         Severity="Informational"
         Title="Info"
         Message="Operation completed successfully."
         IsClosable="True"
         Margin="0,0,0,16" />
```

### ProgressRing for Loading

```xml
<ProgressRing IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
              Width="40"
              Height="40" />
```

### Cards with Elevation

```xml
<Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
        BorderThickness="1"
        CornerRadius="8"
        Padding="16"
        Shadow="{StaticResource CardShadow}">
    <!-- Card content -->
</Border>
```

## Color Tokens

Agent uses theme-aware color resources, never hard-coded values.

```xml
<!-- ✓ Correct - Theme resources -->
<TextBlock Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
<Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}" />
<Border BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}" />

<!-- ✗ Avoid - Hard-coded colors -->
<TextBlock Foreground="#000000" />
<Border Background="#FFFFFF" />
```

### Common Color Tokens

| Token | Use Case |
|-------|----------|
| TextFillColorPrimaryBrush | Primary text |
| TextFillColorSecondaryBrush | Secondary text, captions |
| TextFillColorDisabledBrush | Disabled text |
| CardBackgroundFillColorDefaultBrush | Card surfaces |
| SubtleFillColorSecondaryBrush | Subtle backgrounds |
| DividerStrokeColorDefaultBrush | Dividers, borders |
| AccentFillColorDefaultBrush | Accent elements |

## Button Patterns

```xml
<!-- Primary action -->
<Button Content="Save Changes" 
        Style="{StaticResource AccentButtonStyle}"
        Padding="24,10" />

<!-- Standard action -->
<Button Content="Cancel" 
        Padding="24,10" />

<!-- Icon button -->
<Button Width="40" Height="40" Padding="8">
    <FontIcon Glyph="&#xE74D;" />
</Button>
```

## Data Display

### ListView with Selection

```xml
<ListView ItemsSource="{x:Bind ViewModel.Items}"
          SelectionMode="Single"
          SelectedItem="{x:Bind ViewModel.SelectedItem, Mode=TwoWay}">
    <ListView.ItemTemplate>
        <DataTemplate x:DataType="local:Item">
            <Grid Padding="12" ColumnSpacing="12">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto" />
                    <ColumnDefinition Width="*" />
                </Grid.ColumnDefinitions>
                
                <FontIcon Glyph="{x:Bind Icon}" />
                <StackPanel Grid.Column="1" Spacing="4">
                    <TextBlock Text="{x:Bind Title}" 
                               Style="{StaticResource BodyStrongTextBlockStyle}" />
                    <TextBlock Text="{x:Bind Description}" 
                               Style="{StaticResource CaptionTextBlockStyle}"
                               Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                </StackPanel>
            </Grid>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

### DataGrid (Community Toolkit)

```xml
<controls:DataGrid ItemsSource="{x:Bind ViewModel.Items}"
                   AutoGenerateColumns="False"
                   GridLinesVisibility="None"
                   HeadersVisibility="Column"
                   RowHeight="56"
                   AlternatingRowBackground="{ThemeResource SubtleFillColorSecondaryBrush}">
    <controls:DataGrid.Columns>
        <controls:DataGridTextColumn Header="Name" Binding="{Binding Name}" />
        <controls:DataGridTextColumn Header="Status" Binding="{Binding Status}" />
    </controls:DataGrid.Columns>
</controls:DataGrid>
```

## Icons

Agent uses Segoe Fluent Icons font with proper sizing.

```xml
<!-- Navigation icons (20px) -->
<FontIcon Glyph="&#xE80F;" FontSize="20" />

<!-- Content icons (16px) -->
<FontIcon Glyph="&#xE946;" FontSize="16" />

<!-- Or use SymbolIcon -->
<SymbolIcon Symbol="Home" />
```

## Motion and Transitions

```xml
<!-- Page transitions -->
<Frame.ContentTransitions>
    <TransitionCollection>
        <NavigationThemeTransition>
            <NavigationThemeTransition.DefaultNavigationTransitionInfo>
                <EntranceNavigationTransitionInfo />
            </NavigationThemeTransition.DefaultNavigationTransitionInfo>
        </NavigationThemeTransition>
    </TransitionCollection>
</Frame.ContentTransitions>

<!-- Staggered list animations -->
<ItemsControl.ItemContainerTransitions>
    <TransitionCollection>
        <AddDeleteThemeTransition />
        <ReorderThemeTransition />
    </TransitionCollection>
</ItemsControl.ItemContainerTransitions>
```

## Accessibility

```xml
<!-- Proper labeling -->
<Button AutomationProperties.Name="Close window">
    <FontIcon Glyph="&#xE711;" />
</Button>

<!-- Keyboard navigation -->
<ListView IsTabStop="True"
          KeyboardAcceleratorPlacementMode="Auto">
    <!-- Items -->
</ListView>

<!-- Focus visual -->
<Button UseSystemFocusVisuals="True" />
```

## Common Patterns

| Pattern | WinUI 3 Implementation |
|---------|------------------------|
| Window chrome | Mica backdrop + custom title bar + ExtendsContentIntoTitleBar |
| Navigation | NavigationView (Left/LeftCompact/Top pane modes) |
| Status messages | InfoBar with Severity (Info/Success/Warning/Error) |
| Loading state | ProgressRing with IsActive binding |
| Cards | Border with CardBackground + CornerRadius="8" + Shadow |
| Forms | StackPanel with Spacing="12", label + control pairs |
| Data display | ListView (simple) or DataGrid (tabular) |
| Dialogs | ContentDialog with PrimaryButton + SecondaryButton |

## Verification Checklist

| Check | Test | Pass |
|-------|------|------|
| Mica visibility | Run app, inspect window background. | Mica material visible, translucent. |
| Title bar | Inspect window chrome. | Custom title bar with app icon/title, drag area works. |
| Typography | Review one page. | Clear hierarchy with WinUI 3 text styles. |
| Spacing | Inspect cards and layout. | 8px-aligned spacing, 16px card padding. |
| Colors | Switch between light/dark themes. | All colors use theme resources, no hard-coded values. |
| Icons | Review icon usage. | Segoe Fluent Icons at 16px (content) or 20px (navigation). |
| Accessibility | Tab through UI. | Focus visible, keyboard navigation works. |

## References

- [WinUI 3 Documentation](https://learn.microsoft.com/windows/apps/winui/winui3)
- [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk)
- [Mica Material](https://learn.microsoft.com/windows/apps/design/style/mica)
- [Acrylic Material](https://learn.microsoft.com/windows/apps/design/style/acrylic)
- [WinUI 3 Gallery](https://github.com/microsoft/WinUI-Gallery)
