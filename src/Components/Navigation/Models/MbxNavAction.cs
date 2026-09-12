using Microsoft.AspNetCore.Components;

namespace MudShell.Components.Navigation.Models;

/// <summary>Une action représentée par une icône cliquable en fin d'un <see cref="MbxNavNode"/>.</summary>
/// <param name="Icon">Icône MudBlazor (ex. <c>Icons.Material.Outlined.PushPin</c>).</param>
/// <param name="Tooltip">Texte affiché au survol / lu par les lecteurs d'écran.</param>
/// <param name="OnClick">Invoqué avec l'id du nœud lorsque l'action est cliquée.</param>
public sealed record MbxNavAction(string Icon, string Tooltip, EventCallback<string> OnClick);
