using MudShell.Components.Navigation.Models;

namespace MudShell.Components.Navigation.State;

public static class MbxNavStateResolver
{
    public static MbxNavState Resolve(
        MbxNavTree? tree,
        string? currentUri,
        IReadOnlySet<string>? extraExpandedNodeIds = null,
        bool singleExpandLevel2 = true)
    {
        if (tree is null || tree.Roots.Count == 0)
            return MbxNavState.Empty;

        var currentPath = NormalizePath(currentUri);
        var allPaths = EnumeratePaths(tree.Roots);

        var best = allPaths
            .Select(path => new { Path = path, Node = path[^1], Score = MatchScore(path[^1], currentPath) })
            .Where(x => x.Score >= 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Path.Count)
            .FirstOrDefault();

        var activePathNodes = best?.Path ?? [];
        var activePath = activePathNodes.Select(n => n.Id).ToArray();
        var activeNodeId = activePath.LastOrDefault();
        var activeRootId = activePath.FirstOrDefault();

        var expanded = new HashSet<string>(StringComparer.Ordinal);

        foreach (var root in tree.Roots.Where(r => r.Visible))
        {
            ApplyDefaultExpanded(root, expanded, depth: 1);
        }

        foreach (var ancestor in activePathNodes.Take(activePathNodes.Count - 1))
        {
            expanded.Add(ancestor.Id);
        }

        if (extraExpandedNodeIds is not null)
        {
            foreach (var nodeId in extraExpandedNodeIds)
                expanded.Add(nodeId);
        }

        var contextNodes = ResolveContextNodes(tree, activeRootId);
        var activeLevel2 = activePathNodes.Count >= 2 ? activePathNodes[1] : null;

        if (singleExpandLevel2 && contextNodes.Count > 0)
        {
            var keepOpenId = activeLevel2?.Id
                ?? contextNodes.FirstOrDefault(n => expanded.Contains(n.Id))?.Id
                ?? contextNodes.FirstOrDefault(n => n.DefaultExpanded)?.Id;

            foreach (var l2 in contextNodes.Where(c => c.Children.Count > 0))
            {
                if (!string.Equals(l2.Id, keepOpenId, StringComparison.Ordinal))
                    expanded.Remove(l2.Id);
            }

            if (keepOpenId is not null)
                expanded.Add(keepOpenId);
        }

        var visibleSubEntries = ResolveVisibleSubEntries(contextNodes, expanded, activeLevel2);

        return new MbxNavState(
            ActiveNodeId: activeNodeId,
            ActiveRootId: activeRootId,
            ActivePath: activePath,
            ExpandedNodeIds: expanded,
            ContextPanelNodes: contextNodes,
            VisibleSubEntries: visibleSubEntries);
    }

    private static List<MbxNavNode> ResolveContextNodes(MbxNavTree tree, string? activeRootId)
    {
        if (activeRootId is null)
            return [];

        var root = tree.Roots.FirstOrDefault(r => string.Equals(r.Id, activeRootId, StringComparison.Ordinal));
        return root?.Children.Where(c => c.Visible).ToList() ?? [];
    }

    private static List<MbxNavNode> ResolveVisibleSubEntries(
        IReadOnlyList<MbxNavNode> contextNodes,
        IReadOnlySet<string> expanded,
        MbxNavNode? activeLevel2)
    {
        MbxNavNode? owner = activeLevel2;

        if (owner is null || owner.Children.Count == 0)
        {
            owner = contextNodes.FirstOrDefault(c => c.Children.Count > 0 && expanded.Contains(c.Id))
                ?? contextNodes.FirstOrDefault(c => c.Children.Count > 0);
        }

        return owner?.Children.Where(c => c.Visible).ToList() ?? [];
    }

    private static void ApplyDefaultExpanded(MbxNavNode node, HashSet<string> expanded, int depth)
    {
        if (!node.Visible)
            return;

        var isExpanded = node.Expanded ?? node.DefaultExpanded;
        if (isExpanded && depth < 3)
            expanded.Add(node.Id);

        foreach (var child in node.Children.Where(c => c.Visible))
        {
            ApplyDefaultExpanded(child, expanded, depth + 1);
        }
    }

    private static List<IReadOnlyList<MbxNavNode>> EnumeratePaths(IReadOnlyList<MbxNavNode> roots)
    {
        var result = new List<IReadOnlyList<MbxNavNode>>();
        foreach (var root in roots.Where(r => r.Visible))
        {
            EnumeratePathsRecursive(root, [], result);
        }
        return result;
    }

    private static void EnumeratePathsRecursive(
        MbxNavNode node,
        IReadOnlyList<MbxNavNode> currentPath,
        List<IReadOnlyList<MbxNavNode>> result)
    {
        if (!node.Visible)
            return;

        var path = currentPath.Append(node).ToArray();
        result.Add(path);

        foreach (var child in node.Children.Where(c => c.Visible))
        {
            EnumeratePathsRecursive(child, path, result);
        }
    }

    private static int MatchScore(MbxNavNode node, string currentPath)
    {
        if (string.IsNullOrWhiteSpace(node.Href))
            return -1;

        var nodePath = NormalizePath(node.Href);
        if (nodePath.Length == 0 && currentPath.Length == 0)
            return 10_000;

        return node.Match switch
        {
            MbxNavMatchMode.All => string.Equals(nodePath, currentPath, StringComparison.OrdinalIgnoreCase)
                ? 10_000 + nodePath.Length
                : -1,
            MbxNavMatchMode.Prefix => IsPathPrefix(nodePath, currentPath)
                ? 1_000 + nodePath.Length
                : -1,
            _ => -1
        };
    }

    private static bool IsPathPrefix(string prefixPath, string currentPath)
    {
        if (string.Equals(prefixPath, currentPath, StringComparison.OrdinalIgnoreCase))
            return true;

        if (prefixPath.Length == 0)
            return currentPath.Length == 0;

        if (!currentPath.StartsWith(prefixPath, StringComparison.OrdinalIgnoreCase))
            return false;

        return currentPath.Length > prefixPath.Length && currentPath[prefixPath.Length] == '/';
    }

    private static string NormalizePath(string? uriOrPath)
    {
        if (string.IsNullOrWhiteSpace(uriOrPath))
            return "/";

        var path = uriOrPath;
        if (Uri.TryCreate(uriOrPath, UriKind.Absolute, out var absoluteUri))
            path = absoluteUri.AbsolutePath;

        path ??= "/";

        if (!path.StartsWith('/'))
            path = "/" + path;

        path = path.TrimEnd('/');
        return path.Length == 0 ? "/" : path;
    }
}

