---
sidebar_position: 1
---

# MbxAppShell

Full-page layout shell. Owns the sidebar, main content area, background layer, and bottom nav slot.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `SidebarContent` | `RenderFragment?` | — | Content rendered inside the sidebar nav |
| `ChildContent` | `RenderFragment?` | — | Main page body |
| `BottomNavContent` | `RenderFragment?` | — | Shown in the fixed bottom slot on mobile (≤ 959 px) |
| `BackgroundMode` | `MbxBackgroundMode` | `Palette` | `Palette` or `Image` |
| `BackgroundImageUrl` | `string?` | `null` | URL of the background image (Image mode only) |
| `SidebarExpanded` | `bool` | `false` | Controls sidebar width (icon-only vs. labelled) |
| `SidebarExpandedChanged` | `EventCallback<bool>` | — | Two-way bind support |

## Public methods

| Method | Description |
|---|---|
| `ToggleSidebar()` | Flips `SidebarExpanded` and triggers re-render |
| `SetBackgroundMode(mode, imageUrl?)` | Changes background mode at runtime |

## Minimal example

```razor
<MbxAppShell @ref="_shell"
             BackgroundMode="MbxAppShell.MbxBackgroundMode.Palette">
  <SidebarContent>
    <MbxSidebar OnToggle="@(() => _shell.ToggleSidebar())" ... />
  </SidebarContent>
  <ChildContent>@Body</ChildContent>
  <BottomNavContent>
    <MbxBottomNav Items="@navItems" />
  </BottomNavContent>
</MbxAppShell>
```

## With background image

```razor
<MbxAppShell BackgroundMode="MbxAppShell.MbxBackgroundMode.Image"
             BackgroundImageUrl="/images/hero.jpg">
  ...
</MbxAppShell>
```

## Switching mode from a page

```razor
[CascadingParameter] public MainLayout MainLayoutRef { get; set; } = default!;

protected override void OnInitialized()
    => MainLayoutRef.SetBackgroundMode(MbxAppShell.MbxBackgroundMode.Image, "/images/bg.jpg");
```