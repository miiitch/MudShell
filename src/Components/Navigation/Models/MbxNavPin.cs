namespace MudShell.Components.Navigation.Models;

/// <summary>Position figée d'un <see cref="MbxNavNode"/> au sein des enfants de son parent.</summary>
public enum MbxNavPin
{
    /// <summary>Nœud triable (segment central).</summary>
    None,

    /// <summary>Toujours affiché en tête du sous-menu, dans l'ordre reçu ; non déplaçable par glisser-déposer.</summary>
    Top,

    /// <summary>Toujours affiché en fin du sous-menu, dans l'ordre reçu ; non déplaçable par glisser-déposer.</summary>
    Bottom,
}
