# MdsStatTile and MdsStatStrip

`MdsStatTile` shows one number: label, value (tabular numerals), optional delta. `MdsStatStrip` lays tiles out
on **one row** that scrolls horizontally instead of wrapping.

**Colour is state only.** Use a non-default `Tone` only for abnormal values (or an explicitly healthy one);
a nominal tile stays neutral.

## MdsStatTile parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Label` | `string?` | `null` | Small caption |
| `Value` | `string?` | `null` | Plain value |
| `ValueContent` | `RenderFragment?` | `null` | Rich value (overrides `Value`) |
| `Delta` | `string?` | `null` | Delta text, e.g. `+12` |
| `DeltaTone` | `MbxDeltaTone` | `Neutral` | `Neutral`, `Positive`, `Negative` |
| `Tone` | `MbxTone` | `Default` | `Default`, `Success`, `Warning`, `Danger` |
| `OnClick` | `EventCallback<MouseEventArgs>` | none | When bound, renders a `<button>` with hover and focus ring |
| `Loading` | `bool` | `false` | Skeleton instead of the value |
| `Variant` | `MbxStatTileVariant` | `Tile` | `Tile` (bordered card) or `Cell` (compact label-over-value, for detail-header metric rows) |
| `Class`, `AdditionalAttributes` | | | Extra class; splatted attributes |

## MdsStatStrip parameters

| Parameter | Type | Description |
|---|---|---|
| `ChildContent` | `RenderFragment?` | Tiles |
| `Class`, `AdditionalAttributes` | | Extra class; splatted attributes |

## Example

```razor
<MdsStatStrip>
  <MdsStatTile Label="Total" Value="@_total.ToString("N0")" />
  <MdsStatTile Label="Stale > 7d" Value="387" Tone="MbxTone.Warning" OnClick="@(() => ApplyFilter("stale"))" />
  <MdsStatTile Label="Score" Value="84" Delta="+2" DeltaTone="MbxDeltaTone.Positive" />
</MdsStatStrip>

@* Detail header metric row *@
<MdsStatStrip>
  <MdsStatTile Variant="MbxStatTileVariant.Cell" Label="Owner" Value="platform" />
  <MdsStatTile Variant="MbxStatTileVariant.Cell" Label="Findings" Value="7" Tone="MbxTone.Danger" />
</MdsStatStrip>
```
