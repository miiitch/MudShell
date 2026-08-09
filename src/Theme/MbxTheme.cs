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
        Ruby,
        Saffron,
        Azure,
        MagmaMist,
        AuburnDunes,
        OlivineMatisse,
        FernGreen,
        PeacockDusk,
        DeepCharcoal,
        EarthRoot,
        ObsidianPulse,
        Onyx,
        SteelMist,
        ObsidianInk,
        MidnightStatic,
        StormSlate,
        AbyssalNavy,
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
        new(MbxThemePreset.Ruby,    "Ruby",    "#C81E3A"), // Rouge rubis
        new(MbxThemePreset.Saffron, "Saffron", "#D6A800"), // Jaune safran
        new(MbxThemePreset.Azure,   "Azure",   "#1D4ED8"), // Bleu azur
        new(MbxThemePreset.MagmaMist, "Magma Mist", "#F06F0E"), // Orange volcanique / or / vert-de-mer
        new(MbxThemePreset.AuburnDunes, "Auburn Dunes", "#71351A"), // Auburn / sable / cacao
        new(MbxThemePreset.OlivineMatisse, "Olivine Matisse", "#A2BD7F"), // Olivine / matisse / mousse
        new(MbxThemePreset.FernGreen, "Fern Green", "#557743"), // Vert fougère / feuille / sauge
        new(MbxThemePreset.PeacockDusk, "Peacock Dusk", "#09443D"), // Vert pétrole / lin / prune
        new(MbxThemePreset.DeepCharcoal, "Deep Charcoal", "#222222"), // Charbon profond / or vert / abricot
        new(MbxThemePreset.EarthRoot,    "Earth Root",    "#48261D"), // Terre / feu / ciel
        new(MbxThemePreset.ObsidianPulse, "Obsidian Pulse", "#4C1413"), // Obsidienne / rosé / ciel
        new(MbxThemePreset.Onyx,         "Onyx",         "#151311"), // Onyx / mauve / sable
        new(MbxThemePreset.SteelMist,    "Steel Mist",   "#5B6E7D"), // Acier / bordeaux / blush
        new(MbxThemePreset.ObsidianInk,   "Obsidian Ink",   "#151311"), // Palette sombre, terre et charbon
        new(MbxThemePreset.MidnightStatic, "Midnight Static", "#1B1C20"), // Palette indigo, rose et nuit
        new(MbxThemePreset.StormSlate,    "Storm Slate",    "#5B6E7D"), // Palette gris-bleu, bordeaux et blush
        new(MbxThemePreset.AbyssalNavy,   "Abyssal Navy",   "#0D3651"), // Palette marine, corail et sable
    ];

    public static string GetPresetPrimary(MbxThemePreset preset)
        => Presets.First(p => p.Preset == preset).PrimaryColor;

    /// <summary>Returns a <see cref="MudTheme"/> pre-configured with both palettes.</summary>
    public static MudTheme CreateTheme() => CreateTheme(MbxThemePreset.Cobalt);

    /// <summary>Creates a theme from a curated preset.</summary>
    public static MudTheme CreateTheme(MbxThemePreset preset)
        => preset switch
        {
            MbxThemePreset.Lime => CreateLimeTheme(),
            MbxThemePreset.Ruby => CreateThemeCore(
                ParseHex("#C81E3A"),
                ParseHex("#E77A86"),
                ParseHex("#F8CAD0"),
                ParseHex("#F4B15C"),
                ParseHex("#8F1D2C"),
                darkSidebar: ParseHex("#4B1621"),
                darkMain: ParseHex("#651C2E"),
                darkSubSection: ParseHex("#250C12"),
                darkBackground: ParseHex("#14070A"),
                lightSidebar: ParseHex("#F5C1C6"),
                lightSubSection: ParseHex("#FCEBEC"),
                lightBackground: ParseHex("#FFF7F8"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.Saffron => CreateThemeCore(
                ParseHex("#D6A800"),
                ParseHex("#C7A24B"),
                ParseHex("#F4E4A6"),
                ParseHex("#E2B547"),
                ParseHex("#9F5C1D"),
                darkSidebar: ParseHex("#4B3F0A"),
                darkMain: ParseHex("#63510B"),
                darkSubSection: ParseHex("#221C05"),
                darkBackground: ParseHex("#120F03"),
                lightSidebar: ParseHex("#F1E0A6"),
                lightSubSection: ParseHex("#FBF6D9"),
                lightBackground: ParseHex("#FFFDF2"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.Azure => CreateThemeCore(
                ParseHex("#1D4ED8"),
                ParseHex("#63A4F4"),
                ParseHex("#D3E4FA"),
                ParseHex("#E0B645"),
                ParseHex("#B24B5E"),
                darkSidebar: ParseHex("#12305C"),
                darkMain: ParseHex("#173F74"),
                darkSubSection: ParseHex("#0A1830"),
                darkBackground: ParseHex("#060B19"),
                lightSidebar: ParseHex("#C7D7F4"),
                lightSubSection: ParseHex("#EDF3FB"),
                lightBackground: ParseHex("#F8FAFE"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.MagmaMist => CreateThemeCore(
                ParseHex("#F06F0E"),
                ParseHex("#D95A42"),
                ParseHex("#F0C4B6"),
                ParseHex("#FFB342"),
                ParseHex("#C84F13"),
                darkSidebar: ParseHex("#4A1916"),
                darkMain: ParseHex("#6B2320"),
                darkSubSection: ParseHex("#2E1413"),
                darkBackground: ParseHex("#180A09"),
                lightSidebar: ParseHex("#FFD8A1"),
                lightSubSection: ParseHex("#FFF0D0"),
                lightBackground: ParseHex("#FFF7EB"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.AuburnDunes => CreateThemeCore(
                ParseHex("#71351A"),
                ParseHex("#B3A38A"),
                ParseHex("#887456"),
                ParseHex("#4E2A26"),
                ParseHex("#A85C3F"),
                darkSidebar: ParseHex("#4E2A26"),
                darkMain: ParseHex("#5B332D"),
                darkSubSection: ParseHex("#3B201E"),
                darkBackground: ParseHex("#241312"),
                lightSidebar: ParseHex("#B3A38A"),
                lightSubSection: ParseHex("#E6D7C6"),
                lightBackground: ParseHex("#F5EFE6"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.OlivineMatisse => CreateThemeCore(
                ParseHex("#A2BD7F"),
                ParseHex("#016278"),
                ParseHex("#BBD0BE"),
                ParseHex("#7F9F63"),
                ParseHex("#0B4E60"),
                darkSidebar: ParseHex("#016278"),
                darkMain: ParseHex("#2D6D67"),
                darkSubSection: ParseHex("#244E41"),
                darkBackground: ParseHex("#102D2C"),
                lightSidebar: ParseHex("#DDE8C8"),
                lightSubSection: ParseHex("#EEF4E8"),
                lightBackground: ParseHex("#F7FBF5"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.FernGreen => CreateThemeCore(
                ParseHex("#557743"),
                ParseHex("#BBD0BE"),
                ParseHex("#8DA07A"),
                ParseHex("#A9C49B"),
                ParseHex("#4A6638"),
                darkSidebar: ParseHex("#557743"),
                darkMain: ParseHex("#405C33"),
                darkSubSection: ParseHex("#2C4022"),
                darkBackground: ParseHex("#1B2815"),
                lightSidebar: ParseHex("#BBD0BE"),
                lightSubSection: ParseHex("#E5EEE1"),
                lightBackground: ParseHex("#F6F9F4"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.PeacockDusk => CreateThemeCore(
                ParseHex("#09443D"),
                ParseHex("#C9C1B0"),
                ParseHex("#F0EDDF"),
                ParseHex("#960D41"),
                ParseHex("#7D1436"),
                darkSidebar: ParseHex("#09443D"),
                darkMain: ParseHex("#0F5A50"),
                darkSubSection: ParseHex("#174F42"),
                darkBackground: ParseHex("#061E1A"),
                lightSidebar: ParseHex("#C9C1B0"),
                lightSubSection: ParseHex("#F0EDDF"),
                lightBackground: ParseHex("#FAF8F3"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.DeepCharcoal => CreateDeepCharcoalTheme(),
            MbxThemePreset.EarthRoot => CreateThemeFromTriad("#48261D", "#F94C00", "#CAE7F7"),
            MbxThemePreset.ObsidianPulse => CreateThemeFromTriad("#4C1413", "#CAE7F7", "#EA6B7E"),
            MbxThemePreset.Onyx => CreateThemeCore(
                ParseHex("#151311"),
                ParseHex("#4B262F"),
                ParseHex("#EED3BA"),
                ParseHex("#FFB342"),
                ParseHex("#C84F13"),
                darkSidebar: ParseHex("#2A2326"),
                darkMain: ParseHex("#35292D"),
                darkSubSection: ParseHex("#1E181B"),
                darkBackground: ParseHex("#141012"),
                lightSidebar: ParseHex("#D7C8AE"),
                lightSubSection: ParseHex("#F1E8DA"),
                lightBackground: ParseHex("#FBF8F2"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.SteelMist => CreateThemeFromTriad("#5B6E7D", "#5C0403", "#EDB1B0"),
            MbxThemePreset.ObsidianInk => CreateThemeFromTriad("#151311", "#4B262F", "#EED3BA"),
            MbxThemePreset.MidnightStatic => CreateThemeCore(
                ParseHex("#1B1C20"),
                ParseHex("#144EA0"),
                ParseHex("#CF98AF"),
                ParseHex("#ffb545"),
                ParseHex("#ff4d6d"),
                darkSidebar: ParseHex("#2A2C37"),
                darkMain: ParseHex("#222431"),
                darkSubSection: ParseHex("#1B1D27"),
                darkBackground: ParseHex("#11131A")),
            MbxThemePreset.StormSlate => CreateThemeFromTriad("#5B6E7D", "#5C0403", "#EDB1B0"),
            MbxThemePreset.AbyssalNavy => CreateThemeFromTriad("#0D3651", "#EB313F", "#FFF7AE"),
            _ => CreateThemeFromPrimary(GetPresetPrimary(preset)),
        };

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

    private static MudTheme CreateDeepCharcoalTheme()
    {
        var theme = CreateThemeCore(
            ParseHex("#222222"),
            ParseHex("#CAC426"),
            ParseHex("#EED3BA"),
            ParseHex("#D8A84A"),
            ParseHex("#A85C3F"),
            darkSidebar: ParseHex("#2F3138"),
            darkMain: ParseHex("#262830"),
            darkSubSection: ParseHex("#1B1D23"),
            darkBackground: ParseHex("#111217"),
            lightSidebar: ParseHex("#DADDE6"),
            lightSubSection: ParseHex("#EEF1F6"),
            lightBackground: ParseHex("#F8F9FB"),
            lightMain: ParseHex("#FFFFFF"));

        theme.PaletteDark.PrimaryContrastText = "#F7F8FB";
        theme.PaletteLight.PrimaryContrastText = "#F7F8FB";
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
