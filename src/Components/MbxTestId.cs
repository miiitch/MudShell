namespace MudShell;

/// <summary>
/// Builds the <c>data-testid</c> values MudShell emits on elements it renders from data rather
/// than from consumer markup.
/// </summary>
/// <remarks>
/// Navigation links and filter tabs come out of <c>MbxNavNode</c> / <c>MbxTabItem</c> collections,
/// so a consumer has no markup to hang an attribute on. Deriving the id from the node id gives UI
/// tests a stable selector without asking every application to restate it.
/// </remarks>
public static class MbxTestId
{
    /// <summary>Test id of a navigation link rendered for the node <paramref name="nodeId"/>.</summary>
    public static string Nav(string nodeId) => $"nav-{nodeId}";

    /// <summary>Test id of the expandable group rendered for the node <paramref name="nodeId"/>.</summary>
    public static string NavGroup(string nodeId) => $"nav-group-{nodeId}";

    /// <summary>Test id of a filter tab carrying <paramref name="value"/>.</summary>
    public static string Tab(object? value) => $"tab-{value}";
}
