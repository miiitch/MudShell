namespace MudShell.Components.Sidebar;

/// <summary>A navigation item used by <see cref="MdsSidebar"/> and <see cref="MudShell.Components.BottomNav.MdsBottomNav"/>.</summary>
public record MbxNavItem(string Icon, string Label, string? Href = null);
