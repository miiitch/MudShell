# MdsPageHeader

Unified page header for **icon + title + breadcrumb path**, with optional right-side contextual actions.
The title can also be synced automatically to browser `PageTitle`.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Title` | `string?` | `null` | Page title (rendered as `h5`) |
| `PageTitle` | `string?` | `null` | Browser title override (`<title>`). Falls back to `Title` |
| `SyncPageTitle` | `bool` | `true` | Enables automatic browser title sync |
| `Icon` | `string?` | `null` | Leading icon when `StartContent` is not provided |
| `BreadcrumbItems` | `IReadOnlyList<BreadcrumbItem>?` | `null` | Breadcrumb path rendered next to the title |
| `HeaderActions` | `RenderFragment?` | `null` | Right-aligned configurable action zone (e.g. `MudIconButton` list) |
| `StartContent` | `RenderFragment?` | `null` | Left slot (e.g. back button, spacer) |
| `EndContent` | `RenderFragment?` | `null` | Right slot (e.g. action button) |
| `ActionsContent` | `RenderFragment?` | `null` | Right slot alias (preferred name) |

## Example — icon + path + contextual actions

```razor
<MdsPageHeader Title="Weather"
               Icon="@Icons.Material.Outlined.Cloud"
               BreadcrumbItems="@_breadcrumbs">
  <HeaderActions>
    <MudIconButton Icon="@Icons.Material.Outlined.ColorLens"
                   Size="Size.Small"
                   Color="Color.Inherit"
                   title="Theme and background"
                   OnClick="@OpenThemeDialog" />
  </HeaderActions>
</MdsPageHeader>

@code {
  private readonly List<BreadcrumbItem> _breadcrumbs =
  [
    new("Home", href: "/"),
    new("Weather", href: null, disabled: true)
  ];

}
```

## Example — title only

```razor
<MdsPageHeader Title="Paramètres" />
```

## Responsive

On xs (≤ 599 px):
- Header wraps cleanly
- Core row (icon/title/path) stays vertically centered
- Right action area moves below while staying left-aligned

## Customisation

```css
/* Adjust padding */
.mbx-page-header {
  padding: 16px 24px;
}
```
