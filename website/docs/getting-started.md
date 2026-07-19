---
sidebar_position: 1
---

# Getting Started

![MudShell — AppShell with dark background, collapsible sidebar and ChatBar](/img/screenshots/home.png)

*Sample app: `MdsAppShell` with background palette mode, icon-only sidebar, and `MdsChatBar`.*

## Prerequisites

- .NET 10 SDK
- A Blazor Server or WebAssembly project

## Step 1 — Add the project reference

In your app's `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\MudShell\MudShell.csproj" />
</ItemGroup>
```

## Step 2 — Register services

In `Program.cs`, replace `AddMudServices()` (or call it in addition):

```csharp
builder.Services.AddMudShell();
```

## Step 3 — Add imports

In your app's `_Imports.razor`:

```razor
@using MudShell
@using MudShell.Components.AppShell
@using MudShell.Components.Sidebar
@using MudShell.Components.BottomNav
@using MudShell.Components.ChatBar
@using MudShell.Components.DocumentCard
@using MudShell.Components.FilterTabBar
@using MudShell.Components.PageHeader
```

## Step 4 — Set up the layout

Replace the contents of `MainLayout.razor` with:

```razor
@inherits LayoutComponentBase

<MdsAppShell @ref="_shell"
             BackgroundMode="MdsAppShell.MbxBackgroundMode.Palette">
  <SidebarContent>
    <MdsSidebar IsExpanded="@_expanded"
                OnToggle="@(() => _shell.ToggleSidebar())"
                PrimaryItems="@_navItems" />
  </SidebarContent>
  <ChildContent>@Body</ChildContent>
  <BottomNavContent>
    <MdsBottomNav Items="@_navItems" />
  </BottomNavContent>
</MdsAppShell>

@code {
    private MdsAppShell _shell = default!;
    private bool _expanded;

    private readonly MbxNavItem[] _navItems =
    [
        new(Icons.Material.Outlined.GridView, "Home", "/"),
        new(Icons.Material.Outlined.Settings, "Settings", "/settings"),
    ];
}
```

## Step 5 — Run

```bash
dotnet run --project YourApp
```

Navigate to `https://localhost:5001`. You should see the dark shell with a collapsible sidebar.
On a narrow viewport (≤ 959 px) the sidebar disappears and a bottom navigation bar appears.

## Next steps

- [Theming](theming.md) — customise colours
- [Architecture](architecture.md) — understand the shell pattern
- [Responsive](responsive.md) — breakpoint behaviour