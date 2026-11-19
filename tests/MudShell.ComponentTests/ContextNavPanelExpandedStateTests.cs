using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.Navigation;
using MudShell.Components.Navigation.Models;
using Xunit;

namespace MudShell.ComponentTests;

/// <summary>
/// The host can own the expanded and collapsed node sets, so that a panel re-created on navigation
/// (for example through <c>@key</c>) keeps the open state the user chose.
/// </summary>
public class ContextNavPanelExpandedStateTests
{
    private const string CurrentUri = "http://localhost/root/plain";
    private const string GroupToggle = "[data-testid='nav-group-group'] > button";
    private const string DefaultedToggle = "[data-testid='nav-group-defaulted'] > button";

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
            new MbxNavNode(id: "defaulted", text: "Defaulted", icon: "x", defaultExpanded: true, children:
            [
                new MbxNavNode(id: "defaulted-child", text: "Defaulted child", href: "/root/defaulted/child"),
            ]),
        ]),
    ]);

    // SingleExpandedLevel2 is off so that a group which is not on the current page can stay open.
    private static IRenderedComponent<MdsContextNavPanel> RenderPanel(
        TestContext context,
        HashSet<string> expanded,
        HashSet<string> collapsed) =>
        context.RenderComponent<MdsContextNavPanel>(p => p
            .Add(c => c.Tree, Tree())
            .Add(c => c.CurrentUri, CurrentUri)
            .Add(c => c.SingleExpandedLevel2, false)
            .Add(c => c.ExpandedNodeIds, expanded)
            .Add(c => c.CollapsedNodeIds, collapsed));

    [Fact]
    public void Should_KeepToggledOpenGroup_When_PanelIsRecreatedWithSharedSets()
    {
        using var context = NewContext();
        var expanded = new HashSet<string>(StringComparer.Ordinal);
        var collapsed = new HashSet<string>(StringComparer.Ordinal);

        var first = RenderPanel(context, expanded, collapsed);
        Assert.Equal("false", first.Find(GroupToggle).GetAttribute("aria-expanded"));

        first.Find(GroupToggle).Click();
        Assert.Equal("true", first.Find(GroupToggle).GetAttribute("aria-expanded"));
        first.Dispose();

        var second = RenderPanel(context, expanded, collapsed);

        Assert.Equal("true", second.Find(GroupToggle).GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Should_KeepUserCollapsedDefaultGroupCollapsed_When_PanelIsRecreatedWithSharedSets()
    {
        using var context = NewContext();
        var expanded = new HashSet<string>(StringComparer.Ordinal);
        var collapsed = new HashSet<string>(StringComparer.Ordinal);

        var first = RenderPanel(context, expanded, collapsed);
        Assert.Equal("true", first.Find(DefaultedToggle).GetAttribute("aria-expanded"));

        first.Find(DefaultedToggle).Click();
        Assert.Equal("false", first.Find(DefaultedToggle).GetAttribute("aria-expanded"));
        Assert.Contains("defaulted", collapsed);
        first.Dispose();

        var second = RenderPanel(context, expanded, collapsed);

        Assert.Equal("false", second.Find(DefaultedToggle).GetAttribute("aria-expanded"));
    }
}
