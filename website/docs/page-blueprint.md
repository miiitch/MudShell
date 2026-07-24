---
sidebar_position: 7
---

# Page Blueprint

This blueprint is based on the sample `MeteoPage` and is the recommended way to compose a MudShell page.

## The structure

```text
MudContainer
├── MdsPageHeader
├── MdsMainToolbar            ← actions only
├── MudStack + MudChip        ← filters only, on page background
├── MdsMainPart / Section     ← content block 1
└── MdsMainPart / Section     ← content block 2
```

## Design rules

1. **Header first** — use `MdsPageHeader` for page identity, breadcrumbs, and top-level page metadata.
2. **Action toolbar second** — use `MdsMainToolbar` only for actions and UI configuration.
3. **Filters below** — render filters as `MudChip` items in a horizontal `MudStack`.
4. **No paper wrapper around filters** — the filter row sits directly on the page background.
5. **Content blocks next** — use `MdsMainPart` or `MdsMainSection` for the body.

## Example

```razor
@page "/weather"

<MudContainer MaxWidth="MaxWidth.False" Class="px-4 pb-4 pt-0">

    <MdsPageHeader Title="Weather"
                   Icon="@Icons.Material.Outlined.Cloud"
                   BreadcrumbItems="@_breadcrumbs">
        <HeaderActions>
            <MudIconButton Icon="@Icons.Material.Outlined.ColorLens"
                           Size="Size.Small"
                           Color="Color.Inherit"
                           title="Theme and background"
                           OnClick="@OpenThemeDialog" />
        </HeaderActions>
    </MdsPageHeader>

    <MdsMainToolbar AccentBorderSides="MbxAccentBorderSides.Left"
                    AccentBorderWidth="3">
        <MudButton Variant="Variant.Outlined"
                   StartIcon="@Icons.Material.Outlined.RestartAlt"
                   OnClick="@ResetFilters">
            Reset filters
        </MudButton>
        <MudSpacer />
        <MudSwitch T="bool" @bind-Value="_isDense" Label="Dense table" />
    </MdsMainToolbar>

    <MudStack Row="true"
              AlignItems="AlignItems.Center"
              Spacing="1"
              Wrap="MudBlazor.Wrap.Wrap"
              Class="mb-3">
        <MudText Typo="Typo.caption" Color="Color.Secondary">Period</MudText>
        <MudChip T="string" OnClick="@(() => _selectedPeriod = "today")">Today</MudChip>
        <MudChip T="string" OnClick="@(() => _selectedPeriod = "week")">This week</MudChip>
        <MudChip T="string" OnClick="@(() => _selectedPeriod = "month")">This month</MudChip>
    </MudStack>

    <MdsMainPart Title="Summary"
                 Icon="@Icons.Material.Outlined.WbSunny"
                 AccentBorderSides="@(MbxAccentBorderSides.Top | MbxAccentBorderSides.Left)"
                 AccentBorderColor="#5a72ff"
                 AccentBorderWidth="4">
        ...
    </MdsMainPart>

    <MdsMainPart Title="Hourly forecast"
                 Icon="@Icons.Material.Outlined.Schedule"
                 AccentBorderSides="MbxAccentBorderSides.Left">
        ...
    </MdsMainPart>

</MudContainer>
```

## When to use each wrapper

### `MdsPageHeader`

Use for:

- page title,
- breadcrumbs,
- page-level badges,
- page-level header actions.

### `MdsMainToolbar`

Use for:

- create/export/reset actions,
- density toggles,
- view mode toggles,
- non-filter UI controls.

Do not put business filters here unless there is a strong reason not to separate them.

### Filter `MudStack`

Use for:

- period chips,
- status chips,
- region/category chips,
- low-friction filter toggles.

### `MdsMainPart`

Use for:

- cards,
- tables,
- charts,
- API-backed page blocks with loading and error states.

### `MdsMainSection`

Use for:

- broader grouped content,
- a section with description and divider,
- component galleries or grouped page areas.

## Practical guidance

- Keep actions sparse and readable.
- Keep filters scannable and horizontal.
- Let MudBlazor controls render the actual content details.
- Prefer multiple small `MdsMainPart` blocks over one oversized page slab.
- Keep page padding at the container level (`MudContainer Class="px-4 pb-4 pt-0"` in the sample).
- Use accent borders sparingly to create hierarchy (`AccentBorderSides`, `AccentBorderColor`, `AccentBorderWidth`).
- Accent border width is clamped to **6px max** by design token.
