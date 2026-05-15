using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

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
    public static IServiceCollection AddMudShell(this IServiceCollection services)
    {
        services.AddMudServices();
        return services;
    }
}
