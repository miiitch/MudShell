using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using MudShell.Components.MainContent;
using Xunit;

namespace MudShell.ComponentTests;

public class MainContentTypographyTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Section_UsesNeutralTextForDescriptionAndLoading(bool loading)
    {
        using var context = NewContext();
        var rendered = context.RenderComponent<MdsMainSection>(p => p
            .Add(c => c.Title, "Section")
            .Add(c => c.Description, "Description")
            .Add(c => c.IsLoading, loading));

        Assert.Equal(Color.Default, rendered.Instance.TitleColor);
        Assert.NotEmpty(rendered.FindAll(".mud-text-secondary"));
        Assert.Empty(rendered.FindAll(".mud-typography.mud-secondary-text"));
        Assert.Empty(rendered.FindAll(".mud-typography.mud-primary-text"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Part_UsesNeutralTextForSubtitleAndLoading(bool loading)
    {
        using var context = NewContext();
        var rendered = context.RenderComponent<MdsMainPart>(p => p
            .Add(c => c.Title, "Part")
            .Add(c => c.Subtitle, "Subtitle")
            .Add(c => c.IsLoading, loading));

        Assert.Equal(Color.Default, rendered.Instance.TitleColor);
        Assert.NotEmpty(rendered.FindAll(".mud-text-secondary"));
        Assert.Empty(rendered.FindAll(".mud-typography.mud-secondary-text"));
        Assert.Empty(rendered.FindAll(".mud-typography.mud-primary-text"));
    }

    [Fact]
    public void ExplicitTitleColor_IsPreserved()
    {
        using var context = NewContext();
        var section = context.RenderComponent<MdsMainSection>(p => p
            .Add(c => c.Title, "Section")
            .Add(c => c.TitleColor, Color.Primary));
        var part = context.RenderComponent<MdsMainPart>(p => p
            .Add(c => c.Title, "Part")
            .Add(c => c.TitleColor, Color.Info));

        Assert.NotNull(section.Find(".mud-typography.mud-primary-text"));
        Assert.NotNull(part.Find(".mud-typography.mud-info-text"));
    }

    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }
}
