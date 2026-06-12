# MudShell

> A Razor Class Library of opinionated Blazor components built on top of **MudBlazor 9**, designed for dark-mode AI-shell style applications.

## Features

- 🎨 Pre-configured dark/light `MudTheme` — just call `CreateDarkTheme()`
- 🗂 `MbxAppShell` — full-page shell with collapsible sidebar, background image/palette modes
- 🌲 Unified hierarchical navigation with `MbxNavTree` (`MbxSidebarNav`, `MbxContextNavPanel`, `MbxMobileDrilldownNav`)
- 📱 Fully **responsive** — sidebar on desktop, `MbxBottomNav` on mobile (≤ 959 px)
- 💬 `MbxChatBar` — glassmorphism input bar
- 🃏 `MbxDocumentCard`, `MbxFilterTabBar`, `MbxPageHeader`
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
@using MudShell.Components.PageHeader
```

### 4. Add MudBlazor providers to your root component

`MbxAppShell` is a pure layout shell — it does **not** register MudBlazor providers internally.
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

### 5. Use `MbxAppShell` in your layout

```razor
@* MainLayout.razor *@
@inherits LayoutComponentBase

<MbxAppShell BackgroundMode="MbxAppShell.MbxBackgroundMode.Palette">
  <SidebarContent>
    <!-- your nav -->
  </SidebarContent>
  <ContextPanelContent>
    <!-- optional level-2/3 context panel -->
  </ContextPanelContent>
  <ChildContent>
    @Body
  </ChildContent>
  <BottomNavContent>
    <MbxBottomNav Items="@navItems" />
  </BottomNavContent>
</MbxAppShell>
```

### Hierarchical single-source navigation

```razor
<MbxAppShell @ref="_shell"
  ContextPanelExpanded="@_contextExpanded"
  ContextPanelExpandedChanged="@(v => _contextExpanded = v)"
  ContextPanelWidth="320"
  ContextPanelCollapsedWidth="72">
  <SidebarContent>
    <MbxSidebarNav Tree="@AppNav.Tree" />
  </SidebarContent>
  <ContextPanelContent>
    <MbxContextNavPanel Tree="@AppNav.Tree" IsExpanded="@_contextExpanded" />
  </ContextPanelContent>
  <BottomNavContent>
    <MbxMobileDrilldownNav Tree="@AppNav.Tree" />
  </BottomNavContent>
  <ChildContent>@Body</ChildContent>
</MbxAppShell>
```

You can collapse/expand the level-2 panel at runtime with:

```csharp
_shell.ToggleContextPanel();
```

## Documentation

- [Getting started](docs/getting-started.md)
- [Theming](docs/theming.md)
- [Responsive](docs/responsive.md)
- [Architecture](docs/architecture.md)
- Components: [AppShell](docs/components/app-shell.md) · [Sidebar](docs/components/sidebar.md) · [BottomNav](docs/components/bottom-nav.md) · [ChatBar](docs/components/chat-bar.md) · [DocumentCard](docs/components/document-card.md) · [FilterTabBar](docs/components/filter-tab-bar.md) · [PageHeader](docs/components/page-header.md)
