# MdsTabbedFill and MdsTabHeader

`MudTabs` with fill CSS: the tab bar keeps its height, the active panel takes the rest and clips, and an
`MdsFillRegion` or `MdsSplitPane` inside the panel scrolls. `MdsTabHeader` renders a tab label with an
optional count; give the count a tone only when it is abnormal.

## MdsTabbedFill parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | `MudTabPanel` children |
| `ActivePanelIndex` / `ActivePanelIndexChanged` | `int` / `EventCallback<int>` | `0` | Two-way bindable |
| `KeepPanels` | `bool` | `false` | Keep inactive panels rendered (maps to `KeepPanelsAlive`) |
| `Class`, `AdditionalAttributes` | | | Extra class; splatted onto the `MudTabs` |

## MdsTabHeader parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Text` | `string?` | `null` | Tab label |
| `Count` | `int?` | `null` | Count badge, hidden when null |
| `Tone` | `MbxTone` | `Default` | Count tone |
| `Class`, `AdditionalAttributes` | | | Extra class; splatted attributes |

## Example

```razor
<MdsTabbedFill>
  <MudTabPanel>
    <TabContent><MdsTabHeader Text="Findings" Count="7" Tone="MbxTone.Danger" /></TabContent>
    <ChildContent>
      <MdsFillRegion><MudDataGrid .../></MdsFillRegion>
    </ChildContent>
  </MudTabPanel>
</MdsTabbedFill>
```
