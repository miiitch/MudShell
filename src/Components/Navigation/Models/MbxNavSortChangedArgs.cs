namespace MudShell.Components.Navigation.Models;

/// <summary>Raised after a successful drag-and-drop reorder within the sortable segment of a sub-menu.</summary>
/// <param name="GroupId">Id of the "sub-section" node (the parent of the reordered items).</param>
/// <param name="OrderedIds">Ids of the sortable segment only, in their new order.</param>
public sealed record MbxNavSortChangedArgs(string GroupId, IReadOnlyList<string> OrderedIds);
