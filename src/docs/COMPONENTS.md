# Generic data components

The generic data components are available from `MudShell.Components.Data`.
They use MudBlazor controls and the shared `MbxTone` state vocabulary.

Add this namespace to your `_Imports.razor` to use the components below:

```razor
@using MudShell.Components.Data
```

## `MdsKeyValueGrid`

Renders labelled values as a one- or two-column definition list. `Content` takes
precedence over `Href` and `Value`; otherwise, a nonblank `Href` makes the value
a link. Empty plain-text values display as an em dash.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Items` | `IEnumerable<MbxKeyValue>?` | `null` | Values to render |
| `Columns` | `int` | `1` | Number of columns, clamped to one or two |
| `Class`, `AdditionalAttributes` | | | Extra root class and splatted attributes |

`MbxKeyValue` accepts `Key`, `Value`, optional `Tone`, `Mono`, `Href`, and rich
`Content`.

```razor
<MdsKeyValueGrid Items="@_values" Columns="2" />

@code {
    private readonly MbxKeyValue[] _values =
    [
        new("Owner", "platform"),
        new("Build", "a1b2c3d4", Mono: true, Href: "/builds/a1b2c3d4"),
        new("State", "Failed", Tone: MbxTone.Danger),
    ];
}
```

## `MdsCopyableValue`

Displays a value in a compact monospace style and provides a button that copies
the complete value to the clipboard. Long values show a shortened visual form
while retaining the full value for assistive technology and in the copy action.
A link is rendered only when `Href` is nonblank. If clipboard access is denied,
the component shows a warning snackbar and leaves the value available to copy
manually.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Value` | `string` | required | Complete value to display and copy |
| `CopyLabel` | `string` | `"Copy to clipboard"` | Accessible button label and tooltip |
| `Href` | `string?` | `null` | Optional link destination |
| `Class`, `AdditionalAttributes` | | | Extra root class and splatted attributes |

The host must register MudBlazor services and provide a snackbar provider as
described in [Getting started](getting-started.md). Copying also requires the
browser Clipboard API and its permission.

```razor
<MdsCopyableValue Value="@commitHash" Href="@commitUrl" />
```

## `MdsAttentionBlock`

Shows generic positive-count items as compact status messages. It renders a
status region, filters out items whose counts are zero or negative, and applies
the item's `MbxTone`. `Href` is optional; items without one are not rendered as
links.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Items` | `IEnumerable<MbxAttentionItem>?` | `null` | Generic items to render |
| `Class`, `AdditionalAttributes` | | | Extra root class and splatted attributes |

`MbxAttentionItem` accepts `Label`, `Count`, optional `Tone` (default
`MbxTone.Warning`), and optional `Href`.

```razor
<MdsAttentionBlock Items="@_attentionItems" />

@code {
    private readonly MbxAttentionItem[] _attentionItems =
    [
        new("failed checks", 2, MbxTone.Danger, "/checks"),
        new("pending reviews", 1, MbxTone.Warning),
    ];
}
```
