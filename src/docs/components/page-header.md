# MdsPageHeader

Unified page header for **icon + breadcrumb path + title**, with optional right-side contextual actions.
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
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | `null` | Undeclared attributes, splatted onto the root element (e.g. `data-testid`, ARIA attributes) |

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
    new("Home", href: "/")
  ];

}
```

> `BreadcrumbItems` should list ancestor pages only — `Title` already displays the current page name, so repeating it as the last breadcrumb entry is redundant.

## Example — title only

```razor
<MdsPageHeader Title="Paramètres" />
```

## Responsive

- The header height is `auto`: a long title wraps and the header grows, it never overlaps the next block.
- 600-1279 px: the actions stay beside the title when they fit and wrap onto a new row otherwise.
- On xs (≤ 599 px): the actions move under the title, left-aligned, and wrap; they are never clipped.
- The header ends with `margin-bottom: var(--mbx-block-gap)`; inside a `.mbx-stack` the stack's gap is used instead
  (see [Responsive](../responsive.md)).
- Implementation note: the root `MudToolBar` is wrapped in a `display: contents` host (`.mds-page-header-host`) that owns the CSS
  isolation scope, because Blazor does not stamp it on a child component's root.

## Customisation

```css
/* Adjust padding */
/* in your global stylesheet */
.mbx-page-header.mud-toolbar {
  padding: 16px 0 8px;
}
```
