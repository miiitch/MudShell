# MudShell — Copilot Integration Skill

> This file provides GitHub Copilot with context about the **MudShell** library so it can suggest correct component usage, prop names, and integration patterns.

---

## What is MudShell?

A Razor Class Library of opinionated Blazor components built on top of **MudBlazor 9**, designed for dark-mode AI-shell style applications. Targets **.NET 10**.

NuGet package ID: `MudShell`

---

## Setup

### 1. Add package reference
```xml
<PackageReference Include="MudShell" Version="0.*" />
```

### 2. Register services in `Program.cs`
```csharp
builder.Services.AddMudShell();
// This also calls AddMudServices() — do NOT call both.
```

### 3. Add to `_Imports.razor`
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

### 4. Add MudBlazor CSS to `App.razor` or `index.html`
```html
<link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
```
And at the bottom:
```html
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
```

---

## Component reference

### `MdsAppShell`
Full-page layout shell with sidebar, main content, and bottom nav slots.

**Parameters:**
| Parameter | Type | Default | Notes |
|-----------|------|---------|-------|
| `SidebarContent` | `RenderFragment?` | — | Nav content inside the sidebar |
| `ChildContent` | `RenderFragment?` | — | Main page body |
| `BottomNavContent` | `RenderFragment?` | — | Mobile bottom bar slot (≤959 px) |
| `BackgroundMode` | `MbxBackgroundMode` | `Palette` | `Palette` or `Image` |
| `BackgroundImageUrl` | `string?` | `null` | Image mode only |
| `SidebarExpanded` | `bool` | `false` | Two-way bindable |
| `SidebarExpandedChanged` | `EventCallback<bool>` | — | |

**Public methods:** `ToggleSidebar()`, `SetBackgroundMode(mode, imageUrl?)`

```razor
<MdsAppShell @ref="_shell" BackgroundMode="MdsAppShell.MbxBackgroundMode.Palette">
  <SidebarContent>
    <MdsSidebar OnToggle="@(() => _shell.ToggleSidebar())" PrimaryItems="@_nav" />
  </SidebarContent>
  <ChildContent>@Body</ChildContent>
  <BottomNavContent>
    <MdsBottomNav Items="@_nav" />
  </BottomNavContent>
</MdsAppShell>

@code {
    private MdsAppShell _shell = default!;
    private readonly MbxNavItem[] _nav =
    [
        new(Icons.Material.Outlined.Home, "Home", "/"),
        new(Icons.Material.Outlined.Settings, "Settings", "/settings"),
    ];
}
```

---

### `MdsSidebar`
Collapsible vertical navigation. Hidden on mobile (≤959 px).

**Parameters:**
| Parameter | Type | Default |
|-----------|------|---------|
| `IsExpanded` | `bool` | `false` |
| `OnToggle` | `EventCallback` | — |
| `PrimaryItems` | `MbxNavItem[]?` | `null` |
| `SecondaryItems` | `MbxNavItem[]?` | `null` |
| `LogoContent` | `RenderFragment?` | `null` |
| `BottomContent` | `RenderFragment?` | `null` |

**`MbxNavItem` record:**
```csharp
public record MbxNavItem(string Icon, string Label, string? Href = null);
```

---

### `MdsBottomNav`
Mobile-only fixed bottom navigation bar (visible ≤959 px).

**Parameters:**
| Parameter | Type | Default |
|-----------|------|---------|
| `Items` | `MbxNavItem[]?` | `null` |
| `ActiveHref` | `string?` | `null` |

```razor
@inject NavigationManager Nav
<MdsBottomNav Items="@_nav" ActiveHref="@Nav.Uri" />
```

---

### `MdsChatBar`
Glassmorphism AI-style input bar with `backdrop-filter: blur`.

**Parameters:**
| Parameter | Type | Default |
|-----------|------|---------|
| `Placeholder` | `string?` | `"Message…"` |
| `Value` | `string?` | `null` |
| `ValueChanged` | `EventCallback<string?>` | — |
| `Actions` | `RenderFragment?` | `null` |
| `MaxWidth` | `string` | `"680px"` |

```razor
<MdsChatBar @bind-Value="_message" Placeholder="Ask anything…" MaxWidth="720px">
  <Actions>
    <MudIconButton Icon="@Icons.Material.Filled.Send" Color="Color.Primary" />
  </Actions>
</MdsChatBar>
```

---

### `MdsDocumentCard`
Card for a document/item with icon, type label, title, description.

**Parameters:**
| Parameter | Type | Default |
|-----------|------|---------|
| `Icon` | `string` | `Description` (outlined) |
| `TypeLabel` | `string?` | `"Document"` |
| `Title` | `string?` | `null` |
| `Description` | `string?` | `null` |
| `OnClick` | `EventCallback` | — |

```razor
<MdsDocumentCard Title="My Doc" Description="Summary…" OnClick="@Open" />
```

---

### `MdsFilterTabBar<T>`
Pill-style tab/filter bar. Scrolls horizontally on mobile.

**Parameters:**
| Parameter | Type | Default |
|-----------|------|---------|
| `Value` | `T?` | `null` |
| `ValueChanged` | `EventCallback<T>` | — |
| `Items` | `IEnumerable<MbxTabItem<T>>?` | `null` |
| `TrailingContent` | `RenderFragment?` | `null` |

**`MbxTabItem<T>` record:** `(T Value, string Label)`

```razor
<MdsFilterTabBar T="string" @bind-Value="_tab" Items="@_tabs" />
@code {
    private string _tab = "all";
    private readonly MbxTabItem<string>[] _tabs =
    [
        new("all", "All"), new("docs", "Documents"), new("images", "Images"),
    ];
}
```

---

### `MdsPageHeader`
Three-column page header: start | centred title | end. Stacks on mobile.

**Parameters:**
| Parameter | Type | Default |
|-----------|------|---------|
| `Title` | `string?` | `null` |
| `StartContent` | `RenderFragment?` | `null` |
| `EndContent` | `RenderFragment?` | `null` |

```razor
<MdsPageHeader Title="Library">
  <EndContent>
    <MudButton Variant="Variant.Outlined" Color="Color.Primary">New</MudButton>
  </EndContent>
</MdsPageHeader>
```

---

## Theming

`MdsAppShell` applies `MbxTheme.CreateDarkTheme()` automatically.

To customise:
```csharp
using MudShell.Theme;

var theme = MbxTheme.CreateDarkTheme();
theme.PaletteDark.Primary = "#ff6b6b";
```

Key CSS variables (override in `app.css`):
- `--mud-palette-primary` — accent colour
- `--mud-palette-surface` — card / sidebar background
- `--mud-palette-background` — app background

---

## Common pitfalls

1. **Missing MudBlazor CSS/JS** — components will render without styles. Always include `_content/MudBlazor/MudBlazor.min.css` and the JS script.
2. **Calling both `AddMudServices()` and `AddMudShell()`** — `AddMudShell()` already calls `AddMudServices()`. Calling both is harmless but redundant.
3. **`MdsAppShell` ref is null on first render** — use `@ref="_shell"` and call methods only after `OnAfterRenderAsync` with `firstRender: true`.
4. **Sidebar visible on mobile** — by design, use `MdsBottomNav` inside `BottomNavContent` for mobile navigation.
5. **Render mode** — when using Blazor Web App (interactive), ensure components have a compatible render mode (`InteractiveServer` or `InteractiveWebAssembly`).
