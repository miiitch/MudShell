using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.Navigation;
using MudShell.Components.Navigation.Models;
using Xunit;

namespace MudShell.ComponentTests;

public class ContextNavPanelCollapsedParentTests
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
        new MbxNavNode(id: "root", text: "Root", href: "/root", children:
        [
            new MbxNavNode(id: "plain", text: "Plain", href: "/root/plain"),
            new MbxNavNode(id: "group", text: "Group", icon: "x", children:
            [
                new MbxNavNode(id: "child", text: "Child", href: "/root/group/child"),
            ]),
        ]),
    ]);

    [Fact]
    public void Should_HighlightCollapsedParent_When_OnADescendantPage()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsContextNavPanel>(p => p
            .Add(c => c.Tree, Tree())
            .Add(c => c.CurrentUri, "http://localhost/root/group/child")
            .Add(c => c.IsExpanded, false));

        var parent = rendered.Find("[data-testid='nav-group']");
        Assert.Contains("mbx-context-collapsed-link-active", parent.ClassList);
    }

    [Fact]
    public void Should_NotHighlightCollapsedParent_When_OnAnotherPage()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsContextNavPanel>(p => p
            .Add(c => c.Tree, Tree())
            .Add(c => c.CurrentUri, "http://localhost/root/plain")
            .Add(c => c.IsExpanded, false));

        Assert.DoesNotContain("mbx-context-collapsed-link-active", rendered.Find("[data-testid='nav-group']").ClassList);
    }

    [Fact]
    public void Should_RequestExpansion_When_CollapsedParentClicked()
    {
        using var context = NewContext();
        bool? requested = null;

        var rendered = context.RenderComponent<MdsContextNavPanel>(p => p
            .Add(c => c.Tree, Tree())
            .Add(c => c.CurrentUri, "http://localhost/root/group/child")
            .Add(c => c.IsExpanded, false)
            .Add(c => c.IsExpandedChanged, v => requested = v));

        rendered.Find("[data-testid='nav-group']").Click();

        Assert.True(requested);
    }
}
