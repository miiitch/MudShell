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
