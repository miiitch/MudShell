# MdsBottomNav

The mobile navigation: a level-1 bottom bar, plus a bottom sheet for level 2 and level 3.
It replaces the former `MdsMobileDrilldownNav` (still available as a thin wrapper around it).

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Items` | `MbxNavItem[]?` | `null` | Flat level-1 entries, no sheet (shares the same record as `MdsSidebar`). Ignored when `Tree` is set |
| `Tree` | `MbxNavTree?` | `null` | Navigation tree. Its visible roots are the level-1 entries, and their children feed the sheet |
| `CurrentUri` | `string?` | `null` | URI used to compute the active path. Defaults to the current location |
| `ActiveHref` | `string?` | `null` | Href of the currently active item, highlighted with the primary colour (flat `Items`) |
| `MaxVisible` | `int` | `5` | Level-1 entries shown directly. Further ones go under the overflow entry |
| `MoreLabel` | `string` | `"Plus"` | Label of the overflow entry |
| `Class` / `Style` | `string?` | `null` | Classes and inline style of the bar |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | `null` | Undeclared attributes, splatted onto the bar element (e.g. `data-testid`, ARIA attributes) |

## Public methods

| Method | Description |
|---|---|
| `OpenMenu()` | Opens the sheet listing every level-1 entry. Wire it to the mobile top bar's menu button |
| `CloseMenuAsync()` | Closes the sheet if it is open |

## Mobile behaviour

- **Bar.** Shows up to `MaxVisible` level-1 entries. When there are more, an overflow entry opens the rest in the sheet.
- **Entries without children** navigate directly.
- **Entries with children** open the sheet with those children. An entry with an `Href` also navigates to it.
- **Drill-down.** A child that has its own children opens a deeper level. The back row returns one level up.
- **Leaf** entries navigate and close the sheet.
- **Closing.** Scrim tap, the close button, Esc, and any navigation that the sheet did not start.
- **Accessibility.** The sheet is `role="dialog"` with `aria-modal="true"`. Focus is trapped inside it and returns to the trigger on close.
- **Placement.** Render it outside `MdsAppShell`. On phones (under 600 px) the bar is `position: fixed` to the bottom edge, and the shell reserves room for it.

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

## Navigation tree with sheet

```razor
<MdsBottomNav @ref="_mobileNav" Tree="@_navTree" MaxVisible="5" />
```

Pass a `MbxNavTree` to get level 2 and level 3 in the sheet. Call `_mobileNav.OpenMenu()` from the shell's `MobileMenuClicked` to show every entry.

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
