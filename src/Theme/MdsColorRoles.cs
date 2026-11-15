namespace MudShell.Theme;

/// <summary>How the shell shares the theme colours between the main action and interactive states.</summary>
public enum MdsColorRoles
{
    /// <summary>Interactive states and card surfaces are tinted with the primary colour.</summary>
    Unified,

    /// <summary>
    /// The primary colour is kept for the main action. Hover, selection, active navigation and the focus ring
    /// use the secondary colour, and card surfaces are not tinted.
    /// </summary>
    Split,
}
