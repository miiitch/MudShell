# MbxFilterTabBar

Pill-style tab bar wrapping `MudToggleGroup`. Supports any type `T` and scrolls horizontally on mobile.

## Type parameter

`T` — the type of the tab value (typically `string`, `int`, or an enum).

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Value` | `T?` | `null` | Currently selected tab value |
| `ValueChanged` | `EventCallback<T>` | — | Two-way bind support |
| `Items` | `IEnumerable<MbxTabItem<T>>?` | `null` | Tab definitions |
| `TrailingContent` | `RenderFragment?` | `null` | Optional content after the last tab (e.g. a dropdown button) |

## MbxTabItem record

```csharp
public record MbxTabItem<T>(T Value, string Label);
```

## Example

```razor
<MbxFilterTabBar T="string" @bind-Value="_tab" Items="@_tabs">
  <TrailingContent>
    <MudIconButton Icon="@Icons.Material.Filled.KeyboardArrowDown" Size="Size.Small" />
  </TrailingContent>
</MbxFilterTabBar>

@code {
    private string _tab = "all";
    private readonly MbxTabItem<string>[] _tabs =
    [
        new("all",    "All"),
        new("images", "Images"),
        new("docs",   "Documents"),
    ];
}
```

## Responsive

On mobile (≤ 959 px), the tab bar scrolls horizontally (`overflow-x: auto`).
The scrollbar is hidden (`scrollbar-width: none`) for a clean look.
