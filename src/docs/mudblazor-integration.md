# Integrating MudShell into an Existing MudBlazor App

This guide assumes you already have a MudBlazor application and want to layer MudShell on top of it without rebuilding the app from scratch.

## What changes and what stays the same

Keep:

- your existing MudBlazor components,
- your existing pages,
- your existing data and application services.

Change:

- the root layout,
- the navigation structure,
- page composition conventions,
- theme/background orchestration if you want the full sample experience.

## 1. Install MudShell

Use a package reference or project reference.

```xml
<PackageReference Include="MudShell" Version="0.*" />
```

Or:

```xml
<ProjectReference Include="..\MudShell\MudShell.csproj" />
```

## 2. Register services

In `Program.cs`:

```csharp
builder.Services.AddMudShell();
```

Do **not** call `AddMudServices()` as well. `AddMudShell()` already wires MudBlazor services.

If you want sample-style theme/background orchestration, also register a scoped theme state service:

```csharp
builder.Services.AddScoped<ThemeState>();
```

## 3. Add imports

In `_Imports.razor`:

```razor
@using MudShell
@using MudShell.Components.AppShell
@using MudShell.Components.BottomNav
@using MudShell.Components.ChatBar
@using MudShell.Components.DocumentCard
@using MudShell.Components.FilterTabBar
@using MudShell.Components.MainContent
@using MudShell.Components.Navigation
@using MudShell.Components.Navigation.Models
@using MudShell.Components.PageHeader
@using MudShell.Components.Sidebar
```

## 4. Keep MudBlazor providers in the root component

`MdsAppShell` is a layout component, not a provider host.

Declare providers once in your root component:

```razor
<MudThemeProvider Theme="@ThemeState.CurrentTheme" IsDarkMode="@ThemeState.IsDarkMode" />
<MudPopoverProvider />
<MudSnackbarProvider />
<MudDialogProvider />

<Router AppAssembly="typeof(Program).Assembly">
    ...
</Router>
```

## 5. Replace the layout

Move your existing layout into an `MdsAppShell`:

```razor
@inherits LayoutComponentBase

<MdsAppShell @ref="_shell"
             BackgroundMode="@ThemeState.BackgroundMode"
             BackgroundImageUrl="@ThemeState.BackgroundImageUrl"
             IsDarkMode="@ThemeState.IsDarkMode"
             IsDarkModeChanged="@ThemeState.SetDarkMode"
             SidebarExpanded="@_sidebarExpanded"
             SidebarExpandedChanged="@(v => _sidebarExpanded = v)">
    <SidebarContent>
        <NavMenu ShellRef="@_shell" IsExpanded="@_sidebarExpanded" />
    </SidebarContent>

    <ChildContent>
        @Body
    </ChildContent>
</MdsAppShell>
```

## 6. Migrate navigation gradually

You do not need to convert every route at once.

Recommended migration order:

1. top-level nav items,
2. optional context panel tree,
3. page-level structure.

Keep top-level nav focused on business areas. Move deep demo/component variants into the context panel.

## 7. Recompose pages instead of rewriting page content

The goal is not to replace every `MudPaper`, `MudGrid`, or `MudTable`.

Instead:

- replace ad-hoc page headers with `MdsPageHeader`,
- consolidate page actions into `MdsMainToolbar`,
- separate filters into a dedicated chip row,
- wrap major sections in `MdsMainPart` or `MdsMainSection`.

## 8. Add optional theme/background state

If you want the sample app behavior, create a scoped state object that owns:

- `IsDarkMode`
- `CurrentTheme`
- `BackgroundMode`
- `BackgroundImageUrl`

Then bind those values into `MdsAppShell` and `MudThemeProvider`.

## 9. Common migration mistakes

1. Leaving `AddMudServices()` and adding `AddMudShell()` on top.
2. Declaring MudBlazor providers in both the root and a layout subtree.
3. Keeping filters inside the action toolbar instead of splitting the two concepts.
4. Treating MudShell as a replacement for MudBlazor content components.
5. Moving everything to a new layout in one large refactor instead of migrating page by page.
