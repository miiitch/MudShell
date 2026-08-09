---
sidebar_position: 2
---

# Theming

MudShell ships with curated `MudTheme` presets accessed via `MbxTheme`.

## Built-in presets

The default preset is `Cobalt`. The curated list also includes `Teal`, `Violet`, `Forest`, `Lime`, `Amber`, and `Crimson`.
The palette-based presets from the reference images also include `Ruby`, `Saffron`, `Azure`, `Magma Mist`, `Auburn Dunes`, `Olivine Matisse`, `Fern Green`, and `Peacock Dusk`.
Existing image-based presets still available are `Deep Charcoal`, `Earth Root`, `Obsidian Pulse`, `Onyx`, `Steel Mist`, `Obsidian Ink`, `Midnight Static`, `Storm Slate`, and `Abyssal Navy`.

```csharp
using MudShell.Theme;

MudTheme theme = MbxTheme.CreateTheme(); // default preset
MudTheme limeTheme = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.Lime);
MudTheme rubyTheme = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.Ruby);
MudTheme saffronTheme = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.Saffron);
MudTheme azureTheme = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.Azure);
MudTheme magmaMist = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.MagmaMist);
MudTheme fernGreen = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.FernGreen);
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