using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

/// <summary>
/// Basic smoke tests: shell renders, sidebar present, no Blazor error UI.
/// Key regression for the duplicate-provider section-ID crash.
/// </summary>
public class AppShellTests : PlaywrightTestBase
{
    [Fact]
    public Task HomePage_RendersShellWithoutErrors() => RunAsync(nameof(HomePage_RendersShellWithoutErrors), async () =>
    {
        await Page.GotoAsync("/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Assertions.Expect(Page.Locator(".mbx-shell")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
    });

    [Fact]
    public Task Shell_SidebarIsPresent() => RunAsync(nameof(Shell_SidebarIsPresent), async () =>
    {
        await Page.GotoAsync("/");

        await Assertions.Expect(Page.Locator(".mbx-sidebar")).ToBeVisibleAsync();
    });

    [Fact]
    public Task Shell_MainContentAreaIsPresent() => RunAsync(nameof(Shell_MainContentAreaIsPresent), async () =>
    {
        await Page.GotoAsync("/");

        await Assertions.Expect(Page.Locator(".mbx-main")).ToBeVisibleAsync();
    });
}
