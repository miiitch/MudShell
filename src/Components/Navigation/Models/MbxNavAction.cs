using Microsoft.AspNetCore.Components;

namespace MudShell.Components.Navigation.Models;

/// <summary>An action rendered as a clickable trailing icon on an <see cref="MbxNavNode"/>.</summary>
/// <param name="Icon">MudBlazor icon (e.g. <c>Icons.Material.Outlined.PushPin</c>).</param>
/// <param name="Tooltip">Text shown on hover / read by screen readers.</param>
/// <param name="OnClick">Invoked with the node id when the action is clicked.</param>
public sealed record MbxNavAction(string Icon, string Tooltip, EventCallback<string> OnClick);
