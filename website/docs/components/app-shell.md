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

## Public methods

| Method | Description |
|---|---|
| `ToggleSidebar()` | Flips `SidebarExpanded` and triggers re-render |
| `ToggleContextPanel()` | Flips `ContextPanelExpanded` and triggers re-render |
| `SetBackgroundMode(mode, imageUrl?)` | Changes background mode at runtime |

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