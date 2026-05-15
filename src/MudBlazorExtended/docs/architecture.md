# Architecture

## Shell pattern

The core of MudBlazorExtended is the **shell pattern**: a single root component (`MbxAppShell`) owns the layout and exposes named `RenderFragment` slots.

```
MbxAppShell
├── SidebarContent  ← RenderFragment (nav, logo, profile)
├── ChildContent    ← RenderFragment (routed page body)
└── BottomNavContent ← RenderFragment (mobile bottom nav)
```

This separates structure (AppShell) from content (page) and navigation (Sidebar/BottomNav), making each piece independently replaceable.

---

## BackgroundMode

`MbxAppShell` supports two visual modes:

| Mode | Description |
|---|---|
| `Palette` | Opaque background using `--mud-palette-background`. Sidebar and main use `--mud-palette-surface`. |
| `Image` | A full-bleed background image with glassmorphism sidebar (`backdrop-filter: blur`). Main area is transparent. |

Switch mode from any page:

```razor
[CascadingParameter] public MainLayout MainLayoutRef { get; set; } = default!;

protected override void OnInitialized()
{
    MainLayoutRef.SetBackgroundMode(MbxAppShell.MbxBackgroundMode.Image, "/images/bg.jpg");
}
```

> `MainLayout` exposes `SetBackgroundMode()` which delegates to the `_shell` reference.

---

## CascadingValue

`MbxAppShell` wraps itself in a `CascadingValue<MbxAppShell>`. Child components (e.g. a nav button) can receive it:

```razor
[CascadingParameter] public MbxAppShell Shell { get; set; } = default!;

void Toggle() => Shell.ToggleSidebar();
```

---

## Theme

`MbxAppShell` instantiates the theme via `MbxTheme.CreateDarkTheme()` and passes it to `MudThemeProvider`. All MudBlazor components inside the shell inherit this theme automatically.

---

## Component namespace convention

All library components live under `MudBlazorExtended.Components.<ComponentName>` and use the `Mbx` prefix to avoid collision with MudBlazor's `Mud` prefix.

```
MudBlazorExtended.Components.AppShell.MbxAppShell
MudBlazorExtended.Components.Sidebar.MbxSidebar
MudBlazorExtended.Components.Sidebar.MbxNavItem   ← shared record
...
```
