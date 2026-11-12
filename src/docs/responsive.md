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
- The tablet and mobile overlay starts at the edge of the icon rail. It does not follow a custom sidebar width: the rail is fixed at 56 px on tablet.
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


---

## Page content: wrapping, clipping and the block gap

Every page-level component adapts below 1280 px instead of clipping:

| Component | Tablet / mobile behaviour |
|---|---|
| `MdsPageHeader` | Grows in height; the title wraps; the actions sit under the title (< 600 px) or beside it (600-1279 px) and wrap |
| `MdsMainToolbar` | Buttons wrap onto new rows |
| `MdsMainSection` / `MdsMainPart` | Header actions wrap under the title |
| `MdsStatStrip` | Stays one row at every width and scrolls horizontally (thin scrollbar plus snap points) |
| `MdsFilterTabBar` | Scrolls horizontally with a thin scrollbar below 1280 px |
| `MdsSplitPane` | < 600 px: the side pane stacks under the main area |

### One vertical gap: `--mbx-block-gap`

`MdsAppShell` defines `--mbx-block-gap` (default `var(--mbx-space-4)`, so 16 px, 12 px in compact density) next to the other
`--mbx-*` tokens. Put sibling page blocks in a `.mbx-stack` (flex column, `gap: var(--mbx-block-gap)`):

```razor
<MudContainer MaxWidth="MaxWidth.False" Class="mbx-stack px-4 pb-4 pt-0">
  <MdsPageHeader Title="Orders" />
  <MdsMainToolbar>...</MdsMainToolbar>
  <MdsStatStrip>...</MdsStatStrip>
  <MdsMainSection Title="Latest">...</MdsMainSection>
</MudContainer>
```

Blocks (`MdsMainToolbar`, `MdsMainSection`, `MdsMainPart`, `MdsMainEmptyState`) no longer carry their own outer margin; the stack owns
the rhythm. `MdsPageHeader` keeps a `margin-bottom: var(--mbx-block-gap)` for use outside a stack. Do not add Mud spacing utilities
(`mb-3`, `my-2`: they are `!important`) to blocks inside a stack. `MudGrid` rows keep their own `Spacing` (use `4` to match 16 px).

### One horizontal edge: `--mbx-page-gutter`

`--mbx-page-gutter` (default `var(--mbx-space-4)`, 8 px in compact density) is the left/right padding of `.mbx-main-inner`. The page header
(which has no horizontal padding of its own), its actions and every block share that edge, so pages must not add their own gutter:
no `px-4` on blocks, and use `<MudContainer MaxWidth="MaxWidth.False" Gutters="false" Class="mbx-stack">` (a default `MudContainer`
adds its own 16-24 px gutter and a `MaxWidth` centres the content). Avoid an empty `<StartContent><div></div></StartContent>` in the
header: it adds a 12 px gap before the title.

### Pitfall: `::deep` on a component root (library contributors)

Blazor CSS isolation stamps its `b-xxxx` attribute on **plain HTML elements of the .razor file only**, never on the root element
of a child component (`MudToolBar`, `MudPaper`, `MudNavMenu`...). A rule such as `::deep .my-root.mud-toolbar { ... }`
(leading `::deep`, no scoped ancestor) or a bare `.my-root { ... }` on a Mud* element therefore matches nothing, silently, including
its `@media` variants. Fix: wrap the component in a layout-neutral host that owns the scope
(`<div class="mds-xxx-host">` with `display: contents`) and write `.mds-xxx-host ::deep .my-root { ... }`. Used by `MdsPageHeader`,
`MdsMainPart`, `MdsSidebarNav` and `MdsTabbedFill`.
