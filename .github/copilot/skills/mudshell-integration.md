# MudShell — Integration and Page Structure Skill

> Use this skill when integrating **MudShell** into an existing **MudBlazor** app, when building new pages with MudShell conventions, or when refactoring pages to the recommended structure.

---

## What MudShell is

MudShell is a Razor Class Library built on **MudBlazor 9** for app-shell and page composition patterns.

Targets:

- **.NET 10**
- Blazor Web App / Server / WebAssembly scenarios

MudShell is best understood as three layers:

1. **shell** — `MdsAppShell`
2. **navigation** — `MdsSidebar`, `MdsContextNavPanel`, `MbxNavTree`
3. **page composition** — `MdsPageHeader`, `MdsMainToolbar`, `MdsMainPart`, `MdsMainSection`, `MdsMainEmptyState`

It does **not** replace MudBlazor content components. Use MudShell for layout and composition, then keep using `MudGrid`, `MudTable`, `MudForm`, `MudChip`, and other MudBlazor primitives inside those wrappers.

---

## Integration into an existing MudBlazor app

### 1. Add the package

```xml
<PackageReference Include="MudShell" Version="0.*" />
```

Or with a project reference:

```xml
<ProjectReference Include="..\MudShell\MudShell.csproj" />
```

### 2. Register services

In `Program.cs`:

```csharp
builder.Services.AddMudShell();
```

**Important:** do **not** also call `AddMudServices()`. `AddMudShell()` already wires MudBlazor services.

If you want sample-style theme orchestration, also register a scoped theme state service:

```csharp
builder.Services.AddScoped<ThemeState>();
```

### 3. Add imports

In `_Imports.razor`:

```razor
@using MudShell
@using MudShell.Components.AppShell
@using MudShell.Components.ChatBar
@using MudShell.Components.DocumentCard
@using MudShell.Components.FilterTabBar
@using MudShell.Components.MainContent
@using MudShell.Components.Navigation
@using MudShell.Components.Navigation.Models
@using MudShell.Components.PageHeader
@using MudShell.Components.Sidebar
```

### 4. Keep MudBlazor providers in the app root

Declare providers **once** in the root component (`Routes.razor`, `App.razor`, or equivalent):

```razor
<MudThemeProvider Theme="@ThemeState.CurrentTheme" IsDarkMode="@ThemeState.IsDarkMode" />
<MudPopoverProvider />
<MudSnackbarProvider />
<MudDialogProvider />

<Router AppAssembly="typeof(Program).Assembly">
    ...
</Router>
```

Do not duplicate them in layouts or pages.

### 5. Replace the layout with `MdsAppShell`

```razor
@inherits LayoutComponentBase
@inject ThemeState ThemeState

<MdsAppShell @ref="_shell"
             BackgroundMode="@ThemeState.BackgroundMode"
             BackgroundImageUrl="@ThemeState.BackgroundImageUrl"
             IsDarkMode="@ThemeState.IsDarkMode"
             IsDarkModeChanged="@ThemeState.SetDarkMode"
             SidebarExpanded="@_sidebarExpanded"
             SidebarExpandedChanged="@(v => _sidebarExpanded = v)"
             ContextPanelExpanded="@_contextPanelExpanded"
             ContextPanelExpandedChanged="@(v => _contextPanelExpanded = v)"
             ContextPanelContent="@ContextPanel">
    <SidebarContent>
        <NavMenu ShellRef="@_shell" IsExpanded="@_sidebarExpanded" />
    </SidebarContent>

    <ChildContent>
        @Body
    </ChildContent>
</MdsAppShell>

@code {
    private MdsAppShell _shell = default!;
    private bool _sidebarExpanded;
    private bool _contextPanelExpanded = true;
    private RenderFragment? ContextPanel => null;
}
```

### 6. Migrate navigation deliberately

Recommended rule:

- keep **business areas** in primary navigation,
- put **variants, demos, families, and sub-pages** in the context panel.

For simple apps:

- sidebar = top-level sections

For larger apps:

- sidebar = top-level sections
- context panel = second-level navigation tree

---

## Recommended page structure

Base every new page on the `MeteoPage` pattern.

### Structure

```text
MudContainer
├── MdsPageHeader
├── MdsMainToolbar         ← actions only
├── MudStack + MudChip     ← filters only, directly on page background
├── MdsMainPart            ← content block
└── MdsMainPart            ← content block
```

### Rules

1. **Page header**: use `MdsPageHeader` for icon, title, breadcrumb path, and page-level metadata.
2. **Action bar**: use `MdsMainToolbar` for actions and UI configuration only.
3. **Filter row**: render filters in a horizontal `MudStack` using `MudChip`.
4. **No paper around filters**: keep the filter row directly on the page background.
5. **Content blocks**: use `MdsMainPart` and `MdsMainSection` for body content.

### Copy-ready blueprint

```razor
@page "/weather"
@rendermode InteractiveServer

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
        <MudChip T="string"
                 Color="@(_selectedPeriod == "today" ? Color.Primary : Color.Default)"
                 Variant="@(_selectedPeriod == "today" ? Variant.Filled : Variant.Outlined)"
                 OnClick="@(() => _selectedPeriod = "today")">
            Today
        </MudChip>
        <MudChip T="string"
                 Color="@(_selectedPeriod == "week" ? Color.Primary : Color.Default)"
                 Variant="@(_selectedPeriod == "week" ? Variant.Filled : Variant.Outlined)"
                 OnClick="@(() => _selectedPeriod = "week")">
            This week
        </MudChip>
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

---

## When to use each component

### `MdsPageHeader`

Use for:

- title,
- breadcrumbs,
- contextual icon actions on the far right (`HeaderActions`),
- top-level page actions,
- page badges.

### `MdsMainToolbar`

Use for:

- create/export/reset buttons,
- density or view toggles,
- UI configuration controls.

Avoid using it as a general filter bar when a chip row would be clearer.

Accent border parameters:

- `AccentBorderSides` (flags, combinable with `|`),
- `AccentBorderColor` (any CSS color/token),
- `AccentBorderWidth` (clamped to `6px` max).

### `MdsMainPart`

Use for:

- tables,
- charts,
- metrics,
- API-backed blocks with loading and error states.

`MdsMainPart` and `MdsMainSection` also expose the same accent border parameters for visual hierarchy while keeping layout consistent.

Built-in affordances:

- `IsLoading`
- `LoadingText`
- `Error`
- `OnRetry`
- `HeaderActions`

### `MdsMainSection`

Use for:

- larger grouped areas,
- descriptive sections,
- wrapper blocks that need a title + description + divider.

### `MdsMainEmptyState`

Use for:

- empty search results,
- empty folders,
- “nothing selected yet” states.

### `MdsMainButton`

Use when you want action styling consistent with the rest of the main content zone.

### `MdsMainStatusBadge`

Use for:

- active/pending/error info,
- lightweight state flags,
- compact summary metadata.

---

## Sample navigation pattern

A practical pattern used in the sample:

- **primary nav**: `Home`, `Weather`, `Europe Cities`, `Library`, `Demo`, `Components`
- **context panel**:
  - demo subsection: showcase + render mode pages
  - components subsection: component-oriented sample pages

This keeps the main sidebar short and moves dense page groups into second-level navigation.

---

## Common pitfalls

1. Calling both `AddMudServices()` and `AddMudShell()`.
2. Adding MudBlazor providers in multiple places.
3. Mixing actions and filters in one toolbar.
4. Wrapping the filter row in bordered `MudPaper`.
5. Replacing MudBlazor content components instead of composing them with MudShell wrappers.
6. Making every page unique instead of reusing the `MeteoPage` structure.

---

## If Copilot is modifying a MudShell page

Prefer these transformations:

1. add or keep `MdsPageHeader`,
2. consolidate page actions into `MdsMainToolbar`,
3. move filters into a chip row on the page background,
4. wrap large content zones in `MdsMainPart` / `MdsMainSection`,
5. keep layout consistent with existing MudShell sample pages.
