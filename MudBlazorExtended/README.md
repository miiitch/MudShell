# MudBlazorExtended

> A Razor Class Library of opinionated Blazor components built on top of **MudBlazor 9**, designed for dark-mode AI-shell style applications.

## Features

- 🎨 Pre-configured dark/light `MudTheme` — just call `CreateDarkTheme()`
- 🗂 `MbxAppShell` — full-page shell with collapsible sidebar, background image/palette modes
- 📱 Fully **responsive** — sidebar on desktop, `MbxBottomNav` on mobile (≤ 959 px)
- 💬 `MbxChatBar` — glassmorphism input bar
- 🃏 `MbxDocumentCard`, `MbxFilterTabBar`, `MbxPageHeader`
- All components use **scoped CSS** — zero global style pollution

## Quick start

### 1. Add the project reference

```xml
<!-- YourApp.csproj -->
<ItemGroup>
  <ProjectReference Include="..\MudBlazorExtended\MudBlazorExtended.csproj" />
</ItemGroup>
```

### 2. Register services

```csharp
// Program.cs
builder.Services.AddMudBlazorExtended();
```

### 3. Add global imports

```razor
@* _Imports.razor *@
@using MudBlazorExtended
@using MudBlazorExtended.Components.AppShell
@using MudBlazorExtended.Components.Sidebar
@using MudBlazorExtended.Components.BottomNav
@using MudBlazorExtended.Components.ChatBar
@using MudBlazorExtended.Components.DocumentCard
@using MudBlazorExtended.Components.FilterTabBar
@using MudBlazorExtended.Components.PageHeader
```

### 4. Use `MbxAppShell` in your layout

```razor
@* MainLayout.razor *@
@inherits LayoutComponentBase

<MbxAppShell BackgroundMode="MbxAppShell.MbxBackgroundMode.Palette">
  <SidebarContent>
    <!-- your nav -->
  </SidebarContent>
  <ChildContent>
    @Body
  </ChildContent>
  <BottomNavContent>
    <MbxBottomNav Items="@navItems" />
  </BottomNavContent>
</MbxAppShell>
```

## Documentation

- [Getting started](docs/getting-started.md)
- [Theming](docs/theming.md)
- [Responsive](docs/responsive.md)
- [Architecture](docs/architecture.md)
- Components: [AppShell](docs/components/app-shell.md) · [Sidebar](docs/components/sidebar.md) · [BottomNav](docs/components/bottom-nav.md) · [ChatBar](docs/components/chat-bar.md) · [DocumentCard](docs/components/document-card.md) · [FilterTabBar](docs/components/filter-tab-bar.md) · [PageHeader](docs/components/page-header.md)
