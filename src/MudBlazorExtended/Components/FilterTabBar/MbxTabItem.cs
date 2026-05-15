namespace MudBlazorExtended.Components.FilterTabBar;

/// <summary>Represents a single tab entry for <see cref="MbxFilterTabBar{T}"/>.</summary>
public record MbxTabItem<T>(T Value, string Label);
