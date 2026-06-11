using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

/// <summary>
/// Base class for all Playwright tests.
/// Manages browser lifecycle and captures a full-page screenshot on any test failure.
/// </summary>
public abstract class PlaywrightTestBase : IAsyncLifetime
{
    protected static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("PLAYWRIGHT_TEST_BASE_URL") ?? "http://localhost:5265";

    // Screenshots land in {repo-root}/screenshots/ when tests run via `dotnet test`
    private static readonly string ScreenshotsDir =
        Path.Combine(Directory.GetCurrentDirectory(), "screenshots");

    protected IPlaywright Playwright = null!;
    protected IBrowser Browser = null!;
    protected IBrowserContext Context = null!;
    protected IPage Page = null!;

    protected virtual BrowserNewContextOptions ContextOptions => new()
    {
        BaseURL = BaseUrl,
        IgnoreHTTPSErrors = true,
    };

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(ScreenshotsDir);
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new() { Headless = true });
        Context = await Browser.NewContextAsync(ContextOptions);
        Page = await Context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }

    /// <summary>
    /// Runs <paramref name="test"/> and captures a full-page screenshot if it throws.
    /// The file is saved as screenshots/{testName}_{timestamp}.png.
    /// </summary>
    protected async Task RunAsync(string testName, Func<Task> test)
    {
        try
        {
            await test();
        }
        catch
        {
            var path = Path.Combine(
                ScreenshotsDir,
                $"{testName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");

            await Page.ScreenshotAsync(new() { Path = path, FullPage = true });
            throw;
        }
    }
}
