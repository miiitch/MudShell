using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.Sidebar;
using Xunit;

namespace MudShell.ComponentTests;

/// <summary>
/// Regression coverage for the "Home" item staying highlighted on every page: it has
/// <c>href="/"</c>, which is a Blazor <c>NavLinkMatch.Prefix</c> match for every route unless
/// <see cref="MdsSidebar"/> computes its own active state instead of relying on that default.
/// </summary>
public class MdsSidebarActiveStateTests
{
    private static readonly MbxNavItem[] PrimaryItems =
    [
        new("home-icon", "Home", "/"),
        new("settings-icon", "Settings", "/admin/settings"),
    ];

    [Fact]
    public void Should_NotMarkHomeActive_When_CurrentRouteIsUnderSettings()
    {
        // Given
        using var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var navigation = context.Services.GetRequiredService<FakeNavigationManager>();
        navigation.NavigateTo("admin/users");

        // When
        var rendered = context.RenderComponent<MdsSidebar>(parameters => parameters
            .Add(c => c.IsExpanded, true)
            .Add(c => c.PrimaryItems, PrimaryItems));

        // Then
        var homeLink = rendered.Find("a[href='/']");
        Assert.DoesNotContain("mbx-nav-link-active", homeLink.ParentElement!.ClassList);
    }

    [Fact]
    public void Should_MarkHomeActive_When_CurrentRouteIsRoot()
    {
        // Given
        using var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var navigation = context.Services.GetRequiredService<FakeNavigationManager>();
        navigation.NavigateTo("");

        // When
        var rendered = context.RenderComponent<MdsSidebar>(parameters => parameters
            .Add(c => c.IsExpanded, true)
            .Add(c => c.PrimaryItems, PrimaryItems));

        // Then
        var homeLink = rendered.Find("a[href='/']");
        Assert.Contains("mbx-nav-link-active", homeLink.ParentElement!.ClassList);
    }
}
