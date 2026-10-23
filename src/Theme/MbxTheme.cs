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
        [Obsolete("Merged into Crimson (too close visually). Resolves to Crimson.")]
        Ruby,
        Saffron,
        [Obsolete("Merged into Cobalt (too close visually). Resolves to Cobalt.")]
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
        [Obsolete("Merged into DeepCharcoal (too close visually). Resolves to DeepCharcoal.")]
        MidnightStatic,
        StormSlate,
        AbyssalNavy,
        CosmicNight,
        MorningButter,
        RaspberryRed,
        [Obsolete("Merged into AbyssalNavy (too close visually). Resolves to AbyssalNavy.")]
        DeepSpaceBlue,
        FairyTale,
        GiantsOrange,
        LavenderFog,
        [Obsolete("Merged into DeepCharcoal (too close visually). Resolves to DeepCharcoal.")]
        Ganache,
        [Obsolete("Merged into FairyTale (too close visually). Resolves to FairyTale.")]
        CottonRose,
        HotFuchsia,
        Chartreuse,
        ElectricRose,
        IcyBlue,
        [Obsolete("Merged into Crimson (too close visually). Resolves to Crimson.")]
        BerryRed,
        SandyClay,
        [Obsolete("Merged into DeepCharcoal (too close visually). Resolves to DeepCharcoal.")]
        ShadowGrey,
        PurpleMood,
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
        new(MbxThemePreset.Saffron, "Saffron", "#D6A800"), // Jaune safran 
        new(MbxThemePreset.MagmaMist, "Magma Mist", "#F06F0E"), // Orange volcanique / or / vert-de-mer
        new(MbxThemePreset.AuburnDunes, "Auburn Dunes", "#71351A"), // Auburn / sable / cacao
        new(MbxThemePreset.OlivineMatisse, "Olivine Matisse", "#A2BD7F"), // Olivine / matisse / mousse
        new(MbxThemePreset.FernGreen, "Fern Green", "#557743"), // Vert fougère / feuille / sauge
        new(MbxThemePreset.PeacockDusk, "Peacock Dusk", "#09443D"), // Vert pétrole / lin / prune
        new(MbxThemePreset.DeepCharcoal, "Deep Charcoal", "#222222"), // Charbon profond / or vert / abricot
        new(MbxThemePreset.EarthRoot,    "Earth Root",    "#48261D"), // Terre / feu / ciel
        new(MbxThemePreset.ObsidianPulse, "Obsidian Pulse", "#4C1413"), // Obsidienne / rosé / ciel
        new(MbxThemePreset.Onyx,         "Onyx",         "#151311"), // Onyx / mauve / sable 
        new(MbxThemePreset.StormSlate,     "Storm Slate",    "#5B6E7D"), // Palette gris-bleu, bordeaux et blush
        new(MbxThemePreset.AbyssalNavy,    "Abyssal Navy",   "#0D3651"), // Palette marine, corail et sable
        new(MbxThemePreset.CosmicNight,    "Cosmic Night",   "#FFA500"), // Noir profond avec accents orange chauds
        new(MbxThemePreset.MorningButter,  "Morning Butter", "#8662F6"), // Crème, lavande et accents pastel
        new(MbxThemePreset.RaspberryRed,   "Raspberry Red",  "#EE005A"), // Rose framboise / nuit profonde 
        new(MbxThemePreset.FairyTale,      "Fairy Tale",     "#FFCEEB"), // Rose bonbon / orange
        new(MbxThemePreset.GiantsOrange,   "Giants Orange",  "#FF6634"), // Orange vitaminé / rose
        new(MbxThemePreset.LavenderFog,    "Lavender Fog",   "#D5C9DD"), // Lavande / chocolat  
        new(MbxThemePreset.HotFuchsia,     "Hot Fuchsia",    "#F8395A"), // Rose chaud / blush
        new(MbxThemePreset.Chartreuse,     "Chartreuse",     "#C1FE1A"), // Chartreuse / fuchsia
        new(MbxThemePreset.ElectricRose,   "Electric Rose",  "#FE00AE"), // Vert acide / rose électrique
        new(MbxThemePreset.IcyBlue,        "Icy Blue",       "#B3E6FB"), // Bleu glace / rouge 
        new(MbxThemePreset.SandyClay,      "Sandy Clay",     "#D4AA7D"), // Sable / charbon 
        new(MbxThemePreset.PurpleMood,     "Purple Mood",    "#512BD4"), // Violet profond / nuit encrée
    ];

    public static string GetPresetPrimary(MbxThemePreset preset)
        => Presets.First(p => p.Preset == Resolve(preset)).PrimaryColor;

#pragma warning disable CS0618 // Obsolete presets are mapped to their replacement here.
    /// <summary>Maps retired presets to the preset that replaced them.</summary>
    public static MbxThemePreset Resolve(MbxThemePreset preset)
        => preset switch
        {
            MbxThemePreset.Azure => MbxThemePreset.Cobalt,
            MbxThemePreset.MidnightStatic => MbxThemePreset.DeepCharcoal,
            MbxThemePreset.Ganache => MbxThemePreset.DeepCharcoal,
            MbxThemePreset.ShadowGrey => MbxThemePreset.DeepCharcoal,
            MbxThemePreset.DeepSpaceBlue => MbxThemePreset.AbyssalNavy,
            MbxThemePreset.CottonRose => MbxThemePreset.FairyTale,
            MbxThemePreset.Ruby => MbxThemePreset.Crimson,
            MbxThemePreset.BerryRed => MbxThemePreset.Crimson,
            _ => preset,
        };
#pragma warning restore CS0618

    /// <summary>Returns a <see cref="MudTheme"/> pre-configured with both palettes.</summary>
    public static MudTheme CreateTheme() => CreateTheme(MbxThemePreset.Cobalt);

    /// <summary>Creates a theme from a curated preset.</summary>
    public static MudTheme CreateTheme(MbxThemePreset preset)
        => Resolve(preset) switch
        {
            MbxThemePreset.Lime => CreateLimeTheme(),
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
                darkPrimary: ParseHex("#9CC285"),
                darkSidebar: ParseHex("#2C4022"),
                darkMain: ParseHex("#24361C"),
                darkSubSection: ParseHex("#1B2815"),
                darkBackground: ParseHex("#121C0E"),
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
                darkPrimary: ParseHex("#3FB8A6"),
                darkSidebar: ParseHex("#0C2E29"),
                darkMain: ParseHex("#0E3A34"),
                darkSubSection: ParseHex("#0A2A25"),
                darkBackground: ParseHex("#061E1A"),
                lightSidebar: ParseHex("#C9C1B0"),
                lightSubSection: ParseHex("#F0EDDF"),
                lightBackground: ParseHex("#FAF8F3"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.DeepCharcoal => CreateDeepCharcoalTheme(),
            MbxThemePreset.EarthRoot => CreateThemeFromTriadCore("#48261D", "#F94C00", "#CAE7F7", darkPrimary: "#F94C00"),
            MbxThemePreset.ObsidianPulse => CreateThemeFromTriadCore("#4C1413", "#CAE7F7", "#EA6B7E", darkPrimary: "#EA6B7E"),
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
            MbxThemePreset.StormSlate => CreateThemeFromTriad("#5B6E7D", "#5C0403", "#EDB1B0"),
            MbxThemePreset.AbyssalNavy => CreateThemeFromTriad("#0D3651", "#EB313F", "#FFF7AE"),
            MbxThemePreset.CosmicNight => CreateCosmicNightTheme(),
            MbxThemePreset.MorningButter => CreateThemeCore(
                ParseHex("#8662F6"),
                ParseHex("#5A9CE6"),
                ParseHex("#2DA96F"),
                ParseHex("#E6A51B"),
                ParseHex("#E7777D"),
                darkPrimary: ParseHex("#D6A800"),
                darkSidebar: ParseHex("#352A4F"),
                darkMain: ParseHex("#30283B"),
                darkSubSection: ParseHex("#241E2D"),
                darkBackground: ParseHex("#18141E"),
                lightSidebar: ParseHex("#F7EFD9"),
                lightSubSection: ParseHex("#FCF7EB"),
                lightBackground: ParseHex("#FAF3E3"),
                lightMain: ParseHex("#FFFCF5"),
                lightTextPrimary: ParseHex("#29251F"),
                lightTextSecondary: ParseHex("#716857"),
                lightNeutral: ParseHex("#F4EAD4"),
                lightBorder: ParseHex("#EADAB6")),
            MbxThemePreset.RaspberryRed => CreateThemeCore(
                ParseHex("#EE005A"),
                ParseHex("#012641"),
                ParseHex("#F3D98F"),
                ParseHex("#FFB545"),
                ParseHex("#C62828"),
                darkSidebar: ParseHex("#5E0024"),
                darkMain: ParseHex("#8A0038"),
                darkSubSection: ParseHex("#2A0A16"),
                darkBackground: ParseHex("#11040A"),
                lightSidebar: ParseHex("#F8CCD8"),
                lightSubSection: ParseHex("#FFF1F6"),
                lightBackground: ParseHex("#FFF8FA"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.FairyTale => CreateThemeCore(
                ParseHex("#FFCEEB"),
                ParseHex("#FF6634"),
                ParseHex("#F8395A"),
                ParseHex("#F3D98F"),
                ParseHex("#EE005A"),
                darkSidebar: ParseHex("#7C3A66"),
                darkMain: ParseHex("#A75E8A"),
                darkSubSection: ParseHex("#3E2238"),
                darkBackground: ParseHex("#1C1019"),
                lightSidebar: ParseHex("#FFF0F8"),
                lightSubSection: ParseHex("#FFF7FB"),
                lightBackground: ParseHex("#FFFDFE"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.GiantsOrange => CreateThemeCore(
                ParseHex("#FF6634"),
                ParseHex("#FFCEEB"),
                ParseHex("#F8395A"),
                ParseHex("#D5C9DD"),
                ParseHex("#34292A"),
                darkSidebar: ParseHex("#7C2F16"),
                darkMain: ParseHex("#A84A25"),
                darkSubSection: ParseHex("#35170E"),
                darkBackground: ParseHex("#170B08"),
                lightSidebar: ParseHex("#FFE7D7"),
                lightSubSection: ParseHex("#FFF4ED"),
                lightBackground: ParseHex("#FFF9F6"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.LavenderFog => CreateThemeCore(
                ParseHex("#D5C9DD"),
                ParseHex("#34292A"),
                ParseHex("#FFCEEB"),
                ParseHex("#D4AA7D"),
                ParseHex("#EE005A"),
                darkSidebar: ParseHex("#5C4B63"),
                darkMain: ParseHex("#75647D"),
                darkSubSection: ParseHex("#2F2633"),
                darkBackground: ParseHex("#161217"),
                lightSidebar: ParseHex("#EEE7F2"),
                lightSubSection: ParseHex("#F8F4FA"),
                lightBackground: ParseHex("#FCFAFD"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.HotFuchsia => CreateThemeCore(
                ParseHex("#F8395A"),
                ParseHex("#EEB3B5"),
                ParseHex("#FFCEEB"),
                ParseHex("#D4AA7D"),
                ParseHex("#012641"),
                darkSidebar: ParseHex("#7A1D35"),
                darkMain: ParseHex("#A62A4A"),
                darkSubSection: ParseHex("#340D18"),
                darkBackground: ParseHex("#16070B"),
                lightSidebar: ParseHex("#F9CED6"),
                lightSubSection: ParseHex("#FFF1F4"),
                lightBackground: ParseHex("#FFF9FA"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.Chartreuse => CreateThemeCore(
                ParseHex("#C1FE1A"),
                ParseHex("#FE00AE"),
                ParseHex("#012641"),
                ParseHex("#F3D98F"),
                ParseHex("#EE005A"),
                darkSidebar: ParseHex("#4D6608"),
                darkMain: ParseHex("#70920C"),
                darkSubSection: ParseHex("#1F2B03"),
                darkBackground: ParseHex("#0D1201"),
                lightSidebar: ParseHex("#E6F8A6"),
                lightSubSection: ParseHex("#F8FCE7"),
                lightBackground: ParseHex("#FCFEEF"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.ElectricRose => CreateThemeCore(
                ParseHex("#FE00AE"),
                ParseHex("#C1FE1A"),
                ParseHex("#FFCEEB"),
                ParseHex("#F3D98F"),
                ParseHex("#012641"),
                darkSidebar: ParseHex("#6E006C"),
                darkMain: ParseHex("#98008C"),
                darkSubSection: ParseHex("#2E002B"),
                darkBackground: ParseHex("#110011"),
                lightSidebar: ParseHex("#F9C6EA"),
                lightSubSection: ParseHex("#FFF0FA"),
                lightBackground: ParseHex("#FFF9FD"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.IcyBlue => CreateThemeCore(
                ParseHex("#B3E6FB"),
                ParseHex("#C21121"),
                ParseHex("#7298C7"),
                ParseHex("#F3D98F"),
                ParseHex("#EE005A"),
                darkSidebar: ParseHex("#4A6C82"),
                darkMain: ParseHex("#6B91AA"),
                darkSubSection: ParseHex("#1E303B"),
                darkBackground: ParseHex("#0B141A"),
                lightSidebar: ParseHex("#DBF1FD"),
                lightSubSection: ParseHex("#F4FAFE"),
                lightBackground: ParseHex("#FBFDFF"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.SandyClay => CreateThemeCore(
                ParseHex("#D4AA7D"),
                ParseHex("#272727"),
                ParseHex("#EEB3B5"),
                ParseHex("#D5C9DD"),
                ParseHex("#FF6634"),
                darkSidebar: ParseHex("#7D6042"),
                darkMain: ParseHex("#9E7C5A"),
                darkSubSection: ParseHex("#382B23"),
                darkBackground: ParseHex("#181411"),
                lightSidebar: ParseHex("#EAD0B3"),
                lightSubSection: ParseHex("#F6EEE7"),
                lightBackground: ParseHex("#FCFAF7"),
                lightMain: ParseHex("#FFFFFF")),
            MbxThemePreset.PurpleMood => CreateThemeCore(
                ParseHex("#512BD4"),
                ParseHex("#A98BFF"),
                ParseHex("#4CB782"),
                ParseHex("#F2B84B"),
                ParseHex("#F2555A"),
                darkSidebar: ParseHex("#2E2B52"),
                darkMain: ParseHex("#242142"),
                darkSubSection: ParseHex("#1C1A38"),
                darkBackground: ParseHex("#14132A"),
                lightSidebar: ParseHex("#DDD5F5"),
                lightSubSection: ParseHex("#F1EDFB"),
                lightBackground: ParseHex("#F7F5FC"),
                lightMain: ParseHex("#FFFFFF")),
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
        => CreateThemeFromTriadCore(primaryHex, secondaryHex, tertiaryHex);

    private static MudTheme CreateThemeFromTriadCore(string primaryHex, string secondaryHex, string tertiaryHex, string? darkPrimary = null)
    {
        var primary = ParseHex(primaryHex);
        var secondary = ParseHex(secondaryHex);
        var tertiary = ParseHex(tertiaryHex);

        var warning = Mix(primary, ParseHex("#ffb545"), 0.68);
        var error = Mix(primary, ParseHex("#ff4d6d"), 0.60);

        return CreateThemeCore(primary, secondary, tertiary, warning, error,
            darkPrimary: darkPrimary is null ? null : ParseHex(darkPrimary));
    }

    private static MudTheme CreateCosmicNightTheme()
    {
        var primary = ParseHex("#FFA500");      // Orange chaud
        var info = ParseHex("#4ECDC4");         // Teal/Cyan
        var success = ParseHex("#00D98E");      // Vert émeraude
        var warning = ParseHex("#FFB84D");      // Orange pâle
        var error = ParseHex("#FF6B6B");        // Rouge corail

        return CreateThemeCore(
            primary,
            info,
            success,
            warning,
            error,
            darkSidebar: ParseHex("#1a1a1a"),
            darkMain: ParseHex("#0f0f0f"),
            darkSubSection: ParseHex("#0a0a0a"),
            darkBackground: ParseHex("#050505"),
            lightSidebar: ParseHex("#FFE5CC"),
            lightSubSection: ParseHex("#FFF3E0"),
            lightBackground: ParseHex("#FFFBF7"),
            lightMain: ParseHex("#FFFFFF"));
    }

    private static MudTheme CreateLimeTheme()
    {
        var primary = ParseHex("#A9C93A");
        var info = ParseHex("#8bbf3f");
        var success = ParseHex("#5fb86b");
        var warning = ParseHex("#d8a84a");
        var error = ParseHex("#d96a74");

        return CreateThemeCore(
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
    }

    private static MudTheme CreateDeepCharcoalTheme()
    {
        return CreateThemeCore(
            ParseHex("#222222"),
            ParseHex("#CAC426"),
            ParseHex("#EED3BA"),
            ParseHex("#D8A84A"),
            ParseHex("#A85C3F"),
            darkPrimary: ParseHex("#CAC426"),
            darkSidebar: ParseHex("#2F3138"),
            darkMain: ParseHex("#262830"),
            darkSubSection: ParseHex("#1B1D23"),
            darkBackground: ParseHex("#111217"),
            lightSidebar: ParseHex("#DADDE6"),
            lightSubSection: ParseHex("#EEF1F6"),
            lightBackground: ParseHex("#F8F9FB"),
            lightMain: ParseHex("#FFFFFF"));
    }

    private static MudTheme CreateThemeCore(
        HexColor primary,
        HexColor info,
        HexColor success,
        HexColor warning,
        HexColor error,
        HexColor? darkPrimary = null,
        HexColor? darkSidebar = null,
        HexColor? darkMain = null,
        HexColor? darkSubSection = null,
        HexColor? darkBackground = null,
        HexColor? lightSidebar = null,
        HexColor? lightSubSection = null,
        HexColor? lightBackground = null,
        HexColor? lightMain = null,
        HexColor? lightTextPrimary = null,
        HexColor? lightTextSecondary = null,
        HexColor? lightNeutral = null,
        HexColor? lightBorder = null)
    {
        // ── Dark mode: DrawerBackground > Surface > BackgroundGray > Background ──
        // Surface (cards) MUST be lighter than BackgroundGray (page area) so cards pop.
        // Each layer is capped in luminance so the fixed dark text colors stay readable (WCAG AA),
        // whatever the primary is (pastel primaries would otherwise produce mid-tone surfaces).
        var resolvedDarkSidebar    = DarkenToLuminance(darkSidebar ?? Mix(primary, ParseHex("#2a2d4a"), 0.50), DarkDrawerMaxLuminance); // DrawerBackground — lightest
        var resolvedDarkMain       = DarkenToLuminance(darkMain ?? Mix(primary, ParseHex("#1a1d33"), 0.72), DarkSurfaceMaxLuminance);   // Surface — cards (medium)
        var resolvedDarkSubSection = DarkenToLuminance(darkSubSection ?? Mix(primary, ParseHex("#12142a"), 0.85), LayerBelow(resolvedDarkMain)); // BackgroundGray — page area (darker)
        var resolvedDarkBackground = DarkenToLuminance(darkBackground ?? Mix(primary, ParseHex("#0a0c1c"), 0.91), LayerBelow(resolvedDarkSubSection)); // Background — darkest

        // ── Light mode: Surface stays white; page background uses neutral gray for stronger contrast ──
        var resolvedLightSidebar    = LightenToLuminance(lightSidebar ?? Mix(primary, ParseHex("#e0e0f4"), 0.65), LightDrawerMinLuminance); // DrawerBackground — clearly tinted
        var resolvedLightSubSection = lightSubSection ?? Mix(primary, ParseHex("#f5f5ff"), 0.93); // BackgroundGray — subtle
        var resolvedLightMain       = lightMain       ?? ParseHex("#ffffff");                      // Surface — pure white
        var resolvedLightBackground = DarkenToLuminance(lightBackground ?? ParseHex("#f1f3f6"), LayerBelow(resolvedLightMain)); // Background — neutral gray, visibly darker than cards

        // Accents must stand out (WCAG 1.4.11, ≥ 3:1) against every surface they are drawn on.
        // Dark mode lightens them, light mode darkens them; the hue is preserved.
        HexColor[] darkLayers = [resolvedDarkSidebar, resolvedDarkMain, resolvedDarkSubSection, resolvedDarkBackground];
        HexColor[] lightLayers = [resolvedLightSidebar, resolvedLightMain, resolvedLightSubSection, resolvedLightBackground];
        HexColor ForDark(HexColor c) => EnsureFillReadable(EnsureContrast(c, darkLayers, AccentMinContrast, lighten: true), lighten: true);
        HexColor ForLight(HexColor c) => EnsureFillReadable(EnsureContrast(c, lightLayers, AccentMinContrast, lighten: false), lighten: false);

        var resolvedDarkPrimary = ForDark(darkPrimary ?? primary);
        primary = ForLight(primary);
        var resolvedLightTextPrimary = lightTextPrimary ?? ParseHex("#1b2742");
        var resolvedLightTextSecondary = lightTextSecondary ?? ParseHex("#435070");
        var resolvedLightNeutral = lightNeutral ?? ParseHex("#d9e2f2");
        var resolvedLightBorder = lightBorder ?? Mix(resolvedLightSubSection, ParseHex("#b9c7df"), 0.46);

        // MudBlazor falls back to its own default (a bright pink, "#FF4081") for any color role left
        // unset, and Color.Secondary is commonly reached for by consumers. Derive a deliberate accent
        // from Primary instead of leaving this gap — see https://github.com/miiitch/MudShell/issues/14.
        var darkSecondary = ForDark(HueShiftAccent(resolvedDarkPrimary, degrees: 150));
        var secondary = ForLight(HueShiftAccent(primary, degrees: 150));

        var darkPalette = new PaletteDark
        {
            Primary = ToHex(resolvedDarkPrimary),
            PrimaryContrastText = ToHex(BestContrastText(resolvedDarkPrimary)),
            Secondary = ToHex(darkSecondary),
            SecondaryContrastText = ToHex(BestContrastText(darkSecondary)),
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
        };
        ApplyStatusColors(darkPalette, ForDark(info), ForDark(success), ForDark(warning), ForDark(error));

        var lightPalette = new PaletteLight
        {
            Primary = ToHex(primary),
            PrimaryContrastText = ToHex(BestContrastText(primary)),
            Secondary = ToHex(secondary),
            SecondaryContrastText = ToHex(BestContrastText(secondary)),
            Black = ToHex(lightTextPrimary ?? ParseHex("#0f1325")),
            Surface = ToHex(resolvedLightMain),
            Background = ToHex(resolvedLightBackground),
            BackgroundGray = ToHex(resolvedLightSubSection),
            DrawerBackground = ToHex(resolvedLightSidebar),
            AppbarBackground = ToRgba(resolvedLightBackground, 0.90),
            AppbarText = ToHex(lightTextPrimary ?? ParseHex("#1a2540")),
            TextPrimary = ToHex(resolvedLightTextPrimary),
            TextSecondary = ToHex(resolvedLightTextSecondary),
            ActionDefault = ToHex(resolvedLightTextSecondary),
            GrayLight = ToHex(Mix(resolvedLightSubSection, resolvedLightNeutral, 0.34)),
            GrayLighter = ToHex(Mix(resolvedLightMain, lightNeutral ?? ParseHex("#f2f6fd"), 0.38)),
            LinesDefault = ToHex(resolvedLightBorder),
            TableLines = ToHex(resolvedLightBorder),
            Divider = ToHex(lightBorder ?? Mix(resolvedLightSubSection, ParseHex("#9fb2d3"), 0.40)),
            OverlayLight = ToRgba(ParseHex("#ffffff"), 0.60),
        };
        ApplyStatusColors(lightPalette, ForLight(info), ForLight(success), ForLight(warning), ForLight(error));

        return new MudTheme
        {
            PaletteDark = darkPalette,
            PaletteLight = lightPalette,
            LayoutProperties = new LayoutProperties(),
        };
    }

    private readonly record struct HexColor(int R, int G, int B);

    // Luminance caps derived from the fixed dark-mode text colors:
    // TextSecondary #9fa9c7 ≥ 4.5:1 on Surface, DrawerText #c4cce2 ≥ 4.5:1 on the drawer.
    private const double DarkSurfaceMaxLuminance = 0.045;
    private const double DarkDrawerMaxLuminance = 0.088;
    // Light TextSecondary #435070 ≥ 4.5:1 on the tinted drawer.
    private const double LightDrawerMinLuminance = 0.56;
    private const double AccentMinContrast = 3.0;
    private const double LayerSeparation = 1.06;
    private static readonly HexColor DarkContrastText = new(0x11, 0x13, 0x0f);
    private static readonly HexColor White = new(255, 255, 255);
    private static readonly HexColor Black = new(0, 0, 0);

    private static void ApplyStatusColors(Palette palette, HexColor info, HexColor success, HexColor warning, HexColor error)
    {
        palette.Info = ToHex(info);
        palette.InfoContrastText = ToHex(BestContrastText(info));
        palette.Success = ToHex(success);
        palette.SuccessContrastText = ToHex(BestContrastText(success));
        palette.Warning = ToHex(warning);
        palette.WarningContrastText = ToHex(BestContrastText(warning));
        palette.Error = ToHex(error);
        palette.ErrorContrastText = ToHex(BestContrastText(error));
    }

    internal static double RelativeLuminance(int r, int g, int b)
    {
        static double Channel(int c)
        {
            var v = c / 255d;
            return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Channel(r) + 0.7152 * Channel(g) + 0.0722 * Channel(b);
    }

    private static double Luminance(HexColor c) => RelativeLuminance(c.R, c.G, c.B);

    private static double ContrastRatio(HexColor a, HexColor b)
    {
        var la = Luminance(a);
        var lb = Luminance(b);
        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }

    /// <summary>Max luminance for a layer that must sit visibly below <paramref name="above"/>.</summary>
    private static double LayerBelow(HexColor above)
        => Math.Min((Luminance(above) + 0.05) / LayerSeparation - 0.05, Luminance(above));

    private static HexColor DarkenToLuminance(HexColor color, double maxLuminance)
    {
        var result = color;
        for (var i = 1; i <= 100 && Luminance(result) > maxLuminance; i++)
            result = Mix(color, Black, i / 100d);
        return result;
    }

    private static HexColor LightenToLuminance(HexColor color, double minLuminance)
    {
        var result = color;
        for (var i = 1; i <= 100 && Luminance(result) < minLuminance; i++)
            result = Mix(color, White, i / 100d);
        return result;
    }

    /// <summary>
    /// Shifts the HSL lightness of <paramref name="color"/> (keeping its hue) until it reaches
    /// <paramref name="minRatio"/> against every background.
    /// </summary>
    private static HexColor EnsureContrast(HexColor color, IReadOnlyList<HexColor> backgrounds, double minRatio, bool lighten)
    {
        bool Ok(HexColor c) => backgrounds.All(bg => ContrastRatio(c, bg) >= minRatio);
        if (Ok(color))
            return color;

        var (h, s, l) = ToHsl(color);
        var step = lighten ? 0.01 : -0.01;
        for (var next = l + step; next is >= 0d and <= 1d; next += step)
        {
            var candidate = FromHsl(h, s, next);
            if (Ok(candidate))
                return candidate;
        }

        return lighten ? White : Black;
    }

    /// <summary>White or near-black, whichever reads best on <paramref name="background"/>.</summary>
    private static HexColor BestContrastText(HexColor background)
        => ContrastRatio(White, background) >= ContrastRatio(DarkContrastText, background) ? White : DarkContrastText;

    /// <summary>
    /// Mid-tone fills read poorly with both white and dark text; nudge the lightness (in the
    /// direction that also keeps the accent contrast against the surfaces) until one text reaches 4.5:1.
    /// </summary>
    private static HexColor EnsureFillReadable(HexColor color, bool lighten)
    {
        const double minTextContrast = 4.5;
        bool Ok(HexColor c) => ContrastRatio(BestContrastText(c), c) >= minTextContrast;
        if (Ok(color))
            return color;

        var (h, s, l) = ToHsl(color);
        var step = lighten ? 0.005 : -0.005;
        for (var next = l + step; next is >= 0d and <= 1d; next += step)
        {
            var candidate = FromHsl(h, s, next);
            if (Ok(candidate))
                return candidate;
        }

        return color;
    }

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

    /// <summary>
    /// Rotates a color's hue to derive a distinct-but-related accent (e.g. Secondary from Primary),
    /// clamping saturation/lightness so the result stays legible as an accent in both palettes.
    /// </summary>
    private static HexColor HueShiftAccent(HexColor color, double degrees)
    {
        var (h, s, l) = ToHsl(color);
        var shiftedHue = (h + degrees) % 360;
        var accentSaturation = Math.Clamp(s, 0.45, 0.85);
        var accentLightness = Math.Clamp(l, 0.42, 0.62);
        return FromHsl(shiftedHue, accentSaturation, accentLightness);
    }

    private static (double H, double S, double L) ToHsl(HexColor color)
    {
        var r = color.R / 255d;
        var g = color.G / 255d;
        var b = color.B / 255d;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var l = (max + min) / 2d;

        if (max == min)
            return (0d, 0d, l);

        var delta = max - min;
        var s = l > 0.5 ? delta / (2d - max - min) : delta / (max + min);

        double h;
        if (max == r)
            h = (g - b) / delta + (g < b ? 6d : 0d);
        else if (max == g)
            h = (b - r) / delta + 2d;
        else
            h = (r - g) / delta + 4d;

        return (h * 60d, s, l);
    }

    private static HexColor FromHsl(double h, double s, double l)
    {
        if (s == 0d)
        {
            var gray = (int)Math.Round(l * 255d);
            return new HexColor(gray, gray, gray);
        }

        var q = l < 0.5 ? l * (1d + s) : l + s - l * s;
        var p = 2d * l - q;
        var hue = h / 360d;

        return new HexColor(
            (int)Math.Round(HueToRgb(p, q, hue + 1d / 3d) * 255d),
            (int)Math.Round(HueToRgb(p, q, hue) * 255d),
            (int)Math.Round(HueToRgb(p, q, hue - 1d / 3d) * 255d));
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0d) t += 1d;
        if (t > 1d) t -= 1d;
        if (t < 1d / 6d) return p + (q - p) * 6d * t;
        if (t < 1d / 2d) return q;
        if (t < 2d / 3d) return p + (q - p) * (2d / 3d - t) * 6d;
        return p;
    }

    private static string ToRgba(HexColor color, double alpha)
        => $"rgba({color.R},{color.G},{color.B},{Math.Clamp(alpha, 0d, 1d).ToString("0.##", CultureInfo.InvariantCulture)})";

    /// <inheritdoc cref="CreateTheme"/>
    public static MudTheme CreateDarkTheme() => CreateTheme();
}
