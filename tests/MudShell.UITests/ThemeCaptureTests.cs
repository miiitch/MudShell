using Microsoft.Playwright;
using MudShell.Theme;
using Xunit;

namespace MudShell.UITests;

/// <summary>
/// Captures every theme preset in light and dark mode (via <c>?preset=&amp;mode=</c>)
/// into <c>screenshots/themes/</c> for visual contrast review.
/// </summary>
[Trait("Category", "ThemeCapture")]
public class ThemeCaptureTests : PlaywrightTestBase
{
    protected override BrowserNewContextOptions ContextOptions => new()
    {
        BaseURL = BaseUrl,
        IgnoreHTTPSErrors = true,
        ViewportSize = new() { Width = 1280, Height = 800 },
    };

    public static TheoryData<string, string> PresetModes()
    {
        var data = new TheoryData<string, string>();
        foreach (var preset in MbxTheme.Presets)
        {
            data.Add(preset.Preset.ToString(), "light");
            data.Add(preset.Preset.ToString(), "dark");
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(PresetModes))]
    public Task Capture_Theme(string preset, string mode) => RunAsync($"{nameof(Capture_Theme)}_{preset}_{mode}", async () =>
    {
        foreach (var (route, suffix) in new[] { ("/", "home"), ("/demo", "demo") })
        {
            await Page.GotoAsync($"{route}?preset={preset}&mode={mode}",
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60_000 });
            await Assertions.Expect(Page.Locator(".mbx-shell")).ToBeVisibleAsync();
            await Page.WaitForTimeoutAsync(500);

            var dir = Path.Combine(Directory.GetCurrentDirectory(), "screenshots", "themes");
            Directory.CreateDirectory(dir);
            await Page.ScreenshotAsync(new() { Path = Path.Combine(dir, $"{preset}-{mode}-{suffix}.png") });
        }
    });
}
