---
sidebar_position: 8
---

# MdsSidebarNav

Tree-based sidebar navigation: renders a flat list of `MbxNavNode` roots, each optionally expandable
into a group of children (a "sub-menu"). Unlike `MdsSidebar` (which takes flat `MbxNavItem[]`
arrays), `MdsSidebarNav` is driven by an `MbxNavTree` and supports nested groups, active-state
highlighting from the current URL, per-item pinning, trailing actions, and drag-and-drop reordering.

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Tree` | `MbxNavTree?` | `null` | The nodes to render |
| `IsExpanded` | `bool` | `true` | Full labels vs. icon-only rail |
| `CurrentUri` | `string?` | `null` | Overrides the URI used for active-state matching (defaults to the current navigation URI) |
| `Class` | `string?` | `null` | Extra CSS class on the root `<nav>` / rail `<div>` |
| `OnSortChanged` | `EventCallback<MbxNavSortChangedArgs>` | — | Fires after a drag-and-drop reorder inside a group's sortable segment |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | `null` | Undeclared attributes, splatted onto the root element |

## MbxNavNode

```csharp
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
        IReadOnlyList<MbxNavAction>? trailingActions = null);

    // ...
    public MbxNavPin Pin { get; init; }
    public IReadOnlyList<MbxNavAction>? TrailingActions { get; init; }
}
```

A node with `Children` renders as an expandable group (`MudNavGroup`); a node without renders as a
plain link. `MbxNavTree` enforces at most 3 levels of nesting and unique ids across the whole tree.

## Sub-menu items: pinning, trailing actions, sorting

Within a group's children (its "sub-menu"), each child can independently be:

- **pinned** to the top or bottom via `Pin` (`MbxNavPin.Top` / `MbxNavPin.Bottom`), keeping it out of
  drag-and-drop reordering. Everything left at `MbxNavPin.None` sorts freely between those two fixed
  segments — in the order: fixed-top items, sortable items, fixed-bottom items.
- given **trailing icon actions** via `TrailingActions` — a list of `MbxNavAction(Icon, Tooltip,
  OnClick)`, rendered as small icon buttons after the item's label (revealed on hover), each
  `OnClick` invoked with the node's id when clicked. A common use is a pin/unpin or "remove from
  list" icon.
- **reordered by the user** via drag-and-drop, if `Pin == MbxNavPin.None`. Dropping on the top half
  of another item inserts before it; dropping on the bottom half inserts after it — every position,
  including the very first and very last slot, is reachable. A thin line appears between items (or
  at the very top/bottom of the segment) while dragging, showing exactly where the item will land.

```csharp
public enum MbxNavPin { None, Top, Bottom }

public sealed record MbxNavAction(string Icon, string Tooltip, EventCallback<string> OnClick);

public sealed record MbxNavSortChangedArgs(string GroupId, IReadOnlyList<string> OrderedIds);
```

`MdsSidebarNav` does not own the order itself — it is a controlled component for sorting, the same
way an `<input>` is controlled for text: you supply the current order via `Tree`, and
`OnSortChanged` tells you the order the user just produced so you can persist it and pass an updated
`Tree` back down. `GroupId` is the id of the parent node whose children were reordered, and
`OrderedIds` lists only the sortable segment's ids (fixed-top/bottom items are never included, since
their order can't change).

### Example: a sub-menu with a pinned item, sortable items, and a remove action

```razor
<MdsSidebarNav Tree="@_tree" IsExpanded="true" OnSortChanged="@HandleSortChanged" />

@code {
    private List<string> _order = ["doc-1", "doc-2", "doc-3"];
    private readonly HashSet<string> _removed = [];

    private MbxNavTree _tree => new(
    [
        new(
            id: "recent-docs",
            text: "Recent documents",
            icon: Icons.Material.Outlined.Folder,
            children: BuildChildren())
    ]);

    private List<MbxNavNode> BuildChildren()
    {
        var children = new List<MbxNavNode>
        {
            new(id: "pinned-doc", text: "Welcome guide", pin: MbxNavPin.Top),
        };

        foreach (var id in _order)
        {
            if (_removed.Contains(id))
                continue;

            children.Add(new(
                id: id,
                text: id,
                trailingActions:
                [
                    new MbxNavAction(
                        Icons.Material.Outlined.Close,
                        "Remove from recents",
                        EventCallback.Factory.Create<string>(this, id2 => _removed.Add(id2)))
                ]));
        }

        return children;
    }

    private void HandleSortChanged(MbxNavSortChangedArgs args)
    {
        _order = args.OrderedIds.ToList();
        StateHasChanged();
    }
}
```

### `MdsSidebarSubMenu`

Each group's sub-menu is rendered internally by `MdsSidebarSubMenu` — one drag-and-drop container
instance per group, holding its own drag-tracking state (which item is currently hovered, for the
indicator line) and calling `MudDropContainer.Refresh()` when the sortable list's ids change (e.g.
after a removal), so the visible items stay in sync. It is an implementation detail of
`MdsSidebarNav` — you do not use it directly, but its presence explains the extra
`<div class="mbx-nav-submenu">` wrapper you will see around a group's children in the rendered DOM.

## See also

- [Model](../model.md) for how `MdsSidebarNav` fits alongside `MdsSidebar` and `MdsContextNavPanel`.
- [Migrating to 0.3](../migration-0.3.md) if you are upgrading from a version before sub-menu
  pinning/sorting existed.
