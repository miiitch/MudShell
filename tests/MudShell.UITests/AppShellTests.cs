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

    [Fact]
    public Task Shell_ContextPanelIsOptionalAndVisibleOnLibrary() => RunAsync(nameof(Shell_ContextPanelIsOptionalAndVisibleOnLibrary), async () =>
    {
        await Page.GotoAsync("/");
        Assert.Equal(0, await Page.Locator(".mbx-context-panel").CountAsync());

        await Page.GotoAsync("/library");
        await Assertions.Expect(Page.Locator(".mbx-context-panel")).ToBeVisibleAsync();
    });

    [Fact]
    public Task Shell_PaletteMode_KeepsMainRoundedWithContextPanel() => RunAsync(nameof(Shell_PaletteMode_KeepsMainRoundedWithContextPanel), async () =>
    {
        await Page.GotoAsync("/library");

        await Assertions.Expect(Page.Locator(".mbx-context-panel")).ToBeVisibleAsync();

        // ToHaveCSSAsync retries until the value matches. Reading the computed style through
        // EvaluateAsync instead races the Blazor circuit's first interactive render: the handle
        // resolves against a node that is not attached yet and the evaluation yields "".
        await Assertions.Expect(Page.Locator(".mbx-main"))
            .ToHaveCSSAsync("border-top-left-radius", "0px");
        await Assertions.Expect(Page.Locator(".mbx-main"))
            .ToHaveCSSAsync("border-top-right-radius", "0px");

        // Asserted as an inequality, so ToHaveCSSAsync does not apply. The visibility assertion
        // above already guarantees the panel is attached before its style is read.
        var panelTopLeftRadius = await Page.Locator(".mbx-context-panel").EvaluateAsync<string>(
            "el => window.getComputedStyle(el).borderTopLeftRadius");

        Assert.NotEqual("0px", panelTopLeftRadius);
    });

    [Fact]
    public Task Shell_ImageMode_ContextPanelAndMainRenderAsExpected() => RunAsync(nameof(Shell_ImageMode_ContextPanelAndMainRenderAsExpected), async () =>
    {
        await Page.GotoAsync("/library/bg");

        await Assertions.Expect(Page.Locator(".mbx-shell.mbx-mode-image")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator(".mbx-context-panel")).ToBeVisibleAsync();
    });

    [Fact]
    public Task Shell_ContextPanelCanCollapseFromToggle() => RunAsync(nameof(Shell_ContextPanelCanCollapseFromToggle), async () =>
    {
        await Page.GotoAsync("/library");

        var panel = Page.Locator(".mbx-context-panel");
        await Assertions.Expect(panel).ToBeVisibleAsync();
        var isExpanded = await panel.EvaluateAsync<bool>("el => el.classList.contains('mbx-context-panel-expanded')");
        Assert.True(isExpanded);

        await Page.Locator(".mbx-context-panel .mud-icon-button").First.ClickAsync();

        var isCollapsed = await panel.EvaluateAsync<bool>("el => el.classList.contains('mbx-context-panel-collapsed')");
        Assert.True(isCollapsed);
    });
}
