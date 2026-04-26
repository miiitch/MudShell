using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace MudBlazorExtended;

/// <summary>
/// Extension methods for registering MudBlazorExtended services.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Registers MudBlazor services required by MudBlazorExtended components.
    /// Call this in <c>Program.cs</c> instead of (or in addition to) <c>AddMudServices()</c>.
    /// </summary>
    public static IServiceCollection AddMudBlazorExtended(this IServiceCollection services)
    {
        services.AddMudServices();
        return services;
    }
}
