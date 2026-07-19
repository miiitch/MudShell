---
sidebar_position: 4
---

# Architecture

## Shell pattern

The core of MudShell is the **shell pattern**: a single root component (`MdsAppShell`) owns the layout and exposes named `RenderFragment` slots.

```
MdsAppShell
├── SidebarContent  ← RenderFragment (nav, logo, profile)
├── ChildContent    ← RenderFragment (routed page body)
└── BottomNavContent ← RenderFragment (mobile bottom nav)
```

This separates structure (AppShell) from content (page) and navigation (Sidebar/BottomNav), making each piece independently replaceable.

---

## BackgroundMode

`MdsAppShell` supports two visual modes:

| Mode | Description |
|---|---|
| `Palette` | Opaque background using `--mud-palette-background`. Sidebar and main use `--mud-palette-surface`. |
| `Image` | A full-bleed background image with glassmorphism sidebar (`backdrop-filter: blur`). Main area is transparent. |

Switch mode from any page:

```razor
[CascadingParameter] public MainLayout MainLayoutRef { get; set; } = default!;

protected override void OnInitialized()
{
    MainLayoutRef.SetBackgroundMode(MdsAppShell.MbxBackgroundMode.Image, "/images/bg.jpg");
}
```

> `MainLayout` exposes `SetBackgroundMode()` which delegates to the `_shell` reference.

---

## CascadingValue

`MdsAppShell` wraps itself in a `CascadingValue<MdsAppShell>`. Child components (e.g. a nav button) can receive it:

```razor
[CascadingParameter] public MdsAppShell Shell { get; set; } = default!;

void Toggle() => Shell.ToggleSidebar();
```

---

## Theme

`MdsAppShell` instantiates the theme via `MbxTheme.CreateDarkTheme()` and passes it to `MudThemeProvider`. All MudBlazor components inside the shell inherit this theme automatically.

---

## Component namespace convention

All library components live under `MudShell.Components.<ComponentName>` and use the `Mbx` prefix to avoid collision with MudBlazor's `Mud` prefix.

```
MudShell.Components.AppShell.MdsAppShell
MudShell.Components.Sidebar.MdsSidebar
MudShell.Components.Sidebar.MbxNavItem   ← shared record
...
```