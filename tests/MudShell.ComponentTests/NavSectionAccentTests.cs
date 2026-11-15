using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.Navigation;
using MudShell.Components.Navigation.Models;
using MudShell.Components.Sidebar;
using Xunit;

namespace MudShell.ComponentTests;

/// <summary>A section accent is opt-in per rail entry and per context panel; entries without one are unchanged.</summary>
public class NavSectionAccentTests
{
    private static readonly MbxNavItem[] Items =
    [
        new("home-icon", "Home", "/"),
        new("cmdb-icon", "CMDB", "/cmdb", Accent: "#60A5FA"),
    ];

    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Rail_Should_CarryAccent_OnlyOnEntriesThatDeclareOne(bool expanded)
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsSidebar>(p => p
            .Add(c => c.IsExpanded, expanded)
            .Add(c => c.PrimaryItems, Items));

        var accented = rendered.FindAll(".mbx-nav-item-accented");
        var entry = Assert.Single(accented);
        Assert.Contains("--mbx-nav-item-accent:#60A5FA", entry.GetAttribute("style"));
        Assert.NotNull(entry.QuerySelector("a[href='/cmdb']") ?? (entry.GetAttribute("href") == "/cmdb" ? entry : null));
    }

    [Fact]
    public void ContextPanel_Should_CarryAccent_When_AccentIsSet()
    {
        using var context = NewContext();
        var tree = new MbxNavTree([new MbxNavNode(id: "root", text: "Root", href: "/root", children: [new MbxNavNode(id: "leaf", text: "Leaf", href: "/root/leaf")])]);

        var rendered = context.RenderComponent<MdsContextNavPanel>(p => p
            .Add(c => c.Tree, tree)
            .Add(c => c.CurrentUri, "http://localhost/root/leaf")
            .Add(c => c.Accent, "#60A5FA"));

        var root = rendered.Find(".mbx-context-nav-panel");
        Assert.Contains("mbx-context-accented", root.ClassList);
        Assert.Contains("--mbx-section-accent:#60A5FA", root.GetAttribute("style"));
    }

    [Fact]
    public void ContextPanel_Should_NotCarryAccent_When_AccentIsNotSet()
    {
        using var context = NewContext();
        var tree = new MbxNavTree([new MbxNavNode(id: "root", text: "Root", href: "/root", children: [new MbxNavNode(id: "leaf", text: "Leaf", href: "/root/leaf")])]);

        var rendered = context.RenderComponent<MdsContextNavPanel>(p => p
            .Add(c => c.Tree, tree)
            .Add(c => c.CurrentUri, "http://localhost/root/leaf"));

        var root = rendered.Find(".mbx-context-nav-panel");
        Assert.DoesNotContain("mbx-context-accented", root.ClassList);
        Assert.Null(root.GetAttribute("style"));
    }
}
