# MdsRunStrip

Last-N run outcomes as small dots, oldest to newest, for grid cells. Each dot has a `title` tooltip.

| Outcome | Look |
|---|---|
| `Success` | muted green dot (quiet nominal state) |
| `Failed` | red dot |
| `Running` | hollow ring |
| `Cancelled` | hollow grey ring |
| `Queued` | faint grey dot |

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Runs` | `IEnumerable<MbxRun>?` | `null` | `MbxRun(MbxRunOutcome Outcome, string? Tooltip = null)`, oldest first. Tooltip defaults to the outcome name |
| `Max` | `int` | `10` | Only the last `Max` runs are shown |
| `Class`, `AdditionalAttributes` | | | Extra class; splatted attributes |

```razor
<MdsRunStrip Runs="@row.Runs" Max="8" />
```

Native `title` tooltips are used on purpose: a grid can show hundreds of strips and a popover per dot is too heavy.
