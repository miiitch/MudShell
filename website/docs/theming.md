---
sidebar_position: 2
---

# Theming

MudShell ships with curated `MudTheme` presets accessed via `MbxTheme`.

## Built-in presets

The default preset is `Cobalt`. The curated list also includes `Teal`, `Violet`, `Forest`, `Lime`, `Amber`, and `Crimson`.
The palette-based presets from the reference images also include `Ruby`, `Saffron`, `Azure`, `Magma Mist`, `Auburn Dunes`, `Olivine Matisse`, `Fern Green`, and `Peacock Dusk`.
Existing image-based presets still available are `Deep Charcoal`, `Earth Root`, `Obsidian Pulse`, `Onyx`, `Midnight Static`, `Storm Slate`, and `Abyssal Navy`.

```csharp
using MudShell.Theme;

MudTheme theme = MbxTheme.CreateTheme(); // default preset
MudTheme limeTheme = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.Lime);
MudTheme crimsonTheme = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.Crimson);
MudTheme saffronTheme = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.Saffron);
MudTheme cobaltTheme = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.Cobalt);
MudTheme magmaMist = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.MagmaMist);
MudTheme fernGreen = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.FernGreen);
MudTheme customTheme = MbxTheme.CreateThemeFromPrimary("#6f63ff");
```

## Explicit theme definition

The presets derive every color from one primary. When a product needs its own color code, write each role out for both
modes and pass the definition to `CreateTheme`. Values are used as given: nothing is derived and no contrast correction
is applied, so check the palette for readability yourself.

```csharp
MudTheme theme = MbxTheme.CreateTheme(new MbxThemeDefinition(
    Dark: new MbxPaletteDefinition
    {
        Primary = "#FFA500", Secondary = "#6CB6FF",
        Info = "#6CB6FF", Success = "#3FCF8E", Warning = "#F5C542", Error = "#FF7B72",
        Background = "#090B10", PageBackground = "#0D1017", Surface = "#151A24", NavigationBackground = "#10141C",
        TextPrimary = "#E6EAF2", TextSecondary = "#A7B0C4", Border = "#2A3242",
    },
    Light: new MbxPaletteDefinition
    {
        Primary = "#FFA500", Secondary = "#0B62C4",
        Info = "#0B62C4", Success = "#17794A", Warning = "#8A6100", Error = "#C62828",
        Background = "#E7EBF3", PageBackground = "#F4F6FA", Surface = "#FFFFFF", NavigationBackground = "#EEF1F7",
        TextPrimary = "#1A2030", TextSecondary = "#4E586E", Border = "#D5DAE5",
    }));
```

## Colour roles

By default the primary colour does several jobs: it marks the main action, and it also tints hover, selection, active
navigation, the focus ring and card surfaces. With a strong primary the whole page takes its hue.

`ColorRoles="MdsColorRoles.Split"` on `MdsAppShell` separates the jobs. The primary keeps the main action, the
secondary colours every interactive state, and card surfaces are no longer tinted. It is opt-in; the default
(`Unified`) renders exactly as before.

```razor
<MdsAppShell ColorRoles="MdsColorRoles.Split" ...>
```

Components read the interactive colour from one token, which you can also set yourself:

| Variable | Usage |
|---|---|
| `--mbx-interactive` | Hover, selection, active navigation, focus ring. Primary by default, secondary with `Split`. |

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