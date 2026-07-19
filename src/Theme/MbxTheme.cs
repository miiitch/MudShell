using MudBlazor;
using System.Globalization;

namespace MudShell.Theme;

/// <summary>
/// Theme factory for MudShell.
/// Supports curated presets and generated themes from one color or a triadic palette.
/// </summary>
public static class MbxTheme
{
    public enum MbxThemePreset
    {
        Indigo,
        Emerald,
        Rose,
        Amber,
        Ocean,
        Violet,
    }

    public sealed record PresetInfo(MbxThemePreset Preset, string Label, string PrimaryColor);

    public static readonly IReadOnlyList<PresetInfo> Presets =
    [
        new(MbxThemePreset.Indigo, "Indigo", "#6f63ff"),
        new(MbxThemePreset.Emerald, "Emerald", "#1f9d78"),
        new(MbxThemePreset.Rose, "Rose", "#d4457f"),
        new(MbxThemePreset.Amber, "Amber", "#d98a14"),
        new(MbxThemePreset.Ocean, "Ocean", "#2f7fd9"),
        new(MbxThemePreset.Violet, "Violet", "#8a55d8"),
    ];

    public static string GetPresetPrimary(MbxThemePreset preset)
        => Presets.First(p => p.Preset == preset).PrimaryColor;

    /// <summary>Returns a <see cref="MudTheme"/> pre-configured with both palettes.</summary>
    public static MudTheme CreateTheme() => CreateTheme(MbxThemePreset.Indigo);

    /// <summary>Creates a theme from a curated preset.</summary>
    public static MudTheme CreateTheme(MbxThemePreset preset)
        => CreateThemeFromPrimary(GetPresetPrimary(preset));

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

    private static MudTheme CreateThemeCore(HexColor primary, HexColor info, HexColor success, HexColor warning, HexColor error)
    {
        var darkSidebar = Mix(primary, ParseHex("#212b40"), 0.58);
        var darkSubSection = Mix(primary, ParseHex("#192236"), 0.70);
        var darkMain = Mix(primary, ParseHex("#131b2d"), 0.80);
        var darkBackground = Mix(primary, ParseHex("#0d1424"), 0.87);

        var lightSidebar = Mix(primary, ParseHex("#c7d2ea"), 0.55);
        var lightSubSection = Mix(primary, ParseHex("#e6edf9"), 0.80);
        var lightMain = Mix(primary, ParseHex("#ffffff"), 0.94);
        var lightBackground = Mix(primary, ParseHex("#ffffff"), 0.97);

        var darkPalette = new PaletteDark
        {
            Primary = ToHex(primary),
            Surface = ToHex(darkMain),
            Background = ToHex(darkBackground),
            BackgroundGray = ToHex(darkSubSection),
            DrawerBackground = ToHex(darkSidebar),
            AppbarBackground = ToRgba(Mix(darkSidebar, darkMain, 0.60), 0.90),
            AppbarText = "#bcc4de",
            TextPrimary = "#d5dbed",
            TextSecondary = "#9fa9c7",
            TextDisabled = "#ffffff40",
            DrawerIcon = "#b4bdd7",
            DrawerText = "#c4cce2",
            ActionDefault = "#a1abc9",
            ActionDisabled = "#9a9a9a4d",
            ActionDisabledBackground = "#5a60744d",
            GrayLight = ToHex(Mix(darkSubSection, ParseHex("#ffffff"), 0.08)),
            GrayLighter = ToHex(Mix(darkMain, ParseHex("#ffffff"), 0.06)),
            LinesDefault = ToHex(Mix(darkMain, ParseHex("#ffffff"), 0.16)),
            TableLines = ToHex(Mix(darkMain, ParseHex("#ffffff"), 0.16)),
            Divider = ToHex(Mix(darkMain, ParseHex("#ffffff"), 0.12)),
            OverlayLight = ToRgba(darkMain, 0.60),
            Info = ToHex(info),
            Success = ToHex(success),
            Warning = ToHex(warning),
            Error = ToHex(error),
        };

        var lightPalette = new PaletteLight
        {
            Primary = ToHex(primary),
            Black = "#0f1325",
            Surface = ToHex(lightMain),
            Background = ToHex(lightBackground),
            BackgroundGray = ToHex(lightSubSection),
            DrawerBackground = ToHex(lightSidebar),
            AppbarBackground = ToRgba(lightBackground, 0.90),
            AppbarText = "#1a2540",
            TextPrimary = "#1b2742",
            TextSecondary = "#435070",
            ActionDefault = "#435070",
            GrayLight = ToHex(Mix(lightSubSection, ParseHex("#d9e2f2"), 0.34)),
            GrayLighter = ToHex(Mix(lightMain, ParseHex("#f2f6fd"), 0.38)),
            LinesDefault = ToHex(Mix(lightSubSection, ParseHex("#b9c7df"), 0.46)),
            TableLines = ToHex(Mix(lightSubSection, ParseHex("#b9c7df"), 0.46)),
            Divider = ToHex(Mix(lightSubSection, ParseHex("#9fb2d3"), 0.40)),
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
