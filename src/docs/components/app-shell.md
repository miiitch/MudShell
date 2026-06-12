# MbxAppShell

Full-page layout shell. Owns the sidebar, main content area, background layer, and bottom nav slot.

## Important — MudBlazor providers

`MbxAppShell` is a pure layout shell and does **not** register MudBlazor providers internally.
You must declare `<MudThemeProvider>`, `<MudPopoverProvider>`, `<MudSnackbarProvider>`, and
`<MudDialogProvider>` **once** in your app's root component (e.g. `Routes.razor`).
Registering them in both places causes a `System.InvalidOperationException` (duplicate section ID).

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
<MbxAppShell @ref="_shell"
             BackgroundMode="MbxAppShell.MbxBackgroundMode.Palette"
             ContextPanelExpanded="@_contextExpanded"
             ContextPanelExpandedChanged="@(v => _contextExpanded = v)"
             ContextPanelWidth="320"
             ContextPanelCollapsedWidth="72">
  <SidebarContent>
    <MbxSidebar OnToggle="@(() => _shell.ToggleSidebar())" ... />
  </SidebarContent>
  <ContextPanelContent>
    <MbxContextNavPanel Tree="@navTree" IsExpanded="@_contextExpanded" />
  </ContextPanelContent>
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
