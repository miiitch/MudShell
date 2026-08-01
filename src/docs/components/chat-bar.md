# MdsChatBar

Glassmorphism input bar inspired by AI chat interfaces.
Uses `backdrop-filter: blur` for the frosted glass effect.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Placeholder` | `string?` | `"Message…"` | Input placeholder text |
| `Value` | `string?` | `null` | Current text value |
| `ValueChanged` | `EventCallback<string?>` | — | Two-way bind support |
| `Actions` | `RenderFragment?` | `null` | Row of action buttons below the input |
| `MaxWidth` | `string` | `"680px"` | CSS `max-width`. Use `"100%"` for full-width. |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | `null` | Undeclared attributes, splatted onto the root element (e.g. `data-testid`, ARIA attributes) |

## Example

```razor
<MdsChatBar Placeholder="Ask anything…"
            @bind-Value="_message"
            MaxWidth="720px">
  <Actions>
    <MudIconButton Icon="@Icons.Material.Filled.Add" Size="Size.Small" Color="Color.Inherit" />
    <div style="flex:1"></div>
    <MudIconButton Icon="@Icons.Material.Filled.Send" Size="Size.Small" Color="Color.Primary" />
  </Actions>
</MdsChatBar>
```

## Responsive

- Desktop: centred, `max-width` respected
- Mobile (≤ 599 px): full width, reduced padding, border-radius reduced to 16 px

## Customisation

Override the glass background via CSS:

```css
.mbx-chat-bar {
  background: rgba(10, 10, 20, 0.92);
}
```
