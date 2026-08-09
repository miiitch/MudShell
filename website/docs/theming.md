---
sidebar_position: 2
---

# Theming

MudShell ships with curated `MudTheme` presets accessed via `MbxTheme`.

## Built-in presets

The default preset is `Cobalt`. The curated list also includes `Teal`, `Violet`, `Forest`, `Lime`, `Amber`, and `Crimson`.

```csharp
using MudShell.Theme;

MudTheme theme = MbxTheme.CreateTheme(); // default preset
MudTheme limeTheme = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.Lime);
MudTheme customTheme = MbxTheme.CreateThemeFromPrimary("#6f63ff");
```

## Dark theme usage

`MdsAppShell` already wires the theme, so explicit configuration is optional unless you need custom branding. The new `Lime` preset is the Orrik-style dark option.

## CSS custom properties

MudBlazor exposes its palette as CSS variables. MudShell components consume them:

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
  --mud-palette-primary: #d7f52b;
}
```