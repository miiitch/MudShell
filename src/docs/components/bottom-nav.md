# MdsBottomNav

Compact bottom navigation bar component you can place in your own layouts.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Items` | `MbxNavItem[]?` | `null` | Navigation items (shares the same record as `MdsSidebar`) |
| `ActiveHref` | `string?` | `null` | Href of the currently active item — highlighted with primary colour |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | `null` | Undeclared attributes, splatted onto the root element (e.g. `data-testid`, ARIA attributes) |

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
