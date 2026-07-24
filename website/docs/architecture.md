---
sidebar_position: 4
---

# Architecture

## Shell pattern

The core of MudShell is the **shell pattern**: a single root component (`MdsAppShell`) owns the layout and exposes named `RenderFragment` slots.

```
MdsAppShell
├── SidebarContent  ← RenderFragment (nav, logo, profile)
└── ChildContent    ← RenderFragment (routed page body)
```

This separates structure (AppShell) from content (page) and navigation (Sidebar), making each piece independently replaceable.

---

## BackgroundMode

`MdsAppShell` supports two visual modes:

| Mode | Description |
|---|---|
| `Palette` | Opaque background using `--mud-palette-background`. Sidebar and main use `--mud-palette-surface`. |
| `Image` | A full-bleed background image with glassmorphism sidebar (`backdrop-filter: blur`). Main area is transparent. |

In the sample app, background mode is coordinated through a state container injected into the layout:

```razor
@inject ThemeState ThemeState

protected override void OnInitialized()
{
    ThemeState.SetBackgroundMode(MdsAppShell.MbxBackgroundMode.Image, "/imgs/bg1.jpg");
}
```

For simpler apps, you can also bind `BackgroundMode` and `BackgroundImageUrl` directly on `MdsAppShell`.

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