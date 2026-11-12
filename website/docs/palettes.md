---
sidebar_position: 3
---

# Palettes

This page lists every curated MudShell palette in alphabetical order.

## Maintenance note

When you add a new palette, add:

- the palette name,
- a light preview image,
- a dark preview image,
- a short note if needed.

Keep this page sorted alphabetically so all palettes stay easy to compare in one view.

## Generating previews

The sample app accepts `?preset=<MbxThemePreset>&mode=light|dark` in the URL. The `ThemeCapture` UI tests use it to screenshot every preset in both modes:

```powershell
dotnet run --project samples/MyApplication --launch-profile http
$env:PLAYWRIGHT_TEST_BASE_URL = 'http://localhost:5265'
dotnet test tests/MudShell.UITests --filter "Category=ThemeCapture"
```

Screenshots land in `tests/MudShell.UITests/bin/Debug/net10.0/screenshots/themes/`.

## Contrast guarantees

Every preset is generated through the same pipeline, which enforces WCAG contrast automatically (dark surfaces are capped in luminance, accents reach 3:1 on every surface, and contrast text reaches 4.5:1 on its accent). `MbxThemeContrastTests` guards these rules for every preset in both modes.

## Retired presets

These presets were visually too close to another one. They are marked `[Obsolete]` and resolve to their replacement:

| Retired | Now resolves to |
|---|---|
| Azure | Cobalt |
| Berry Red, Ruby | Crimson |
| Cotton Rose | Fairy Tale |
| Deep Space Blue | Abyssal Navy |
| Ganache, Midnight Static, Shadow Grey | Deep Charcoal |

## Palette gallery

| Theme | Light | Dark |
|---|---|---|
| Abyssal Navy | ![Abyssal Navy light palette](/img/palettes/abyssal-navy-light.png) | ![Abyssal Navy dark palette](/img/palettes/abyssal-navy-dark.png) |
| Amber | ![Amber light palette](/img/palettes/amber-light.png) | ![Amber dark palette](/img/palettes/amber-dark.png) |
| Auburn Dunes | ![Auburn Dunes light palette](/img/palettes/auburn-dunes-light.png) | ![Auburn Dunes dark palette](/img/palettes/auburn-dunes-dark.png) |
| Chartreuse | ![Chartreuse light palette](/img/palettes/chartreuse-light.png) | ![Chartreuse dark palette](/img/palettes/chartreuse-dark.png) |
| Cosmic Night | ![Cosmic Night light palette](/img/palettes/cosmic-night-light.png) | ![Cosmic Night dark palette](/img/palettes/cosmic-night-dark.png) |
| Crimson | ![Crimson light palette](/img/palettes/crimson-light.png) | ![Crimson dark palette](/img/palettes/crimson-dark.png) |
| Deep Charcoal | ![Deep Charcoal light palette](/img/palettes/deep-charcoal-light.png) | ![Deep Charcoal dark palette](/img/palettes/deep-charcoal-dark.png) |
| Earth Root | ![Earth Root light palette](/img/palettes/earth-root-light.png) | ![Earth Root dark palette](/img/palettes/earth-root-dark.png) |
| Electric Rose | ![Electric Rose light palette](/img/palettes/electric-rose-light.png) | ![Electric Rose dark palette](/img/palettes/electric-rose-dark.png) |
| Fairy Tale | ![Fairy Tale light palette](/img/palettes/fairy-tale-light.png) | ![Fairy Tale dark palette](/img/palettes/fairy-tale-dark.png) |
| Fern Green | ![Fern Green light palette](/img/palettes/fern-green-light.png) | ![Fern Green dark palette](/img/palettes/fern-green-dark.png) |
| Giants Orange | ![Giants Orange light palette](/img/palettes/giants-orange-light.png) | ![Giants Orange dark palette](/img/palettes/giants-orange-dark.png) |
| Hot Fuchsia | ![Hot Fuchsia light palette](/img/palettes/hot-fuchsia-light.png) | ![Hot Fuchsia dark palette](/img/palettes/hot-fuchsia-dark.png) |
| Icy Blue | ![Icy Blue light palette](/img/palettes/icy-blue-light.png) | ![Icy Blue dark palette](/img/palettes/icy-blue-dark.png) |
| Lavender Fog | ![Lavender Fog light palette](/img/palettes/lavender-fog-light.png) | ![Lavender Fog dark palette](/img/palettes/lavender-fog-dark.png) |
| Lime | ![Lime light palette](/img/palettes/lime-light.png) | ![Lime dark palette](/img/palettes/lime-dark.png) |
| Magma Mist | ![Magma Mist light palette](/img/palettes/magma-mist-light.png) | ![Magma Mist dark palette](/img/palettes/magma-mist-dark.png) |
| Morning Butter | ![Morning Butter light palette](/img/palettes/morning-butter-light.png) | ![Morning Butter dark palette](/img/palettes/morning-butter-dark.png) |
| Obsidian Pulse | ![Obsidian Pulse light palette](/img/palettes/obsidian-pulse-light.png) | ![Obsidian Pulse dark palette](/img/palettes/obsidian-pulse-dark.png) |
| Olivine Matisse | ![Olivine Matisse light palette](/img/palettes/olivine-matisse-light.png) | ![Olivine Matisse dark palette](/img/palettes/olivine-matisse-dark.png) |
| Onyx | ![Onyx light palette](/img/palettes/onyx-light.png) | ![Onyx dark palette](/img/palettes/onyx-dark.png) |
| Peacock Dusk | ![Peacock Dusk light palette](/img/palettes/peacock-dusk-light.png) | ![Peacock Dusk dark palette](/img/palettes/peacock-dusk-dark.png) |
| Purple Mood | ![Purple Mood light palette](/img/palettes/purple-mood-light.png) | ![Purple Mood dark palette](/img/palettes/purple-mood-dark.png) |
| Raspberry Red | ![Raspberry Red light palette](/img/palettes/raspberry-red-light.png) | ![Raspberry Red dark palette](/img/palettes/raspberry-red-dark.png) |
| Saffron | ![Saffron light palette](/img/palettes/saffron-light.png) | ![Saffron dark palette](/img/palettes/saffron-dark.png) |
| Sandy Clay | ![Sandy Clay light palette](/img/palettes/sandy-clay-light.png) | ![Sandy Clay dark palette](/img/palettes/sandy-clay-dark.png) |
| Storm Slate | ![Storm Slate light palette](/img/palettes/storm-slate-light.png) | ![Storm Slate dark palette](/img/palettes/storm-slate-dark.png) |