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

### Desktop (md+)
- **Sidebar** visible, icon-only (56 px wide)
- Toggle button expands to 240 px with labels
- **BottomNav** hidden (`display: none` on `.mbx-bottom-nav-slot`)

### Mobile (xs / sm — ≤ 959 px)
- **Sidebar** hidden (`display: none` on `.mbx-sidebar`)
- **BottomNav** appears — fixed at the bottom, 56 px tall
- `app-main` loses its margin and border-radius → full-bleed content
- `mbx-main-inner` gets `padding-bottom: 56px` to avoid content hidden behind BottomNav

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