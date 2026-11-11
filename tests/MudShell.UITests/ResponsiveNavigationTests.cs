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

        await Assertions.Expect(Page).ToHaveURLAsync(new Regex("/modes$"), new() { Timeout = 60_000 });
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

        // Assert on the URL only: /modes is a heavy render-mode page whose "load" event can exceed
        // the default navigation timeout on slow CI runners.
        await Assertions.Expect(Page).ToHaveURLAsync(new Regex("/modes$"), new() { Timeout = 60_000 });
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

/// <summary>
/// Regression: the leaf bottom-nav entry is a NavLink-rendered &lt;a&gt; (no Blazor CSS-isolation scope attribute),
/// so it used to be laid out differently from the &lt;button&gt; entries. Failure messages name the element type.
/// </summary>
public class ResponsiveNavigationBottomBarConsistencyTests : ResponsiveNavigationTestsBase
{
    protected override int ViewportWidth => 375;
    protected override int ViewportHeight => 812;

    private const string ItemMetricsScript = @"() => [...document.querySelector('[data-testid=bottom-nav]').children].map(el => {
        const r = el.getBoundingClientRect();
        const cs = getComputedStyle(el);
        const icon = el.querySelector('svg, .mud-icon-root');
        const ir = icon?.getBoundingClientRect();
        return {
            tag: el.tagName.toLowerCase(), testid: el.getAttribute('data-testid'),
            x: r.x, w: r.width, h: r.height, right: r.right,
            iw: ir ? ir.width : -1, ih: ir ? ir.height : -1,
            display: cs.display, flexDirection: cs.flexDirection, alignItems: cs.alignItems,
            textDecoration: cs.textDecorationLine, minHeight: parseFloat(cs.minHeight),
            bg: cs.backgroundColor, color: cs.color,
        };
    })";

    private record Item(string Tag, string Testid, double X, double W, double H, double Right,
        double Iw, double Ih, string Display, string FlexDirection, string AlignItems,
        string TextDecoration, double MinHeight, string Bg, string Color);

    private async Task<List<Item>> MeasureAsync(string path, string expectedTag)
    {
        await GotoAsync(path, "mobile");
        await Assertions.Expect(BottomNav).ToBeVisibleAsync();
        await Page.WaitForTimeoutAsync(400); // transitions on colour/background
        var json = await Page.EvaluateAsync<System.Text.Json.JsonElement>(ItemMetricsScript);
        var items = json.EnumerateArray().Select(e => new Item(
            e.GetProperty("tag").GetString()!, e.GetProperty("testid").GetString()!,
            e.GetProperty("x").GetDouble(), e.GetProperty("w").GetDouble(), e.GetProperty("h").GetDouble(),
            e.GetProperty("right").GetDouble(), e.GetProperty("iw").GetDouble(), e.GetProperty("ih").GetDouble(),
            e.GetProperty("display").GetString()!, e.GetProperty("flexDirection").GetString()!,
            e.GetProperty("alignItems").GetString()!, e.GetProperty("textDecoration").GetString()!,
            e.GetProperty("minHeight").GetDouble(), e.GetProperty("bg").GetString()!, e.GetProperty("color").GetString()!)).ToList();
        Assert.True(items.Count >= 4, $"expected >= 4 bottom-nav items, got {items.Count}");
        Assert.Contains(items, i => i.Tag == "a");      // leaf entry (Home) is a NavLink anchor
        Assert.Contains(items, i => i.Tag == "button"); // entries with children are buttons
        return items;
    }

    private static string Describe(IEnumerable<Item> items) =>
        string.Join("; ", items.Select(i => $"<{i.Tag}> {i.Testid} x={i.X:0.#} w={i.W:0.#} h={i.H:0.#}"));

    [Theory]
    [InlineData("/library")] // active root is a <button> (has children)
    [InlineData("/")]        // active root is the leaf <a>
    public Task BottomBar_AllItems_SameSize_EvenlySpaced_SpanBar_AnchorVsButton(string path) => RunAsync($"{nameof(BottomBar_AllItems_SameSize_EvenlySpaced_SpanBar_AnchorVsButton)}_{path.Trim('/')}", async () =>
    {
        var items = await MeasureAsync(path, "a");
        var d = Describe(items);

        var refItem = items.First(i => i.Tag == "button");
        foreach (var i in items)
        {
            Assert.True(Math.Abs(i.W - refItem.W) <= 1, $"<{i.Tag}> '{i.Testid}' width {i.W:0.#} differs from <button> width {refItem.W:0.#}. {d}");
            Assert.True(Math.Abs(i.H - refItem.H) <= 1, $"<{i.Tag}> '{i.Testid}' height {i.H:0.#} differs from <button> height {refItem.H:0.#}. {d}");
        }

        var ordered = items.OrderBy(i => i.X).ToList();
        for (var k = 1; k < ordered.Count; k++)
        {
            var gap = ordered[k].X - ordered[k - 1].Right;
            Assert.InRange(gap, -2, 10);
            Assert.True(Math.Abs(gap - (ordered[1].X - ordered[0].Right)) <= 2, $"uneven spacing before <{ordered[k].Tag}> '{ordered[k].Testid}'. {d}");
        }

        // Items span the bar (bar is 375 wide with 8px padding).
        Assert.InRange(ordered[0].X, 0, 16);
        Assert.InRange(ordered[^1].Right, 375 - 16, 375);
    });

    [Theory]
    [InlineData("/library")]
    [InlineData("/")]
    public Task BottomBar_Icons_HaveIdenticalSize_AnchorVsButton(string path) => RunAsync($"{nameof(BottomBar_Icons_HaveIdenticalSize_AnchorVsButton)}_{path.Trim('/')}", async () =>
    {
        var items = await MeasureAsync(path, "a");
        var refItem = items.First(i => i.Tag == "button");
        Assert.True(refItem.Iw > 0, "button icon not found");
        foreach (var i in items)
        {
            Assert.True(i.Iw > 0, $"<{i.Tag}> '{i.Testid}' has no icon");
            Assert.True(Math.Abs(i.Iw - refItem.Iw) <= 0.5 && Math.Abs(i.Ih - refItem.Ih) <= 0.5,
                $"<{i.Tag}> '{i.Testid}' icon {i.Iw:0.#}x{i.Ih:0.#} differs from <button> icon {refItem.Iw:0.#}x{refItem.Ih:0.#}");
        }
    });

    [Theory]
    [InlineData("/library")]
    [InlineData("/")]
    public Task BottomBar_AnchorComputedStyle_MatchesButton(string path) => RunAsync($"{nameof(BottomBar_AnchorComputedStyle_MatchesButton)}_{path.Trim('/')}", async () =>
    {
        var items = await MeasureAsync(path, "a");
        var btn = items.First(i => i.Tag == "button");
        foreach (var a in items.Where(i => i.Tag == "a"))
        {
            Assert.True(a.Display == btn.Display, $"<a> display '{a.Display}' != <button> '{btn.Display}'");
            Assert.True(a.FlexDirection == btn.FlexDirection, $"<a> flex-direction '{a.FlexDirection}' != <button> '{btn.FlexDirection}'");
            Assert.True(a.AlignItems == btn.AlignItems, $"<a> align-items '{a.AlignItems}' != <button> '{btn.AlignItems}'");
            Assert.True(a.TextDecoration == "none", $"<a> text-decoration-line is '{a.TextDecoration}', expected none");
            Assert.True(a.MinHeight >= 44, $"<a> min-height {a.MinHeight} < 44");
        }
        foreach (var b in items.Where(i => i.Tag == "button"))
            Assert.True(b.MinHeight >= 44, $"<button> '{b.Testid}' min-height {b.MinHeight} < 44");
        Assert.Equal("flex", btn.Display);
        Assert.Equal("column", btn.FlexDirection);
    });

    [Fact]
    public Task BottomBar_ActiveState_Anchor_MatchesActiveButton() => RunAsync(nameof(BottomBar_ActiveState_Anchor_MatchesActiveButton), async () =>
    {
        var onLeaf = await MeasureAsync("/", "a");
        var activeAnchor = onLeaf.First(i => i.Testid == "nav-home");
        var inactiveButton = onLeaf.First(i => i.Tag == "button");
        Assert.Equal("a", activeAnchor.Tag);

        var onButton = await MeasureAsync("/library", "a");
        var activeButton = onButton.First(i => i.Testid == "nav-library");
        Assert.Equal("button", activeButton.Tag);

        Assert.True(activeAnchor.Bg == activeButton.Bg, $"active <a> background '{activeAnchor.Bg}' != active <button> '{activeButton.Bg}'");
        Assert.True(activeAnchor.Color == activeButton.Color, $"active <a> colour '{activeAnchor.Color}' != active <button> '{activeButton.Color}'");
        // And it really is different from an inactive entry.
        Assert.True(activeAnchor.Color != inactiveButton.Color, $"active <a> colour '{activeAnchor.Color}' equals inactive <button> colour");
        Assert.True(activeAnchor.Bg != inactiveButton.Bg, $"active <a> background '{activeAnchor.Bg}' equals inactive <button> background");
    });
}

/// <summary>Guard: no blank strip on the left; content starts right of the rail (tablet) or at the edge (mobile).</summary>
public class ResponsiveNavigationLayoutGuardTests : ResponsiveNavigationTestsBase
{
    protected override int ViewportWidth => 375;
    protected override int ViewportHeight => 812;

    private async Task AssertContentStartsAtAsync(string path, string mode, int vw, int offset)
    {
        await GotoAsync(path, mode);
        await Page.WaitForTimeoutAsync(400);

        foreach (var sel in new[] { ".mbx-main-outer", ".mbx-main", ".mbx-main-inner" })
        {
            var box = await Page.Locator(sel).First.BoundingBoxAsync();
            Assert.NotNull(box);
            Assert.True(Math.Abs(box!.X - offset) <= 0.5, $"{sel} x={box.X} at {path} ({mode}), expected {offset}");
            Assert.True(Math.Abs(box.Width - (vw - offset)) <= 1, $"{sel} width={box.Width} at {path} ({mode}), expected {vw - offset}");
        }

        var heading = await Page.EvaluateAsync<System.Text.Json.JsonElement>("() => { const h = document.querySelector('.mbx-main-inner h1, .mbx-main-inner h2, .mbx-main-inner h3, .mbx-main-inner .mud-typography-h1, .mbx-main-inner .mud-typography-h2, .mbx-main-inner .mud-typography-h3, .mbx-main-inner .mud-typography-h4, .mbx-main-inner .mud-typography-h5, .mbx-main-inner .mud-typography-h6'); if (!h) return { x: -1, chain: '' }; const chain = []; for (let e = h; e && !e.classList.contains('mbx-main-inner'); e = e.parentElement) chain.push(e.tagName + '.' + String(e.className).split(' ')[0] + '@' + Math.round(e.getBoundingClientRect().x)); return { x: h.getBoundingClientRect().x, chain: chain.join(' < ') }; }");
        var headingX = heading.GetProperty("x").GetDouble();
        Assert.True(headingX >= 0, $"no heading found on {path}");
        Assert.True(headingX <= offset + 48, $"first heading at x={headingX} on {path} ({mode}), expected <= {offset + 48} (container 16 + toolbar 16 + gap 12 = 44 is the page-header design). Ancestors: {heading.GetProperty("chain").GetString()}");

        var hit = await Page.EvaluateAsync<string>("() => { const e = document.elementFromPoint(10, 300); return e ? (e.closest('.mbx-sidebar, .mud-drawer, aside') ? 'chrome:' + e.className : 'content') : 'none'; }");
        if (offset == 0)
            Assert.True(hit == "content", $"element at (10,300) on {path} is nav chrome: {hit}");

        var scroll = await Page.EvaluateAsync<double[]>("() => [document.documentElement.scrollWidth, window.innerWidth]");
        Assert.Equal(scroll[1], scroll[0]);
    }

    [Theory]
    [InlineData("/library")]
    [InlineData("/responsive-nav")]
    public Task Mobile_NoBlankStripOnLeft(string path) => RunAsync($"{nameof(Mobile_NoBlankStripOnLeft)}_{path.Trim('/')}", () =>
        AssertContentStartsAtAsync(path, "mobile", 375, 0));

    [Theory]
    [InlineData("/library")]
    [InlineData("/responsive-nav")]
    public Task Tablet_ContentStartsExactlyAtRail(string path) => RunAsync($"{nameof(Tablet_ContentStartsExactlyAtRail)}_{path.Trim('/')}", async () =>
    {
        await Page.SetViewportSizeAsync(820, 1180);
        await AssertContentStartsAtAsync(path, "tablet", 820, 56);
        var rail = await Page.Locator(".mbx-sidebar").First.BoundingBoxAsync();
        Assert.InRange(rail!.Width, 55.5, 56.5);
    });
}
