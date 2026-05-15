using MudBlazor;

namespace MudShell.Theme;

/// <summary>
/// Central theme configuration for MudShell.
/// Call <see cref="CreateDarkTheme"/> to get a fully configured <see cref="MudTheme"/>.
/// </summary>
public static class MbxTheme
{
    // ── Dark palette ──────────────────────────────────────────────────────────

    public static readonly PaletteDark DarkPalette = new()
    {
        Primary            = "#7e6fff",
        Surface            = "#1e1e2d",
        Background         = "#171723",
        BackgroundGray     = "#13131f",
        AppbarText         = "#92929f",
        AppbarBackground   = "rgba(23,23,35,0.85)",
        DrawerBackground   = "#171723",
        ActionDefault      = "#74718e",
        ActionDisabled     = "#9999994d",
        ActionDisabledBackground = "#605f6d4d",
        TextPrimary        = "#b2b0bf",
        TextSecondary      = "#92929f",
        TextDisabled       = "#ffffff33",
        DrawerIcon         = "#92929f",
        DrawerText         = "#92929f",
        GrayLight          = "#2a2833",
        GrayLighter        = "#1e1e2d",
        Info               = "#4a86ff",
        Success            = "#3dcb6c",
        Warning            = "#ffb545",
        Error              = "#ff3f5f",
        LinesDefault       = "#33323e",
        TableLines         = "#33323e",
        Divider            = "#292838",
        OverlayLight       = "#1e1e2d80",
    };

    // ── Light palette ─────────────────────────────────────────────────────────

    public static readonly PaletteLight LightPalette = new()
    {
        Primary            = "#7e6fff",
        Black              = "#110e2d",
        Background         = "#f0f0f5",
        Surface            = "#ffffff",
        AppbarText         = "#424242",
        AppbarBackground   = "rgba(255,255,255,0.8)",
        DrawerBackground   = "#ffffff",
        TextPrimary        = "#1a1a2e",
        TextSecondary      = "#55556e",
        ActionDefault      = "#55556e",
        GrayLight          = "#e8e8ee",
        GrayLighter        = "#f5f5f9",
        LinesDefault       = "#e0e0e8",
        TableLines         = "#e0e0e8",
        Divider            = "#e0e0e8",
        Info               = "#4a86ff",
        Success            = "#3dcb6c",
        Warning            = "#ffb545",
        Error              = "#ff3f5f",
        OverlayLight       = "#ffffff80",
    };

    // ── Factory ───────────────────────────────────────────────────────────────

    /// <summary>Returns a <see cref="MudTheme"/> pre-configured with both palettes.
    /// Pass <c>IsDarkMode</c> to <see cref="MudThemeProvider"/> to choose which one is active.</summary>
    public static MudTheme CreateTheme() => new()
    {
        PaletteDark      = DarkPalette,
        PaletteLight     = LightPalette,
        LayoutProperties = new LayoutProperties(),
    };

    /// <inheritdoc cref="CreateTheme"/>
    public static MudTheme CreateDarkTheme() => CreateTheme();
}
