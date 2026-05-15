namespace MudShell.Components.Sidebar;

/// <summary>A navigation item used by <see cref="MbxSidebar"/> and <see cref="MudShell.Components.BottomNav.MbxBottomNav"/>.</summary>
public record MbxNavItem(string Icon, string Label, string? Href = null);
