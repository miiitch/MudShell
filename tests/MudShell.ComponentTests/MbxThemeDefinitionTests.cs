using MudShell.Theme;
using Xunit;

namespace MudShell.ComponentTests;

/// <summary>An explicit theme definition is applied as written, role by role, in both modes.</summary>
public class MbxThemeDefinitionTests
{
    private static MbxPaletteDefinition Dark() => new()
    {
        Primary = "#FFA500",
        Secondary = "#6CB6FF",
        Info = "#6CB6FF",
        Success = "#3FCF8E",
        Warning = "#F5C542",
        Error = "#FF7B72",
        Background = "#090B10",
        PageBackground = "#0D1017",
        Surface = "#151A24",
        NavigationBackground = "#10141C",
        TextPrimary = "#E6EAF2",
        TextSecondary = "#A7B0C4",
        Border = "#2A3242",
    };

    private static MbxPaletteDefinition Light() => new()
    {
        Primary = "#FFA500",
        Secondary = "#0B62C4",
        Info = "#0B62C4",
        Success = "#17794A",
        Warning = "#8A6100",
        Error = "#C62828",
        Background = "#E7EBF3",
        PageBackground = "#F4F6FA",
        Surface = "#FFFFFF",
        NavigationBackground = "#EEF1F7",
        TextPrimary = "#1A2030",
        TextSecondary = "#4E586E",
        Border = "#D5DAE5",
    };

    private static string Hex(MudBlazor.Utilities.MudColor color) => color.Value[..7].ToLowerInvariant();

    [Fact]
    public void Should_UseEachRoleAsGiven_When_CreatingThemeFromDefinition()
    {
        // When
        var theme = MbxTheme.CreateTheme(new MbxThemeDefinition(Dark(), Light()));

        // Then
        Assert.Equal("#ffa500", Hex(theme.PaletteDark.Primary));
        Assert.Equal("#6cb6ff", Hex(theme.PaletteDark.Secondary));
        Assert.Equal("#f5c542", Hex(theme.PaletteDark.Warning));
        Assert.Equal("#ff7b72", Hex(theme.PaletteDark.Error));
        Assert.Equal("#151a24", Hex(theme.PaletteDark.Surface));
        Assert.Equal("#0d1017", Hex(theme.PaletteDark.BackgroundGray));
        Assert.Equal("#10141c", Hex(theme.PaletteDark.DrawerBackground));
        Assert.Equal("#e6eaf2", Hex(theme.PaletteDark.TextPrimary));
        Assert.Equal("#2a3242", Hex(theme.PaletteDark.LinesDefault));

        Assert.Equal("#0b62c4", Hex(theme.PaletteLight.Secondary));
        Assert.Equal("#8a6100", Hex(theme.PaletteLight.Warning));
        Assert.Equal("#ffffff", Hex(theme.PaletteLight.Surface));
        Assert.Equal("#f4f6fa", Hex(theme.PaletteLight.BackgroundGray));
        Assert.Equal("#1a2030", Hex(theme.PaletteLight.TextPrimary));
        Assert.Equal("#d5dae5", Hex(theme.PaletteLight.TableLines));
    }

    [Fact]
    public void Should_PickReadableTextOnAccents_When_CreatingThemeFromDefinition()
    {
        // When
        var theme = MbxTheme.CreateTheme(new MbxThemeDefinition(Dark(), Light()));

        // Then: dark text on the bright orange, white text on the deep light-mode blue.
        Assert.NotEqual("#ffffff", Hex(theme.PaletteDark.PrimaryContrastText));
        Assert.Equal("#ffffff", Hex(theme.PaletteLight.SecondaryContrastText));
    }

    [Fact]
    public void Should_Throw_When_DefinitionIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => MbxTheme.CreateTheme((MbxThemeDefinition)null!));
    }
}
