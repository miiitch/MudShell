# MdsBottomNav

Mobile-only fixed bottom navigation bar. Visible on xs/sm (≤ 959 px), hidden on md+.
Place it inside the `BottomNavContent` slot of `MdsAppShell`.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Items` | `MbxNavItem[]?` | `null` | Navigation items (shares the same record as `MdsSidebar`) |
| `ActiveHref` | `string?` | `null` | Href of the currently active item — highlighted with primary colour |

## Layout

- Fixed `position: fixed; bottom: 0; left: 0; right: 0`
- Height: 56 px
- `MdsAppShell` automatically adds `padding-bottom: 56px` to the main content so nothing is hidden behind it

## Example

```razor
@* Inside MainLayout.razor *@
<MdsAppShell @ref="_shell" ...>
  ...
  <BottomNavContent>
    <MdsBottomNav Items="@_navItems" ActiveHref="@_currentHref" />
  </BottomNavContent>
</MdsAppShell>

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
