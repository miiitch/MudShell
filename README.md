# MudShell

> A Razor Class Library of opinionated Blazor components built on top of **MudBlazor 9**, designed for dark-mode AI-shell style applications.

## Features

- 🎨 Pre-configured dark/light `MudTheme` — just call `CreateDarkTheme()`
- 🗂 `MdsAppShell` — full-page shell with collapsible sidebar, background image/palette modes
- 🌲 Unified hierarchical navigation with `MbxNavTree` (`MdsSidebarNav`, `MdsContextNavPanel`, `MbxMobileDrilldownNav`)
- 🖥️ Desktop-first shell with collapsible sidebar navigation
- 💬 `MdsChatBar` — glassmorphism input bar
- 🃏 `MdsDocumentCard`, `MdsFilterTabBar`, `MdsPageHeader`
- 🧩 Main-content primitives (`MdsMainToolbar`, `MdsMainSection`, `MdsMainPart`, `MdsMainEmptyState`)
- All components use **scoped CSS** — zero global style pollution

## Quick start

### 1. Add the project reference

```xml
<!-- YourApp.csproj -->
<ItemGroup>
  <ProjectReference Include="..\MudShell\MudShell.csproj" />
</ItemGroup>
```

### 2. Register services

```csharp
// Program.cs
builder.Services.AddMudShell();
```

### 3. Add global imports

```razor
@* _Imports.razor *@
@using MudShell
@using MudShell.Components.AppShell
@using MudShell.Components.Sidebar
@using MudShell.Components.BottomNav
@using MudShell.Components.ChatBar
@using MudShell.Components.DocumentCard
@using MudShell.Components.FilterTabBar
@using MudShell.Components.MainContent
@using MudShell.Components.PageHeader
```

### 4. Add MudBlazor providers to your root component

`MdsAppShell` is a pure layout shell — it does **not** register MudBlazor providers internally.
You must declare them once in your app's root component (e.g. `Routes.razor`) to avoid a
duplicate section-ID crash at runtime:

```razor
@* Routes.razor *@
<MudThemeProvider Theme="@myTheme" />
<MudPopoverProvider />
<MudSnackbarProvider />
<MudDialogProvider />

<Router AppAssembly="typeof(App).Assembly">
    ...
</Router>
```

### 5. Use `MdsAppShell` in your layout

```razor
@* MainLayout.razor *@
@inherits LayoutComponentBase

<MdsAppShell BackgroundMode="MdsAppShell.MbxBackgroundMode.Palette">
  <SidebarContent>
    <!-- your nav -->
  </SidebarContent>
  <ContextPanelContent>
    <!-- optional level-2/3 context panel -->
  </ContextPanelContent>
  <ChildContent>
    @Body
  </ChildContent>
</MdsAppShell>
```

### Hierarchical single-source navigation

```razor
<MdsAppShell @ref="_shell"
  ContextPanelExpanded="@_contextExpanded"
  ContextPanelExpandedChanged="@(v => _contextExpanded = v)"
  ContextPanelWidth="320"
  ContextPanelCollapsedWidth="72">
  <SidebarContent>
    <MdsSidebarNav Tree="@AppNav.Tree" />
  </SidebarContent>
  <ContextPanelContent>
    <MdsContextNavPanel Tree="@AppNav.Tree" IsExpanded="@_contextExpanded" />
  </ContextPanelContent>
  <ChildContent>@Body</ChildContent>
</MdsAppShell>
```

You can collapse/expand the level-2 panel at runtime with:

```csharp
_shell.ToggleContextPanel();
```

## Recommended page layout pattern (sample app)

For pages with actions and filters, use this structure:

1. **Top action bar** with `MdsMainToolbar` (primary actions on the left, UI config on the right with `MudSpacer`).
2. **Filter row below** with a horizontal `MudStack` of `MudChip` filters.
3. Keep the filter row directly on the page background (no bordered `MudPaper` wrapper).

This pattern is used in the sample pages (for example `MeteoPage` and `EuropeCitiesDashboard`) for consistent UX.

For a full page blueprint and MudBlazor migration guidance, see the dedicated docs below.

## Documentation

- [Getting started](src/docs/getting-started.md)
- [MudBlazor integration](src/docs/mudblazor-integration.md)
- [Page blueprint](src/docs/page-blueprint.md)
- [Model](src/docs/model.md)
- [Theming](src/docs/theming.md)
- [Responsive](src/docs/responsive.md)
- [Architecture](src/docs/architecture.md)
- Components: [AppShell](src/docs/components/app-shell.md) · [Sidebar](src/docs/components/sidebar.md) · [BottomNav](src/docs/components/bottom-nav.md) · [ChatBar](src/docs/components/chat-bar.md) · [DocumentCard](src/docs/components/document-card.md) · [FilterTabBar](src/docs/components/filter-tab-bar.md) · [PageHeader](src/docs/components/page-header.md)
