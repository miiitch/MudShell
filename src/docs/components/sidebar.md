# MdsSidebar

Collapsible vertical navigation. Renders as icon-only (56 px) by default and expands to 240 px with labels.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `IsExpanded` | `bool` | `false` | Controls collapsed/expanded state |
| `OnToggle` | `EventCallback` | — | Called when the toggle button is clicked |
| `PrimaryItems` | `MbxNavItem[]?` | `null` | Main navigation items |
| `SecondaryItems` | `MbxNavItem[]?` | `null` | Secondary items (shown after divider) |
| `LogoContent` | `RenderFragment?` | `null` | Logo area at the top |
| `BottomContent` | `RenderFragment?` | `null` | Profile / settings area at the bottom |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | `null` | Undeclared attributes, splatted onto the root element (e.g. `data-testid`, ARIA attributes) |

## MbxNavItem record

```csharp
public record MbxNavItem(string Icon, string Label, string? Href = null);
```

Items without an `Href` render as clickable `<div>` elements.
Items with an `Href` render as `<a>` tags with client-side navigation.

## Example

```razor
<MdsSidebar IsExpanded="@_expanded"
            OnToggle="@(() => _shell.ToggleSidebar())"
            PrimaryItems="@_nav">
  <LogoContent>
    <MudIcon Icon="@Icons.Material.Filled.AutoAwesome" />
  </LogoContent>
  <BottomContent>
    <MudAvatar>MP</MudAvatar>
  </BottomContent>
</MdsSidebar>

@code {
    private readonly MbxNavItem[] _nav =
    [
        new(Icons.Material.Outlined.Home,     "Home",     "/"),
        new(Icons.Material.Outlined.Settings, "Settings", "/settings"),
    ];
}
```
