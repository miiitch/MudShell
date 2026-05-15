# MudBlazorExtended — Copilot Integration Skill

> This file provides GitHub Copilot with context about the **MudBlazorExtended** library so it can suggest correct component usage, prop names, and integration patterns.

---

## What is MudBlazorExtended?

A Razor Class Library of opinionated Blazor components built on top of **MudBlazor 9**, designed for dark-mode AI-shell style applications. Targets **.NET 10**.

NuGet package ID: `MudBlazorExtended`

---

## Setup

### 1. Add package reference
```xml
<PackageReference Include="MudBlazorExtended" Version="0.*" />
```

### 2. Register services in `Program.cs`
```csharp
builder.Services.AddMudBlazorExtended();
// This also calls AddMudServices() — do NOT call both.
```

### 3. Add to `_Imports.razor`
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

### `MbxAppShell`
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
<MbxAppShell @ref="_shell" BackgroundMode="MbxAppShell.MbxBackgroundMode.Palette">
  <SidebarContent>
    <MbxSidebar OnToggle="@(() => _shell.ToggleSidebar())" PrimaryItems="@_nav" />
  </SidebarContent>
  <ChildContent>@Body</ChildContent>
  <BottomNavContent>
    <MbxBottomNav Items="@_nav" />
  </BottomNavContent>
</MbxAppShell>

@code {
    private MbxAppShell _shell = default!;
    private readonly MbxNavItem[] _nav =
    [
        new(Icons.Material.Outlined.Home, "Home", "/"),
        new(Icons.Material.Outlined.Settings, "Settings", "/settings"),
    ];
}
```

---

### `MbxSidebar`
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

### `MbxBottomNav`
Mobile-only fixed bottom navigation bar (visible ≤959 px).

**Parameters:**
| Parameter | Type | Default |
|-----------|------|---------|
| `Items` | `MbxNavItem[]?` | `null` |
| `ActiveHref` | `string?` | `null` |

```razor
@inject NavigationManager Nav
<MbxBottomNav Items="@_nav" ActiveHref="@Nav.Uri" />
```

---

### `MbxChatBar`
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
<MbxChatBar @bind-Value="_message" Placeholder="Ask anything…" MaxWidth="720px">
  <Actions>
    <MudIconButton Icon="@Icons.Material.Filled.Send" Color="Color.Primary" />
  </Actions>
</MbxChatBar>
```

---

### `MbxDocumentCard`
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
<MbxDocumentCard Title="My Doc" Description="Summary…" OnClick="@Open" />
```

---

### `MbxFilterTabBar<T>`
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
<MbxFilterTabBar T="string" @bind-Value="_tab" Items="@_tabs" />
@code {
    private string _tab = "all";
    private readonly MbxTabItem<string>[] _tabs =
    [
        new("all", "All"), new("docs", "Documents"), new("images", "Images"),
    ];
}
```

---

### `MbxPageHeader`
Three-column page header: start | centred title | end. Stacks on mobile.

**Parameters:**
| Parameter | Type | Default |
|-----------|------|---------|
| `Title` | `string?` | `null` |
| `StartContent` | `RenderFragment?` | `null` |
| `EndContent` | `RenderFragment?` | `null` |

```razor
<MbxPageHeader Title="Library">
  <EndContent>
    <MudButton Variant="Variant.Outlined" Color="Color.Primary">New</MudButton>
  </EndContent>
</MbxPageHeader>
```

---

## Theming

`MbxAppShell` applies `MbxTheme.CreateDarkTheme()` automatically.

To customise:
```csharp
using MudBlazorExtended.Theme;

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
2. **Calling both `AddMudServices()` and `AddMudBlazorExtended()`** — `AddMudBlazorExtended()` already calls `AddMudServices()`. Calling both is harmless but redundant.
3. **`MbxAppShell` ref is null on first render** — use `@ref="_shell"` and call methods only after `OnAfterRenderAsync` with `firstRender: true`.
4. **Sidebar visible on mobile** — by design, use `MbxBottomNav` inside `BottomNavContent` for mobile navigation.
5. **Render mode** — when using Blazor Web App (interactive), ensure components have a compatible render mode (`InteractiveServer` or `InteractiveWebAssembly`).
