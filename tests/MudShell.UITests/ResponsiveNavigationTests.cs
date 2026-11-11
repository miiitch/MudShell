using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

/// <summary>
/// Responsive navigation model: desktop (>= 1280), tablet (600-1279) and mobile (&lt; 600).
/// Pages with a level-2 context panel: /library, /demo, /components.
/// </summary>
public abstract class ResponsiveNavigationTestsBase : PlaywrightTestBase
{
    protected abstract int ViewportWidth { get; }
    protected abstract int ViewportHeight { get; }

    protected override BrowserNewContextOptions ContextOptions => new()
    {
        BaseURL = BaseUrl,
        IgnoreHTTPSErrors = true,
        ViewportSize = new() { Width = ViewportWidth, Height = ViewportHeight },
    };

    protected ILocator Shell => Page.GetByTestId("shell-mode");
    protected ILocator Overlay => Page.GetByTestId("context-overlay");
    protected ILocator Scrim => Page.GetByTestId("nav-scrim");
    protected ILocator Sheet => Page.GetByTestId("context-sheet");
    protected ILocator BottomNav => Page.GetByTestId("bottom-nav");

    /// <summary>Navigates and waits until the Blazor circuit reports the expected tier (proof of interactivity).</summary>
    protected async Task GotoAsync(string path, string expectedMode)
    {
        await Page.GotoAsync(path);
        await Assertions.Expect(Shell).ToHaveAttributeAsync("data-shell-mode", expectedMode);
    }

    protected Task<bool> HasHorizontalScrollAsync() =>
        Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > window.innerWidth + 1");

    protected Task<double> WidthOfAsync(string selector) =>
        Page.Locator(selector).First.EvaluateAsync<double>("el => el.getBoundingClientRect().width");
}

public class ResponsiveNavigationDesktopTests : ResponsiveNavigationTestsBase
{
    protected override int ViewportWidth => 1280;
    protected override int ViewportHeight => 800;

    [Fact]
    public Task Desktop_ShellModeIsDesktop_NoBottomNavNoScrim() => RunAsync(nameof(Desktop_ShellModeIsDesktop_NoBottomNavNoScrim), async () =>
    {
        await GotoAsync("/library", "desktop");

        Assert.Equal(0, await BottomNav.CountAsync());
        Assert.Equal(0, await Scrim.CountAsync());
        await Assertions.Expect(Page.GetByTestId("mobile-topbar")).ToBeHiddenAsync();
        await Assertions.Expect(Overlay).ToBeVisibleAsync();
        Assert.False(await HasHorizontalScrollAsync());
    });

    [Fact]
    public Task Desktop_SidebarWidth_Is56Collapsed_And240Expanded() => RunAsync(nameof(Desktop_SidebarWidth_Is56Collapsed_And240Expanded), async () =>
    {
        await GotoAsync("/", "desktop");
        await Page.WaitForFunctionAsync("() => document.querySelector('.mbx-sidebar')?.getBoundingClientRect().width > 0");
        Assert.InRange(await WidthOfAsync(".mbx-sidebar"), 54, 58);

        await Page.Locator(".mbx-nav-toggle-btn").First.ClickAsync();
        await Page.WaitForTimeoutAsync(300);
        Assert.InRange(await WidthOfAsync(".mbx-sidebar"), 238, 242);
    });

    [Fact]
    public Task Desktop_ContextPanel_IsInlineNotOverlay() => RunAsync(nameof(Desktop_ContextPanel_IsInlineNotOverlay), async () =>
    {
        await GotoAsync("/library", "desktop");

        await Assertions.Expect(Overlay).ToHaveCSSAsync("position", "relative");
        var sidebarRight = await Page.Locator(".mbx-sidebar").First.EvaluateAsync<double>("el => el.getBoundingClientRect().right");
        var panelLeft = await Overlay.EvaluateAsync<double>("el => el.getBoundingClientRect().left");
        Assert.True(panelLeft >= sidebarRight - 1, $"panelLeft={panelLeft}, sidebarRight={sidebarRight}");
    });
}

public class ResponsiveNavigationTabletTests : ResponsiveNavigationTestsBase
{
    protected override int ViewportWidth => 820;
    protected override int ViewportHeight => 1180;

    private ILocator RailToggle => Page.Locator(".mbx-nav-toggle-btn").First;

    private async Task OpenOverlayAsync()
    {
        await RailToggle.ClickAsync();
        await Assertions.Expect(Overlay).ToBeVisibleAsync();
        await Assertions.Expect(Scrim).ToBeVisibleAsync();
        await Page.WaitForTimeoutAsync(500); // let the 250ms slide-in transition settle before measuring
    }

    [Fact]
    public Task Tablet_ShellModeIsTablet_RailIs56_NoHorizontalScroll_NoBottomNav() => RunAsync(nameof(Tablet_ShellModeIsTablet_RailIs56_NoHorizontalScroll_NoBottomNav), async () =>
    {
        await GotoAsync("/library", "tablet");

        await Assertions.Expect(Page.Locator(".mbx-sidebar").First).ToBeVisibleAsync();
        Assert.InRange(await WidthOfAsync(".mbx-sidebar"), 54, 58);
        Assert.False(await HasHorizontalScrollAsync());
        Assert.Equal(0, await BottomNav.CountAsync());
        await Assertions.Expect(Page.GetByTestId("mobile-topbar")).ToBeHiddenAsync();
    });

    [Fact]
    public Task Tablet_RailStaysAt56_EvenAfterToggleClick() => RunAsync(nameof(Tablet_RailStaysAt56_EvenAfterToggleClick), async () =>
    {
        await GotoAsync("/", "tablet");

        await RailToggle.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        Assert.InRange(await WidthOfAsync(".mbx-sidebar"), 54, 58);
    });

    [Fact]
    public Task Tablet_ContextOverlay_ClosedByDefault_ContentNotCovered() => RunAsync(nameof(Tablet_ContextOverlay_ClosedByDefault_ContentNotCovered), async () =>
    {
        await GotoAsync("/library", "tablet");

        await Assertions.Expect(Overlay).ToBeHiddenAsync();
        Assert.Equal(0, await Scrim.CountAsync());

        var mainLeft = await Page.Locator(".mbx-main").EvaluateAsync<double>("el => el.getBoundingClientRect().left");
        Assert.InRange(mainLeft, 54, 60);
        var mainWidth = await Page.Locator(".mbx-main").EvaluateAsync<double>("el => el.getBoundingClientRect().width");
        Assert.True(mainWidth > 700, $"main width {mainWidth}");

        // The element at the centre of the content must belong to the page content, not an overlay.
        var coveredByNav = await Page.EvaluateAsync<bool>(
            "() => { const e = document.elementFromPoint(window.innerWidth / 2, 400); return !!e?.closest('[data-testid=\"context-overlay\"],[data-testid=\"nav-scrim\"]'); }");
        Assert.False(coveredByNav);
    });

    [Fact]
    public Task Tablet_RailMenuButton_OpensOverlay_WithScrim() => RunAsync(nameof(Tablet_RailMenuButton_OpensOverlay_WithScrim), async () =>
    {
        await GotoAsync("/library", "tablet");

        await OpenOverlayAsync();

        await Assertions.Expect(Overlay).ToHaveCSSAsync("position", "fixed");
        var box = await Overlay.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.InRange(box!.X, 54, 58);            // slides out from the rail's right edge
        Assert.True(box.X + box.Width <= 820 + 1);
        // Content keeps its width: the overlay floats above it.
        Assert.False(await HasHorizontalScrollAsync());
        Assert.InRange(await WidthOfAsync(".mbx-sidebar"), 54, 58);
    });

    [Fact]
    public Task Tablet_ScrimClick_ClosesOverlay() => RunAsync(nameof(Tablet_ScrimClick_ClosesOverlay), async () =>
    {
        await GotoAsync("/library", "tablet");
        await OpenOverlayAsync();

        // Click on the scrim area right of the 288px panel.
        await Page.Mouse.ClickAsync(800, 600);

        await Assertions.Expect(Overlay).ToBeHiddenAsync();
        await Assertions.Expect(Scrim).ToHaveCountAsync(0);
    });

    [Fact]
    public Task Tablet_Escape_ClosesOverlay() => RunAsync(nameof(Tablet_Escape_ClosesOverlay), async () =>
    {
        await GotoAsync("/library", "tablet");
        await OpenOverlayAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Assertions.Expect(Overlay).ToBeHiddenAsync();
        await Assertions.Expect(Scrim).ToHaveCountAsync(0);
    });

    [Fact]
    public Task Tablet_Navigation_ClosesOverlay() => RunAsync(nameof(Tablet_Navigation_ClosesOverlay), async () =>
    {
        await GotoAsync("/demo", "tablet");
        await OpenOverlayAsync();

        await Overlay.GetByTestId("nav-demo.render-modes").ClickAsync();

        await Page.WaitForURLAsync(new Regex("/modes$"));
        await Assertions.Expect(Overlay).ToBeHiddenAsync();
        await Assertions.Expect(Scrim).ToHaveCountAsync(0);
    });
}

public class ResponsiveNavigationMobileTests : ResponsiveNavigationTestsBase
{
    protected override int ViewportWidth => 375;
    protected override int ViewportHeight => 812;

    private ILocator Item(string id) => Page.GetByTestId($"nav-{id}");

    private async Task OpenLibrarySheetAsync()
    {
        await BottomNav.GetByTestId("nav-library").ClickAsync();
        await Assertions.Expect(Sheet).ToBeVisibleAsync();
        await Page.WaitForTimeoutAsync(500); // let the 250ms slide-in animation settle before measuring
    }

    [Fact]
    public Task Mobile_ShellModeIsMobile_NoRail_TopBarAndBottomNavVisible() => RunAsync(nameof(Mobile_ShellModeIsMobile_NoRail_TopBarAndBottomNavVisible), async () =>
    {
        await GotoAsync("/library", "mobile");

        await Assertions.Expect(Page.Locator(".mbx-sidebar").First).ToBeHiddenAsync();
        await Assertions.Expect(Page.GetByTestId("mobile-topbar")).ToBeVisibleAsync();
        await Assertions.Expect(Page.GetByTestId("mobile-menu-button")).ToBeVisibleAsync();
        await Assertions.Expect(BottomNav).ToBeVisibleAsync();
        await Assertions.Expect(Overlay).ToBeHiddenAsync(); // level 2 aside is replaced by the sheet
        await Assertions.Expect(Sheet).ToHaveCountAsync(0);
        Assert.False(await HasHorizontalScrollAsync());

        var bar = await BottomNav.BoundingBoxAsync();
        Assert.NotNull(bar);
        Assert.InRange(bar!.Y + bar.Height, 811, 813); // pinned to the bottom edge
    });

    [Theory]
    [InlineData("/")]
    [InlineData("/library")]
    [InlineData("/responsive-nav")]
    public Task Mobile_NoHorizontalPageScroll(string path) => RunAsync($"{nameof(Mobile_NoHorizontalPageScroll)}_{path.Trim('/')}", async () =>
    {
        await GotoAsync(path, "mobile");
        await Page.WaitForTimeoutAsync(300);

        Assert.False(await HasHorizontalScrollAsync());
    });

    [Fact]
    public Task Mobile_BottomNavItems_Have44pxTouchTargets() => RunAsync(nameof(Mobile_BottomNavItems_Have44pxTouchTargets), async () =>
    {
        await GotoAsync("/library", "mobile");

        var items = BottomNav.Locator(".mbx-bottom-nav-item");
        var count = await items.CountAsync();
        Assert.True(count >= 3, $"expected several bottom-nav items, got {count}");
        for (var i = 0; i < count; i++)
        {
            var box = await items.Nth(i).BoundingBoxAsync();
            Assert.NotNull(box);
            Assert.True(box!.Height >= 44, $"item {i} height {box.Height}");
            Assert.True(box.Width >= 44, $"item {i} width {box.Width}");
        }
    });

    [Fact]
    public Task Mobile_BottomNavItemWithChildren_OpensSheet_AboveBar_AtMost70vh() => RunAsync(nameof(Mobile_BottomNavItemWithChildren_OpensSheet_AboveBar_AtMost70vh), async () =>
    {
        await GotoAsync("/library", "mobile");

        await OpenLibrarySheetAsync();

        await Assertions.Expect(Scrim).ToBeVisibleAsync();
        await Assertions.Expect(Sheet.GetByTestId("nav-library.documents")).ToBeVisibleAsync();
        await Assertions.Expect(Sheet.GetByTestId("nav-library.history")).ToBeVisibleAsync();

        var sheet = await Sheet.BoundingBoxAsync();
        var bar = await BottomNav.BoundingBoxAsync();
        Assert.NotNull(sheet);
        Assert.NotNull(bar);
        Assert.True(sheet!.Height <= 812 * 0.70 + 1, $"sheet height {sheet.Height} exceeds 70vh");
        Assert.True(sheet.Y + sheet.Height <= bar!.Y + 1, $"sheet bottom {sheet.Y + sheet.Height} overlaps bar top {bar.Y}");
        Assert.InRange(sheet.Width, 374, 376);
    });

    [Fact]
    public Task Mobile_Sheet_DrillsDownToLevel3_AndBackButtonReturns() => RunAsync(nameof(Mobile_Sheet_DrillsDownToLevel3_AndBackButtonReturns), async () =>
    {
        await GotoAsync("/library", "mobile");
        await OpenLibrarySheetAsync();
        await Assertions.Expect(Page.GetByTestId("drilldown-back")).ToHaveCountAsync(0);

        await Sheet.GetByTestId("nav-library.documents").ClickAsync();

        await Assertions.Expect(Page.GetByTestId("drilldown-back")).ToBeVisibleAsync();
        await Assertions.Expect(Sheet.GetByTestId("nav-library.documents.folder1")).ToBeVisibleAsync();
        await Assertions.Expect(Sheet.GetByTestId("nav-library.documents.folder2")).ToBeVisibleAsync();
        await Assertions.Expect(Sheet.GetByTestId("nav-library.history")).ToHaveCountAsync(0);

        await Page.GetByTestId("drilldown-back").ClickAsync();

        await Assertions.Expect(Page.GetByTestId("drilldown-back")).ToHaveCountAsync(0);
        await Assertions.Expect(Sheet.GetByTestId("nav-library.history")).ToBeVisibleAsync();
        await Assertions.Expect(Sheet).ToBeVisibleAsync();
    });

    [Fact]
    public Task Mobile_LeafTap_Navigates_ClosesSheet_UpdatesUrl() => RunAsync(nameof(Mobile_LeafTap_Navigates_ClosesSheet_UpdatesUrl), async () =>
    {
        await GotoAsync("/library", "mobile");
        await BottomNav.GetByTestId("nav-demo").ClickAsync();
        await Assertions.Expect(Sheet).ToBeVisibleAsync();

        await Sheet.GetByTestId("nav-demo.render-modes").ClickAsync();

        await Page.WaitForURLAsync(new Regex("/modes$"));
        await Assertions.Expect(Sheet).ToHaveCountAsync(0);
        await Assertions.Expect(Scrim).ToHaveCountAsync(0);
        await Assertions.Expect(Shell).ToHaveAttributeAsync("data-shell-mode", "mobile");
    });

    [Fact]
    public Task Mobile_Escape_ClosesSheet() => RunAsync(nameof(Mobile_Escape_ClosesSheet), async () =>
    {
        await GotoAsync("/library", "mobile");
        await OpenLibrarySheetAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Assertions.Expect(Sheet).ToHaveCountAsync(0);
    });

    [Fact]
    public Task Mobile_ScrimTap_And_CloseButton_CloseSheet() => RunAsync(nameof(Mobile_ScrimTap_And_CloseButton_CloseSheet), async () =>
    {
        await GotoAsync("/library", "mobile");

        await OpenLibrarySheetAsync();
        await Page.Mouse.ClickAsync(187, 60); // scrim, above the sheet
        await Assertions.Expect(Sheet).ToHaveCountAsync(0);

        await OpenLibrarySheetAsync();
        await Page.GetByTestId("context-sheet-close").ClickAsync();
        await Assertions.Expect(Sheet).ToHaveCountAsync(0);
    });

    [Fact]
    public Task Mobile_TopBarMenuButton_OpensSheetListingLevel1() => RunAsync(nameof(Mobile_TopBarMenuButton_OpensSheetListingLevel1), async () =>
    {
        await GotoAsync("/", "mobile");

        await Page.GetByTestId("mobile-menu-button").ClickAsync();

        await Assertions.Expect(Sheet).ToBeVisibleAsync();
        await Assertions.Expect(Sheet.GetByTestId("nav-library")).ToBeVisibleAsync();
        await Assertions.Expect(Sheet.GetByTestId("nav-demo")).ToBeVisibleAsync();
        await Assertions.Expect(Sheet.GetByTestId("nav-components")).ToBeVisibleAsync();
    });

    [Fact]
    public Task Mobile_BottomNavLeafRoot_NavigatesWithoutSheet() => RunAsync(nameof(Mobile_BottomNavLeafRoot_NavigatesWithoutSheet), async () =>
    {
        await GotoAsync("/library", "mobile");

        await BottomNav.GetByTestId("nav-home").ClickAsync();

        await Page.WaitForURLAsync(new Regex(@"/$"));
        await Assertions.Expect(Sheet).ToHaveCountAsync(0);
    });

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Mobile_SheetAndBars_RenderInLightAndDarkMode(bool dark) => RunAsync($"{nameof(Mobile_SheetAndBars_RenderInLightAndDarkMode)}_{(dark ? "dark" : "light")}", async () =>
    {
        await GotoAsync("/library", "mobile");
        // No theme control is reachable on mobile (it lives in the rail); drive the shell's light class directly.
        // The class is re-applied only on shell re-render, which the sheet opening does not trigger for the root.
        await Page.EvaluateAsync($"() => document.querySelector('.mbx-shell').classList.toggle('mbx-light', {(dark ? "false" : "true")})");

        await OpenLibrarySheetAsync();

        var barBg = await BottomNav.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        var sheetBg = await Sheet.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        var textColor = await Sheet.GetByTestId("nav-library.history").EvaluateAsync<string>("el => getComputedStyle(el).color");
        Assert.False(string.IsNullOrEmpty(sheetBg) || sheetBg == "rgba(0, 0, 0, 0)", $"sheet background transparent: {sheetBg}");
        Assert.NotEqual(sheetBg, textColor); // text is readable against the sheet
        Assert.NotEqual("rgba(0, 0, 0, 0)", barBg);
        Assert.False(await HasHorizontalScrollAsync());
        await Assertions.Expect(Sheet.GetByTestId("nav-library.history")).ToBeVisibleAsync();
    });
}

public class ResponsiveNavigationResizeTests : ResponsiveNavigationTestsBase
{
    protected override int ViewportWidth => 1280;
    protected override int ViewportHeight => 800;

    [Fact]
    public Task LiveResize_CrossingBreakpoints_UpdatesShellMode() => RunAsync(nameof(LiveResize_CrossingBreakpoints_UpdatesShellMode), async () =>
    {
        await GotoAsync("/library", "desktop");

        await Page.SetViewportSizeAsync(820, 1180);
        await Assertions.Expect(Shell).ToHaveAttributeAsync("data-shell-mode", "tablet");
        Assert.InRange(await WidthOfAsync(".mbx-sidebar"), 54, 58);
        await Assertions.Expect(Overlay).ToBeHiddenAsync();

        await Page.SetViewportSizeAsync(375, 812);
        await Assertions.Expect(Shell).ToHaveAttributeAsync("data-shell-mode", "mobile");
        await Assertions.Expect(Page.GetByTestId("bottom-nav")).ToBeVisibleAsync();
        await Assertions.Expect(Page.GetByTestId("mobile-topbar")).ToBeVisibleAsync();
        Assert.False(await HasHorizontalScrollAsync());

        await Page.SetViewportSizeAsync(1280, 800);
        await Assertions.Expect(Shell).ToHaveAttributeAsync("data-shell-mode", "desktop");
        await Assertions.Expect(Page.GetByTestId("bottom-nav")).ToHaveCountAsync(0);
        await Assertions.Expect(Page.GetByTestId("mobile-topbar")).ToBeHiddenAsync();
        await Assertions.Expect(Page.Locator(".mbx-sidebar").First).ToBeVisibleAsync();
    });

    [Fact]
    public Task LiveResize_BreakpointEdges() => RunAsync(nameof(LiveResize_BreakpointEdges), async () =>
    {
        await GotoAsync("/", "desktop");

        await Page.SetViewportSizeAsync(1279, 800);
        await Assertions.Expect(Shell).ToHaveAttributeAsync("data-shell-mode", "tablet");
        await Page.SetViewportSizeAsync(600, 800);
        await Assertions.Expect(Shell).ToHaveAttributeAsync("data-shell-mode", "tablet");
        await Page.SetViewportSizeAsync(599, 800);
        await Assertions.Expect(Shell).ToHaveAttributeAsync("data-shell-mode", "mobile");
    });

    [Fact]
    public Task LiveResize_OpenTabletOverlay_ClosedWhenGoingMobile() => RunAsync(nameof(LiveResize_OpenTabletOverlay_ClosedWhenGoingMobile), async () =>
    {
        await GotoAsync("/library", "desktop");
        await Page.SetViewportSizeAsync(820, 1180);
        await Assertions.Expect(Shell).ToHaveAttributeAsync("data-shell-mode", "tablet");
        await Page.Locator(".mbx-nav-toggle-btn").First.ClickAsync();
        await Assertions.Expect(Page.GetByTestId("context-overlay")).ToBeVisibleAsync();

        await Page.SetViewportSizeAsync(375, 812);

        await Assertions.Expect(Shell).ToHaveAttributeAsync("data-shell-mode", "mobile");
        await Assertions.Expect(Page.GetByTestId("nav-scrim")).ToHaveCountAsync(0);
        await Assertions.Expect(Page.GetByTestId("context-sheet")).ToHaveCountAsync(0);
    });
}
