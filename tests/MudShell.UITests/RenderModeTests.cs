using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

/// <summary>
/// Verifies that MbxAppShell renders correctly under each Blazor render mode.
/// Each page uses DemoLayout which wraps content in MbxAppShell.
/// </summary>
public class RenderModeTests : PlaywrightTestBase
{
    [Fact]
    public Task StaticSSR_ShellRendersWithoutErrors() => RunAsync(nameof(StaticSSR_ShellRendersWithoutErrors), async () =>
    {
        await Page.GotoAsync("/modes/ssr", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Assertions.Expect(Page.Locator(".mbx-shell")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
    });

    [Fact]
    public Task InteractiveServer_ShellRendersWithoutErrors() => RunAsync(nameof(InteractiveServer_ShellRendersWithoutErrors), async () =>
    {
        await Page.GotoAsync("/modes/server", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Assertions.Expect(Page.Locator(".mbx-shell")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
    });

    [Fact]
    public Task WebAssembly_ShellRendersWithoutErrors() => RunAsync(nameof(WebAssembly_ShellRendersWithoutErrors), async () =>
    {
        // WASM requires downloading the .NET runtime — allow extra time.
        await Page.GotoAsync("/modes/wasm", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60_000,
        });

        await Assertions.Expect(Page.Locator(".mbx-shell")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
    });

    [Fact]
    public Task AutoRenderMode_ShellRendersWithoutErrors() => RunAsync(nameof(AutoRenderMode_ShellRendersWithoutErrors), async () =>
    {
        // Auto starts as Server then may switch to WASM — allow extra time.
        await Page.GotoAsync("/modes/auto", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60_000,
        });

        await Assertions.Expect(Page.Locator(".mbx-shell")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
    });
}
