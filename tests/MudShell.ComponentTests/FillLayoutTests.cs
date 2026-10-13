using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.Layout;
using Xunit;

namespace MudShell.ComponentTests;

public class FillLayoutTests
{
    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    [Fact]
    public void Should_RenderHeaderBandsAndBodyInOrder_When_AllSlotsProvided()
    {
        // Given
        using var context = NewContext();

        // When
        var rendered = context.RenderComponent<MdsFillLayout>(p => p
            .Add(c => c.Header, (RenderFragment)(b => b.AddMarkupContent(0, "<i id='h'></i>")))
            .Add(c => c.Bands, (RenderFragment)(b => b.AddMarkupContent(0, "<i id='b'></i>")))
            .Add(c => c.Body, (RenderFragment)(b => b.AddMarkupContent(0, "<i id='x'></i>"))));

        // Then
        var children = rendered.Find(".mds-fill-layout").Children;
        Assert.Equal(
            ["mds-fill-layout-header", "mds-fill-layout-bands", "mds-fill-layout-body"],
            children.Select(c => c.ClassList.First()).ToArray());
        Assert.NotNull(rendered.Find(".mds-fill-layout-header #h"));
        Assert.NotNull(rendered.Find(".mds-fill-layout-bands #b"));
        Assert.NotNull(rendered.Find(".mds-fill-layout-body #x"));
    }

    [Fact]
    public void Should_OmitHeaderAndBandsWrappers_When_SlotsNotProvided()
    {
        // Given
        using var context = NewContext();

        // When
        var rendered = context.RenderComponent<MdsFillLayout>(p => p
            .Add(c => c.ChildContent, (RenderFragment)(b => b.AddContent(0, "body"))));

        // Then
        Assert.Empty(rendered.FindAll(".mds-fill-layout-header"));
        Assert.Empty(rendered.FindAll(".mds-fill-layout-bands"));
        Assert.Equal("body", rendered.Find(".mds-fill-layout-body").TextContent);
    }

    [Fact]
    public void Should_SplatAdditionalAttributes_When_Provided()
    {
        // Given
        using var context = NewContext();

        // When
        var rendered = context.RenderComponent<MdsFillLayout>(p => p.AddUnmatched("data-testid", "page"));

        // Then
        Assert.Equal("page", rendered.Find(".mds-fill-layout").GetAttribute("data-testid"));
    }

    [Fact]
    public void Should_RenderChildContentInsideScrollingRoot_When_FillRegionUsed()
    {
        // Given
        using var context = NewContext();

        // When
        var rendered = context.RenderComponent<MdsFillRegion>(p => p
            .Add(c => c.Class, "extra")
            .Add(c => c.ChildContent, (RenderFragment)(b => b.AddContent(0, "content"))));

        // Then
        var root = rendered.Find(".mds-fill-region");
        Assert.Contains("extra", root.ClassList);
        Assert.Equal("content", root.TextContent);
    }
}
