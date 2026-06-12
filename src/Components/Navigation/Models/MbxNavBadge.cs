using MudBlazor;

namespace MudShell.Components.Navigation.Models;

public sealed record MbxNavBadge(
    string? Text = null,
    Color Color = Color.Default,
    Variant Variant = Variant.Filled,
    bool Dot = false);

