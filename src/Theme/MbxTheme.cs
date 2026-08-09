using MudBlazor;
using System.Globalization;

namespace MudShell.Theme;

/// <summary>
/// Theme factory for MudShell.
/// Supports curated presets and generated themes from one color or a triadic palette.
/// </summary>
public static class MbxTheme
{
    public static class DesignTokens
    {
        public const int MaxAccentBorderWidth = 6;
    }

    public enum MbxThemePreset
    {
        Cobalt,
        Teal,
        Violet,
        Forest,
        Lime,
        Amber,
        Crimson,
    }

    public sealed record PresetInfo(MbxThemePreset Preset, string Label, string PrimaryColor);

    public static readonly IReadOnlyList<PresetInfo> Presets =
    [
        new(MbxThemePreset.Cobalt,  "Cobalt",  "#1D4ED8"), // Bleu enterprise profond
        new(MbxThemePreset.Teal,    "Teal",    "#0D9488"), // Teal moderne
        new(MbxThemePreset.Violet,  "Violet",  "#7C3AED"), // Violet premium
        new(MbxThemePreset.Forest,  "Forest",  "#16A34A"), // Vert finance/nature
        new(MbxThemePreset.Lime,    "Lime",    "#A9C93A"), // Lime adouci Orrik
        new(MbxThemePreset.Amber,   "Amber",   "#B45309"), // Ambre profond
        new(MbxThemePreset.Crimson, "Crimson", "#BE123C"), // Cramoisi professionnel
    ];

    public static string GetPresetPrimary(MbxThemePreset preset)
        => Presets.First(p => p.Preset == preset).PrimaryColor;

    /// <summary>Returns a <see cref="MudTheme"/> pre-configured with both palettes.</summary>
    public static MudTheme CreateTheme() => CreateTheme(MbxThemePreset.Cobalt);

    /// <summary>Creates a theme from a curated preset.</summary>
    public static MudTheme CreateTheme(MbxThemePreset preset)
        => preset == MbxThemePreset.Lime
            ? CreateLimeTheme()
            : CreateThemeFromPrimary(GetPresetPrimary(preset));

    /// <summary>
    /// Creates a theme from one primary color and derives secondary tokens.
    /// Light mode keeps the center close to white with a color tint.
    /// Dark mode uses a readable progression where sidebar is lighter than center.
    /// </summary>
    public static MudTheme CreateThemeFromPrimary(string primaryHex)
    {
        var primary = ParseHex(primaryHex);
        var info = Mix(primary, ParseHex("#4a86ff"), 0.30);
        var success = Mix(primary, ParseHex("#2dbd6e"), 0.40);
        var warning = Mix(primary, ParseHex("#ffb545"), 0.72);
        var error = Mix(primary, ParseHex("#ff4d6d"), 0.62);

        return CreateThemeCore(primary, info, success, warning, error);
    }

    /// <summary>
    /// Creates a theme from a triadic palette.
    /// Primary drives accents; secondary and tertiary are reused for status colors.
    /// </summary>
    public static MudTheme CreateThemeFromTriad(string primaryHex, string secondaryHex, string tertiaryHex)
    {
        var primary = ParseHex(primaryHex);
        var secondary = ParseHex(secondaryHex);
        var tertiary = ParseHex(tertiaryHex);

        var warning = Mix(primary, ParseHex("#ffb545"), 0.68);
        var error = Mix(primary, ParseHex("#ff4d6d"), 0.60);

        return CreateThemeCore(primary, secondary, tertiary, warning, error);
    }

    private static MudTheme CreateLimeTheme()
    {
        var primary = ParseHex("#A9C93A");
        var info = ParseHex("#8bbf3f");
        var success = ParseHex("#5fb86b");
        var warning = ParseHex("#d8a84a");
        var error = ParseHex("#d96a74");

        var theme = CreateThemeCore(
            primary,
            info,
            success,
            warning,
            error,
            darkSidebar: ParseHex("#23271d"),
            darkMain: ParseHex("#181b15"),
            darkSubSection: ParseHex("#11130f"),
            darkBackground: ParseHex("#0b0c0a"),
            lightSidebar: ParseHex("#dde5be"),
            lightSubSection: ParseHex("#f1f4e6"),
            lightBackground: ParseHex("#f4f5ef"),
            lightMain: ParseHex("#ffffff"));

        theme.PaletteDark.PrimaryContrastText = "#11130f";
        theme.PaletteLight.PrimaryContrastText = "#11130f";
        return theme;
    }

    private static MudTheme CreateThemeCore(
        HexColor primary,
        HexColor info,
        HexColor success,
        HexColor warning,
        HexColor error,
        HexColor? darkSidebar = null,
        HexColor? darkMain = null,
        HexColor? darkSubSection = null,
        HexColor? darkBackground = null,
        HexColor? lightSidebar = null,
        HexColor? lightSubSection = null,
        HexColor? lightBackground = null,
        HexColor? lightMain = null)
    {
        // ── Dark mode: DrawerBackground > Surface > BackgroundGray > Background ──
        // Surface (cards) MUST be lighter than BackgroundGray (page area) so cards pop
        var resolvedDarkSidebar    = darkSidebar    ?? Mix(primary, ParseHex("#2a2d4a"), 0.50); // DrawerBackground — lightest
        var resolvedDarkMain       = darkMain       ?? Mix(primary, ParseHex("#1a1d33"), 0.72); // Surface — cards (medium)
        var resolvedDarkSubSection = darkSubSection ?? Mix(primary, ParseHex("#12142a"), 0.85); // BackgroundGray — page area (darker)
        var resolvedDarkBackground = darkBackground ?? Mix(primary, ParseHex("#0a0c1c"), 0.91); // Background — darkest

        // ── Light mode: Surface stays white; page background uses neutral gray for stronger contrast ──
        var resolvedLightSidebar    = lightSidebar    ?? Mix(primary, ParseHex("#e0e0f4"), 0.65); // DrawerBackground — clearly tinted
        var resolvedLightSubSection = lightSubSection ?? Mix(primary, ParseHex("#f5f5ff"), 0.93); // BackgroundGray — subtle
        var resolvedLightBackground = lightBackground ?? ParseHex("#f1f3f6");                      // Background — neutral gray (palette-independent)
        var resolvedLightMain       = lightMain       ?? ParseHex("#ffffff");                      // Surface — pure white

        var darkPalette = new PaletteDark
        {
            Primary = ToHex(primary),
            Surface = ToHex(resolvedDarkMain),
            Background = ToHex(resolvedDarkBackground),
            BackgroundGray = ToHex(resolvedDarkSubSection),
            DrawerBackground = ToHex(resolvedDarkSidebar),
            AppbarBackground = ToRgba(Mix(resolvedDarkSidebar, resolvedDarkMain, 0.60), 0.90),
            AppbarText = "#bcc4de",
            TextPrimary = "#d5dbed",
            TextSecondary = "#9fa9c7",
            TextDisabled = "#ffffff40",
            DrawerIcon = "#b4bdd7",
            DrawerText = "#c4cce2",
            ActionDefault = "#a1abc9",
            ActionDisabled = "#9a9a9a4d",
            ActionDisabledBackground = "#5a60744d",
            GrayLight = ToHex(Mix(resolvedDarkSubSection, ParseHex("#ffffff"), 0.08)),
            GrayLighter = ToHex(Mix(resolvedDarkMain, ParseHex("#ffffff"), 0.06)),
            LinesDefault = ToHex(Mix(resolvedDarkMain, ParseHex("#ffffff"), 0.16)),
            TableLines = ToHex(Mix(resolvedDarkMain, ParseHex("#ffffff"), 0.16)),
            Divider = ToHex(Mix(resolvedDarkMain, ParseHex("#ffffff"), 0.12)),
            OverlayLight = ToRgba(resolvedDarkMain, 0.60),
            Info = ToHex(info),
            Success = ToHex(success),
            Warning = ToHex(warning),
            Error = ToHex(error),
        };

        var lightPalette = new PaletteLight
        {
            Primary = ToHex(primary),
            Black = "#0f1325",
            Surface = ToHex(resolvedLightMain),
            Background = ToHex(resolvedLightBackground),
            BackgroundGray = ToHex(resolvedLightSubSection),
            DrawerBackground = ToHex(resolvedLightSidebar),
            AppbarBackground = ToRgba(resolvedLightBackground, 0.90),
            AppbarText = "#1a2540",
            TextPrimary = "#1b2742",
            TextSecondary = "#435070",
            ActionDefault = "#435070",
            GrayLight = ToHex(Mix(resolvedLightSubSection, ParseHex("#d9e2f2"), 0.34)),
            GrayLighter = ToHex(Mix(resolvedLightMain, ParseHex("#f2f6fd"), 0.38)),
            LinesDefault = ToHex(Mix(resolvedLightSubSection, ParseHex("#b9c7df"), 0.46)),
            TableLines = ToHex(Mix(resolvedLightSubSection, ParseHex("#b9c7df"), 0.46)),
            Divider = ToHex(Mix(resolvedLightSubSection, ParseHex("#9fb2d3"), 0.40)),
            OverlayLight = ToRgba(ParseHex("#ffffff"), 0.60),
            Info = ToHex(info),
            Success = ToHex(success),
            Warning = ToHex(warning),
            Error = ToHex(error),
        };

        return new MudTheme
        {
            PaletteDark = darkPalette,
            PaletteLight = lightPalette,
            LayoutProperties = new LayoutProperties(),
        };
    }

    private readonly record struct HexColor(int R, int G, int B);

    private static HexColor ParseHex(string value)
    {
        var hex = value.Trim();
        if (hex.StartsWith('#'))
            hex = hex[1..];

        if (hex.Length == 3)
            hex = string.Concat(hex.Select(c => new string(c, 2)));

        if (hex.Length != 6)
            throw new ArgumentException("Hex color must be in #RGB or #RRGGBB format.", nameof(value));

        var r = Convert.ToInt32(hex[0..2], 16);
        var g = Convert.ToInt32(hex[2..4], 16);
        var b = Convert.ToInt32(hex[4..6], 16);
        return new HexColor(r, g, b);
    }

    private static HexColor Mix(HexColor from, HexColor to, double ratio)
    {
        var t = Math.Clamp(ratio, 0d, 1d);
        var r = (int)Math.Round(from.R + (to.R - from.R) * t);
        var g = (int)Math.Round(from.G + (to.G - from.G) * t);
        var b = (int)Math.Round(from.B + (to.B - from.B) * t);
        return new HexColor(r, g, b);
    }

    private static string ToHex(HexColor color)
        => $"#{color.R:X2}{color.G:X2}{color.B:X2}".ToLowerInvariant();

    private static string ToRgba(HexColor color, double alpha)
        => $"rgba({color.R},{color.G},{color.B},{Math.Clamp(alpha, 0d, 1d).ToString("0.##", CultureInfo.InvariantCulture)})";

    /// <inheritdoc cref="CreateTheme"/>
    public static MudTheme CreateDarkTheme() => CreateTheme();
}
