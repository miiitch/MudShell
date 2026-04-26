# Getting Started

## Prerequisites

- .NET 10 SDK
- A Blazor Server or WebAssembly project

## Step 1 — Add the project reference

In your app's `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\MudBlazorExtended\MudBlazorExtended.csproj" />
</ItemGroup>
```

## Step 2 — Register services

In `Program.cs`, replace `AddMudServices()` (or call it in addition):

```csharp
builder.Services.AddMudBlazorExtended();
```

## Step 3 — Add imports

In your app's `_Imports.razor`:

```razor
@using MudBlazorExtended
@using MudBlazorExtended.Components.AppShell
@using MudBlazorExtended.Components.Sidebar
@using MudBlazorExtended.Components.BottomNav
@using MudBlazorExtended.Components.ChatBar
@using MudBlazorExtended.Components.DocumentCard
@using MudBlazorExtended.Components.FilterTabBar
@using MudBlazorExtended.Components.PageHeader
```

## Step 4 — Set up the layout

Replace the contents of `MainLayout.razor` with:

```razor
@inherits LayoutComponentBase

<MbxAppShell @ref="_shell"
             BackgroundMode="MbxAppShell.MbxBackgroundMode.Palette">
  <SidebarContent>
    <MbxSidebar IsExpanded="@_expanded"
                OnToggle="@(() => _shell.ToggleSidebar())"
                PrimaryItems="@_navItems" />
  </SidebarContent>
  <ChildContent>@Body</ChildContent>
  <BottomNavContent>
    <MbxBottomNav Items="@_navItems" />
  </BottomNavContent>
</MbxAppShell>

@code {
    private MbxAppShell _shell = default!;
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
