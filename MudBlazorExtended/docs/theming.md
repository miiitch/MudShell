# Theming

MudBlazorExtended ships with a pre-configured `MudTheme` accessed via `MbxTheme`.

## Using the built-in dark theme

`MbxAppShell` calls `MbxTheme.CreateDarkTheme()` internally — you don't need to configure anything.

## Accessing the palettes directly

```csharp
using MudBlazorExtended.Theme;

// Read a token
string primary = MbxTheme.DarkPalette.Primary; // "#7e6fff"
```

## Creating a custom theme

Override individual tokens by starting from the built-in palette:

```csharp
var myTheme = MbxTheme.CreateDarkTheme();
myTheme.PaletteDark.Primary = "#ff6b6b";
myTheme.PaletteDark.Surface = "#1a1a2e";
```

Then pass it to `MbxAppShell` — add a `Theme` parameter if needed, or extend the component in your own project.

## CSS custom properties

MudBlazor exposes its palette as CSS variables. MudBlazorExtended components consume them:

| Variable | Usage |
|---|---|
| `--mud-palette-background` | App background |
| `--mud-palette-surface` | Card / sidebar background |
| `--mud-palette-primary` | Accent colour (active state, borders on hover) |
| `--mud-palette-text-primary` | Main text |
| `--mud-palette-text-secondary` | Muted text, icons |
| `--mud-palette-divider` | Dividers, borders |
| `--mud-palette-lines-default` | Card borders |

You can override any of these in your own `app.css`:

```css
:root {
  --mud-palette-primary: #ff6b6b;
}
```
