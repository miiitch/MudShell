# MdsFillLayout

Full-screen page frame: a fixed header, fixed bands (toolbar, filter bar, stat strip) and a body that takes
all the remaining height. **The page never scrolls**; only leaf regions (`MdsFillRegion`, a grid body,
a side pane, a tab body) scroll internally.

Layout blocks own sizing and scrolling, content blocks never do. Compose a page by stacking blocks.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Header` | `RenderFragment?` | `null` | Fixed band at the top. `flex-shrink: 0` |
| `Bands` | `RenderFragment?` | `null` | Fixed bands under the header. Each child stays on one row (no wrap) and overflows horizontally |
| `Body` | `RenderFragment?` | `null` | Remaining space (`flex: 1 1 0; min-height: 0; overflow: hidden`) |
| `ChildContent` | `RenderFragment?` | `null` | Alias of `Body` when no `<Body>` slot is given |
| `Class` | `string?` | `null` | Extra CSS class on the root |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | `null` | Splatted onto the root element |

## Example

```razor
<MdsFillLayout>
  <Header><MdsPageHeader Title="Assets" /></Header>
  <Bands>
    <MdsMainToolbar>...</MdsMainToolbar>
    <MdsStatStrip>...</MdsStatStrip>
  </Bands>
  <Body>
    <MdsFillRegion>
      <MudDataGrid T="Row" Items="@_rows" FixedHeader="true">...</MudDataGrid>
    </MdsFillRegion>
  </Body>
</MdsFillLayout>
```

## Inside `MdsAppShell`

`MdsAppShell`'s main area (`.mbx-main-inner`) has a definite height (`100%`) and `box-sizing: border-box`,
so `MdsFillLayout` (`height: 100%`) fits exactly inside its padding and the shell content does not scroll.
Pages that do not use `MdsFillLayout` keep scrolling as before.

## Rules

- Put exactly one scrolling leaf (or an `MdsSplitPane` / `MdsTabbedFill` containing them) in the body.
- Don't add `overflow`, `height` or `flex` styles on content blocks; the layout does it.
