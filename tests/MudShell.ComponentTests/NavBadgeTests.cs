using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using MudShell.Components.Navigation;
using MudShell.Components.Navigation.Models;
using Xunit;

namespace MudShell.ComponentTests;

public class NavBadgeTests
{
    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    // Sidebar renders the roots; the context panel renders the children of the active root.
    private static MbxNavTree Tree(MbxNavBadge? badge, MbxNavBadge? childBadge = null) => new(
    [
        new MbxNavNode(id: "root", text: "Root", href: "/root", badge: badge, children:
        [
            new MbxNavNode(id: "a", text: "Alpha", href: "/root/a", badge: badge),
            new MbxNavNode(id: "g", text: "Group", href: "/root/g", expanded: true, children:
            [
                new MbxNavNode(id: "g.c", text: "Child", href: "/root/g/c", badge: childBadge),
            ]),
        ]),
    ]);

    private const string PanelUri = "http://localhost/root/a";

    [Fact]
    public void Should_RenderTextBadgeWithTooltip_When_ContextPanelExpanded()
    {
        using var context = NewContext();
        var tree = Tree(new MbxNavBadge("12", Color.Error, Tooltip: "12 failures"));

        var rendered = context.RenderComponent<MdsContextNavPanel>(p => p
            .Add(c => c.Tree, tree).Add(c => c.CurrentUri, PanelUri));

        var badge = rendered.Find(".mds-nav-badge-text");
        Assert.Equal("12", badge.TextContent);
        Assert.Equal("12 failures", badge.GetAttribute("title"));
        Assert.Contains("mds-nav-badge-error", badge.ClassList);
    }

    [Fact]
    public void Should_UseNeutralClass_When_BadgeColorDefault()
    {
        using var context = NewContext();
        var tree = Tree(new MbxNavBadge("3"));

        var rendered = context.RenderComponent<MdsContextNavPanel>(p => p
            .Add(c => c.Tree, tree).Add(c => c.CurrentUri, PanelUri));

        Assert.Contains("mds-nav-badge-default", rendered.Find(".mds-nav-badge-text").ClassList);
    }

    [Fact]
    public void Should_RenderDot_When_BadgeIsDot()
    {
        using var context = NewContext();
        var tree = Tree(new MbxNavBadge(Dot: true, Color: Color.Warning));

        var rendered = context.RenderComponent<MdsContextNavPanel>(p => p
            .Add(c => c.Tree, tree).Add(c => c.CurrentUri, PanelUri));

        Assert.NotNull(rendered.Find(".mds-nav-badge-dot.mds-nav-badge-warning"));
        Assert.Empty(rendered.FindAll(".mds-nav-badge-text"));
    }

    [Fact]
    public void Should_RenderBadgeOnNestedChild_When_ChildHasBadge()
    {
        using var context = NewContext();
        var tree = Tree(null, new MbxNavBadge("5", Color.Warning));

        var rendered = context.RenderComponent<MdsContextNavPanel>(p => p
            .Add(c => c.Tree, tree).Add(c => c.CurrentUri, "http://localhost/root/g/c"));

        Assert.Equal("5", rendered.Find(".mbx-context-nav-child .mds-nav-badge-text").TextContent);
    }

    [Fact]
    public void Should_RenderDotOnly_When_ContextPanelCollapsed()
    {
        using var context = NewContext();
        var tree = Tree(new MbxNavBadge("12", Color.Error));

        var rendered = context.RenderComponent<MdsContextNavPanel>(p => p
            .Add(c => c.Tree, tree).Add(c => c.IsExpanded, false).Add(c => c.CurrentUri, PanelUri));

        Assert.NotNull(rendered.Find(".mds-nav-badge-dot.mds-nav-badge-overlay"));
        Assert.Empty(rendered.FindAll(".mds-nav-badge-text"));
    }

    [Fact]
    public void Should_RenderNoBadgeElement_When_NodeHasNoBadge()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsContextNavPanel>(p => p
            .Add(c => c.Tree, Tree(null)).Add(c => c.CurrentUri, PanelUri));

        Assert.Empty(rendered.FindAll("[class*='mds-nav-badge']"));
    }

    [Fact]
    public void Should_RenderDotOnIcon_When_SidebarNavCollapsed()
    {
        using var context = NewContext();
        var tree = Tree(new MbxNavBadge("9", Color.Error, Tooltip: "9 open"));

        var rendered = context.RenderComponent<MdsSidebarNav>(p => p
            .Add(c => c.Tree, tree).Add(c => c.IsExpanded, false).Add(c => c.CurrentUri, PanelUri));

        var dot = rendered.Find(".mbx-sidebar-nav-icon-link .mds-nav-badge-dot");
        Assert.Equal("9 open", dot.GetAttribute("title"));
        Assert.Empty(rendered.FindAll(".mds-nav-badge-text"));
    }

    [Fact]
    public void Should_RenderDotNotText_When_SidebarNavExpanded()
    {
        using var context = NewContext();
        var tree = Tree(new MbxNavBadge("9", Color.Error));

        var rendered = context.RenderComponent<MdsSidebarNav>(p => p
            .Add(c => c.Tree, tree).Add(c => c.IsExpanded, true).Add(c => c.CurrentUri, PanelUri));

        Assert.NotNull(rendered.Find(".mds-nav-badge-dot"));
        Assert.Empty(rendered.FindAll(".mds-nav-badge-text"));
    }
}
