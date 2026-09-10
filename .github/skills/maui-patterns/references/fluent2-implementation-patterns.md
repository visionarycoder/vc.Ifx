---
title: Fluent 2 .NET MAUI Implementation Patterns
doc_type: reference
status: active
last_updated: 2026-08-30
---

# Fluent 2 .NET MAUI Implementation Patterns

Agent uses these patterns when implementing Fluent 2 design in .NET MAUI cross-platform applications.

## Platform-Specific Fluent UI

MAUI uses platform-specific Fluent UI libraries for Windows. Agent applies Fluent patterns where platform allows.

### Windows-Specific Fluent UI

```xml
<!-- MauiProgram.cs -->
builder
    .UseMauiApp<App>()
    .ConfigureFonts(fonts =>
    {
        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
#if WINDOWS
        fonts.AddFont("Segoe Fluent Icons.ttf", "FluentIcons");
#endif
    });
```

## Typography

Agent defines shared text styles in App.xaml using Fluent 2 type ramp.

```xml
<Application.Resources>
    <ResourceDictionary>
        <!-- Display / Page Title -->
        <Style x:Key="TitleLarge" TargetType="Label">
            <Setter Property="FontSize" Value="40" />
            <Setter Property="FontAttributes" Value="Bold" />
            <Setter Property="Margin" Value="0,0,0,16" />
        </Style>
        
        <!-- Page Title -->
        <Style x:Key="Title" TargetType="Label">
            <Setter Property="FontSize" Value="28" />
            <Setter Property="FontAttributes" Value="Bold" />
            <Setter Property="Margin" Value="0,0,0,12" />
        </Style>
        
        <!-- Section Headers -->
        <Style x:Key="Subtitle" TargetType="Label">
            <Setter Property="FontSize" Value="20" />
            <Setter Property="FontAttributes" Value="Bold" />
            <Setter Property="Margin" Value="0,0,0,8" />
        </Style>
        
        <!-- Body Text -->
        <Style x:Key="Body" TargetType="Label">
            <Setter Property="FontSize" Value="14" />
            <Setter Property="LineHeight" Value="1.5" />
        </Style>
        
        <!-- Captions -->
        <Style x:Key="Caption" TargetType="Label">
            <Setter Property="FontSize" Value="12" />
            <Setter Property="TextColor" Value="{AppThemeBinding 
                Light={StaticResource Gray600}, 
                Dark={StaticResource Gray400}}" />
        </Style>
    </ResourceDictionary>
</Application.Resources>
```

## Color Tokens

Agent defines Fluent-aligned colors in Resources/Styles/Colors.xaml.

```xml
<ResourceDictionary>
    <!-- Light theme -->
    <Color x:Key="Primary">#0078D4</Color>
    <Color x:Key="PrimaryLight">#106EBE</Color>
    <Color x:Key="PrimaryDark">#005A9E</Color>
    
    <Color x:Key="Gray50">#FAFAFA</Color>
    <Color x:Key="Gray100">#F3F3F3</Color>
    <Color x:Key="Gray200">#E5E5E5</Color>
    <Color x:Key="Gray300">#D1D1D1</Color>
    <Color x:Key="Gray400">#A3A3A3</Color>
    <Color x:Key="Gray500">#737373</Color>
    <Color x:Key="Gray600">#525252</Color>
    <Color x:Key="Gray700">#404040</Color>
    <Color x:Key="Gray800">#262626</Color>
    <Color x:Key="Gray900">#171717</Color>
    
    <!-- Semantic colors -->
    <Color x:Key="TextPrimary">{AppThemeBinding Light={StaticResource Gray900}, Dark={StaticResource Gray50}}</Color>
    <Color x:Key="TextSecondary">{AppThemeBinding Light={StaticResource Gray600}, Dark={StaticResource Gray400}}</Color>
    <Color x:Key="BackgroundPrimary">{AppThemeBinding Light={StaticResource Gray50}, Dark={StaticResource Gray900}}</Color>
    <Color x:Key="BackgroundSecondary">{AppThemeBinding Light=White, Dark={StaticResource Gray800}}</Color>
    <Color x:Key="DividerColor">{AppThemeBinding Light={StaticResource Gray200}, Dark={StaticResource Gray700}}</Color>
</ResourceDictionary>
```

## Spacing System

Agent uses 8px grid spacing with MAUI thickness values.

```xml
<!-- Card with standard padding -->
<Border Padding="16" Margin="0,0,0,16"
        BackgroundColor="{AppThemeBinding Light=White, Dark={StaticResource Gray800}}"
        StrokeThickness="1"
        Stroke="{StaticResource Gray200}"
        StrokeShape="RoundRectangle 8">
    <VerticalStackLayout Spacing="12">
        <Label Text="Card Title" Style="{StaticResource Subtitle}" />
        <Label Text="Card content..." Style="{StaticResource Body}" />
    </VerticalStackLayout>
</Border>
```

### Spacing Values

| Use Case | Value |
|----------|-------|
| Card padding | 16 (standard), 12 (compact) |
| Card margin | 0,0,0,16 |
| Stack spacing | 12-16 (content), 8 (tight) |
| Button spacing | 8-12 |
| Section spacing | 24 |
| Page padding | 16 (mobile), 24 (tablet/desktop) |

## Button Patterns

```xml
<!-- Primary button -->
<Button Text="Save Changes"
        BackgroundColor="{StaticResource Primary}"
        TextColor="White"
        Padding="24,12"
        CornerRadius="4"
        Command="{Binding SaveCommand}" />

<!-- Secondary button -->
<Button Text="Cancel"
        BackgroundColor="{AppThemeBinding Light={StaticResource Gray100}, Dark={StaticResource Gray700}}"
        TextColor="{StaticResource TextPrimary}"
        Padding="24,12"
        CornerRadius="4"
        Command="{Binding CancelCommand}" />

<!-- Text button -->
<Button Text="Learn More"
        BackgroundColor="Transparent"
        TextColor="{StaticResource Primary}"
        Padding="12,8"
        Command="{Binding LearnMoreCommand}" />
```

## Shell Navigation

Agent uses MAUI Shell with Fluent-aligned visuals.

```xml
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:local="clr-namespace:MyApp"
       x:Class="MyApp.AppShell"
       FlyoutBackgroundColor="{AppThemeBinding Light=White, Dark={StaticResource Gray900}}">
    
    <Shell.FlyoutHeaderTemplate>
        <DataTemplate>
            <Grid BackgroundColor="{StaticResource Primary}" 
                  Padding="16"
                  HeightRequest="120">
                <Label Text="My App" 
                       TextColor="White"
                       FontSize="24"
                       FontAttributes="Bold"
                       VerticalOptions="Center" />
            </Grid>
        </DataTemplate>
    </Shell.FlyoutHeaderTemplate>
    
    <TabBar>
        <ShellContent Title="Home"
                      Icon="home.png"
                      ContentTemplate="{DataTemplate local:HomePage}" />
        <ShellContent Title="Data"
                      Icon="library.png"
                      ContentTemplate="{DataTemplate local:DataPage}" />
        <ShellContent Title="Settings"
                      Icon="settings.png"
                      ContentTemplate="{DataTemplate local:SettingsPage}" />
    </TabBar>
</Shell>
```

## Cards and Elevation

```xml
<!-- Card with shadow (elevation) -->
<Border Padding="16"
        BackgroundColor="{AppThemeBinding Light=White, Dark={StaticResource Gray800}}"
        StrokeThickness="0"
        StrokeShape="RoundRectangle 8">
    <Border.Shadow>
        <Shadow Brush="{StaticResource Gray900}"
                Opacity="0.1"
                Radius="8"
                Offset="0,2" />
    </Border.Shadow>
    
    <VerticalStackLayout Spacing="8">
        <Label Text="Card Title" Style="{StaticResource Subtitle}" />
        <Label Text="Card description..." Style="{StaticResource Body}" />
    </VerticalStackLayout>
</Border>
```

## Forms and Input

```xml
<VerticalStackLayout Spacing="16" Padding="16">
    <!-- Text input -->
    <VerticalStackLayout Spacing="4">
        <Label Text="Email" Style="{StaticResource Body}" />
        <Entry Placeholder="Enter email"
               Text="{Binding Email}"
               Keyboard="Email" />
    </VerticalStackLayout>
    
    <!-- Picker -->
    <VerticalStackLayout Spacing="4">
        <Label Text="Category" Style="{StaticResource Body}" />
        <Picker Title="Select category"
                ItemsSource="{Binding Categories}"
                SelectedItem="{Binding SelectedCategory}" />
    </VerticalStackLayout>
    
    <!-- Switch -->
    <HorizontalStackLayout Spacing="12">
        <Switch IsToggled="{Binding IsEnabled}" />
        <Label Text="Enable notifications"
               VerticalOptions="Center"
               Style="{StaticResource Body}" />
    </HorizontalStackLayout>
</VerticalStackLayout>
```

## CollectionView

```xml
<CollectionView ItemsSource="{Binding Items}"
                SelectionMode="Single"
                SelectedItem="{Binding SelectedItem}">
    <CollectionView.ItemTemplate>
        <DataTemplate x:DataType="local:Item">
            <Border Padding="16" Margin="0,0,0,8"
                    BackgroundColor="{AppThemeBinding Light=White, Dark={StaticResource Gray800}}"
                    StrokeThickness="1"
                    Stroke="{StaticResource Gray200}"
                    StrokeShape="RoundRectangle 8">
                <Grid ColumnSpacing="12">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />
                        <ColumnDefinition Width="*" />
                    </Grid.ColumnDefinitions>
                    
                    <Image Source="{Binding Icon}"
                           WidthRequest="40"
                           HeightRequest="40" />
                    
                    <VerticalStackLayout Grid.Column="1" Spacing="4">
                        <Label Text="{Binding Title}" 
                               Style="{StaticResource Subtitle}"
                               FontSize="16" />
                        <Label Text="{Binding Description}" 
                               Style="{StaticResource Caption}" />
                    </VerticalStackLayout>
                </Grid>
            </Border>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

## Loading and Empty States

```xml
<!-- Activity indicator -->
<ActivityIndicator IsRunning="{Binding IsBusy}"
                   IsVisible="{Binding IsBusy}"
                   Color="{StaticResource Primary}"
                   WidthRequest="40"
                   HeightRequest="40" />

<!-- Empty state -->
<VerticalStackLayout Spacing="16"
                     HorizontalOptions="Center"
                     VerticalOptions="Center"
                     IsVisible="{Binding IsEmpty}">
    <Image Source="empty_state.png"
           WidthRequest="120"
           HeightRequest="120"
           Opacity="0.5" />
    <Label Text="No items yet"
           Style="{StaticResource Subtitle}"
           HorizontalOptions="Center" />
    <Label Text="Get started by adding your first item"
           Style="{StaticResource Body}"
           HorizontalOptions="Center"
           TextColor="{StaticResource TextSecondary}" />
    <Button Text="Add Item"
            Command="{Binding AddItemCommand}"
            HorizontalOptions="Center" />
</VerticalStackLayout>
```

## Platform-Specific Handlers

Agent uses handlers for Windows-specific Fluent UI integration.

```csharp
// MauiProgram.cs
#if WINDOWS
builder
    .ConfigureMauiHandlers(handlers =>
    {
        handlers.AddHandler<Button, FluentButtonHandler>();
        handlers.AddHandler<Entry, FluentEntryHandler>();
    });
#endif
```

```csharp
// Platforms/Windows/Handlers/FluentButtonHandler.cs
#if WINDOWS
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

public class FluentButtonHandler : ButtonHandler
{
    protected override Button CreatePlatformView()
    {
        var button = base.CreatePlatformView();
        
        // Apply Fluent styling
        button.CornerRadius = new Microsoft.UI.Xaml.CornerRadius(4);
        
        return button;
    }
}
#endif
```

## Responsive Layout

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="{OnIdiom Phone=*, Tablet=300, Desktop=300}" />
        <ColumnDefinition Width="{OnIdiom Phone=0, Tablet=*, Desktop=*}" />
    </Grid.ColumnDefinitions>
    
    <!-- Sidebar (always visible on tablet/desktop) -->
    <VerticalStackLayout Grid.Column="0" Padding="16" Spacing="8">
        <Label Text="Navigation" Style="{StaticResource Subtitle}" />
        <!-- Nav items -->
    </VerticalStackLayout>
    
    <!-- Main content -->
    <ScrollView Grid.Column="1" Padding="16">
        <VerticalStackLayout Spacing="16">
            <!-- Page content -->
        </VerticalStackLayout>
    </ScrollView>
</Grid>
```

## Theme Support

```csharp
// Enable theme switching
Application.Current.UserAppTheme = AppTheme.Light; // or Dark
```

```xml
<!-- Theme-aware colors -->
<Label TextColor="{AppThemeBinding Light={StaticResource Gray900}, Dark={StaticResource Gray50}}" />
<BoxView BackgroundColor="{AppThemeBinding Light=White, Dark={StaticResource Gray900}}" />
```

## Accessibility

```xml
<!-- Semantic descriptions -->
<Button Text="Delete"
        SemanticProperties.Description="Delete this item"
        SemanticProperties.Hint="Double tap to delete" />

<!-- Headings -->
<Label Text="Section Title"
       SemanticProperties.HeadingLevel="Level1"
       Style="{StaticResource Title}" />

<!-- Screen reader -->
<Image Source="icon.png"
       SemanticProperties.Description="App logo" />
```

## Common Patterns

| Pattern | MAUI Implementation |
|---------|---------------------|
| App shell | Shell with FlyoutHeader + TabBar/FlyoutItem |
| Navigation | Shell.GoToAsync with route-based navigation |
| Cards | Border with rounded corners + Shadow |
| Lists | CollectionView with ItemTemplate |
| Forms | VerticalStackLayout with label + control pairs |
| Loading | ActivityIndicator with IsRunning binding |
| Empty state | ContentView with IsVisible binding |
| Responsive | OnIdiom/OnPlatform for layout adaptation |

## Verification Checklist

| Check | Test | Pass |
|-------|------|------|
| Typography | Review one page. | Clear hierarchy with defined text styles. |
| Spacing | Inspect cards and layout. | 8px-aligned spacing, consistent padding. |
| Colors | Switch between light/dark themes. | All colors use theme resources, readable in both themes. |
| Navigation | Test Shell navigation. | Routes work, back button functions correctly. |
| Responsive | Test on phone, tablet, desktop. | Layout adapts appropriately. |
| Platform | Run on Windows. | Windows-specific handlers apply Fluent styling. |
| Accessibility | Enable screen reader. | Semantic properties provide context. |

## References

- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui)
- [MAUI Graphics](https://learn.microsoft.com/dotnet/maui/user-interface/graphics)
- [MAUI Shell](https://learn.microsoft.com/dotnet/maui/fundamentals/shell)
- [MAUI Community Toolkit](https://learn.microsoft.com/dotnet/communitytoolkit/maui)
- [Platform Integration](https://learn.microsoft.com/dotnet/maui/platform-integration)
