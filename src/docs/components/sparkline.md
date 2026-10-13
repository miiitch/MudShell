# MdsSparkline

Inline SVG trend line for grid cells and tiles. Min-max scaled; a flat series renders a centred line.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Values` | `IReadOnlyList<double>?` | `null` | At least two values to draw |
| `Width` / `Height` | `int` | `80` / `20` | Size in px |
| `Tone` | `MbxTone` | `Default` | Neutral by default; tone only for abnormal trends |
| `ShowLastPoint` | `bool` | `true` | Dot on the latest value |
| `AriaLabel` | `string?` | `"Trend"` | Accessible label |
| `Class`, `AdditionalAttributes` | | | Extra class; splatted attributes |

```razor
<MdsSparkline Values="@row.Last12Weeks" Tone="@(row.Declining ? MbxTone.Danger : MbxTone.Default)" />
```
