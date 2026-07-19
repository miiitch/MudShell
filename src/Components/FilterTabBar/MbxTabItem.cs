namespace MudShell.Components.FilterTabBar;

/// <summary>Represents a single tab entry for <see cref="MdsFilterTabBar{T}"/>.</summary>
public record MbxTabItem<T>(T Value, string Label);
