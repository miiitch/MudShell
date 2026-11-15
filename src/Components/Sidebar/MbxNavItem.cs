namespace MudShell.Components.Sidebar;

/// <summary>A navigation item used by <see cref="MdsSidebar"/> and <see cref="MudShell.Components.BottomNav.MdsBottomNav"/>.</summary>
/// <param name="ActiveHref">
/// Path prefix used to decide whether this item is highlighted as active, when it differs from
/// <paramref name="Href"/> — e.g. an item that links to one page of a section (<c>Href="/admin/settings"</c>)
/// but should stay highlighted across the whole section (<c>ActiveHref="/admin"</c>). Defaults to
/// <paramref name="Href"/> when not set.
/// </param>
/// <summary>An entry of the navigation rail.</summary>
/// <param name="Icon">Icon of the entry.</param>
/// <param name="Label">Text shown when the rail is expanded, and as a tooltip when it is collapsed.</param>
/// <param name="Href">Target of the entry.</param>
/// <param name="ActiveHref">Path prefix that marks the entry as active, when it differs from <paramref name="Href"/>.</param>
/// <param name="Accent">
/// Optional CSS color identifying the section. It colors the entry's icon and its active background instead of the
/// shell's interactive color.
/// </param>
public record MbxNavItem(string Icon, string Label, string? Href = null, string? ActiveHref = null, string? Accent = null);
