# MdsFillRegion

A leaf that fills its parent and scrolls internally. Drop any content in it.

A `MudDataGrid` or `MudTable` placed **directly** inside fills the region: its table container scrolls,
the header stays sticky and the pager stays fixed below. (A single `d-flex flex-column` wrapper in between
is tolerated.) Other content simply scrolls inside the region.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Content |
| `Class` | `string?` | `null` | Extra CSS class on the root |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | `null` | Splatted onto the root element |

## Example

```razor
<MdsFillRegion>
  <MudDataGrid T="Row" Items="@_rows" FixedHeader="true" Dense="true">
    <Columns>...</Columns>
    <PagerContent><MudDataGridPager T="Row" /></PagerContent>
  </MudDataGrid>
</MdsFillRegion>
```

Used by `MdsFillLayout`'s body, `MdsSplitPane` (both sides) and tab panels of `MdsTabbedFill`.
