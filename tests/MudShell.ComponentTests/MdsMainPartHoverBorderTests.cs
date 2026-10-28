using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.MainContent;
using Xunit;

namespace MudShell.ComponentTests;

public class MdsMainPartHoverBorderTests
{
    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    [Fact]
    public void Should_NotAddHoverClassOrVariables_When_HoverBorderIsOff()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsMainPart>(p => p.Add(c => c.Title, "Part"));

        var root = rendered.Find(".mud-paper");
        Assert.DoesNotContain("mds-main-part-hoverable", root.ClassName);
        Assert.DoesNotContain("--mds-part-hover-color", root.GetAttribute("style"));
    }

    [Fact]
    public void Should_AddHoverClassAndColorVariable_When_HoverBorderIsOn()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsMainPart>(p => p
            .Add(c => c.Title, "Part")
            .Add(c => c.HoverBorder, true)
            .Add(c => c.HoverBorderColor, "#ff0000")
            .Add(c => c.HoverBorderDuration, 400));

        var root = rendered.Find(".mud-paper");
        Assert.Contains("mds-main-part-hoverable", root.ClassName);
        var style = root.GetAttribute("style");
        Assert.Contains("--mds-part-hover-color:#ff0000", style);
        Assert.Contains("--mds-part-hover-ms:400ms", style);
    }
}
