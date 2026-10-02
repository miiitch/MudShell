using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.Chat;
using Xunit;

namespace MudShell.ComponentTests;

public class ChatPartRegistryTests
{
    private sealed class ProbePart : ComponentBase
    {
        [Parameter] public string Payload { get; set; } = "";

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "b");
            builder.AddContent(1, $"probe:{Payload}");
            builder.CloseElement();
        }
    }

    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    [Fact]
    public void Should_RenderRegisteredComponent_When_KindMatches()
    {
        // Given
        using var context = NewContext();
        var registry = new MdsChatPartRegistry().Register<ProbePart, string>("probe");

        // When
        var cut = context.RenderComponent<MdsChatPartRenderer>(p => p
            .Add(c => c.Registry, registry)
            .Add(c => c.Parts, new[] { new MdsChatPart("probe", "hello") }));

        // Then
        Assert.Equal("probe:hello", cut.Find("b").TextContent);
    }

    [Fact]
    public void Should_RenderFallback_When_KindUnknownOrPayloadTypeWrong()
    {
        // Given
        using var context = NewContext();
        var registry = new MdsChatPartRegistry().Register<ProbePart, string>("probe");

        // When
        var cut = context.RenderComponent<MdsChatPartRenderer>(p => p
            .Add(c => c.Registry, registry)
            .Add(c => c.Parts, new[] { new MdsChatPart("new-kind", "raw text"), new MdsChatPart("probe", 42) }));

        // Then
        Assert.Empty(cut.FindAll("b"));
        Assert.Contains("raw text", cut.Markup);
        Assert.Contains("42", cut.Markup);
    }

    [Fact]
    public void Should_RenderParts_When_PassedToMessage()
    {
        // Given
        using var context = NewContext();
        var registry = new MdsChatPartRegistry().Register<ProbePart, string>("probe");

        // When
        var cut = context.RenderComponent<MdsChatMessage>(p => p
            .Add(c => c.Registry, registry)
            .Add(c => c.Parts, new[] { new MdsChatPart("probe", "x") }));

        // Then
        Assert.Equal("probe:x", cut.Find("b").TextContent);
    }

    [Fact]
    public void Should_ToggleBetweenFabAndPanel_When_OpenedAndClosed()
    {
        // Given
        using var context = NewContext();
        var cut = context.RenderComponent<MdsChatFloating>(p => p.AddChildContent("<i>chat</i>"));
        Assert.Empty(cut.FindAll($"[data-testid='{MbxTestId.ChatFloatingPanel}']"));

        // When
        cut.Find($"[data-testid='{MbxTestId.ChatFloatingFab}']").Click();

        // Then
        Assert.Contains("chat", cut.Find($"[data-testid='{MbxTestId.ChatFloatingPanel}']").InnerHtml);

        // When
        cut.Find($"[data-testid='{MbxTestId.ChatFloatingClose}']").Click();

        // Then
        Assert.Empty(cut.FindAll($"[data-testid='{MbxTestId.ChatFloatingPanel}']"));
        Assert.False(cut.Instance.IsOpen);
    }
}
