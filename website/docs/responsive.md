---
sidebar_position: 3
---

# Responsive Design

MudShell follows **MudBlazor's breakpoints** (Material Design):

| Alias | Range | Layout behaviour |
|---|---|---|
| `xs` | 0 – 599 px | Mobile portrait |
| `sm` | 600 – 959 px | Mobile landscape / small tablet |
| `md` | 960 – 1279 px | Tablet |
| `lg` | 1280 – 1919 px | Desktop |
| `xl` | ≥ 1920 px | Large desktop |

---

## Shell behaviour

`MdsAppShell` renders three layout tiers. The breakpoints are the ones above: mobile below 600 px, tablet from 600 to 1279 px, desktop from 1280 px.

| Tier | Level 1 (sidebar / bar) | Level 2 (context panel) |
|---|---|---|
| Desktop (≥ 1280 px) | Sidebar rail, 56 px, expands to 240 px with labels | Side panel, 72 px collapsed, 288 px expanded |
| Tablet (600–1279 px) | Icon rail fixed at 56 px, no expanded mode | Overlay sliding from the rail over the content, with a scrim. Content keeps its full width. Closes on an outside tap, Esc, or navigation |
| Mobile (< 600 px) | No rail. Top bar (menu button, title, actions) and a bottom bar with up to five entries plus "Plus" for the overflow | Bottom sheet, at most 70 vh, above the bottom bar, with a scrim and a grip. Sub-sections drill down with a back row |

### Choosing the tier
- `ResponsiveMode="Auto"` (default): CSS media queries set the visuals. A `matchMedia` observer in `mudshell-layout.js` reports the tier to .NET. It is exposed as `ShellMode` (`MbxShellMode.Desktop`, `Tablet` or `Mobile`), cascaded to descendants as `MbxShellMode`, and raised through `ShellModeChanged`.
- `ResponsiveMode="Desktop"`: the desktop layout at every width. No observer and no tier-specific rules.
- Until the observer reports, the tier is `Desktop`. Visuals already follow the viewport through CSS in the meantime.

### Rendering by tier
- Choose between the rail and the bottom bar from `ShellMode`. Render `MdsBottomNav` outside `MdsAppShell`, so the bar stays pinned to the bottom edge (see the sample `MainLayout`).
- On tablet the rail's menu button has no expanded state to toggle. Call `ToggleContextPanel()` there instead of `ToggleSidebar()`.
- When the tier leaves desktop, the context panel closes so it does not cover the content on load.

### Accessibility
- The overlay and the sheet use `role="dialog"` and `aria-modal="true"`. Focus moves into them and stays there, Esc closes them, and focus returns to the element that opened them.
- Touch targets are at least 44 px.
- Slide transitions are disabled under `prefers-reduced-motion: reduce`.

### Test ids

| `data-testid` | Element |
|---|---|
| `shell-mode` | Shell root. Also carries `data-shell-mode` set to `desktop`, `tablet` or `mobile` |
| `mobile-menu-button` | Menu button of the mobile top bar |
| `nav-scrim` | Scrim behind the tablet overlay or the mobile sheet |
| `context-overlay` | Level-2 panel of the shell (overlay on tablet) |
| `context-sheet` | Level 2 and 3 bottom sheet on mobile |
| `bottom-nav` | Mobile bottom bar |
| `bottom-nav-more` | "Plus" overflow entry |
| `drilldown-back` | Back row inside the mobile sheet |

---

## Component-level behaviour

### MdsChatBar
- Desktop: `max-width` defaults to `680px`, centred
- Mobile: `max-width: 100%`, reduced padding (`12px 14px`)

### MdsFilterTabBar
- Desktop: normal flex row
- Mobile: horizontal scroll (`overflow-x: auto`, `scrollbar-width: none`)

### MdsPageHeader
- Desktop: three-column flex (`start | title | end`)
- Mobile (≤ 599 px): wraps — start, then title (full width, centred), then end

### MdsDocumentCard
The card itself is not opinionated about layout width.
Recommended `MudGrid` usage:

```razor
<MudGrid Spacing="3">
  @foreach (var doc in docs)
  {
    <MudItem xs="12" sm="6" md="4" lg="3">
      <MdsDocumentCard ... />
    </MudItem>
  }
</MudGrid>
```

---

## Adding breakpoint logic in C#

Use MudBlazor's `IBreakpointService` for programmatic checks:

```csharp
@inject IBreakpointService BreakpointService

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        var bp = await BreakpointService.GetCurrentBreakpointAsync();
        _isMobile = bp is Breakpoint.Xs or Breakpoint.Sm;
    }
}
```