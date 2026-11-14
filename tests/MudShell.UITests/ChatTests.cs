using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

/// <summary>Covers the chat demo on "/chat": landing, composer keyboard handling, streaming and stop.</summary>
public class ChatTests : PlaywrightTestBase
{
    [Theory]
    [InlineData(390)]
    [InlineData(1280)]
    public Task Transcript_ScrollsWithoutMovingComposer(int width) => RunAsync(
        $"{nameof(Transcript_ScrollsWithoutMovingComposer)}_{width}", async () =>
    {
        await Page.SetViewportSizeAsync(width, 900);
        await OpenAsync();
        await Page.GetByTestId("chat-input").FillAsync(
            string.Join(" ", Enumerable.Repeat("Explain scrolling transcripts in detail.", 10)));
        await Page.GetByTestId("chat-input").PressAsync("Enter");
        // Measure once the turn is over: the composer shrinks back after the long draft is sent.
        await Assertions.Expect(Page.GetByTestId("chat-stop")).ToBeVisibleAsync();
        await Assertions.Expect(Page.GetByTestId("chat-stop")).ToHaveCountAsync(0, new() { Timeout = 60000 });

        var composer = Page.Locator(".mds-chat-input");
        var before = await composer.BoundingBoxAsync();
        Assert.True(before!.Height > 0);
        Assert.InRange(before.Y + before.Height, 0, 900);
        var scroll = Page.Locator(".mds-chat-transcript-scroll");
        Assert.True(await scroll.EvaluateAsync<bool>("el => el.scrollHeight > el.clientHeight"));
        await scroll.EvaluateAsync("el => el.scrollTop = 0");
        var after = await composer.BoundingBoxAsync();
        Assert.Equal(before.Y, after!.Y);
        Assert.True(await Page.Locator(".mbx-main-inner")
            .EvaluateAsync<bool>("el => el.scrollHeight <= el.clientHeight + 1"));
    });

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

        await Assertions.Expect(Page.Locator(".sc-ai-message--user")).ToContainTextAsync("line two");
        await Assertions.Expect(input).ToHaveValueAsync("");
    }

    [Fact]
    public async Task Should_SendPrompt_When_SuggestionChipClicked()
    {
        await OpenAsync();

        await Page.GetByTestId("chat-prompt-0").ClickAsync();

        await Assertions.Expect(Page.Locator(".sc-ai-message--user")).ToContainTextAsync("Show my usage");
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
        await Assertions.Expect(Page.Locator(".sc-ai-message--assistant")).ToContainTextAsync("You asked");

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
    public async Task Should_RenderCustomBlockRenderer_ForUsageToolCall()
    {
        await OpenAsync();
        var input = Page.GetByTestId("chat-input");
        await input.FillAsync("show my usage");
        await input.PressAsync("Enter");

        // The page's own BlockRenderer overrides MudShell's default tool-call card.
        await Assertions.Expect(Page.GetByTestId("chat-part-chart")).ToBeVisibleAsync(new() { Timeout = 30000 });
        await Assertions.Expect(Page.GetByTestId("chat-tool-call")).ToHaveCountAsync(0);
        await Assertions.Expect(Page.GetByTestId("chat-reasoning")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task Should_AskForApproval_And_ContinueAfterApprove()
    {
        await OpenAsync();
        var input = Page.GetByTestId("chat-input");
        await input.FillAsync("please delete it");
        await input.PressAsync("Enter");

        var approval = Page.GetByTestId("chat-approval");
        await Assertions.Expect(approval).ToBeVisibleAsync(new() { Timeout = 30000 });
        await Page.GetByTestId("chat-approve").ClickAsync();

        await Assertions.Expect(approval).ToHaveAttributeAsync("data-status", "approved");
        await Assertions.Expect(Page.Locator(".sc-ai-message--assistant")).ToContainTextAsync("report was deleted");
    }

    [Fact]
    public async Task Should_NotRunTool_When_ApprovalRejected()
    {
        await OpenAsync();
        var input = Page.GetByTestId("chat-input");
        await input.FillAsync("please delete it");
        await input.PressAsync("Enter");

        await Assertions.Expect(Page.GetByTestId("chat-approval")).ToBeVisibleAsync(new() { Timeout = 30000 });
        await Page.GetByTestId("chat-reject").ClickAsync();

        await Assertions.Expect(Page.Locator(".sc-ai-message--assistant")).ToContainTextAsync("nothing was deleted");
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
        await Assertions.Expect(panel.Locator(".sc-ai-message--user")).ToContainTextAsync("from the corner");

        await panel.PressAsync("Escape");
        await Assertions.Expect(panel).ToBeHiddenAsync();
        await Assertions.Expect(fab).ToBeVisibleAsync();

        // The conversation lives in the agent and the panel stays mounted: reopening shows it again.
        await fab.ClickAsync();
        await Assertions.Expect(panel.Locator(".sc-ai-message--user")).ToContainTextAsync("from the corner");
    }
}
