# Copilot instructions for MudBlazorExt

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
