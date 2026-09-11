using MudShell.Theme;
using Xunit;

namespace MudShell.ComponentTests;

/// <summary>
/// Guards against MudBlazor's default pink ("#FF4081") leaking through for <c>Color.Secondary</c>
/// consumers when a preset never sets <c>Palette.Secondary</c> explicitly.
/// </summary>
public class MbxThemeSecondaryColorTests
{
    private const string MudBlazorDefaultSecondary = "#ff4081";

    public static TheoryData<MbxTheme.MbxThemePreset> AllPresets()
    {
        var data = new TheoryData<MbxTheme.MbxThemePreset>();
        foreach (var preset in Enum.GetValues<MbxTheme.MbxThemePreset>())
            data.Add(preset);
        return data;
    }

    [Theory]
    [MemberData(nameof(AllPresets))]
    public void Should_SetDeliberateSecondary_When_CreatingThemeFromPreset(MbxTheme.MbxThemePreset preset)
    {
        // When
        var theme = MbxTheme.CreateTheme(preset);

        // Then
        Assert.NotEqual(MudBlazorDefaultSecondary, theme.PaletteDark.Secondary.Value.ToLowerInvariant());
        Assert.NotEqual(MudBlazorDefaultSecondary, theme.PaletteLight.Secondary.Value.ToLowerInvariant());
    }
}
