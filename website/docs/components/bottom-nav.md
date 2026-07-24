---
sidebar_position: 3
---

# MdsBottomNav

Compact bottom navigation bar component you can place in your own layouts.

![MdsBottomNav — fixed bottom bar on mobile viewport with active highlight](/img/screenshots/home-mobile.png)

*`MdsBottomNav` rendered in a compact container.*



## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Items` | `MbxNavItem[]?` | `null` | Navigation items (shares the same record as `MdsSidebar`) |
| `ActiveHref` | `string?` | `null` | Href of the currently active item — highlighted with primary colour |

## Example

```razor
<MudPaper Outlined="true">
  <MdsBottomNav Items="@_navItems" ActiveHref="@_currentHref" />
</MudPaper>

@code {
    private readonly MbxNavItem[] _navItems =
    [
        new(Icons.Material.Outlined.Home,     "Home",    "/"),
        new(Icons.Material.Outlined.Folder,   "Library", "/library"),
        new(Icons.Material.Outlined.Settings, "Settings"),
    ];
}
```

## Active item

Pass the current URL to highlight the active tab:

```razor
@inject NavigationManager Nav

<MdsBottomNav Items="@_navItems" ActiveHref="@Nav.Uri" />
```