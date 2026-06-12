using MudShell.Components.Navigation.Models;

namespace MudShell.Components.Navigation.State;

public sealed record MbxNavState(
    string? ActiveNodeId,
    string? ActiveRootId,
    IReadOnlyList<string> ActivePath,
    IReadOnlySet<string> ExpandedNodeIds,
    IReadOnlyList<MbxNavNode> ContextPanelNodes,
    IReadOnlyList<MbxNavNode> VisibleSubEntries)
{
    public static MbxNavState Empty { get; } = new(
        ActiveNodeId: null,
        ActiveRootId: null,
        ActivePath: [],
        ExpandedNodeIds: new HashSet<string>(StringComparer.Ordinal),
        ContextPanelNodes: [],
        VisibleSubEntries: []);
}

