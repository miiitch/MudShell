using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

public class AppShellTests : IAsyncLifetime
{
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("PLAYWRIGHT_TEST_BASE_URL") ?? "http://localhost:5265";

    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
        _context = await _browser.NewContextAsync(new()
        {
            BaseURL = BaseUrl,
            IgnoreHTTPSErrors = true,
        });
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    /// <summary>
    /// Smoke test: the shell renders and Blazor reports no unhandled error.
    /// This is the key regression test for the duplicate-provider bug.
    /// </summary>
    [Fact]
    public async Task HomePage_RendersShellWithoutErrors()
    {
        await _page.GotoAsync("/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Assertions.Expect(_page.Locator(".mbx-shell")).ToBeVisibleAsync();
        await Assertions.Expect(_page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
    }

    [Fact]
    public async Task Shell_SidebarIsPresent()
    {
        await _page.GotoAsync("/");

        await Assertions.Expect(_page.Locator(".mbx-sidebar")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Shell_MainContentAreaIsPresent()
    {
        await _page.GotoAsync("/");

        await Assertions.Expect(_page.Locator(".mbx-main")).ToBeVisibleAsync();
    }
}
