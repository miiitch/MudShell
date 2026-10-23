using MudShell.Theme;
using Xunit;

namespace MudShell.ComponentTests;

public class MbxThemeMorningButterTests
{
    [Fact]
    public void Should_UseWarmCreamAndLavenderPalette_When_CreatingTheme()
    {
        var theme = MbxTheme.CreateTheme(MbxTheme.MbxThemePreset.MorningButter);

        Assert.Equal("#8662F6", MbxTheme.GetPresetPrimary(MbxTheme.MbxThemePreset.MorningButter));
        Assert.Equal("#8662f6", theme.PaletteLight.Primary.Value[..7].ToLowerInvariant());
        Assert.Equal("#faf3e3", theme.PaletteLight.Background.Value[..7].ToLowerInvariant());
        Assert.Equal("#fffcf5", theme.PaletteLight.Surface.Value[..7].ToLowerInvariant());
        Assert.Equal("#29251f", theme.PaletteLight.TextPrimary.Value[..7].ToLowerInvariant());
        Assert.Equal("#eadab6", theme.PaletteLight.LinesDefault.Value[..7].ToLowerInvariant());
        Assert.Equal("#2da96f", theme.PaletteLight.Success.Value[..7].ToLowerInvariant());
        Assert.Equal("#e6a51b", theme.PaletteLight.Warning.Value[..7].ToLowerInvariant());
        Assert.Equal("#e7777d", theme.PaletteLight.Error.Value[..7].ToLowerInvariant());
        Assert.Equal("#18141e", theme.PaletteDark.Background.Value[..7].ToLowerInvariant());
        Assert.Equal("#30283b", theme.PaletteDark.Surface.Value[..7].ToLowerInvariant());
        Assert.Equal("#d6a800", theme.PaletteDark.Primary.Value[..7].ToLowerInvariant());
    }
}
