---
sidebar_position: 1
---

# MdsAppShell

Full-page layout shell. Owns the sidebar, main content area, and background layer.

![MdsAppShell — desktop, icon-only sidebar, background palette mode](/img/screenshots/home.png)

*Desktop (1280 px): icon-only sidebar collapsed, `Palette` background mode.*

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `SidebarContent` | `RenderFragment?` | — | Content rendered inside the sidebar nav |
| `ChildContent` | `RenderFragment?` | — | Main page body |
| `BackgroundMode` | `MbxBackgroundMode` | `Palette` | `Palette` or `Image` |
| `BackgroundImageUrl` | `string?` | `null` | URL of the background image (Image mode only) |
| `SidebarExpanded` | `bool` | `false` | Controls sidebar width (icon-only vs. labelled) |
| `SidebarExpandedChanged` | `EventCallback<bool>` | — | Two-way bind support |
| `SidebarWidth` | `int` | `240` | Expanded sidebar width (px) |
| `SidebarCollapsedWidth` | `int` | `56` | Collapsed sidebar width (px) |
| `ContextPanelContent` | `RenderFragment?` | — | Optional secondary/context panel content (level 2/3 nav) |
| `ContextPanelExpanded` | `bool` | `true` | Expanded/collapsed state of the context panel |
| `ContextPanelExpandedChanged` | `EventCallback<bool>` | — | Two-way bind support for context panel state |
| `ContextPanelWidth` | `int` | `288` | Expanded width of the context panel (px) |
| `ContextPanelCollapsedWidth` | `int` | `72` | Collapsed width of the context panel (px) |
| `ResponsiveMode` | `MbxResponsiveMode` | `Auto` | `Auto` adapts to the viewport (see [Responsive](../responsive.md)). `Desktop` pins the desktop layout at every width |
| `ShellModeChanged` | `EventCallback<MbxShellMode>` | — | Raised when the tier changes (`Auto` only). Use it to pick the rail or the bottom bar |
| `MobileTitle` | `string?` | `null` | Title in the mobile top bar |
| `MobileActions` | `RenderFragment?` | — | Trailing actions in the mobile top bar |
| `MobileMenuClicked` | `EventCallback` | — | Raised by the mobile top bar's menu button |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | `null` | Undeclared attributes, splatted onto the root element (e.g. `data-testid`, ARIA attributes) |

`ContextPanelExpanded` also drives the tablet overlay: `true` opens it, `false` closes it. Outside desktop it starts closed.

## Public members

| Member | Description |
|---|---|
| `ShellMode` | Current tier: `MbxShellMode.Desktop`, `Tablet` or `Mobile`. Also cascaded to descendants as `CascadingParameter(Name = "MbxShellMode")` |
| `ToggleSidebar()` | Flips `SidebarExpanded` and triggers re-render |
| `ToggleContextPanel()` | Flips `ContextPanelExpanded` and triggers re-render |
| `OpenContextPanelOnNextNavigation()` | Tablet only: opens the level-2 overlay now and keeps it open across the navigation that follows (navigation normally closes it). Call it from `MdsSidebar.OnItemClick`. No-op on desktop and mobile |
| `SetBackgroundMode(mode, imageUrl?)` | Changes background mode at runtime |

## Responsive tiers

```razor
<MdsAppShell ContextPanelContent="@_context"
             ShellModeChanged="@(m => _shellMode = m)"
             MobileTitle="Copilot"
             MobileMenuClicked="@OpenMobileMenu">
  <SidebarContent>
    @if (_shellMode != MdsAppShell.MbxShellMode.Mobile)
    {
      <MdsSidebar ... />
    }
  </SidebarContent>
  <ChildContent>@Body</ChildContent>
</MdsAppShell>

@if (_shellMode == MdsAppShell.MbxShellMode.Mobile)
{
  <MdsBottomNav @ref="_mobileNav" Tree="@_navTree" />
}
```

Tablet and mobile need no extra wiring for the context panel: outside desktop it becomes an overlay
(tablet) or is replaced by the bottom sheet of `MdsBottomNav` (mobile). See [responsive.md](../responsive.md) for the full behaviour and the test ids.

## Minimal example

```razor
<MdsAppShell @ref="_shell"
             BackgroundMode="MdsAppShell.MbxBackgroundMode.Palette"
             ContextPanelExpanded="@_contextExpanded"
             ContextPanelExpandedChanged="@(v => _contextExpanded = v)"
             ContextPanelWidth="320"
             ContextPanelCollapsedWidth="72">
  <SidebarContent>
    <MdsSidebar OnToggle="@(() => _shell.ToggleSidebar())" ... />
  </SidebarContent>
  <ContextPanelContent>
    <MdsContextNavPanel Tree="@navTree" IsExpanded="@_contextExpanded" />
  </ContextPanelContent>
  <ChildContent>@Body</ChildContent>
</MdsAppShell>
```

## With background image

```razor
<MdsAppShell BackgroundMode="MdsAppShell.MbxBackgroundMode.Image"
             BackgroundImageUrl="/images/hero.jpg">
  ...
</MdsAppShell>
```

## Switching mode from app state

```razor
@inject ThemeState ThemeState

protected override void OnInitialized()
    => ThemeState.SetBackgroundMode(MdsAppShell.MbxBackgroundMode.Image, "/imgs/bg1.jpg");
```