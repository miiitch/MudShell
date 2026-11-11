using System.Text.Json;
using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

/// <summary>
/// Regression tests for the responsive page components (MdsPageHeader, main blocks, block gap token, gutters)
/// and the bottom bar on WASM layout pages. All assertions measure the settled DOM; no screenshots are compared.
/// </summary>
public class ResponsivePageComponentsTests : PlaywrightTestBase
{
    private static readonly string[] AllRoutes =
    [
        "/", "/components", "/demo", "/density", "/europe-cities", "/fill-demo", "/fill-demo/detail",
        "/library", "/library/bg", "/main-components", "/main-components/bg", "/meteo", "/modes", "/modes/ssr",
        "/modes/ssr/bg", "/modes/server", "/modes/server/grid", "/modes/server/table", "/modes/wasm",
        "/modes/auto", "/responsive-nav", "/scheduler",
    ];

    private static readonly string[] HeaderRoutes = ["/main-components", "/demo", "/modes/ssr/bg", "/library/bg"];

    private static string ModeFor(int width) => width >= 1280 ? "desktop" : width >= 600 ? "tablet" : "mobile";

    protected override BrowserNewContextOptions ContextOptions => new()
    {
        BaseURL = BaseUrl,
        IgnoreHTTPSErrors = true,
        ViewportSize = new() { Width = 1280, Height = 800 },
    };

    /// <summary>Navigates and waits until the page is settled: interactive shell, network idle, plus 500ms.</summary>
    private async Task SettleAsync(string path, int width, int height = 900)
    {
        await Page.SetViewportSizeAsync(width, height);
        await Page.GotoAsync(path, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Assertions.Expect(Page.GetByTestId("shell-mode")).ToHaveAttributeAsync("data-shell-mode", ModeFor(width));
        await Page.WaitForSelectorAsync(".mbx-main-inner");
        await Page.WaitForTimeoutAsync(500);
    }

    // Detector: elements inside .mbx-main-inner whose right edge exceeds the nearest clipping ancestor
    // (or the viewport), ignoring anything inside an overflow-x auto/scroll container and fixed overlays.
    private const string ClippedDetector = @"() => {
        const root = document.querySelector('.mbx-main-inner');
        const out = [];
        const vw = window.innerWidth;
        for (const el of root.querySelectorAll('*')) {
            if (el.closest('svg') && el.tagName.toLowerCase() !== 'svg') continue;
            const r = el.getBoundingClientRect();
            if (r.width <= 1 || r.height <= 1) continue;
            const cs = getComputedStyle(el);
            if (cs.visibility === 'hidden' || cs.display === 'none' || cs.position === 'fixed') continue;
            let scrolls = false, clipRight = vw;
            for (let a = el.parentElement; a; a = a.parentElement) {
                if (a === root) break; // the root is the page-level clip boundary, handled below
                const acs = getComputedStyle(a);
                if (a.matches('.mud-tabs-tabbar') || acs.position === 'fixed' || acs.overflowX === 'auto' || acs.overflowX === 'scroll') { scrolls = true; break; }
                if (acs.overflowX === 'hidden' || acs.overflowX === 'clip') clipRight = Math.min(clipRight, a.getBoundingClientRect().right);
            }
            if (scrolls) continue;
            // The root itself is the clip boundary even when it is overflow:visible: nothing may poke out of the page.
            clipRight = Math.min(clipRight, root.getBoundingClientRect().right);
            if (r.right > clipRight + 1) out.push(el.tagName.toLowerCase() + '.' + String(el.className).split(' ').slice(0, 2).join('.') + ' right=' + Math.round(r.right) + ' clip=' + Math.round(clipRight));
        }
        return out;
    }";

    // Top-level blocks of the page: children of .mbx-main-inner, flattening containers, stacks and fill slots.
    private const string BlocksFn = @"
        const flattenSel = '.mud-container, .mbx-stack, [class*=""mbx-fill""]';
        const blocks = [];
        const walk = (parent) => {
            for (const c of parent.children) {
                const cs = getComputedStyle(c);
                if (['SCRIPT','STYLE','TEMPLATE'].includes(c.tagName) || cs.display === 'none' || cs.position === 'fixed' || cs.position === 'absolute') continue;
                const r = c.getBoundingClientRect();
                if (r.height === 0 && r.width === 0) continue;
                if (c.matches(flattenSel) && c.children.length) walk(c); else blocks.push(c);
            }
        };
        walk(document.querySelector('.mbx-main-inner'));";

    private async Task<string[]> ClippedAsync() => await Page.EvaluateAsync<string[]>(ClippedDetector);

    // ── 3. No clipped content ────────────────────────────────────────────────

    [Fact]
    public Task ClippedContentDetector_FlagsInjectedWideElement_PositiveControl() => RunAsync(nameof(ClippedContentDetector_FlagsInjectedWideElement_PositiveControl), async () =>
    {
        foreach (var width in new[] { 375, 820 })
        {
            await SettleAsync("/library", width);
            await Page.EvaluateAsync("() => { const d = document.createElement('div'); d.id = 'wide-control'; d.style.cssText = 'width:2000px;height:20px;background:red'; document.querySelector('.mbx-main-inner').appendChild(d); }");
            var offenders = await ClippedAsync();
            Assert.True(offenders.Any(o => o.StartsWith("div.")), $"detector did not flag the 2000px control at {width}px: [{string.Join(", ", offenders)}]");
        }
    });

    [Theory]
    [InlineData(375)]
    [InlineData(820)]
    public Task NoContentClippedAtRight_OnEveryDemoRoute(int width) => RunAsync($"{nameof(NoContentClippedAtRight_OnEveryDemoRoute)}_{width}", async () =>
    {
        var failures = new List<string>();
        foreach (var route in AllRoutes)
        {
            await SettleAsync(route, width);
            var offenders = await ClippedAsync();
            if (offenders.Length > 0) failures.Add($"{route} @ {width}: {string.Join(" | ", offenders.Take(6))}");
        }
        Assert.True(failures.Count == 0, "Content clipped at right:\n" + string.Join("\n", failures));
    });

    // ── 1. Page header height ────────────────────────────────────────────────

    [Fact]
    public Task PageHeader_GrowsWithWrappedTitle_NoOverlapWithNextBlock_Mobile() => RunAsync(nameof(PageHeader_GrowsWithWrappedTitle_NoOverlapWithNextBlock_Mobile), async () =>
    {
        var failures = new List<string>();
        foreach (var route in HeaderRoutes)
        {
            await SettleAsync(route, 375, 812);
            var json = await Page.EvaluateAsync<JsonElement>(@"() => {" + BlocksFn + @"
                const h = document.querySelector('.mbx-page-header');
                if (!h) return { missing: true };
                const hr = h.getBoundingClientRect();
                let maxBottom = 0;
                for (const d of h.querySelectorAll('*')) { const r = d.getBoundingClientRect(); if (r.height > 0) maxBottom = Math.max(maxBottom, r.bottom); }
                const host = h.closest('.mds-page-header-host') || h;
                const i = blocks.indexOf(host);
                const next = i >= 0 ? blocks[i + 1] : null;
                return { missing: false, sh: h.scrollHeight, ch: h.clientHeight, hb: hr.bottom, maxBottom,
                         nextTop: next ? next.getBoundingClientRect().top : null, hostIdx: i };
            }");
            if (json.GetProperty("missing").GetBoolean()) { failures.Add($"{route}: no .mbx-page-header"); continue; }
            var sh = json.GetProperty("sh").GetDouble(); var ch = json.GetProperty("ch").GetDouble();
            var hb = json.GetProperty("hb").GetDouble(); var mb = json.GetProperty("maxBottom").GetDouble();
            if (sh > ch + 1) failures.Add($"{route}: header content taller than header (scrollHeight {sh} > clientHeight {ch})");
            if (mb > hb + 1) failures.Add($"{route}: a header descendant's bottom {mb:0.#} exceeds header bottom {hb:0.#}");
            var nt = json.GetProperty("nextTop");
            if (nt.ValueKind == JsonValueKind.Number && nt.GetDouble() < hb - 1)
                failures.Add($"{route}: next block top {nt.GetDouble():0.#} overlaps header bottom {hb:0.#}");
        }
        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    // ── 2. Header actions not clipped ────────────────────────────────────────

    [Theory]
    [InlineData(375)]
    [InlineData(820)]
    public Task PageHeader_DescendantsNotClippedAtRight(int width) => RunAsync($"{nameof(PageHeader_DescendantsNotClippedAtRight)}_{width}", async () =>
    {
        var failures = new List<string>();
        foreach (var route in HeaderRoutes)
        {
            await SettleAsync(route, width);
            var bad = await Page.EvaluateAsync<string[]>(@"() => {
                const h = document.querySelector('.mbx-page-header');
                if (!h) return ['no .mbx-page-header'];
                const hr = h.getBoundingClientRect(), vw = window.innerWidth, out = [];
                for (const d of h.querySelectorAll('*')) {
                    if (d.closest('svg') && d.tagName.toLowerCase() !== 'svg') continue;
                    const r = d.getBoundingClientRect();
                    if (r.width <= 1 || r.height <= 1) continue;
                    if (r.right > vw + 1 || r.right > hr.right + 1) out.push(d.tagName.toLowerCase() + '.' + String(d.className).split(' ')[0] + ' right=' + Math.round(r.right) + ' header=' + Math.round(hr.right) + ' vw=' + vw);
                }
                return out;
            }");
            if (bad.Length > 0) failures.Add($"{route} @ {width}: {string.Join(" | ", bad.Take(5))}");
        }
        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    // ── 4. Block spacing ─────────────────────────────────────────────────────

    private const string GapScript = @"() => {" + BlocksFn + @"
        const shell = document.querySelector('.mbx-shell');
        const probe = document.createElement('div');
        probe.style.cssText = 'height:var(--mbx-block-gap, 0px);width:1px;position:absolute;visibility:hidden';
        shell.appendChild(probe); const token = probe.getBoundingClientRect().height; probe.remove();
        const gaps = [];
        for (let i = 1; i < blocks.length; i++) {
            const a = blocks[i - 1], b = blocks[i];
            gaps.push({ a: a.tagName.toLowerCase() + '.' + String(a.className).split(' ')[0], b: b.tagName.toLowerCase() + '.' + String(b.className).split(' ')[0],
                        gap: (b.getBoundingClientRect().top - Math.min(0, parseFloat(getComputedStyle(b).marginTop) || 0)) - a.getBoundingClientRect().bottom });
        }
        return { token, gaps, count: blocks.length };
    }";

    private async Task<List<string>> GapFailuresAsync(string route, int width, double? expectedToken)
    {
        var json = await Page.EvaluateAsync<JsonElement>(GapScript);
        var token = json.GetProperty("token").GetDouble();
        var failures = new List<string>();
        if (route == "/main-components" && json.GetProperty("gaps").GetArrayLength() < 2)
            failures.Add($"{route} @ {width}: only {json.GetProperty("gaps").GetArrayLength()} measurable block gaps (of {json.GetProperty("count").GetInt32()} blocks); test would be vacuous");
        if (expectedToken is double e && Math.Abs(token - e) > 0.5) failures.Add($"{route} @ {width}: --mbx-block-gap resolves to {token}px, expected {e}px");
        foreach (var g in json.GetProperty("gaps").EnumerateArray())
        {
            var gap = g.GetProperty("gap").GetDouble();
            if (Math.Abs(gap - token) > 1) failures.Add($"{route} @ {width}: gap {gap:0.#}px between {g.GetProperty("a").GetString()} and {g.GetProperty("b").GetString()}, token {token}px");
        }
        return failures;
    }

    [Theory]
    [InlineData(375)]
    [InlineData(820)]
    [InlineData(1280)]
    public Task BlockGap_EqualsToken_BetweenTopLevelBlocks(int width) => RunAsync($"{nameof(BlockGap_EqualsToken_BetweenTopLevelBlocks)}_{width}", async () =>
    {
        var failures = new List<string>();
        foreach (var route in AllRoutes)
        {
            await SettleAsync(route, width);
            failures.AddRange(await GapFailuresAsync(route, width, 16));
        }
        Assert.True(failures.Count == 0, "Block gap differs from --mbx-block-gap (MudGrid negative top margin compensated):\n" + string.Join("\n", failures));
    });

    [Fact]
    public Task BlockGap_FollowsToken_InCompactDensity() => RunAsync(nameof(BlockGap_FollowsToken_InCompactDensity), async () =>
    {
        await SettleAsync("/main-components", 375);
        await Page.EvaluateAsync("() => document.querySelector('.mbx-shell').classList.add('mbx-density-compact')");
        await Page.WaitForTimeoutAsync(300);
        var failures = await GapFailuresAsync("/main-components", 375, 12);
        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    // ── 5. Gutters ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(375)]
    [InlineData(820)]
    [InlineData(1280)]
    public Task Gutters_HeaderCoreAndBlocksAlignToPageGutter_AcrossAllRoutes(int width) => RunAsync($"{nameof(Gutters_HeaderCoreAndBlocksAlignToPageGutter_AcrossAllRoutes)}_{width}", async () =>
    {
        var failures = new List<string>();
        foreach (var route in AllRoutes)
        {
            await SettleAsync(route, width);
            var j = await Page.EvaluateAsync<JsonElement>(@"() => {" + BlocksFn + @"
                const mi = document.querySelector('.mbx-main-inner').getBoundingClientRect();
                const round = v => Math.round(v * 10) / 10;
                const core = document.querySelector('.mbx-page-header-core');
                let end = document.querySelector('.mbx-page-header-end');
                if (end && (end.children.length === 0 || end.getBoundingClientRect().width < 1)) end = null; // no header actions
                const host = document.querySelector('.mds-page-header-host');
                const res = { miLeft: mi.left, miRight: mi.right, coreLeft: core ? round(core.getBoundingClientRect().left) : null,
                              headerRight: end ? round(end.getBoundingClientRect().right) : null,
                              blocks: [] };
                for (const b of blocks) {
                    if (b.matches('.showcase-badge') || b.classList.contains('mds-page-header-host') || getComputedStyle(b).position === 'fixed') continue;
                    const r = b.getBoundingClientRect();
                    res.blocks.push({ n: b.tagName.toLowerCase() + '.' + String(b.className).split(' ')[0], l: round(r.left - Math.min(0, parseFloat(getComputedStyle(b).marginLeft) || 0)), r: round(r.right) });
                }
                return res;
            }");
            var expLeft = j.GetProperty("miLeft").GetDouble() + 16;
            var expRights = new[] { j.GetProperty("miRight").GetDouble() - 16, j.GetProperty("miRight").GetDouble() - 27 }; // 27 = with a classic scrollbar
            bool RightOk(double r) => expRights.Any(e => Math.Abs(r - e) <= 1);

            var coreLeft = j.GetProperty("coreLeft");
            if (coreLeft.ValueKind == JsonValueKind.Number && Math.Abs(coreLeft.GetDouble() - expLeft) > 1)
                failures.Add($"{route} @ {width}: header core left {coreLeft.GetDouble()} != main-inner left + 16 = {expLeft}");
            var hr = j.GetProperty("headerRight");
            if (hr.ValueKind == JsonValueKind.Number && !RightOk(hr.GetDouble()))
                failures.Add($"{route} @ {width}: header right {hr.GetDouble()} not at main-inner right - 16/27 ({string.Join("/", expRights)})");
            foreach (var b in j.GetProperty("blocks").EnumerateArray())
            {
                var l = b.GetProperty("l").GetDouble(); var r = b.GetProperty("r").GetDouble();
                if (Math.Abs(l - expLeft) > 1) failures.Add($"{route} @ {width}: block {b.GetProperty("n").GetString()} left {l} != {expLeft}");
                if (!RightOk(r)) failures.Add($"{route} @ {width}: block {b.GetProperty("n").GetString()} right {r} not at {string.Join("/", expRights)}");
            }
        }
        Assert.True(failures.Count == 0, "Gutter mismatch (.showcase-badge and fixed blocks skipped; MudGrid negative left margin compensated):\n" + string.Join("\n", failures));
    });

    // ── 6. Bottom bar on WASM / auto layout pages ────────────────────────────

    [Theory]
    [InlineData("/density")]
    [InlineData("/modes/wasm")]
    public Task BottomBar_OnWasmLayoutPages_EqualWidths_LabelsNotTruncated(string route) => RunAsync($"{nameof(BottomBar_OnWasmLayoutPages_EqualWidths_LabelsNotTruncated)}_{route.Trim('/').Replace('/', '_')}", async () =>
    {
        await SettleAsync(route, 375, 812);
        await Assertions.Expect(Page.GetByTestId("bottom-nav")).ToBeVisibleAsync();

        var json = await Page.EvaluateAsync<JsonElement>(@"() => ({
            widths: [...document.querySelector('[data-testid=bottom-nav]').children].map(e => ({ tag: e.tagName.toLowerCase(), w: e.getBoundingClientRect().width })),
            labels: [...document.querySelectorAll('[data-testid=bottom-nav] .mbx-bottom-nav-label')].map(l => ({ text: l.textContent, sw: l.scrollWidth, cw: l.clientWidth }))
        })");
        var widths = json.GetProperty("widths").EnumerateArray().Select(e => (Tag: e.GetProperty("tag").GetString()!, W: e.GetProperty("w").GetDouble())).ToList();
        Assert.InRange(widths.Count, 4, 6);
        var w0 = widths[0].W;
        foreach (var (tag, w) in widths)
            Assert.True(Math.Abs(w - w0) <= 1, $"{route}: <{tag}> item width {w:0.#} differs from first item {w0:0.#}; widths: {string.Join(", ", widths.Select(x => $"<{x.Tag}> {x.W:0.#}"))}");

        foreach (var l in json.GetProperty("labels").EnumerateArray())
        {
            var sw = l.GetProperty("sw").GetDouble(); var cw = l.GetProperty("cw").GetDouble();
            Assert.True(sw <= cw, $"{route}: label '{l.GetProperty("text").GetString()}' is truncated (scrollWidth {sw} > clientWidth {cw})");
        }
    });
}

/// <summary>
/// Regression: at tablet width the level-2 overlay used to be visible at first paint (SSR renders ShellMode=Desktop with
/// the panel expanded, so the tablet CSS showed it until C# collapsed it). It must stay hidden unless the aside has
/// mbx-context-overlay-open.
/// </summary>
public class ResponsiveContextOverlayFirstPaintTests : PlaywrightTestBase
{
    protected override BrowserNewContextOptions ContextOptions => new()
    {
        BaseURL = BaseUrl,
        IgnoreHTTPSErrors = true,
        ViewportSize = new() { Width = 820, Height = 1180 },
    };

    private const string SampleScript = @"() => {
        // Before the render-blocking stylesheets have applied nothing has been painted: not a first-paint state.
        if ([...document.querySelectorAll('link[rel=stylesheet]')].some(l => !l.sheet)) return null;
        const p = document.querySelector('.mbx-context-panel');
        const shell = document.querySelector('[data-testid=shell-mode]');
        const rail = document.querySelector('.mbx-sidebar');
        const el = document.elementFromPoint(28, 100);
        if (!p) return null;
        const r = p.getBoundingClientRect();
        return { vis: getComputedStyle(p).visibility, x: r.x, w: r.width, open: p.classList.contains('mbx-context-overlay-open'),
                 mode: shell ? shell.getAttribute('data-shell-mode') : null,
                 railCovered: !!rail && !!el && !el.closest('.mbx-sidebar') };
    }";

    [Theory]
    [InlineData("/demo")]
    [InlineData("/library")]
    [InlineData("/main-components")]
    [InlineData("/modes/ssr")]
    [InlineData("/modes/server")]
    public Task Tablet_ContextOverlay_NeverVisibleAtFirstPaint_AndRailNotCovered(string route) => RunAsync($"{nameof(Tablet_ContextOverlay_NeverVisibleAtFirstPaint_AndRailNotCovered)}_{route.Trim('/').Replace('/', '_')}", async () =>
    {
        await Page.GotoAsync(route, new() { WaitUntil = WaitUntilState.Commit });

        var samples = new List<string>();
        var seen = 0;
        var started = DateTime.UtcNow;
        while ((DateTime.UtcNow - started).TotalMilliseconds < 2000)
        {
            JsonElement? s = null;
            try { s = await Page.EvaluateAsync<JsonElement?>(SampleScript); } catch (PlaywrightException) { /* navigation in flight */ }
            if (s is { ValueKind: JsonValueKind.Object } v)
            {
                seen++;
                var open = v.GetProperty("open").GetBoolean();
                var vis = v.GetProperty("vis").GetString();
                var x = v.GetProperty("x").GetDouble(); var w = v.GetProperty("w").GetDouble();
                var mode = v.GetProperty("mode").ValueKind == JsonValueKind.String ? v.GetProperty("mode").GetString() : null;
                var at = $"t={(DateTime.UtcNow - started).TotalMilliseconds:0}ms mode={mode} vis={vis} x={x:0.#} w={w:0.#}";
                if (!open)
                {
                    Assert.True(vis == "hidden", $"{route}: closed context panel is visible ({at})");
                    Assert.True(x + w <= 57, $"{route}: closed context panel not off-screen left of the rail ({at})");
                }
                Assert.True(!v.GetProperty("railCovered").GetBoolean(), $"{route}: rail icon point (28,100) is covered by something outside .mbx-sidebar ({at})");
                samples.Add(at);
            }
            await Page.WaitForTimeoutAsync(100);
        }
        Assert.True(seen >= 3, $"{route}: only {seen} samples captured with a context panel present");

        // Settled: still closed, rail still uncovered.
        await Assertions.Expect(Page.GetByTestId("shell-mode")).ToHaveAttributeAsync("data-shell-mode", "tablet");
        await Assertions.Expect(Page.Locator(".mbx-context-panel")).ToBeHiddenAsync();
        Assert.Equal(0, await Page.Locator(".mbx-context-overlay-open").CountAsync());
    });

    [Fact]
    public Task Tablet_RailMenuButton_TogglesOverlayOpenClass() => RunAsync(nameof(Tablet_RailMenuButton_TogglesOverlayOpenClass), async () =>
    {
        await Page.GotoAsync("/demo", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Assertions.Expect(Page.GetByTestId("shell-mode")).ToHaveAttributeAsync("data-shell-mode", "tablet");
        var panel = Page.GetByTestId("context-overlay");
        await Assertions.Expect(panel).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("mbx-context-overlay-open"));

        await Page.Locator(".mbx-nav-toggle-btn").First.ClickAsync();

        await Assertions.Expect(panel).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("mbx-context-overlay-open"));
        await Assertions.Expect(panel).ToBeVisibleAsync();
        await Assertions.Expect(Page.GetByTestId("nav-scrim")).ToBeVisibleAsync();
        await Page.WaitForTimeoutAsync(500);
        var box = await panel.BoundingBoxAsync();
        Assert.InRange(box!.X, 55, 57);

        await Page.Mouse.ClickAsync(800, 900);

        await Assertions.Expect(panel).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("mbx-context-overlay-open"));
        await Assertions.Expect(panel).ToBeHiddenAsync();
        await Assertions.Expect(Page.GetByTestId("nav-scrim")).ToHaveCountAsync(0);
    });

    [Fact]
    public Task Desktop_ContextPanel_HasNoOverlayOpenClass_AndIsInline() => RunAsync(nameof(Desktop_ContextPanel_HasNoOverlayOpenClass_AndIsInline), async () =>
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await Page.GotoAsync("/library", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Assertions.Expect(Page.GetByTestId("shell-mode")).ToHaveAttributeAsync("data-shell-mode", "desktop");

        var panel = Page.GetByTestId("context-overlay");
        await Assertions.Expect(panel).ToBeVisibleAsync();
        Assert.Equal(0, await Page.Locator(".mbx-context-overlay-open").CountAsync());
        var pos = await panel.EvaluateAsync<string>("el => getComputedStyle(el).position");
        Assert.True(pos is "static" or "relative", $"desktop context panel position is '{pos}', expected static/relative");
    });
}

/// <summary>Tablet: tapping a rail icon opens level 2, overlay styling and scrim geometry (rail stays visible and clickable).</summary>
public class ResponsiveRailTapAndOverlayStyleTests : PlaywrightTestBase
{
    private static readonly System.Text.RegularExpressions.Regex OpenClass = new("mbx-context-overlay-open");

    protected override BrowserNewContextOptions ContextOptions => new()
    {
        BaseURL = BaseUrl,
        IgnoreHTTPSErrors = true,
        ViewportSize = new() { Width = 820, Height = 1180 },
    };

    private ILocator Panel => Page.GetByTestId("context-overlay");
    private ILocator Scrim => Page.GetByTestId("nav-scrim");
    private ILocator RailIcon(string href) => Page.Locator($".mbx-sidebar-icon-link[href='{href}']").First;

    private async Task GotoTabletAsync(string path, int width = 820, int height = 1180)
    {
        await Page.SetViewportSizeAsync(width, height);
        await Page.GotoAsync(path, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Assertions.Expect(Page.GetByTestId("shell-mode")).ToHaveAttributeAsync("data-shell-mode", width >= 1280 ? "desktop" : width >= 600 ? "tablet" : "mobile");
        await Page.WaitForTimeoutAsync(300);
    }

    private async Task AssertOverlayOpenAsync()
    {
        await Assertions.Expect(Panel).ToHaveClassAsync(OpenClass);
        await Assertions.Expect(Panel).ToBeVisibleAsync();
        await Assertions.Expect(Scrim).ToHaveCountAsync(1);
        await Page.WaitForTimeoutAsync(500); // slide-in
        Assert.InRange((await Panel.BoundingBoxAsync())!.X, 55, 57);
    }

    [Fact]
    public Task Tablet_RailIconTap_OpensLevel2_KeepsItOpenAcrossNavigation_AndHomeClosesIt() => RunAsync(nameof(Tablet_RailIconTap_OpensLevel2_KeepsItOpenAcrossNavigation_AndHomeClosesIt), async () =>
    {
        var errors = new List<string>();
        Page.Console += (_, m) => { if (m.Type == "error") errors.Add(m.Text); };
        Page.PageError += (_, e) => errors.Add("pageerror: " + e);

        await GotoTabletAsync("/");

        await RailIcon("/library").ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex("/library$"));
        await AssertOverlayOpenAsync();

        // Rail stays clickable while the overlay is open; the overlay stays open for the new section.
        await RailIcon("/demo").ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex("/demo$"));
        await AssertOverlayOpenAsync();

        // Home has no context panel: navigation works, nothing is left over.
        await RailIcon("/").ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/$"));
        await Assertions.Expect(Page.Locator(".mbx-context-panel")).ToHaveCountAsync(0);
        await Assertions.Expect(Scrim).ToHaveCountAsync(0);
        Assert.True(errors.Count == 0, "console/page errors: " + string.Join(" | ", errors));
    });

    [Fact]
    public Task Tablet_OverlayOpenedByRailIcon_ClosesOnEscape() => RunAsync(nameof(Tablet_OverlayOpenedByRailIcon_ClosesOnEscape), async () =>
    {
        await GotoTabletAsync("/");
        await RailIcon("/library").ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex("/library$"));
        await AssertOverlayOpenAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Assertions.Expect(Panel).Not.ToHaveClassAsync(OpenClass);
        await Assertions.Expect(Scrim).ToHaveCountAsync(0);
    });

    [Fact]
    public Task Tablet_OverlayOpenedByRailIcon_ClosesOnInPageLinkNavigation() => RunAsync(nameof(Tablet_OverlayOpenedByRailIcon_ClosesOnInPageLinkNavigation), async () =>
    {
        await GotoTabletAsync("/");
        await RailIcon("/demo").ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex("/demo$"));
        await AssertOverlayOpenAsync();

        await Panel.GetByTestId("nav-demo.render-modes").ClickAsync();

        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex("/modes$"));
        await Assertions.Expect(Panel).Not.ToHaveClassAsync(OpenClass);
        await Assertions.Expect(Scrim).ToHaveCountAsync(0);
    });

    [Fact]
    public Task Tablet_TapOnCurrentPageIcon_OpensOverlay_AndLaterNavigationStillCloses() => RunAsync(nameof(Tablet_TapOnCurrentPageIcon_OpensOverlay_AndLaterNavigationStillCloses), async () =>
    {
        await GotoTabletAsync("/demo");

        await RailIcon("/demo").ClickAsync();
        await AssertOverlayOpenAsync();

        await Page.WaitForTimeoutAsync(2200); // the open-on-navigation flag is dropped after 1.5 s
        await Panel.GetByTestId("nav-demo.render-modes").ClickAsync();

        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex("/modes$"));
        await Assertions.Expect(Panel).Not.ToHaveClassAsync(OpenClass);
        await Assertions.Expect(Scrim).ToHaveCountAsync(0);
    });

    [Fact]
    public Task Desktop_RailIconTap_DoesNotOpenOverlay() => RunAsync(nameof(Desktop_RailIconTap_DoesNotOpenOverlay), async () =>
    {
        await GotoTabletAsync("/", 1280, 800);

        await RailIcon("/library").ClickAsync();

        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex("/library$"));
        await Page.WaitForTimeoutAsync(500);
        Assert.Equal(0, await Page.Locator(".mbx-context-overlay-open").CountAsync());
        Assert.Equal(0, await Page.GetByTestId("nav-scrim").CountAsync());
    });

    [Fact]
    public Task Mobile_HasNoRailIcons() => RunAsync(nameof(Mobile_HasNoRailIcons), async () =>
    {
        await GotoTabletAsync("/", 375, 812);

        Assert.Equal(0, await Page.Locator(".mbx-sidebar-icon-link").CountAsync());
        await Assertions.Expect(Page.Locator(".mbx-sidebar").First).ToBeHiddenAsync();
    });

    // ── Palette-mode corner fill ─────────────────────────────────────────────

    private async Task<(string Shadow, string RadiusTl, string RadiusBl, string RailBg, string ShellClass)> StyleAsync() =>
        ToTuple(await Page.EvaluateAsync<string[]>(@"() => {
            const p = document.querySelector('.mbx-context-panel'), cs = getComputedStyle(p);
            return [cs.boxShadow, cs.borderTopLeftRadius, cs.borderBottomLeftRadius,
                    getComputedStyle(document.querySelector('.mbx-sidebar')).backgroundColor,
                    document.querySelector('.mbx-shell').className];
        }"));

    private static (string, string, string, string, string) ToTuple(string[] a) => (a[0], a[1], a[2], a[3], a[4]);

    private static readonly System.Text.RegularExpressions.Regex FillLayer = new(@"^(rgba?\([^)]*\))\s+-20px 0px 0px 0px");

    [Fact]
    public Task Tablet_PaletteMode_OpenOverlay_CornerGapFilledWithRailColour_RadiusKept() => RunAsync(nameof(Tablet_PaletteMode_OpenOverlay_CornerGapFilledWithRailColour_RadiusKept), async () =>
    {
        await GotoTabletAsync("/library");
        await Page.Locator(".mbx-nav-toggle-btn").First.ClickAsync();
        await AssertOverlayOpenAsync();

        var (shadow, tl, bl, railBg, shellClass) = await StyleAsync();

        Assert.Contains("mbx-mode-palette", shellClass);
        Assert.Equal("20px", tl);
        Assert.Equal("20px", bl);
        var m = FillLayer.Match(shadow);
        Assert.True(m.Success, $"first box-shadow layer is not '-20px 0px 0px 0px': {shadow}");
        Assert.True(m.Groups[1].Value == railBg, $"corner fill colour {m.Groups[1].Value} != rail background {railBg}");
    });

    [Fact]
    public Task Tablet_ImageMode_OpenOverlay_HasNoCornerFillShadow() => RunAsync(nameof(Tablet_ImageMode_OpenOverlay_HasNoCornerFillShadow), async () =>
    {
        await GotoTabletAsync("/library/bg");
        await Page.Locator(".mbx-nav-toggle-btn").First.ClickAsync();
        await AssertOverlayOpenAsync();

        var (shadow, _, _, _, shellClass) = await StyleAsync();

        Assert.Contains("mbx-mode-image", shellClass);
        Assert.False(FillLayer.IsMatch(shadow), $"image mode must not get the palette corner fill: {shadow}");
    });

    [Fact]
    public Task Desktop_PaletteMode_ContextPanel_HasNoCornerFillShadow() => RunAsync(nameof(Desktop_PaletteMode_ContextPanel_HasNoCornerFillShadow), async () =>
    {
        await GotoTabletAsync("/library", 1280, 800);

        var (shadow, _, _, _, _) = await StyleAsync();

        Assert.False(shadow.Contains("-20px"), $"desktop panel must not use the tablet corner fill: {shadow}");
    });

    // ── Scrim starts at the rail edge ────────────────────────────────────────

    [Fact]
    public Task Tablet_Scrim_StartsAtRailEdge_RailNotDimmedOrCovered() => RunAsync(nameof(Tablet_Scrim_StartsAtRailEdge_RailNotDimmedOrCovered), async () =>
    {
        await GotoTabletAsync("/demo");
        await Page.Locator(".mbx-nav-toggle-btn").First.ClickAsync();
        await AssertOverlayOpenAsync();

        var box = await Scrim.BoundingBoxAsync();
        Assert.InRange(box!.X, 55, 57);

        var rail = await Page.EvaluateAsync<bool>("() => !!document.elementFromPoint(28, 300)?.closest('.mbx-sidebar')");
        Assert.True(rail, "elementFromPoint(28,300) is not inside .mbx-sidebar");
        var scrim = await Page.EvaluateAsync<bool>("() => !!document.elementFromPoint(600, 600)?.closest('[data-testid=nav-scrim]')");
        Assert.True(scrim, "elementFromPoint(600,600) is not the scrim");
    });
}

/// <summary>
/// One navigation tree feeds the rail, the bottom bar, the hamburger sheet and the context panel: 7 roots, reachable
/// everywhere, plus theme actions in the mobile top bar and wrapped (not truncated) bottom-bar labels.
/// </summary>
public class ResponsiveSingleSourceNavigationTests : PlaywrightTestBase
{
    private static readonly string[] RootPaths = ["/", "/meteo", "/europe-cities", "/library", "/demo", "/components", "/fill-demo"];
    private static readonly string[] RootIds = ["home", "meteo", "europe-cities", "library", "demo", "components", "fill-demo"];

    protected override BrowserNewContextOptions ContextOptions => new()
    {
        BaseURL = BaseUrl,
        IgnoreHTTPSErrors = true,
        ViewportSize = new() { Width = 375, Height = 812 },
    };

    private ILocator Sheet => Page.GetByTestId("context-sheet");
    private ILocator Bar => Page.GetByTestId("bottom-nav");

    private async Task GotoAsync(string path, int width = 375, int height = 812)
    {
        await Page.SetViewportSizeAsync(width, height);
        await Page.GotoAsync(path, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Assertions.Expect(Page.GetByTestId("shell-mode")).ToHaveAttributeAsync("data-shell-mode", width >= 1280 ? "desktop" : width >= 600 ? "tablet" : "mobile");
        await Page.WaitForTimeoutAsync(300);
    }

    private async Task OpenHamburgerAsync()
    {
        await Page.GetByTestId("mobile-menu-button").ClickAsync();
        await Assertions.Expect(Sheet).ToBeVisibleAsync();
    }

    private static string PathOf(string url) => new Uri(url).AbsolutePath;

    [Fact]
    public Task Mobile_BottomBar_ShowsFourRootsPlusPlus_AndPlusListsRemainingThree() => RunAsync(nameof(Mobile_BottomBar_ShowsFourRootsPlusPlus_AndPlusListsRemainingThree), async () =>
    {
        await GotoAsync("/");

        var labels = (await Bar.Locator(".mbx-bottom-nav-item .mbx-bottom-nav-label").AllTextContentsAsync()).Select(t => t.Trim()).ToArray();
        Assert.Equal(new[] { "Home", "Weather", "Europe Cities", "Library", "Plus" }, labels);
        Assert.Equal(5, await Bar.Locator(".mbx-bottom-nav-item").CountAsync());

        await Page.GetByTestId("bottom-nav-more").ClickAsync();

        await Assertions.Expect(Sheet).ToBeVisibleAsync();
        var ids = await Sheet.Locator(".mbx-nav-sheet-item").EvaluateAllAsync<string[]>("els => els.map(e => e.getAttribute('data-testid'))");
        Assert.Equal(new[] { "nav-demo", "nav-components", "nav-fill-demo" }, ids);
    });

    [Fact]
    public Task Mobile_Hamburger_ListsAllSevenRoots() => RunAsync(nameof(Mobile_Hamburger_ListsAllSevenRoots), async () =>
    {
        await GotoAsync("/");

        await OpenHamburgerAsync();

        var ids = await Sheet.Locator(".mbx-nav-sheet-item").EvaluateAllAsync<string[]>("els => els.map(e => e.getAttribute('data-testid'))");
        Assert.Equal(RootIds.Select(i => $"nav-{i}").ToArray(), ids);
    });

    [Theory]
    [InlineData("bar")]
    [InlineData("plus")]
    [InlineData("hamburger")]
    public Task Mobile_EveryRoot_IsReachableByTaps(string via) => RunAsync($"{nameof(Mobile_EveryRoot_IsReachableByTaps)}_{via}", async () =>
    {
        var failures = new List<string>();
        for (var i = 0; i < RootIds.Length; i++)
        {
            var id = RootIds[i]; var path = RootPaths[i];
            // Start away from the target so a URL change is observable (Home starts from /library).
            await GotoAsync(path == "/" ? "/library" : "/");

            switch (via)
            {
                case "bar" when i < 4:
                    await Bar.GetByTestId($"nav-{id}").ClickAsync();
                    break;
                case "bar":
                    continue; // reachable through Plus / hamburger
                case "plus" when i >= 4:
                    await Page.GetByTestId("bottom-nav-more").ClickAsync();
                    await Sheet.GetByTestId($"nav-{id}").ClickAsync();
                    break;
                case "plus":
                    continue;
                default:
                    await OpenHamburgerAsync();
                    await Sheet.GetByTestId($"nav-{id}").ClickAsync();
                    break;
            }

            await Page.WaitForTimeoutAsync(700);
            var actual = PathOf(Page.Url);
            if (actual != path) failures.Add($"{via}: tapping '{id}' ended at {actual}, expected {path}");
        }
        Assert.True(failures.Count == 0, string.Join("\n", failures));
    });

    [Fact]
    public Task Mobile_TopBar_HasThemeButtonAndDarkModeToggle_ThatWork() => RunAsync(nameof(Mobile_TopBar_HasThemeButtonAndDarkModeToggle_ThatWork), async () =>
    {
        await GotoAsync("/");
        var top = Page.GetByTestId("mobile-topbar");
        var themeBtn = top.Locator("[title='Theme and background']");
        await Assertions.Expect(themeBtn).ToBeVisibleAsync();
        var toggle = top.Locator("[title='Light mode'], [title='Dark mode']");
        await Assertions.Expect(toggle).ToHaveCountAsync(1);

        var shell = Page.Locator(".mbx-shell");
        var wasLight = (await shell.GetAttributeAsync("class"))!.Contains("mbx-light");
        var titleBefore = await toggle.GetAttributeAsync("title");

        await toggle.ClickAsync();

        await Page.WaitForFunctionAsync("l => document.querySelector('.mbx-shell').classList.contains('mbx-light') !== l", wasLight);
        var titleAfter = await top.Locator("[title='Light mode'], [title='Dark mode']").GetAttributeAsync("title");
        Assert.NotEqual(titleBefore, titleAfter);
        Assert.Equal(wasLight ? "Light mode" : "Dark mode", titleAfter);

        await themeBtn.ClickAsync();
        await Assertions.Expect(Page.Locator(".mud-dialog")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator(".mud-dialog")).ToContainTextAsync("Theme and background");
    });

    [Fact]
    public Task Mobile_BottomBarLabels_WrapAtMostTwoLines_NeverTruncated() => RunAsync(nameof(Mobile_BottomBarLabels_WrapAtMostTwoLines_NeverTruncated), async () =>
    {
        await GotoAsync("/");

        var rows = await Page.EvaluateAsync<System.Text.Json.JsonElement>(@"() => [...document.querySelectorAll('[data-testid=bottom-nav] .mbx-bottom-nav-label')].map(l => {
            const cs = getComputedStyle(l); const fs = parseFloat(cs.fontSize);
            const lh = parseFloat(cs.lineHeight); 
            return { text: l.textContent.trim(), sh: l.scrollHeight, ch: l.clientHeight, sw: l.scrollWidth, cw: l.clientWidth,
                     h: l.getBoundingClientRect().height, lh: isNaN(lh) ? fs * 1.2 : lh };
        })");
        var texts = new List<string>();
        foreach (var r in rows.EnumerateArray())
        {
            var t = r.GetProperty("text").GetString()!; texts.Add(t);
            Assert.True(r.GetProperty("sh").GetDouble() <= r.GetProperty("ch").GetDouble() + 1, $"label '{t}' clipped vertically (scrollHeight {r.GetProperty("sh")} > clientHeight {r.GetProperty("ch")})");
            Assert.True(r.GetProperty("sw").GetDouble() <= r.GetProperty("cw").GetDouble() + 1, $"label '{t}' clipped horizontally (scrollWidth {r.GetProperty("sw")} > clientWidth {r.GetProperty("cw")})");
            Assert.True(r.GetProperty("h").GetDouble() <= 2 * r.GetProperty("lh").GetDouble() + 1, $"label '{t}' occupies more than 2 lines (height {r.GetProperty("h")}, line-height {r.GetProperty("lh")})");
        }
        Assert.Contains("Europe Cities", texts); // full text in the DOM, not cut
    });

    [Theory]
    [InlineData(820)]
    [InlineData(1280)]
    public Task Rail_HasSevenIconLinks_InOrder(int width) => RunAsync($"{nameof(Rail_HasSevenIconLinks_InOrder)}_{width}", async () =>
    {
        await GotoAsync("/", width, 900);

        var paths = await Page.Locator(".mbx-sidebar-icon-link").EvaluateAllAsync<string[]>("els => els.map(a => a.pathname)");
        Assert.Equal(RootPaths, paths);
    });

    [Fact]
    public Task SingleSource_RailHrefsAt820_EqualRootsReachableFromHamburgerAt375() => RunAsync(nameof(SingleSource_RailHrefsAt820_EqualRootsReachableFromHamburgerAt375), async () =>
    {
        await GotoAsync("/", 820, 1180);
        var rail = (await Page.Locator(".mbx-sidebar-icon-link").EvaluateAllAsync<string[]>("els => els.map(a => a.pathname)")).ToHashSet();

        var reached = new HashSet<string>();
        for (var i = 0; i < RootIds.Length; i++)
        {
            await GotoAsync("/components/", 375, 812); // a start page that is none of the roots' exact paths except by prefix
            await OpenHamburgerAsync();
            var item = Sheet.Locator(".mbx-nav-sheet-item").Nth(i);
            await item.ClickAsync();
            await Page.WaitForTimeoutAsync(700);
            reached.Add(PathOf(Page.Url));
        }

        Assert.True(rail.SetEquals(reached), $"rail (820): [{string.Join(", ", rail.Order())}] vs hamburger (375): [{string.Join(", ", reached.Order())}]");
    });
}
