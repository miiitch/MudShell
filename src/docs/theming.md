# Theming

MudShell uses a generated `MudTheme` from `MbxTheme` and exposes palette tokens as CSS variables consumed by components.

## Creating the built-in theme

```csharp
using MudShell.Theme;

MudTheme theme = MbxTheme.CreateTheme(); // default preset
MudTheme emeraldTheme = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.Emerald);
MudTheme customTheme = MbxTheme.CreateThemeFromPrimary("#6f63ff");
```

`MdsAppShell` already wires the theme, so explicit configuration is optional unless you need custom branding.

## Contrast policy

- Target **WCAG AA for text** across light/dark surfaces.
- Decorative chips/badges may use softer contrast when they do not carry critical text.
- Keep branding gradients fixed, but ensure adjacent text remains readable.
- Keep a stable hierarchy: page background < secondary background < content surface.
- In light mode, content surfaces should stay near white; in dark mode, keep dark surfaces slightly tinted by theme.

## Recommended token mapping

| Usage | Preferred token |
|---|---|
| App/page background | `--mud-palette-background` |
| Elevated surface (cards, panels) | `--mud-palette-surface` |
| Secondary surface / contextual panels | `--mud-palette-background-gray` |
| Primary action / active state | `--mud-palette-primary` |
| Main text | `--mud-palette-text-primary` |
| Muted text / icons | `--mud-palette-text-secondary` |
| Borders / separators | `--mud-palette-lines-default`, `--mud-palette-divider` |
| Overlays / shadows | `--mud-palette-overlay-light` |
| Status accents | `--mud-palette-info`, `--mud-palette-success`, `--mud-palette-warning`, `--mud-palette-error` |

## Component styling guidance

1. Prefer palette variables over hardcoded hex/rgba values.
2. For subtle hovers/tints, use `color-mix` with palette tokens (for example, primary mixed with transparent).
3. Keep visual behavior consistent between `src/` components and `samples/` showcases.
4. For spacing rhythm, prefer MudBlazor utility-style conventions (`ma/mx/my`, `pa/px/py`) to avoid ad-hoc inline spacing.
