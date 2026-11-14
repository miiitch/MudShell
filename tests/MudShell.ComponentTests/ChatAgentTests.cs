using System.Runtime.CompilerServices;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.Chat;
using Xunit;

namespace MudShell.ComponentTests;

/// <summary>MdsChat on top of a real UIAgent driven by a scripted IChatClient.</summary>
public class ChatAgentTests
{
    private sealed class ScriptedClient(Func<IReadOnlyList<ChatMessage>, IEnumerable<AIContent>> script) : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var content in script(messages.ToList()))
            {
                await Task.Yield();
                yield return new ChatResponseUpdate(ChatRole.Assistant, [content]) { MessageId = "m1" };
            }
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private static UIAgent NewAgent(Func<IReadOnlyList<ChatMessage>, IEnumerable<AIContent>> script) =>
        new(new ScriptedClient(script), o => o.AddMdsReasoning());

    private static async Task SendAsync(IRenderedComponent<MdsChat> cut, string text)
    {
        var composer = cut.FindComponent<MdsChatComposer>();
        await cut.InvokeAsync(() => composer.Instance.OnSend.InvokeAsync(text));
    }

    [Fact]
    public async Task Should_StreamAssistantText_After_Send()
    {
        // Given
        using var context = NewContext();
        using var agent = NewAgent(_ => [new TextContent("Hello "), new TextContent("world")]);
        var cut = context.RenderComponent<MdsChat>(p => p.Add(c => c.Agent, agent));

        // When
        await SendAsync(cut, "hi");

        // Then
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("hi", cut.Find(".sc-ai-message--user").TextContent);
            Assert.Contains("Hello world", cut.Find(".sc-ai-message--assistant").TextContent);
        });
    }

    [Fact]
    public void Should_ShowLandingWithPrompts_UntilFirstMessage_And_SendPromptOnClick()
    {
        // Given
        using var context = NewContext();
        using var agent = NewAgent(_ => [new TextContent("ok")]);
        var cut = context.RenderComponent<MdsChat>(p => p
            .Add(c => c.Agent, agent)
            .Add(c => c.Greeting, "Welcome")
            .Add(c => c.Prompts, ["Ask me"]));
        Assert.Contains("Welcome", cut.Markup);

        // When
        cut.Find($"[data-testid='{MbxTestId.ChatPrompt(0)}']").Click();

        // Then
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Ask me", cut.Find(".sc-ai-message--user").TextContent);
            Assert.DoesNotContain("Welcome", cut.Markup);
        });
    }

    [Fact]
    public async Task Should_RenderToolCallCard_WithResult()
    {
        // Given
        using var context = NewContext();
        var call = new FunctionCallContent("c1", "get_weather", new Dictionary<string, object?> { ["city"] = "Paris" });
        using var agent = NewAgent(_ => [call, new FunctionResultContent("c1", "sunny")]);
        var cut = context.RenderComponent<MdsChat>(p => p.Add(c => c.Agent, agent));

        // When
        await SendAsync(cut, "weather?");

        // Then
        cut.WaitForAssertion(() =>
        {
            var card = cut.Find("[data-testid='chat-tool-call']");
            Assert.Equal("done", card.GetAttribute("data-state"));
            Assert.Contains("get_weather", card.TextContent);
            Assert.Contains("Paris", card.TextContent);
            Assert.Contains("sunny", card.TextContent);
        });
    }

    [Fact]
    public async Task Should_RenderReasoningPanel_FromReasoningContent()
    {
        // Given
        using var context = NewContext();
        using var agent = NewAgent(_ => [new TextReasoningContent("Thinking "), new TextReasoningContent("hard"), new TextContent("Answer")]);
        var cut = context.RenderComponent<MdsChat>(p => p.Add(c => c.Agent, agent));

        // When
        await SendAsync(cut, "q");

        // Then
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Thinking hard", cut.Find("[data-testid='chat-reasoning']").TextContent);
            Assert.Contains("Answer", cut.Find(".sc-ai-message--assistant").TextContent);
        });
    }

    [Fact]
    public async Task Should_PromptForApproval_And_ShowDecision()
    {
        // Given
        using var context = NewContext();
        var call = new FunctionCallContent("c1", "delete_report");
        using var agent = NewAgent(messages =>
            messages.LastOrDefault()?.Contents.OfType<ToolApprovalResponseContent>().Any() == true
                ? [new TextContent("Done")]
                : [new ToolApprovalRequestContent("a1", call)]);
        var cut = context.RenderComponent<MdsChat>(p => p.Add(c => c.Agent, agent));
        await SendAsync(cut, "delete");
        cut.WaitForAssertion(() => Assert.Equal("pending", cut.Find("[data-testid='chat-approval']").GetAttribute("data-status")));

        // When
        cut.Find("[data-testid='chat-approve']").Click();

        // Then
        cut.WaitForAssertion(() =>
        {
            Assert.Equal("approved", cut.Find("[data-testid='chat-approval']").GetAttribute("data-status"));
            Assert.Empty(cut.FindAll("[data-testid='chat-approve']"));
            Assert.Contains("Done", cut.Find(".sc-ai-message--assistant").TextContent);
        });
    }

    [Fact]
    public async Task Should_PreferCustomBlockRenderer_OverDefault()
    {
        // Given
        using var context = NewContext();
        var call = new FunctionCallContent("c1", "chart");
        using var agent = NewAgent(_ => [call, new FunctionResultContent("c1", "x")]);
        var cut = context.RenderComponent<MdsChat>(p => p
            .Add(c => c.Agent, agent)
            .Add(c => c.MessageListContent, (RenderFragment)(builder =>
            {
                builder.OpenComponent<BlockRenderer<FunctionInvocationContentBlock>>(0);
                builder.AddComponentParameter(1, "ChildContent", (RenderFragment<FunctionInvocationContentBlock>)(b =>
                    inner => inner.AddMarkupContent(0, "<p data-testid='custom'>custom chart</p>")));
                builder.CloseComponent();
            })));

        // When
        await SendAsync(cut, "go");

        // Then
        cut.WaitForAssertion(() => Assert.Contains("custom chart", cut.Find("[data-testid='custom']").TextContent));
        Assert.Empty(cut.FindAll("[data-testid='chat-tool-call']"));
    }

    [Fact]
    public void Should_ForwardUndeclaredAttribute_ToRoot()
    {
        // Given
        using var context = NewContext();
        using var agent = NewAgent(_ => []);

        // When
        var cut = context.RenderComponent<MdsChat>(p => p
            .Add(c => c.Agent, agent)
            .AddUnmatched("data-testid", "chat-root"));

        // Then
        Assert.Equal("chat-root", cut.Find(".mds-chat-surface").GetAttribute("data-testid"));
    }
}
