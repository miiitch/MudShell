using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.AppShell;
using MudShell.Theme;
using Xunit;

namespace MudShell.ComponentTests;

/// <summary>
/// The split colour roles are opt-in: the shell only carries the class when asked, and the stylesheets route
/// interactive states through <c>--mbx-interactive</c> so that class can recolour them.
/// </summary>
public class MdsColorRolesTests
{
    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private static string ComponentCss(params string[] relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "MudShell.slnx")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return File.ReadAllText(Path.Combine([directory.FullName, "src", "Components", .. relativePath]));
    }

    [Fact]
    public void Shell_Should_NotCarrySplitClass_When_ColorRolesIsDefault()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsAppShell>();

        Assert.DoesNotContain("mbx-roles-split", rendered.Find("[data-testid='shell-mode']").ClassList);
    }

    [Fact]
    public void Shell_Should_CarrySplitClass_When_ColorRolesIsSplit()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsAppShell>(p => p
            .Add(c => c.ColorRoles, MdsColorRoles.Split));

        Assert.Contains("mbx-roles-split", rendered.Find("[data-testid='shell-mode']").ClassList);
    }

    [Fact]
    public void ShellCss_Should_DefaultInteractiveToPrimary_And_SplitToSecondary()
    {
        var css = ComponentCss("AppShell", "MdsAppShell.razor.css");

        Assert.Contains("--mbx-interactive:            var(--mud-palette-primary, #594AE2);", css);
        var split = css[css.IndexOf(".mbx-shell.mbx-roles-split {", StringComparison.Ordinal)..];
        split = split[..split.IndexOf('}')];
        Assert.Contains("--mbx-interactive:", split);
        Assert.Contains("var(--mud-palette-secondary", split);
    }

    [Fact]
    public void ShellCss_Should_RecolourNativeTabsAndLinks_OnlyInSplitMode()
    {
        var css = ComponentCss("AppShell", "MdsAppShell.razor.css");

        Assert.Matches(@"\.mbx-shell\.mbx-roles-split ::deep \.mud-tabs-tabbar:not\([^)]*\) \.mud-tab\.mud-tab-active", css);
        Assert.Matches(@"\.mbx-shell\.mbx-roles-split ::deep \.mud-tabs-tabbar:not\([^)]*\) \.mud-tab-slider", css);
        Assert.Contains(".mbx-shell.mbx-roles-split ::deep .mud-link.mud-primary-text", css);
        // Nothing outside the split class may restyle MudBlazor's tabs or links.
        Assert.DoesNotMatch(@"(?m)^(?!\.mbx-shell\.mbx-roles-split).*\.mud-(tab-active|tab-slider|link\.)", css);
    }

    [Theory]
    [InlineData("Sidebar", "MdsSidebar.razor.css")]
    [InlineData("Navigation", "MdsSidebarNav.razor.css")]
    [InlineData("Navigation", "MdsContextNavPanel.razor.css")]
    [InlineData("Navigation", "MdsSidebarSubMenu.razor.css")]
    [InlineData("BottomNav", "MdsBottomNav.razor.css")]
    [InlineData("FilterTabBar", "MdsFilterTabBar.razor.css")]
    [InlineData("Data", "MdsStatTile.razor.css")]
    [InlineData("Layout", "MdsSplitPane.razor.css")]
    public void InteractiveStates_Should_UseInteractiveToken_NotPrimaryDirectly(string folder, string file)
    {
        var css = ComponentCss(folder, file);

        Assert.Contains("var(--mbx-interactive, var(--mud-palette-primary))", css);
        // A bare primary may only remain as the fallback of another token, never as a property value.
        Assert.DoesNotMatch(@"(?m)^\s*(color|background|background-color|border-color):\s*var\(--mud-palette-primary\)", css);
    }
}
