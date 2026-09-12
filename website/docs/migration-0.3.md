---
sidebar_position: 6
---

# Migrating to 0.3

MudShell is currently on the `0.2.x` line; the next release ships as `0.3.0`. This guide covers the
one change worth knowing about: sub-menu pinning, trailing actions, and drag-and-drop sorting on
`MdsSidebarNav`.

## Is this a breaking change?

No. Nothing about `0.3.0` requires you to change existing code:

- `MbxNavNode` gained two new members — `Pin` and `TrailingActions` — as `init`-only properties
  alongside the existing constructor, not new positional constructor parameters. Any code that
  constructs `MbxNavNode` today, with named or positional arguments, keeps compiling unchanged.
- Both new members default to "do nothing extra": `Pin` defaults to `MbxNavPin.None` (sortable, no
  fixed position) and `TrailingActions` defaults to `null` (no icons rendered). A tree built before
  0.3 renders and behaves identically after upgrading.
- `MdsSidebarNav` gained one new parameter, `OnSortChanged` (`EventCallback<MbxNavSortChangedArgs>`),
  which defaults to a no-op. Consumers that don't set it are unaffected.

The only visible-if-you-look-closely change is in the rendered DOM: a group's children are now
wrapped in an internal `MdsSidebarSubMenu` component, which adds a `<div class="mbx-nav-submenu">`
around them (see [MdsSidebarSubMenu](components/sidebar-nav.md#mdssidebarsubmenu)). This does not
change layout or styling, but a test asserting on the exact DOM shape around sub-menu items (rather
than on visible content or `data-testid` attributes) may need updating.

## Adopting sub-menu pinning, trailing actions, and sorting

These are additive — nothing to migrate away from. To use them on an existing tree:

1. **Pin specific children.** Set `pin: MbxNavPin.Top` or `pin: MbxNavPin.Bottom` on the
   `MbxNavNode`s that should stay put regardless of sorting (e.g. a "favorites" shortcut, or an
   always-last "add new" link).
2. **Add a trailing action.** Pass `trailingActions: [new MbxNavAction(icon, tooltip, callback)]` on
   a child node. The icon renders after the label and calls back with that node's id when clicked.
3. **React to reordering.** Set `OnSortChanged` on `MdsSidebarNav`. On each event, persist
   `args.OrderedIds` however you track item order today (a list in component state, a user
   preference in a backing store, …) and make sure the next `Tree` you supply reflects it —
   `MdsSidebarNav` does not remember the order for you across a full `Tree` replacement.

See [MdsSidebarNav → Sub-menu items](components/sidebar-nav.md#sub-menu-items-pinning-trailing-actions-sorting)
for a full example.

## Nothing to do for `MdsSidebar` / flat `MbxNavItem[]` usage

If you only use `MdsSidebar` with `PrimaryItems`/`SecondaryItems` (flat `MbxNavItem[]`, no
`MbxNavTree`), none of this applies — that API is untouched. Pinning, trailing actions, and sorting
are `MbxNavNode`/`MdsSidebarNav` (tree-based) features only.
