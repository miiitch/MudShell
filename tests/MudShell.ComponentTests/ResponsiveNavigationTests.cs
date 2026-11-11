using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.AppShell;
using MudShell.Components.BottomNav;
using MudShell.Components.Navigation;
using MudShell.Components.Navigation.Models;
using Xunit;

namespace MudShell.ComponentTests;

public class ResponsiveNavigationTests
{
    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private static MbxNavNode Leaf(string id, string href) => new(id: id, text: id, href: href);

    /// <summary>Roots: a (leaf), b (children, one with level 3), c, d, e, f, g (leaves) — seven level-1 entries.</summary>
    private static MbxNavTree Tree() => new(
    [
        new MbxNavNode(id: "a", text: "A", href: "/a"),
        new MbxNavNode(id: "b", text: "B", href: "/b", children:
        [
            new MbxNavNode(id: "b.plain", text: "Plain", href: "/b/plain"),
            new MbxNavNode(id: "b.group", text: "Group", children:
            [
                Leaf("b.group.leaf", "/b/group/leaf"),
            ]),
        ]),
        new MbxNavNode(id: "c", text: "C", href: "/c"),
        new MbxNavNode(id: "d", text: "D", href: "/d"),
        new MbxNavNode(id: "e", text: "E", href: "/e"),
        new MbxNavNode(id: "f", text: "F", href: "/f"),
        new MbxNavNode(id: "g", text: "G", href: "/g"),
    ]);

    [Fact]
    public void BottomNav_Should_ShowMaxVisibleEntriesPlusMoreButton_When_RootsOverflow()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsBottomNav>(p => p
            .Add(c => c.Tree, Tree())
            .Add(c => c.CurrentUri, "http://localhost/a"));

        // MaxVisible (5) counts the "Plus" button: 4 direct entries + Plus.
        Assert.Equal(5, rendered.FindAll(".mbx-bottom-nav-item").Count);
        Assert.NotEmpty(rendered.FindAll("[data-testid='nav-a']"));
        Assert.NotEmpty(rendered.FindAll("[data-testid='nav-d']"));
        Assert.Empty(rendered.FindAll("[data-testid='nav-e']"));
        Assert.Empty(rendered.FindAll("[data-testid='nav-f']"));
        Assert.Empty(rendered.FindAll("[data-testid='nav-g']"));
        var more = rendered.Find("[data-testid='bottom-nav-more']");
        Assert.Contains("Plus", more.TextContent);
    }

    [Fact]
    public void BottomNav_Should_NotShowMoreButton_When_RootsFit()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsBottomNav>(p => p
            .Add(c => c.Tree, Tree())
            .Add(c => c.MaxVisible, 7)
            .Add(c => c.CurrentUri, "http://localhost/a"));

        Assert.Empty(rendered.FindAll("[data-testid='bottom-nav-more']"));
        Assert.Equal(7, rendered.FindAll(".mbx-bottom-nav-item").Count);
    }

    [Fact]
    public void BottomNav_Should_ListOverflowRootsInSheet_When_MoreClicked()
    {
        using var context = NewContext();
        var rendered = context.RenderComponent<MdsBottomNav>(p => p
            .Add(c => c.Tree, Tree())
            .Add(c => c.CurrentUri, "http://localhost/a"));

        rendered.Find("[data-testid='bottom-nav-more']").Click();

        var sheet = rendered.Find("[data-testid='context-sheet']");
        Assert.Contains("Plus", sheet.TextContent);
        Assert.NotEmpty(sheet.QuerySelectorAll("[data-testid='nav-f']"));
        Assert.NotEmpty(sheet.QuerySelectorAll("[data-testid='nav-g']"));
        Assert.Empty(sheet.QuerySelectorAll("[data-testid='nav-a']"));
    }

    [Fact]
    public void BottomNav_Should_NotRenderSheet_Until_ARootWithChildrenIsClicked()
    {
        using var context = NewContext();
        var rendered = context.RenderComponent<MdsBottomNav>(p => p
            .Add(c => c.Tree, Tree())
            .Add(c => c.CurrentUri, "http://localhost/a"));

        Assert.Empty(rendered.FindAll("[data-testid='context-sheet']"));
        Assert.Empty(rendered.FindAll("[data-testid='nav-scrim']"));
    }

    [Fact]
    public void BottomNav_Should_OpenSheetShowingLevel2_When_RootWithChildrenClicked()
    {
        using var context = NewContext();
        var rendered = context.RenderComponent<MdsBottomNav>(p => p
            .Add(c => c.Tree, Tree())
            .Add(c => c.CurrentUri, "http://localhost/a"));

        rendered.Find(".mbx-bottom-nav [data-testid='nav-b']").Click();

        var sheet = rendered.Find("[data-testid='context-sheet']");
        Assert.NotEmpty(rendered.FindAll("[data-testid='nav-scrim']"));
        Assert.NotEmpty(sheet.QuerySelectorAll("[data-testid='nav-b.plain']"));
        Assert.NotEmpty(sheet.QuerySelectorAll("[data-testid='nav-b.group']"));
        Assert.Empty(rendered.FindAll("[data-testid='drilldown-back']"));
    }

    [Fact]
    public void BottomNav_Should_DrillToLevel3WithBackButton_And_ReturnToLevel2()
    {
        using var context = NewContext();
        var rendered = context.RenderComponent<MdsBottomNav>(p => p
            .Add(c => c.Tree, Tree())
            .Add(c => c.CurrentUri, "http://localhost/a"));
        rendered.Find(".mbx-bottom-nav [data-testid='nav-b']").Click();

        rendered.Find("[data-testid='nav-b.group']").Click();

        Assert.NotEmpty(rendered.FindAll("[data-testid='drilldown-back']"));
        Assert.NotEmpty(rendered.FindAll("[data-testid='nav-b.group.leaf']"));
        Assert.Empty(rendered.FindAll("[data-testid='nav-b.plain']"));

        rendered.Find("[data-testid='drilldown-back']").Click();

        Assert.Empty(rendered.FindAll("[data-testid='drilldown-back']"));
        Assert.NotEmpty(rendered.FindAll("[data-testid='nav-b.plain']"));
    }

    [Fact]
    public void BottomNav_Should_NavigateAndCloseSheet_When_LeafClicked()
    {
        using var context = NewContext();
        var nav = context.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
        var rendered = context.RenderComponent<MdsBottomNav>(p => p
            .Add(c => c.Tree, Tree()));
        rendered.Find(".mbx-bottom-nav [data-testid='nav-b']").Click();

        rendered.Find("[data-testid='nav-b.plain']").Click();

        Assert.EndsWith("/b/plain", nav.Uri);
        Assert.Empty(rendered.FindAll("[data-testid='context-sheet']"));
        Assert.Empty(rendered.FindAll("[data-testid='nav-scrim']"));
    }

    [Fact]
    public void BottomNav_Should_CloseSheet_When_ScrimCloseButtonOrEscape()
    {
        using var context = NewContext();
        var rendered = context.RenderComponent<MdsBottomNav>(p => p
            .Add(c => c.Tree, Tree())
            .Add(c => c.CurrentUri, "http://localhost/a"));

        rendered.Find(".mbx-bottom-nav [data-testid='nav-b']").Click();
        rendered.Find("[data-testid='nav-scrim']").Click();
        Assert.Empty(rendered.FindAll("[data-testid='context-sheet']"));

        rendered.Find(".mbx-bottom-nav [data-testid='nav-b']").Click();
        rendered.Find("[data-testid='context-sheet-close']").Click();
        Assert.Empty(rendered.FindAll("[data-testid='context-sheet']"));

        rendered.Find(".mbx-bottom-nav [data-testid='nav-b']").Click();
        rendered.Find("[data-testid='context-sheet']").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Escape" });
        Assert.Empty(rendered.FindAll("[data-testid='context-sheet']"));
    }

    [Fact]
    public void BottomNav_Should_OpenSheetListingRoots_When_OpenMenuCalled()
    {
        using var context = NewContext();
        var rendered = context.RenderComponent<MdsBottomNav>(p => p
            .Add(c => c.Tree, Tree())
            .Add(c => c.CurrentUri, "http://localhost/a"));

        rendered.InvokeAsync(() => rendered.Instance.OpenMenu());

        var sheet = rendered.Find("[data-testid='context-sheet']");
        Assert.NotEmpty(sheet.QuerySelectorAll("[data-testid='nav-a']"));
        Assert.NotEmpty(sheet.QuerySelectorAll("[data-testid='nav-g']"));
    }

    [Fact]
    public void BottomNav_Should_MarkActiveRoot_When_OnDescendantPage()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsBottomNav>(p => p
            .Add(c => c.Tree, Tree())
            .Add(c => c.CurrentUri, "http://localhost/b/group/leaf"));

        Assert.Contains("mbx-bottom-nav-item-active", rendered.Find(".mbx-bottom-nav [data-testid='nav-b']").ClassList);
        Assert.DoesNotContain("mbx-bottom-nav-item-active", rendered.Find(".mbx-bottom-nav [data-testid='nav-c']").ClassList);
    }

    [Fact]
    public void MobileDrilldownNav_Should_RenderWrapperBottomNavFromTree()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsMobileDrilldownNav>(p => p
            .Add(c => c.Tree, Tree())
            .Add(c => c.CurrentUri, "http://localhost/a"));

        Assert.NotEmpty(rendered.FindAll("[data-testid='bottom-nav']"));
        Assert.NotEmpty(rendered.FindAll("[data-testid='nav-b']"));

        rendered.Find("[data-testid='nav-b']").Click();
        Assert.NotEmpty(rendered.FindAll("[data-testid='context-sheet']"));
    }

    [Fact]
    public void AppShell_Should_ExposeDesktopModeAndAddClass_When_ResponsiveModeDesktop()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsAppShell>(p => p
            .Add(c => c.ResponsiveMode, MdsAppShell.MbxResponsiveMode.Desktop));

        var root = rendered.Find("[data-testid='shell-mode']");
        Assert.Equal("desktop", root.GetAttribute("data-shell-mode"));
        Assert.Contains("mbx-responsive-desktop", root.ClassList);
    }

    [Fact]
    public void AppShell_Should_DefaultToDesktopWithoutResponsiveClass_When_Auto()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsAppShell>();

        var root = rendered.Find("[data-testid='shell-mode']");
        Assert.Equal("desktop", root.GetAttribute("data-shell-mode"));
        Assert.DoesNotContain("mbx-responsive-desktop", root.ClassList);
        Assert.Equal(MdsAppShell.MbxShellMode.Desktop, rendered.Instance.ShellMode);
    }

    [Theory]
    [InlineData("tablet", MdsAppShell.MbxShellMode.Tablet)]
    [InlineData("mobile", MdsAppShell.MbxShellMode.Mobile)]
    public void AppShell_Should_SwitchMode_When_ViewportObserverReports(string reported, MdsAppShell.MbxShellMode expected)
    {
        using var context = NewContext();
        MdsAppShell.MbxShellMode? raised = null;
        var rendered = context.RenderComponent<MdsAppShell>(p => p
            .Add(c => c.ShellModeChanged, m => raised = m));

        rendered.InvokeAsync(() => rendered.Instance.OnViewportModeChanged(reported));

        Assert.Equal(reported, rendered.Find("[data-testid='shell-mode']").GetAttribute("data-shell-mode"));
        Assert.Equal(expected, raised);
    }

    [Fact]
    public void AppShell_Should_CollapseContextPanelAndEmitChange_When_LeavingDesktop()
    {
        using var context = NewContext();
        bool? expanded = null;
        var rendered = context.RenderComponent<MdsAppShell>(p => p
            .Add(c => c.ContextPanelExpanded, true)
            .Add(c => c.ContextPanelExpandedChanged, v => expanded = v)
            .Add(c => c.ContextPanelContent, b => b.AddContent(0, "panel")));

        rendered.InvokeAsync(() => rendered.Instance.OnViewportModeChanged("tablet"));

        Assert.False(expanded);
        Assert.Empty(rendered.FindAll("[data-testid='nav-scrim']"));
    }

    [Fact]
    public void AppShell_Should_ShowScrimOnTablet_When_PanelOpened_AndHideItOnScrimClick()
    {
        using var context = NewContext();
        var rendered = context.RenderComponent<MdsAppShell>(p => p
            .Add(c => c.ContextPanelContent, b => b.AddContent(0, "panel")));
        rendered.InvokeAsync(() => rendered.Instance.OnViewportModeChanged("tablet"));
        rendered.InvokeAsync(() => rendered.Instance.ToggleContextPanel());

        Assert.NotEmpty(rendered.FindAll("[data-testid='nav-scrim']"));
        Assert.Contains("mbx-context-panel-expanded", rendered.Find("[data-testid='context-overlay']").ClassList);

        rendered.Find("[data-testid='nav-scrim']").Click();

        Assert.Empty(rendered.FindAll("[data-testid='nav-scrim']"));
        Assert.Contains("mbx-context-panel-collapsed", rendered.Find("[data-testid='context-overlay']").ClassList);
    }

    [Fact]
    public void AppShell_Should_CloseTabletOverlay_On_Escape()
    {
        using var context = NewContext();
        var rendered = context.RenderComponent<MdsAppShell>(p => p
            .Add(c => c.ContextPanelContent, b => b.AddContent(0, "panel")));
        rendered.InvokeAsync(() => rendered.Instance.OnViewportModeChanged("tablet"));
        rendered.InvokeAsync(() => rendered.Instance.ToggleContextPanel());

        rendered.Find("[data-testid='shell-mode']").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Escape" });

        Assert.Empty(rendered.FindAll("[data-testid='nav-scrim']"));
    }

    [Fact]
    public void AppShell_Should_RenderMobileTopBarWithMenuButton_AndRaiseCallback()
    {
        using var context = NewContext();
        var clicked = false;
        var rendered = context.RenderComponent<MdsAppShell>(p => p
            .Add(c => c.MobileTitle, "Title")
            .Add(c => c.MobileMenuClicked, () => clicked = true));

        Assert.Contains("Title", rendered.Find("[data-testid='mobile-topbar']").TextContent);
        rendered.Find("[data-testid='mobile-menu-button']").Click();

        Assert.True(clicked);
    }
}

public class BottomNavElementTypeTests
{
    [Fact]
    public void BottomNav_Should_RenderLeafAsAnchorAndParentsAsButtons_WithSameItemClass()
    {
        using var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var tree = new MbxNavTree(
        [
            new MbxNavNode(id: "home", text: "Home", href: "/"),
            new MbxNavNode(id: "lib", text: "Lib", href: "/lib", children: [new MbxNavNode(id: "lib.x", text: "X", href: "/lib/x")]),
        ]);

        var rendered = context.RenderComponent<MdsBottomNav>(p => p.Add(c => c.Tree, tree).Add(c => c.CurrentUri, "http://localhost/"));

        var anchor = rendered.Find("[data-testid='nav-home']");
        var button = rendered.Find("[data-testid='nav-lib']");
        Assert.Equal("a", anchor.TagName.ToLowerInvariant());
        Assert.Equal("button", button.TagName.ToLowerInvariant());
        Assert.Contains("mbx-bottom-nav-item", anchor.ClassList);
        Assert.Contains("mbx-bottom-nav-item", button.ClassList);
        Assert.Contains("mbx-bottom-nav-item-active", anchor.ClassList);
        Assert.NotNull(anchor.QuerySelector(".mbx-bottom-nav-label"));
        Assert.NotNull(button.QuerySelector(".mbx-bottom-nav-label"));
    }
}
