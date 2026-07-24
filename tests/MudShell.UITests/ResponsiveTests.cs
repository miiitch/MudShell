using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

/// <summary>
/// Verifies desktop shell behaviour.
/// </summary>
public class ResponsiveDesktopTests : PlaywrightTestBase
{
    protected override BrowserNewContextOptions ContextOptions => new()
    {
        BaseURL = BaseUrl,
        IgnoreHTTPSErrors = true,
        ViewportSize = new() { Width = 1280, Height = 800 },
    };

    [Fact]
    public Task Desktop_SidebarIsVisible() => RunAsync(nameof(Desktop_SidebarIsVisible), async () =>
    {
        await Page.GotoAsync("/");

        await Assertions.Expect(Page.Locator(".mbx-sidebar")).ToBeVisibleAsync();
    });

    [Fact]
    public Task Desktop_SidebarCollapsedWidth_StaysFixedOnResize() => RunAsync(nameof(Desktop_SidebarCollapsedWidth_StaysFixedOnResize), async () =>
    {
        await Page.GotoAsync("/");

        var sidebar = Page.Locator(".mbx-sidebar").First;
        await Assertions.Expect(sidebar).ToBeVisibleAsync();

        var widthBefore = await sidebar.EvaluateAsync<double>("el => el.getBoundingClientRect().width");
        Assert.InRange(widthBefore, 54, 58);

        await Page.SetViewportSizeAsync(720, 800);
        await Page.WaitForTimeoutAsync(150);

        var widthAfter = await sidebar.EvaluateAsync<double>("el => el.getBoundingClientRect().width");
        Assert.InRange(widthAfter, 54, 58);
    });

    [Fact]
    public Task Desktop_SidebarExpandedWidth_StaysFixedOnResize() => RunAsync(nameof(Desktop_SidebarExpandedWidth_StaysFixedOnResize), async () =>
    {
        await Page.GotoAsync("/");

        await Page.Locator(".mbx-nav-toggle-btn").First.ClickAsync();
        await Page.WaitForTimeoutAsync(200);

        var sidebar = Page.Locator(".mbx-sidebar").First;
        var widthBefore = await sidebar.EvaluateAsync<double>("el => el.getBoundingClientRect().width");
        Assert.InRange(widthBefore, 238, 242);

        await Page.SetViewportSizeAsync(900, 800);
        await Page.WaitForTimeoutAsync(150);

        var widthAfter = await sidebar.EvaluateAsync<double>("el => el.getBoundingClientRect().width");
        Assert.InRange(widthAfter, 238, 242);
    });

    [Fact]
    public Task Desktop_MainContent_OffsetMatchesCollapsedSidebarWidth() => RunAsync(nameof(Desktop_MainContent_OffsetMatchesCollapsedSidebarWidth), async () =>
    {
        await Page.GotoAsync("/");

        var marginLeft = await Page.Locator(".mud-main-content.mbx-main-outer")
            .EvaluateAsync<string>("el => window.getComputedStyle(el).marginLeft");

        Assert.Equal("56px", marginLeft);
    });

    [Fact]
    public Task Desktop_MainContent_RemainsVisible_WhenExpandedAndViewportNarrows() => RunAsync(nameof(Desktop_MainContent_RemainsVisible_WhenExpandedAndViewportNarrows), async () =>
    {
        await Page.GotoAsync("/");
        await Page.Locator(".mbx-nav-toggle-btn").First.ClickAsync();
        await Page.WaitForTimeoutAsync(200);

        await Page.SetViewportSizeAsync(820, 800);
        await Page.WaitForTimeoutAsync(150);

        var mainWidth = await Page.Locator(".mbx-main").EvaluateAsync<double>("el => el.getBoundingClientRect().width");
        Assert.True(mainWidth > 400, $"Expected .mbx-main width > 400px, got {mainWidth}px.");
    });

    [Fact]
    public Task Desktop_ContextPanel_NotHiddenBySidebar_WhenCollapsed() => RunAsync(nameof(Desktop_ContextPanel_NotHiddenBySidebar_WhenCollapsed), async () =>
    {
        await Page.GotoAsync("/library");

        var sidebar = Page.Locator(".mbx-sidebar").First;
        var panel = Page.Locator(".mbx-context-panel").First;
        await Assertions.Expect(panel).ToBeVisibleAsync();

        var sidebarRight = await sidebar.EvaluateAsync<double>("el => el.getBoundingClientRect().right");
        var panelLeft = await panel.EvaluateAsync<double>("el => el.getBoundingClientRect().left");

        Assert.True(panelLeft >= sidebarRight - 1, $"Context panel is overlapped: panelLeft={panelLeft}, sidebarRight={sidebarRight}");
    });

    [Fact]
    public Task Desktop_ContextPanel_NotHiddenBySidebar_WhenExpanded_AndAfterResize() => RunAsync(nameof(Desktop_ContextPanel_NotHiddenBySidebar_WhenExpanded_AndAfterResize), async () =>
    {
        await Page.GotoAsync("/library");
        await Page.Locator(".mbx-nav-toggle-btn").First.ClickAsync();
        await Page.WaitForTimeoutAsync(200);
        await Page.SetViewportSizeAsync(920, 800);
        await Page.WaitForTimeoutAsync(150);

        var sidebar = Page.Locator(".mbx-sidebar").First;
        var panel = Page.Locator(".mbx-context-panel").First;
        await Assertions.Expect(panel).ToBeVisibleAsync();

        var sidebarRight = await sidebar.EvaluateAsync<double>("el => el.getBoundingClientRect().right");
        var panelLeft = await panel.EvaluateAsync<double>("el => el.getBoundingClientRect().left");

        Assert.True(panelLeft >= sidebarRight - 1, $"Context panel is overlapped after resize: panelLeft={panelLeft}, sidebarRight={sidebarRight}");
    });
}
