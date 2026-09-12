namespace MudShell.Components.Navigation.Models;

/// <summary>Émis après un glisser-déposer réussi dans le segment triable d'un sous-menu.</summary>
/// <param name="GroupId">Id du nœud "sous-section" (le parent des items réordonnés).</param>
/// <param name="OrderedIds">Ids du segment triable uniquement, dans leur nouvel ordre.</param>
public sealed record MbxNavSortChangedArgs(string GroupId, IReadOnlyList<string> OrderedIds);
