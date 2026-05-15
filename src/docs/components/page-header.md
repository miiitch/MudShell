# MbxPageHeader

Three-column page header: start slot | centred title | end slot.
Stacks vertically with centred title on small screens.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Title` | `string?` | `null` | Page title (rendered as `h5`) |
| `StartContent` | `RenderFragment?` | `null` | Left slot (e.g. back button, spacer) |
| `EndContent` | `RenderFragment?` | `null` | Right slot (e.g. action button) |

## Example — with action button

```razor
<MbxPageHeader Title="Bibliothèque">
  <StartContent><div></div></StartContent>
  <EndContent>
    <MudButton Variant="Variant.Outlined" Color="Color.Primary" Size="Size.Small"
               Style="border-radius:20px; text-transform:none;">
      Créer une page
    </MudButton>
  </EndContent>
</MbxPageHeader>
```

## Example — title only

```razor
<MbxPageHeader Title="Paramètres" />
```

## Responsive

On xs (≤ 599 px):
- The three columns wrap (`flex-wrap: wrap`)
- The title takes full width and is centred
- The end action is centred below the title

## Customisation

```css
/* Adjust padding */
.mbx-page-header {
  padding: 16px 24px;
}
```
