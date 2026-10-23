using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

/// <summary>
/// Basic smoke tests: shell renders, sidebar present, no Blazor error UI.
/// Key regression for the duplicate-provider section-ID crash.
/// </summary>
public class AppShellTests : PlaywrightTestBase
{
    [Theory]
    [InlineData(390)]
    [InlineData(820)]
    public Task ResponsiveComponents_WorkWithoutShowcaseStyles(int width) => RunAsync(
        $"{nameof(ResponsiveComponents_WorkWithoutShowcaseStyles)}_{width}", async () =>
    {
        await Page.SetViewportSizeAsync(width, 900);
        await Page.GotoAsync("/components/navigation", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Assertions.Expect(Page.Locator("link[rel='stylesheet'][href*='css/app']")).ToHaveCountAsync(0);

        var panel = Page.Locator(".mbx-context-panel");
        await Assertions.Expect(panel).ToHaveClassAsync(new Regex("mbx-context-panel-collapsed"));
        var main = Page.Locator(".mbx-main");
        var collapsedMain = await main.BoundingBoxAsync();
        await Page.GetByTestId("context-panel-toggle").ClickAsync();
        await Assertions.Expect(panel).ToHaveClassAsync(new Regex("mbx-context-panel-expanded"));
        await Assertions.Expect(panel).ToHaveCSSAsync("position", "absolute");
        await Assertions.Expect(Page.Locator(".mbx-page-header")).ToHaveCSSAsync("flex-wrap", "wrap");
        await Assertions.Expect(Page.Locator(".mbx-page-header")).ToHaveCSSAsync("gap", "8px");

        var expandedMain = await main.BoundingBoxAsync();
        Assert.True(expandedMain!.Width >= collapsedMain!.Width);
        foreach (var margin in new[] { 56, 120, 240 })
        {
            // Different offsets model custom and expanded sidebar widths.
            await Page.Locator(".mbx-main-outer").EvaluateAsync(
                "(el, margin) => el.style.setProperty('margin-left', `${margin}px`, 'important')", margin);
            await Assertions.Expect(Page.Locator(".mbx-main-outer")).ToHaveCSSAsync("margin-left", $"{margin}px");
            var contentBounds = await Page.Locator(".mbx-main-outer").BoundingBoxAsync();
            var panelBounds = await panel.BoundingBoxAsync();
            Assert.InRange(Math.Abs(panelBounds!.X - contentBounds!.X), 0, 1);
            Assert.True(panelBounds.Width <= contentBounds.Width + 1);
        }
    });

    [Fact]
    public Task ConnectionStatus_UsesLibraryAssetsAndHandlesReconnectStates() => RunAsync(
        nameof(ConnectionStatus_UsesLibraryAssetsAndHandlesReconnectStates), async () =>
    {
        await Page.GotoAsync("/", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var stylesheets = await Page.Locator("link[rel='stylesheet']").EvaluateAllAsync<string[]>(
            "links => links.map(link => link.getAttribute('href'))");
        Assert.NotEmpty(stylesheets);
        Assert.All(stylesheets, href => Assert.True(
            href.StartsWith("_content/", StringComparison.Ordinal)
            || href.StartsWith("https://fonts.googleapis.com/", StringComparison.Ordinal),
            $"Unexpected application stylesheet: {href}"));
        foreach (var href in stylesheets.Where(href => href.StartsWith("_content/", StringComparison.Ordinal)))
        {
            var response = await Page.APIRequest.GetAsync(href);
            Assert.True(response.Ok, $"Stylesheet failed to load: {href}");
        }

        var dialog = Page.Locator("#components-reconnect-modal");
        await Assertions.Expect(dialog).ToBeHiddenAsync();
        await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
        await dialog.EvaluateAsync("""
            el => {
                el.classList.add('components-reconnect-show');
                el.dispatchEvent(new CustomEvent('components-reconnect-state-changed', { detail: { state: 'show' } }));
            }
            """);
        await Assertions.Expect(dialog).ToBeVisibleAsync();
        await dialog.EvaluateAsync("""
            el => {
                el.classList.replace('components-reconnect-show', 'components-reconnect-failed');
                el.dispatchEvent(new CustomEvent('components-reconnect-state-changed', { detail: { state: 'failed' } }));
            }
            """);
        await Assertions.Expect(Page.Locator("#components-reconnect-button")).ToBeVisibleAsync();
        await dialog.EvaluateAsync("""
            el => {
                el.classList.replace('components-reconnect-failed', 'components-reconnect-hide');
                el.dispatchEvent(new CustomEvent('components-reconnect-state-changed', { detail: { state: 'hide' } }));
            }
            """);
        await Assertions.Expect(dialog).ToBeHiddenAsync();
    });

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

        await Page.GetByTestId("context-panel-toggle").ClickAsync();

        // ToHaveClassAsync retries until the class appears. A one-shot EvaluateAsync right after
        // the click can race the Blazor Server round trip that applies the collapsed class.
        await Assertions.Expect(panel).ToHaveClassAsync(new Regex("mbx-context-panel-collapsed"));
    });
}
