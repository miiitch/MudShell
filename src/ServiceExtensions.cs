using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Radzen;

namespace MudShell;

/// <summary>
/// Extension methods for registering MudShell services.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Registers MudBlazor services required by MudShell components.
    /// Call this in <c>Program.cs</c> instead of (or in addition to) <c>AddMudServices()</c>.
    /// </summary>
    /// <remarks>
    /// Also registers the Radzen <see cref="DialogService"/> that <c>MdsScheduler</c> needs in its month view.
    /// No other Radzen service is registered.
    /// </remarks>
    public static IServiceCollection AddMudShell(this IServiceCollection services)
    {
        services.AddMudServices();
        services.AddScoped<DialogService>();
        return services;
    }
}
