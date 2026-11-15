namespace MudShell.Theme;

/// <summary>
/// A theme written out role by role, for light and for dark, instead of derived from one primary color.
/// Pass it to <see cref="MbxTheme.CreateTheme(MbxThemeDefinition)"/>.
/// </summary>
/// <param name="Dark">Colors of the dark mode.</param>
/// <param name="Light">Colors of the light mode.</param>
public sealed record MbxThemeDefinition(MbxPaletteDefinition Dark, MbxPaletteDefinition Light);

/// <summary>
/// The colors of one mode. Every value is a hex color (<c>#RGB</c> or <c>#RRGGBB</c>) and is used as given:
/// no contrast correction is applied, so the caller owns the readability of the palette.
/// </summary>
public sealed record MbxPaletteDefinition
{
    /// <summary>Main action color (the one primary button of a page).</summary>
    public required string Primary { get; init; }

    /// <summary>
    /// Second accent. With <see cref="MdsColorRoles.Split"/> it colors hover, selection, active navigation and focus.
    /// </summary>
    public required string Secondary { get; init; }

    public required string Info { get; init; }
    public required string Success { get; init; }
    public required string Warning { get; init; }
    public required string Error { get; init; }

    /// <summary>Outermost layer, behind the page.</summary>
    public required string Background { get; init; }

    /// <summary>Page area the cards sit on.</summary>
    public required string PageBackground { get; init; }

    /// <summary>Cards, grids and panels.</summary>
    public required string Surface { get; init; }

    /// <summary>Navigation rail and panel.</summary>
    public required string NavigationBackground { get; init; }

    public required string TextPrimary { get; init; }
    public required string TextSecondary { get; init; }

    /// <summary>Lines, table rules and dividers.</summary>
    public required string Border { get; init; }
}
