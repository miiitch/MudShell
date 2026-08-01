# MdsDocumentCard

Card displaying a document or item with a type icon, title, and description.
Hover state highlights with the primary colour.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Icon` | `string` | `Description` (outlined) | MudBlazor icon string |
| `TypeLabel` | `string?` | `"Document"` | Short label next to the icon |
| `Title` | `string?` | `null` | Card title |
| `Description` | `string?` | `null` | Card body text (clamped to 2 lines) |
| `OnClick` | `EventCallback` | — | Click handler |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | `null` | Undeclared attributes, splatted onto the root element (e.g. `data-testid`, ARIA attributes) |

## Example

```razor
<MdsDocumentCard Icon="@Icons.Material.Outlined.Description"
                 TypeLabel="Page"
                 Title="Things to do in Tokyo"
                 Description="Tokyo offers a vibrant mix of traditional culture..."
                 OnClick="@OpenDocument" />
```

## Recommended grid layout

```razor
<MudGrid Spacing="3">
  @foreach (var doc in docs)
  {
    <MudItem xs="12" sm="6" md="4" lg="3">
      <MdsDocumentCard Title="@doc.Title" Description="@doc.Body" />
    </MudItem>
  }
</MudGrid>
```

## Customisation

```css
/* Increase minimum card height */
.mbx-doc-card {
  min-height: 180px;
}
```
