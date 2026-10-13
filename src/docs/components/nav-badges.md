# Navigation badges

`MbxNavNode.Badge` (`MbxNavBadge`) is rendered by `MdsContextNavPanel` and `MdsSidebarNav`.

```csharp
public sealed record MbxNavBadge(
    string? Text = null,
    Color Color = Color.Default,
    Variant Variant = Variant.Filled,
    bool Dot = false,
    string? Tooltip = null);   // new in 0.5.0, trailing and optional (non-breaking)
```

| Where | Rendering |
|---|---|
| `MdsContextNavPanel`, expanded, every level | Text badge right-aligned before `TrailingActions`: small text tinted by `Color` (neutral for `Default`). `Dot = true` renders a 6 px dot |
| `MdsContextNavPanel`, collapsed | Dot only, overlaid on the item icon |
| `MdsSidebarNav` | Dot only: overlaid on the icon when collapsed, right-aligned when expanded |

A node without a badge renders nothing. Keep with the colour rule: tint a badge only for abnormal state
(`Error`, `Warning`); use `Default` for neutral counts.

```csharp
new MbxNavNode("jobs", "Jobs", href: "/jobs",
    badge: new MbxNavBadge("12", Color.Error, Tooltip: "12 failed jobs"));
```
