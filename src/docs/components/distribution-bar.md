# MdsDistributionBar

A 6 px stacked bar plus a compact legend. Shows the top `MaxSegments` segments and folds the rest into
"Other". Segments use shades of a single colour ramp by rank (no per-category rainbow) unless a segment
sets a `Tone`.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Segments` | `IEnumerable<MbxDistributionSegment>?` | `null` | `MbxDistributionSegment(string Label, double Value, string? Icon = null, MbxTone Tone = Default)`. Non-positive values are ignored |
| `MaxSegments` | `int` | `5` | Segments shown individually |
| `OtherLabel` | `string` | `"Other"` | Label of the folded segment |
| `Class`, `AdditionalAttributes` | | | Extra class; splatted attributes |

## Example

```razor
<MdsDistributionBar Segments="@_segments" MaxSegments="4" />

@code {
  private readonly List<MbxDistributionSegment> _segments =
  [
      new("Pass", 41, Tone: MbxTone.Success),
      new("Fail", 7, Tone: MbxTone.Danger),
      new("N/A", 5),
  ];
}
```

Use tones only for state (pass/fail), never to tell categories apart.
