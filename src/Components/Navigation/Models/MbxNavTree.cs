using MudShell.Components.Sidebar;

namespace MudShell.Components.Navigation.Models;

public sealed record MbxNavTree
{
    public MbxNavTree(IReadOnlyList<MbxNavNode> roots)
    {
        Roots = roots ?? [];
        Validate();
    }

    public IReadOnlyList<MbxNavNode> Roots { get; }

    public static MbxNavTree Empty { get; } = new([]);

    public static MbxNavTree FromLegacy(MbxNavItem[]? primaryItems, MbxNavItem[]? secondaryItems = null)
    {
        var nodes = new List<MbxNavNode>();

        if (primaryItems is not null)
        {
            for (var i = 0; i < primaryItems.Length; i++)
            {
                var item = primaryItems[i];
                nodes.Add(MbxNavNodeLegacyAdapter.FromLegacyItem(item, $"legacy-primary-{i}"));
            }
        }

        if (secondaryItems is not null && secondaryItems.Length > 0)
        {
            var secondaryChildren = new List<MbxNavNode>();
            for (var i = 0; i < secondaryItems.Length; i++)
            {
                var item = secondaryItems[i];
                secondaryChildren.Add(MbxNavNodeLegacyAdapter.FromLegacyItem(item, $"legacy-secondary-{i}"));
            }

            nodes.Add(new MbxNavNode(
                id: "legacy-secondary-root",
                text: "Secondary",
                icon: null,
                href: null,
                children: secondaryChildren,
                defaultExpanded: true));
        }

        return new MbxNavTree(nodes);
    }

    private void Validate()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var root in Roots.Where(r => r.Visible))
        {
            ValidateNode(root, depth: 1, ids);
        }
    }

    private static void ValidateNode(MbxNavNode node, int depth, HashSet<string> ids)
    {
        if (!ids.Add(node.Id))
            throw new InvalidOperationException($"Duplicate navigation node id '{node.Id}'.");

        if (depth > 3)
            throw new InvalidOperationException($"Navigation depth cannot exceed 3 levels. Node '{node.Id}' exceeds this limit.");

        foreach (var child in node.Children.Where(c => c.Visible))
        {
            ValidateNode(child, depth + 1, ids);
        }
    }
}

