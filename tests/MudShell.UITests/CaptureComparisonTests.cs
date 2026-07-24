using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

public class CaptureDesktopPagesTests : PlaywrightTestBase
{
    protected override BrowserNewContextOptions ContextOptions => new()
    {
        BaseURL = BaseUrl,
        IgnoreHTTPSErrors = true,
        ViewportSize = new() { Width = 1280, Height = 800 },
    };

    [Theory]
    [InlineData("/", "home.png")]
    [InlineData("/library", "library.png")]
    [InlineData("/demo", "components-demo.png")]
    public Task Capture_DesktopPage(string route, string fileName) => RunAsync($"{nameof(Capture_DesktopPage)}_{fileName}", async () =>
    {
        await CaptureRouteAsync(route, fileName);
    });

    [Fact]
    public Task Capture_SidebarExpanded() => RunAsync(nameof(Capture_SidebarExpanded), async () =>
    {
        await Page.GotoAsync("/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60_000 });
        await Assertions.Expect(Page.Locator(".mbx-nav-toggle-btn")).ToBeVisibleAsync();
        await Page.Locator(".mbx-nav-toggle-btn").First.ClickAsync();
        await Page.WaitForTimeoutAsync(350);

        await SaveScreenshotAsync("sidebar-expanded.png");
    });

    private async Task CaptureRouteAsync(string route, string fileName)
    {
        await Page.GotoAsync(route, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60_000 });
        await Assertions.Expect(Page.Locator(".mbx-shell")).ToBeVisibleAsync();
        await Page.WaitForTimeoutAsync(350);
        await SaveScreenshotAsync(fileName);
    }

    protected async Task SaveScreenshotAsync(string fileName)
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "screenshots", "current");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, fileName);
        await Page.ScreenshotAsync(new() { Path = path, FullPage = false });
    }
}
