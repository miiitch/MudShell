using MudShell.Theme;

namespace MudShell.Components.MainContent;

[Flags]
public enum MbxAccentBorderSides
{
    None = 0,
    Top = 1,
    Right = 2,
    Bottom = 4,
    Left = 8,
    All = Top | Right | Bottom | Left
}

internal static class MbxAccentBorderStyle
{
    public static string Build(MbxAccentBorderSides sides, string? color, int width)
    {
        if (sides == MbxAccentBorderSides.None || width <= 0)
            return string.Empty;

        var stroke = Math.Clamp(width, 1, MbxTheme.DesignTokens.MaxAccentBorderWidth);
        var strokeColor = string.IsNullOrWhiteSpace(color)
            ? "var(--mud-palette-primary)"
            : color.Trim();

        var top = (sides & MbxAccentBorderSides.Top) != 0 ? stroke : 0;
        var right = (sides & MbxAccentBorderSides.Right) != 0 ? stroke : 0;
        var bottom = (sides & MbxAccentBorderSides.Bottom) != 0 ? stroke : 0;
        var left = (sides & MbxAccentBorderSides.Left) != 0 ? stroke : 0;

        return $"border-style:solid;border-color:{strokeColor};border-width:{top}px {right}px {bottom}px {left}px;";
    }
}
