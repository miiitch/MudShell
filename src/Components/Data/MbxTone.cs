namespace MudShell.Components.Data;

/// <summary>
/// Semantic state tone shared by the data blocks. Colour in MudShell carries state only:
/// use anything other than <see cref="Default"/> exclusively for abnormal (or explicitly
/// healthy) values, never to categorise.
/// </summary>
public enum MbxTone
{
    Default,
    Success,
    Warning,
    Danger,
}

/// <summary>Direction of a delta shown next to a stat value.</summary>
public enum MbxDeltaTone
{
    Neutral,
    Positive,
    Negative,
}

/// <summary>Visual variant of <c>MdsStatTile</c>.</summary>
public enum MbxStatTileVariant
{
    /// <summary>Bordered card, label over a large value. For stat strips and KPI widgets.</summary>
    Tile,
    /// <summary>Borderless compact label-over-value cell. For detail-header metric rows.</summary>
    Cell,
}

/// <summary>One segment of <c>MdsDistributionBar</c>.</summary>
/// <param name="Label">Legend label.</param>
/// <param name="Value">Weight of the segment (non-positive values are ignored).</param>
/// <param name="Icon">Optional MudBlazor icon (SVG path string) shown in the legend.</param>
/// <param name="Tone">When not <see cref="MbxTone.Default"/>, overrides the rank-based ramp shade.</param>
public sealed record MbxDistributionSegment(string Label, double Value, string? Icon = null, MbxTone Tone = MbxTone.Default);

/// <summary>Outcome of one run in <c>MdsRunStrip</c>.</summary>
public enum MbxRunOutcome
{
    Success,
    Failed,
    Running,
    Cancelled,
    Queued,
}

/// <summary>One dot of <c>MdsRunStrip</c>.</summary>
/// <param name="Outcome">Run outcome.</param>
/// <param name="Tooltip">Tooltip text; defaults to the outcome name.</param>
public sealed record MbxRun(MbxRunOutcome Outcome, string? Tooltip = null);

/// <summary>One labelled value in <c>MdsKeyValueGrid</c>.</summary>
/// <param name="Key">Short label displayed above the value.</param>
/// <param name="Value">Plain-text value, used when <paramref name="Content"/> is not supplied.</param>
/// <param name="Tone">Optional semantic state tone.</param>
/// <param name="Mono">Whether to display the value in a monospace font.</param>
/// <param name="Href">Optional destination for a linked plain-text value.</param>
/// <param name="Content">Optional rich value that takes precedence over <paramref name="Value"/> and <paramref name="Href"/>.</param>
public sealed record MbxKeyValue(
    string Key,
    string? Value,
    MbxTone Tone = MbxTone.Default,
    bool Mono = false,
    string? Href = null,
    Microsoft.AspNetCore.Components.RenderFragment? Content = null);

/// <summary>One generic counted item displayed by <c>MdsAttentionBlock</c>.</summary>
/// <param name="Label">Description of the item.</param>
/// <param name="Count">Count shown alongside the label; non-positive values are omitted.</param>
/// <param name="Tone">Semantic state tone.</param>
/// <param name="Href">Optional destination for the item.</param>
public sealed record MbxAttentionItem(
    string Label,
    int Count,
    MbxTone Tone = MbxTone.Warning,
    string? Href = null);
