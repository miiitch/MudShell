using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.AppShell;
using MudShell.Components.BottomNav;
using MudShell.Components.Navigation;
using MudShell.Components.Navigation.Models;
using MudShell.Components.Sidebar;
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

public class AppShellContextOverlayClassTests
{
    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    [Fact]
    public void AppShell_Should_NeverHaveOverlayOpenClass_When_DefaultDesktopMode_EvenIfExpanded()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsAppShell>(p => p
            .Add(c => c.ContextPanelExpanded, true)
            .Add(c => c.ContextPanelContent, b => b.AddContent(0, "panel")));

        Assert.DoesNotContain("mbx-context-overlay-open", rendered.Find("[data-testid='context-overlay']").ClassList);
    }

    [Fact]
    public void AppShell_Should_AddOverlayOpenClass_Only_When_TabletAndExpanded()
    {
        using var context = NewContext();
        var rendered = context.RenderComponent<MdsAppShell>(p => p
            .Add(c => c.ContextPanelExpanded, true)
            .Add(c => c.ContextPanelContent, b => b.AddContent(0, "panel")));

        // Entering tablet collapses an expanded panel (it starts closed): no class.
        rendered.InvokeAsync(() => rendered.Instance.OnViewportModeChanged("tablet"));
        Assert.DoesNotContain("mbx-context-overlay-open", rendered.Find("[data-testid='context-overlay']").ClassList);

        // Opened by the user (expanded + tablet): class present.
        rendered.InvokeAsync(() => rendered.Instance.ToggleContextPanel());
        Assert.Contains("mbx-context-overlay-open", rendered.Find("[data-testid='context-overlay']").ClassList);

        // Closed again: class removed.
        rendered.InvokeAsync(() => rendered.Instance.ToggleContextPanel());
        Assert.DoesNotContain("mbx-context-overlay-open", rendered.Find("[data-testid='context-overlay']").ClassList);
    }

    [Fact]
    public void AppShell_Should_NotHaveOverlayOpenClass_When_TabletAndCollapsed()
    {
        using var context = NewContext();
        var rendered = context.RenderComponent<MdsAppShell>(p => p
            .Add(c => c.ContextPanelExpanded, false)
            .Add(c => c.ContextPanelContent, b => b.AddContent(0, "panel")));

        rendered.InvokeAsync(() => rendered.Instance.OnViewportModeChanged("tablet"));

        Assert.DoesNotContain("mbx-context-overlay-open", rendered.Find("[data-testid='context-overlay']").ClassList);
    }
}

public class RailTapOpenContextTests
{
    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    [Fact]
    public void Sidebar_Should_RenderCollapsedIconLinks_And_RaiseOnItemClickWithClickedItem()
    {
        using var context = NewContext();
        MbxNavItem? clicked = null;
        var items = new[]
        {
            new MbxNavItem("home-icon", "Home", "/"),
            new MbxNavItem("lib-icon", "Library", "/library"),
        };

        var rendered = context.RenderComponent<MdsSidebar>(p => p
            .Add(c => c.IsExpanded, false)
            .Add(c => c.PrimaryItems, items)
            .Add(c => c.OnItemClick, i => clicked = i));

        var links = rendered.FindAll("a.mbx-sidebar-icon-link");
        Assert.Equal(2, links.Count);
        links[1].Click();

        Assert.NotNull(clicked);
        Assert.Equal("/library", clicked!.Href);
    }

    private static IRenderedComponent<MdsAppShell> Shell(TestContext context, List<bool> changes, bool expanded = false) =>
        context.RenderComponent<MdsAppShell>(p => p
            .Add(c => c.ContextPanelExpanded, expanded)
            .Add(c => c.ContextPanelExpandedChanged, v => changes.Add(v))
            .Add(c => c.ContextPanelContent, b => b.AddContent(0, "panel")));

    [Fact]
    public void AppShell_Tablet_OpenContextPanelOnNextNavigation_OpensAndRaises_FirstNavigationKeepsOpen_SecondCloses()
    {
        using var context = NewContext();
        var nav = context.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
        var changes = new List<bool>();
        var rendered = Shell(context, changes);
        rendered.InvokeAsync(() => rendered.Instance.OnViewportModeChanged("tablet"));
        changes.Clear();

        rendered.InvokeAsync(() => rendered.Instance.OpenContextPanelOnNextNavigation());

        Assert.Equal(new[] { true }, changes);
        Assert.Contains("mbx-context-overlay-open", rendered.Find("[data-testid='context-overlay']").ClassList);

        nav.NavigateTo("/first");
        Assert.Contains("mbx-context-overlay-open", rendered.Find("[data-testid='context-overlay']").ClassList);

        nav.NavigateTo("/second");
        rendered.WaitForAssertion(() =>
            Assert.DoesNotContain("mbx-context-overlay-open", rendered.Find("[data-testid='context-overlay']").ClassList));
        Assert.False(changes.Last());
    }

    [Fact]
    public void AppShell_Desktop_OpenContextPanelOnNextNavigation_IsNoOp()
    {
        using var context = NewContext();
        var nav = context.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
        var changes = new List<bool>();
        var rendered = Shell(context, changes);

        rendered.InvokeAsync(() => rendered.Instance.OpenContextPanelOnNextNavigation());
        nav.NavigateTo("/x");

        Assert.Empty(changes);
        Assert.False(rendered.Instance.ContextPanelExpanded);
        Assert.DoesNotContain("mbx-context-overlay-open", rendered.Find("[data-testid='context-overlay']").ClassList);
    }
}

public class SidebarTreeTests
{
    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private static MbxNavTree Tree() => new(
    [
        new MbxNavNode(id: "home", text: "Home", icon: "i", href: "/", match: MbxNavMatchMode.All),
        new MbxNavNode(id: "lib", text: "Library", icon: "i", href: "/library", children: [new MbxNavNode(id: "lib.x", text: "X", href: "/library/x")]),
        new MbxNavNode(id: "hidden", text: "Hidden", icon: "i", href: "/hidden", visible: false),
        new MbxNavNode(id: "action", text: "Action", icon: "i"),
    ]);

    private static IRenderedComponent<MdsSidebar> Render(TestContext context, Action<ComponentParameterCollectionBuilder<MdsSidebar>> setup) =>
        context.RenderComponent<MdsSidebar>(p => { p.Add(c => c.IsExpanded, false); setup(p); });

    [Fact]
    public void Tree_Should_RenderOneIconLinkPerVisibleRootWithHref_AndExcludeHiddenRoots()
    {
        using var context = NewContext();

        var rendered = Render(context, p => p.Add(c => c.Tree, Tree()));

        var hrefs = rendered.FindAll("a.mbx-sidebar-icon-link").Select(a => a.GetAttribute("href")).ToList();
        Assert.Equal(new[] { "/", "/library" }, hrefs.Select(h => new Uri(new Uri("http://localhost"), h).AbsolutePath).ToArray());
        Assert.DoesNotContain(hrefs, h => h!.Contains("hidden"));
    }

    [Fact]
    public void Tree_Should_RenderRootWithoutHrefAsButton_NotAnchor()
    {
        using var context = NewContext();

        var rendered = Render(context, p => p.Add(c => c.Tree, Tree()));

        Assert.Equal(2, rendered.FindAll("a.mbx-sidebar-icon-link").Count);
        // The href-less root has no anchor: it is a MudIconButton (a <button>) next to the toggle.
        var buttons = rendered.FindAll(".mbx-nav-items button");
        Assert.Single(buttons);
    }

    [Fact]
    public void PrimaryItems_Should_TakePrecedenceOverTree()
    {
        using var context = NewContext();
        var items = new[] { new MbxNavItem("i", "Only", "/only") };

        var rendered = Render(context, p => p.Add(c => c.Tree, Tree()).Add(c => c.PrimaryItems, items));

        var links = rendered.FindAll("a.mbx-sidebar-icon-link");
        Assert.Single(links);
        Assert.EndsWith("/only", links[0].GetAttribute("href"));
    }

    [Fact]
    public void NullTreeAndNullPrimaryItems_Should_RenderNoItems_WithoutThrowing()
    {
        using var context = NewContext();

        var rendered = Render(context, _ => { });

        Assert.Empty(rendered.FindAll("a.mbx-sidebar-icon-link"));
        Assert.Empty(rendered.FindAll(".mbx-nav-items"));
    }

    [Fact]
    public void Tree_Should_FollowActiveStateFromUri_PrefixMatch_AndHomeOnlyOnRoot()
    {
        using var context = NewContext();
        var nav = context.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
        nav.NavigateTo("/library/x");
        var rendered = Render(context, p => p.Add(c => c.Tree, Tree()));

        bool Active(string hrefEnd) => rendered.FindAll("a.mbx-sidebar-icon-link")
            .Single(a => a.GetAttribute("href")!.EndsWith(hrefEnd) && (hrefEnd != "/" || a.GetAttribute("href")!.TrimEnd('/') == "" || a.GetAttribute("href") == "/"))
            .ClassList.Contains("mbx-sidebar-icon-link-active");

        Assert.True(Active("/library"));   // prefix match on a descendant page
        Assert.False(Active("/"));          // '/' is active only on '/'

        nav.NavigateTo("/");
        rendered.WaitForAssertion(() => Assert.True(Active("/")));
        Assert.False(Active("/library"));
    }

    [Fact]
    public void AppShell_Should_DropStaleOpenState_When_TabletAndNoContextPanelContent()
    {
        using var context = NewContext();
        bool? raised = null;
        var rendered = context.RenderComponent<MdsAppShell>(p => p
            .Add(c => c.ContextPanelExpandedChanged, EventCallback.Factory.Create<bool>(this, v => raised = v)));

        rendered.InvokeAsync(() => rendered.Instance.OnViewportModeChanged("tablet"));
        raised = null;

        // A rail tap on a page without level 2 leaves ContextPanelExpanded=true with nothing to show.
        rendered.SetParametersAndRender(p => p.Add(c => c.ContextPanelExpanded, true));

        Assert.False(rendered.Instance.ContextPanelExpanded);
        Assert.False(raised);
    }
}
