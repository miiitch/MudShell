# Copilot instructions for MudShell

This repository contains **MudShell**, a Razor Class Library of Blazor UI components built on MudBlazor 9.

When working in this repository or in a project that references `MudShell`, consult the integration skill for component APIs, setup steps, and usage patterns:

- [MudShell integration skill](.github/copilot/skills/mudshell-integration.md)

## Repository structure

- `src/MudShell/` — the library (Razor Class Library)
- `samples/` — demo Blazor Web App
- `website/` — Docusaurus documentation site
- `.github/workflows/` — CI (build+pack) and CD (NuGet publish) pipelines

## Key rules

- The library targets **net10.0** and **MudBlazor 9.x**.
- Do **not** call `AddMudServices()` alongside `AddMudShell()` — the latter already includes it.
- All components use **scoped CSS** (`.razor.css`) — avoid global style overrides.
- Component namespace pattern: `MudShell.Components.<ComponentName>`.

## UI design preference (sample app)

Use the `MeteoPage` layout as the default pattern when a page has actions and filters:

- **Top bar (`MdsMainToolbar`)**: actions only. Keep primary actions on the left and UI config on the right (`MudSpacer` between groups).
- **Filter row below**: place filters in a plain horizontal `MudStack` with `MudChip` items.
- **No paper/border** for the filter row: chips should sit directly on the page background.
- Keep this structure consistent across pages that expose filtering.
