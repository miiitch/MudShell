# Model

MudShell is built around a small composition model rather than a monolithic app template.

## The model in one view

```text
MdsAppShell
├── navigation model
│   ├── MdsSidebar
│   └── optional MdsContextNavPanel / MbxNavTree
├── page chrome
│   └── MdsPageHeader
└── main content model
    ├── MdsMainToolbar
    ├── plain filter row on page background
    ├── MdsMainSection
    ├── MdsMainPart
    ├── MdsMainButton
    ├── MdsMainStatusBadge
    └── MdsMainEmptyState
```

Each layer has a distinct responsibility:

- `MdsAppShell` owns the application frame.
- navigation components own wayfinding.
- page components own information hierarchy.
- MudBlazor primitives still render the detailed content inside each section.

## Shell model

`MdsAppShell` is the outer frame. It receives:

- `SidebarContent`
- `ChildContent`
- optional `ContextPanelContent`

This lets you keep layout concerns out of individual pages.

## Navigation model

MudShell supports two navigation depths:

1. **primary navigation** in the sidebar,
2. **context navigation** in an optional right-side panel.

Use top-level entries for business areas. Use the context panel for sub-pages, variants, demos, or component families.

## Main-content model

For page bodies, MudShell works best when you separate:

1. **page identity** — `MdsPageHeader`
2. **page actions** — `MdsMainToolbar`
3. **filters** — chips on the page background
4. **content blocks** — `MdsMainSection` and `MdsMainPart`
5. **fallback states** — `MdsMainEmptyState`

This is the pattern used by the sample `MeteoPage`.

## State model

MudShell does not force a single state-management approach.

The sample app uses a scoped `ThemeState` service to coordinate:

- dark / light mode
- theme preset
- background mode
- background image URL

You can keep that pattern or bind values directly on `MdsAppShell`.

## Where MudShell stops

MudShell is intentionally not:

- a data grid abstraction,
- a form engine,
- a business workflow framework,
- a replacement for MudBlazor.

Use MudShell for layout and page composition, then keep using MudBlazor components inside those shells.
