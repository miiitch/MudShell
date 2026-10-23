using MudBlazor;
using MudShell.Theme;
using Xunit;

namespace MudShell.ComponentTests;

/// <summary>
/// WCAG readability guard for every preset in both modes:
/// text ≥ 4.5:1 (TextPrimary ≥ 7:1 on Surface), accents/status colors ≥ 3:1 on every surface,
/// contrast texts ≥ 4.5:1 on their color, and visible separation between page and cards.
/// </summary>
public class MbxThemeContrastTests
{
    public static TheoryData<MbxTheme.MbxThemePreset, bool> PresetModes()
    {
        var data = new TheoryData<MbxTheme.MbxThemePreset, bool>();
        foreach (var preset in Enum.GetValues<MbxTheme.MbxThemePreset>())
        {
            data.Add(preset, false);
            data.Add(preset, true);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(PresetModes))]
    public void Should_MeetReadabilityContrast_When_CreatingThemeFromPreset(MbxTheme.MbxThemePreset preset, bool dark)
    {
        // Given
        var theme = MbxTheme.CreateTheme(preset);
        Palette p = dark ? theme.PaletteDark : theme.PaletteLight;
        var failures = new List<string>();

        void Check(string name, string fg, string bg, double min)
        {
            var ratio = Ratio(fg, bg);
            if (ratio < min)
                failures.Add($"{name}: {ratio:0.00} < {min}");
        }

        var surfaces = new (string Name, string Color)[]
        {
            ("Surface", p.Surface.Value),
            ("Background", p.Background.Value),
            ("BackgroundGray", p.BackgroundGray.Value),
            ("Drawer", p.DrawerBackground.Value),
        };

        // When / Then
        Check("TextPrimary/Surface", p.TextPrimary.Value, p.Surface.Value, 7);
        foreach (var (name, color) in surfaces)
        {
            Check($"TextPrimary/{name}", p.TextPrimary.Value, color, 4.5);
            if (name != "Drawer")
                Check($"TextSecondary/{name}", p.TextSecondary.Value, color, 4.5);
        }

        if (dark)
            Check("DrawerText/Drawer", p.DrawerText.Value, p.DrawerBackground.Value, 4.5);

        var accents = new (string Name, string Color, string Contrast)[]
        {
            ("Primary", p.Primary.Value, p.PrimaryContrastText.Value),
            ("Secondary", p.Secondary.Value, p.SecondaryContrastText.Value),
            ("Info", p.Info.Value, p.InfoContrastText.Value),
            ("Success", p.Success.Value, p.SuccessContrastText.Value),
            ("Warning", p.Warning.Value, p.WarningContrastText.Value),
            ("Error", p.Error.Value, p.ErrorContrastText.Value),
        };

        foreach (var (name, color, contrast) in accents)
        {
            Check($"{name}ContrastText/{name}", contrast, color, 4.5);
            foreach (var (surfaceName, surface) in surfaces)
                Check($"{name}/{surfaceName}", color, surface, 3);
        }

        Check("Surface vs Background", p.Surface.Value, p.Background.Value, 1.06);

        Assert.True(failures.Count == 0, $"{preset} {(dark ? "dark" : "light")}:\n  " + string.Join("\n  ", failures));
    }

    private static double Ratio(string a, string b)
    {
        var la = Luminance(a);
        var lb = Luminance(b);
        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }

    private static double Luminance(string color)
    {
        var (r, g, b) = Parse(color);
        static double C(int c)
        {
            var v = c / 255d;
            return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * C(r) + 0.7152 * C(g) + 0.0722 * C(b);
    }

    private static (int R, int G, int B) Parse(string color)
    {
        var hex = color.TrimStart('#');
        return (Convert.ToInt32(hex[0..2], 16), Convert.ToInt32(hex[2..4], 16), Convert.ToInt32(hex[4..6], 16));
    }
}
