namespace MudShell.Components.Sidebar;

/// <summary>A navigation item used by <see cref="MdsSidebar"/> and <see cref="MudShell.Components.BottomNav.MdsBottomNav"/>.</summary>
/// <param name="ActiveHref">
/// Path prefix used to decide whether this item is highlighted as active, when it differs from
/// <paramref name="Href"/> — e.g. an item that links to one page of a section (<c>Href="/admin/settings"</c>)
/// but should stay highlighted across the whole section (<c>ActiveHref="/admin"</c>). Defaults to
/// <paramref name="Href"/> when not set.
/// </param>
public record MbxNavItem(string Icon, string Label, string? Href = null, string? ActiveHref = null);
