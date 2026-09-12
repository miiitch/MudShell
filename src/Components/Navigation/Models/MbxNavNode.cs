using MudShell.Components.Sidebar;

namespace MudShell.Components.Navigation.Models;

public sealed record MbxNavNode
{
    public MbxNavNode(
        string id,
        string text,
        string? icon = null,
        string? href = null,
        MbxNavMatchMode match = MbxNavMatchMode.Prefix,
        IReadOnlyList<MbxNavNode>? children = null,
        bool defaultExpanded = false,
        bool? expanded = null,
        bool visible = true,
        bool disabled = false,
        MbxNavBadge? badge = null,
        IReadOnlyDictionary<string, object?>? metadata = null,
        MbxNavPin pin = MbxNavPin.None,
        IReadOnlyList<MbxNavAction>? trailingActions = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Navigation node id cannot be null or whitespace.", nameof(id));
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Navigation node text cannot be null or whitespace.", nameof(text));

        Id = id;
        Text = text;
        Icon = icon;
        Href = href;
        Match = match;
        Children = children ?? [];
        DefaultExpanded = defaultExpanded;
        Expanded = expanded;
        Visible = visible;
        Disabled = disabled;
        Badge = badge;
        Metadata = metadata;
        Pin = pin;
        TrailingActions = trailingActions;
    }

    public string Id { get; }
    public string Text { get; }
    public string? Icon { get; }
    public string? Href { get; }
    public MbxNavMatchMode Match { get; }
    public IReadOnlyList<MbxNavNode> Children { get; }
    public bool DefaultExpanded { get; }
    public bool? Expanded { get; }
    public bool Visible { get; }
    public bool Disabled { get; }
    public MbxNavBadge? Badge { get; }
    public IReadOnlyDictionary<string, object?>? Metadata { get; }

    /// <summary>Fixed position of this node among its parent's children: not sortable, always first or always last.</summary>
    public MbxNavPin Pin { get; init; }

    /// <summary>Actions rendered as clickable trailing icons (e.g. pin/remove), in display order.</summary>
    public IReadOnlyList<MbxNavAction>? TrailingActions { get; init; }
}

public static class MbxNavNodeLegacyAdapter
{
    public static MbxNavNode FromLegacyItem(MbxNavItem item, string id, MbxNavMatchMode match = MbxNavMatchMode.Prefix) =>
        new(
            id: id,
            text: item.Label,
            icon: item.Icon,
            href: item.Href,
            match: match
        );
}

