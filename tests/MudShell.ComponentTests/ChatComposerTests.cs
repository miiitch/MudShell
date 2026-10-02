using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.Chat;
using Xunit;

namespace MudShell.ComponentTests;

public class ChatComposerTests
{
    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    [Fact]
    public void Should_RaiseOnSendWithTrimmedText_And_Clear_When_SendClicked()
    {
        // Given
        using var context = NewContext();
        string? sent = null;
        var cut = context.RenderComponent<MdsChatComposer>(p => p
            .Add(c => c.Value, "  hello  ")
            .Add(c => c.OnSend, text => { sent = text; }));

        // When
        cut.Find($"[data-testid='{MbxTestId.ChatSend}']").Click();

        // Then
        Assert.Equal("hello", sent);
        Assert.Equal(string.Empty, cut.Instance.Value);
    }

    [Fact]
    public void Should_ShowStopAndNotSend_When_Busy()
    {
        // Given
        using var context = NewContext();
        var sends = 0;
        var stops = 0;
        var cut = context.RenderComponent<MdsChatComposer>(p => p
            .Add(c => c.Value, "hello")
            .Add(c => c.IsBusy, true)
            .Add(c => c.OnSend, _ => { sends++; })
            .Add(c => c.OnStop, () => { stops++; }));

        // When
        cut.Find($"[data-testid='{MbxTestId.ChatStop}']").Click();
        cut.InvokeAsync(() => cut.Instance.HandleEnter("hello"));

        // Then
        Assert.Equal(1, stops);
        Assert.Equal(0, sends);
        Assert.Empty(cut.FindAll($"[data-testid='{MbxTestId.ChatSend}']"));
    }

    [Fact]
    public void Should_RenderCounterAndMaxLength_When_MaxLengthSet()
    {
        // Given / When
        using var context = NewContext();
        var cut = context.RenderComponent<MdsChatComposer>(p => p
            .Add(c => c.Value, "abcd")
            .Add(c => c.MaxLength, 10));

        // Then
        Assert.Equal("4 / 10", cut.Find($"[data-testid='{MbxTestId.ChatCounter}']").TextContent);
        Assert.Equal("10", cut.Find("textarea").GetAttribute("maxlength"));
    }

    [Fact]
    public void Should_DisableSend_When_TextIsBlank()
    {
        // Given / When
        using var context = NewContext();
        var cut = context.RenderComponent<MdsChatComposer>(p => p.Add(c => c.Value, "   "));

        // Then
        Assert.True(cut.Find($"[data-testid='{MbxTestId.ChatSend}']").HasAttribute("disabled"));
    }

    [Fact]
    public void Should_RenderStreamedText_After_Flush()
    {
        // Given
        using var context = NewContext();
        var cut = context.RenderComponent<MdsChatStream>(p => p.Add(c => c.IsStreaming, true));
        Assert.Contains("Thinking", cut.Markup);

        // When
        cut.Instance.Append("Hello ");
        cut.Instance.Append("world");
        cut.InvokeAsync(() => cut.Instance.CompleteAsync()).Wait();

        // Then
        Assert.Contains("Hello world", cut.Markup);
        Assert.DoesNotContain("Thinking", cut.Markup);
    }
}
