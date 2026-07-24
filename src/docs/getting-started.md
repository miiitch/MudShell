# Getting Started

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

In `Program.cs`, register MudShell and do **not** call `AddMudServices()` separately:

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
@using MudShell.Components.MainContent
@using MudShell.Components.PageHeader
```

## Step 4 — Add MudBlazor providers

`MdsAppShell` is a pure layout shell — it does **not** register MudBlazor providers internally.
Declare them once in your app's root component (e.g. `Routes.razor`) to avoid a duplicate
section-ID crash at runtime:

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

## Step 5 — Set up the layout

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

## Step 6 — Run

```bash
dotnet run --project YourApp
```

Navigate to `https://localhost:5001`. You should see the dark shell with a collapsible sidebar.

## Next steps

- [Theming](theming.md) — customise colours
- [MudBlazor integration](mudblazor-integration.md) — integrate MudShell into an existing MudBlazor app
- [Page blueprint](page-blueprint.md) — build pages with the recommended structure
- [Model](model.md) — understand the MudShell composition model
- [Architecture](architecture.md) — understand the shell pattern
- [Responsive](responsive.md) — breakpoint behaviour
