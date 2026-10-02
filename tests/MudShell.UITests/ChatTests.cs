using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

/// <summary>Covers the chat demo on "/chat": landing, composer keyboard handling, streaming and stop.</summary>
public class ChatTests : PlaywrightTestBase
{
    private async Task OpenAsync()
    {
        await Page.GotoAsync("/chat");
        await Assertions.Expect(Page.GetByTestId("chat-landing")).ToBeVisibleAsync();
        // Let the circuit take over from the prerendered markup before interacting.
        await Page.WaitForTimeoutAsync(1000);
    }

    [Fact]
    public async Task Should_SendOnEnter_And_InsertNewlineOnShiftEnter()
    {
        await OpenAsync();
        var input = Page.GetByTestId("chat-input");

        await input.FillAsync("line one");
        await input.PressAsync("Shift+Enter");
        await input.PressSequentiallyAsync("line two");
        Assert.Equal("line one\nline two", await input.InputValueAsync());

        await input.PressAsync("Enter");

        await Assertions.Expect(Page.GetByTestId("chat-message-user")).ToContainTextAsync("line two");
        await Assertions.Expect(input).ToHaveValueAsync("");
    }

    [Fact]
    public async Task Should_SendPrompt_When_SuggestionChipClicked()
    {
        await OpenAsync();

        await Page.GetByTestId("chat-prompt-0").ClickAsync();

        await Assertions.Expect(Page.GetByTestId("chat-message-user")).ToContainTextAsync("Summarise my week");
    }

    [Fact]
    public async Task Should_StreamReply_And_ShowStop_ThenStopEarly()
    {
        await OpenAsync();
        var input = Page.GetByTestId("chat-input");
        await input.FillAsync("hello");
        await input.PressAsync("Enter");

        var stop = Page.GetByTestId("chat-stop");
        await Assertions.Expect(stop).ToBeVisibleAsync();
        await Assertions.Expect(Page.GetByTestId("chat-message-assistant")).ToContainTextAsync("You asked");

        await stop.ClickAsync();

        await Assertions.Expect(Page.GetByTestId("chat-send")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Should_ShowCounter_When_MaxLengthIsSet()
    {
        await OpenAsync();
        await Page.GetByTestId("chat-input").FillAsync("abc");

        await Assertions.Expect(Page.GetByTestId("chat-counter")).ToHaveTextAsync("3 / 500");
    }

    [Fact]
    public async Task Should_RenderRegisteredPartComponent_And_FallbackForUnknownKind()
    {
        await OpenAsync();
        var input = Page.GetByTestId("chat-input");
        await input.FillAsync("hi");
        await input.PressAsync("Enter");

        await Assertions.Expect(Page.GetByTestId("chat-part-chart")).ToBeVisibleAsync(new() { Timeout = 30000 });
        await Assertions.Expect(Page.Locator("[data-part-kind='unknown-kind']"))
            .ToContainTextAsync("Unregistered kinds use the fallback.");
    }

    [Fact]
    public async Task Should_OpenFloatingPanel_FromFab_And_CloseOnEscape()
    {
        await Page.GotoAsync("/chat-floating");
        var fab = Page.GetByTestId("chat-fab");
        await Assertions.Expect(fab).ToBeVisibleAsync();
        await Page.WaitForTimeoutAsync(1000);

        await fab.ClickAsync();
        var panel = Page.GetByTestId("chat-floating-panel");
        await Assertions.Expect(panel).ToBeVisibleAsync();

        var input = panel.GetByTestId("chat-input");
        await input.FillAsync("from the corner");
        await input.PressAsync("Enter");
        await Assertions.Expect(panel.GetByTestId("floating-message")).ToContainTextAsync("from the corner");

        await panel.PressAsync("Escape");
        await Assertions.Expect(panel).ToBeHiddenAsync();
        await Assertions.Expect(fab).ToBeVisibleAsync();
    }
}
