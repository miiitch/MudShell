using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

/// <summary>
/// Verifies MdsAppShell responsive behaviour.
/// Breakpoint: ≤ 959 px → sidebar hidden, bottom-nav visible.
///             ≥ 960 px → sidebar visible, bottom-nav slot hidden.
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
    public Task Desktop_BottomNavSlotIsHidden() => RunAsync(nameof(Desktop_BottomNavSlotIsHidden), async () =>
    {
        await Page.GotoAsync("/");

        await Assertions.Expect(Page.Locator(".mbx-bottom-nav-slot")).ToBeHiddenAsync();
    });
}

/// <summary>
/// Mobile viewport (390 × 844 — iPhone 14).
/// </summary>
public class ResponsiveMobileTests : PlaywrightTestBase
{
    protected override BrowserNewContextOptions ContextOptions => new()
    {
        BaseURL = BaseUrl,
        IgnoreHTTPSErrors = true,
        ViewportSize = new() { Width = 390, Height = 844 },
    };

    [Fact]
    public Task Mobile_SidebarIsHidden() => RunAsync(nameof(Mobile_SidebarIsHidden), async () =>
    {
        await Page.GotoAsync("/");

        await Assertions.Expect(Page.Locator(".mbx-sidebar")).ToBeHiddenAsync();
    });

    [Fact]
    public Task Mobile_BottomNavSlotIsVisible() => RunAsync(nameof(Mobile_BottomNavSlotIsVisible), async () =>
    {
        await Page.GotoAsync("/");

        await Assertions.Expect(Page.Locator(".mbx-bottom-nav-slot")).ToBeVisibleAsync();
    });

    [Fact]
    public Task Mobile_MainContentAreaIsFullWidth() => RunAsync(nameof(Mobile_MainContentAreaIsFullWidth), async () =>
    {
        await Page.GotoAsync("/");
        await Assertions.Expect(Page.Locator(".mbx-main")).ToBeVisibleAsync();

        // On mobile, .mbx-main has margin:0 and border-radius:0 — it should fill the viewport width.
        var box = await Page.Locator(".mbx-main").BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.Equal(390, (int)box.Width);
    });
}
