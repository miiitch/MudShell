# MdsSplitPane

Main area plus a right-hand side pane. Each side is an `MdsFillRegion`. The side pane is resizable
(drag handle or keyboard) between `MinSideWidth` and `MaxSideWidth`, and collapsible to a narrow rail.
Width and collapsed state are remembered in `localStorage` when `StorageKey` is set.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Main` | `RenderFragment?` | `null` | Main area |
| `Side` | `RenderFragment?` | `null` | Side pane. When null, only the main area is shown |
| `RailContent` | `RenderFragment?` | `null` | Optional content in the collapsed rail, under the expand button |
| `SideOpen` / `SideOpenChanged` | `bool` / `EventCallback<bool>` | `true` | Two-way bindable (`@bind-SideOpen`) |
| `SideWidth` / `SideWidthChanged` | `int` / `EventCallback<int>` | `360` | Side width in px, clamped to Min/Max (`@bind-SideWidth`) |
| `MinSideWidth` | `int` | `240` | Minimum width |
| `MaxSideWidth` | `int` | `640` | Maximum width |
| `StorageKey` | `string?` | `null` | Persist width and open state under this localStorage key |
| `CollapseTooltip` / `ExpandTooltip` | `string` | English | Toggle button tooltips |
| `Class` | `string?` | `null` | Extra CSS class on the root |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | `null` | Splatted onto the root element |

Keyboard on the handle: Left/Right resize by 16 px, Home/End jump to min/max.

## Example

```razor
<MdsSplitPane StorageKey="assets.preview" @bind-SideOpen="_previewOpen">
  <Main><MudDataGrid .../></Main>
  <Side><PreviewContent Item="@_selected" /></Side>
</MdsSplitPane>
```

Stored value format: `"<width>|<1 or 0>"`. Storage failures (private mode, blocked) are ignored.
