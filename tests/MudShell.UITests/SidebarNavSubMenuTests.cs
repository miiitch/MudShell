using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

/// <summary>
/// Covers the sub-menu pin/trailing-action/drag-and-drop-sort demo on the "/demo" showcase page
/// (MdsSidebarNav — see MbxNavNode.Pin, MbxNavNode.TrailingActions, MdsSidebarNav.OnSortChanged).
/// </summary>
public class SidebarNavSubMenuTests : PlaywrightTestBase
{
    private const string GroupTestId = "nav-group-sort-demo-section";

    private async Task GotoAndExpandGroupAsync()
    {
        await Page.GotoAsync("/demo", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var toggle = Page.GetByTestId(GroupTestId);
        await Assertions.Expect(toggle).ToBeVisibleAsync();
        await toggle.ClickAsync();

        await Assertions.Expect(Page.GetByTestId("nav-report-pinned-top")).ToBeVisibleAsync();

        // Blazor Server prerenders "/demo" over plain HTTP before the SignalR circuit takes over;
        // a click issued the instant the interactive DOM appears can still land on the outgoing
        // prerendered markup. Give the circuit a moment to fully take over before interacting further.
        await Page.WaitForTimeoutAsync(200);
    }

    [Fact]
    public Task SubMenu_FixedTopAndBottomItems_AreRenderedAtTheirPositions() => RunAsync(
        nameof(SubMenu_FixedTopAndBottomItems_AreRenderedAtTheirPositions), async () =>
    {
        await GotoAndExpandGroupAsync();

        var subMenu = Page.Locator("nav[aria-label='Reports']");
        var itemTestIds = await subMenu.Locator("[data-testid^='nav-report']")
            .EvaluateAllAsync<string[]>("els => els.map(e => e.getAttribute('data-testid'))");

        Assert.NotEmpty(itemTestIds);
        Assert.Equal("nav-report-pinned-top", itemTestIds[0]);
        Assert.Equal("nav-report-pinned-bottom", itemTestIds[^1]);
    });

    [Fact]
    public Task SubMenu_FixedItems_HaveNoTrailingAction() => RunAsync(
        nameof(SubMenu_FixedItems_HaveNoTrailingAction), async () =>
    {
        await GotoAndExpandGroupAsync();

        await Assertions.Expect(Page.GetByTestId("nav-action-report-pinned-top-0")).Not.ToBeAttachedAsync();
        await Assertions.Expect(Page.GetByTestId("nav-action-report-pinned-bottom-0")).Not.ToBeAttachedAsync();
    });

    [Fact]
    public Task SubMenu_ClickingTrailingAction_RemovesItemAndLogsEvent() => RunAsync(
        nameof(SubMenu_ClickingTrailingAction_RemovesItemAndLogsEvent), async () =>
    {
        await GotoAndExpandGroupAsync();

        await Assertions.Expect(Page.GetByTestId("nav-report-beta")).ToBeVisibleAsync();

        // The trailing action is only visually revealed on hover (opacity transition), but it is
        // already in the DOM and clickable regardless of hover state via Playwright.
        await Page.GetByTestId("nav-action-report-beta-0").ClickAsync();

        await Assertions.Expect(Page.GetByTestId("nav-report-beta")).Not.ToBeAttachedAsync();
        await Assertions.Expect(Page.GetByTestId("sidebar-nav-sort-demo-log"))
            .ToContainTextAsync("Removed: report-beta");

        // Fixed items are unaffected by a sibling's removal.
        await Assertions.Expect(Page.GetByTestId("nav-report-pinned-top")).ToBeVisibleAsync();
        await Assertions.Expect(Page.GetByTestId("nav-report-pinned-bottom")).ToBeVisibleAsync();
    });

    private ILocator SubMenu => Page.Locator("nav[aria-label='Reports']");

    private Task<string[]> GetItemOrderAsync() =>
        SubMenu.Locator("[data-testid^='nav-report']")
            .EvaluateAllAsync<string[]>("els => els.map(e => e.getAttribute('data-testid'))");

    // MudBlazor's drag-and-drop items are native HTML5 draggable elements (draggable="true"), so
    // the drag must go through the native dragstart/dragover/drop sequence — raw mouse
    // move/down/up (what a custom pointer-based DnD would need) does not trigger it.
    //
    // Playwright's synthetic native drag simulation is occasionally swallowed by MudBlazor's JS
    // interop (the drop never registers, so no ItemDropped/OnSortChanged fires at all) — not a
    // product bug, just simulation flakiness. Retry the gesture a few times rather than the whole
    // test, checking the actual DOM order (not the log, which a stray earlier drag could also
    // have touched) to decide whether this attempt took effect.
    private async Task DragAsync(string sourceId, string targetId)
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            var orderBefore = await GetItemOrderAsync();
            await Page.GetByTestId(sourceId).DragToAsync(Page.GetByTestId(targetId));

            var orderAfter = await GetItemOrderAsync();
            if (!orderBefore.SequenceEqual(orderAfter))
                return;
        }

        // Every attempt left the order unchanged. That is the expected, genuine outcome for a
        // pinned item (nothing to reorder), so callers that expect an actual reorder must assert
        // on the resulting order/log themselves — this helper does not throw on a no-op.
    }

    [Fact]
    public Task SubMenu_DragAndDrop_ReordersSortableSegment_AndRaisesOnSortChanged() => RunAsync(
        nameof(SubMenu_DragAndDrop_ReordersSortableSegment_AndRaisesOnSortChanged), async () =>
    {
        await GotoAndExpandGroupAsync();

        var gamma = Page.GetByTestId("nav-report-gamma");
        var alpha = Page.GetByTestId("nav-report-alpha");
        await Assertions.Expect(gamma).ToBeVisibleAsync();
        await Assertions.Expect(alpha).ToBeVisibleAsync();

        await DragAsync("nav-report-gamma", "nav-report-alpha");

        await Assertions.Expect(Page.GetByTestId("sidebar-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged → [report-gamma, report-alpha, report-beta]");

        var order = await GetItemOrderAsync();
        Assert.Equal(
            ["nav-report-pinned-top", "nav-report-gamma", "nav-report-alpha", "nav-report-beta", "nav-report-pinned-bottom"],
            order);
    });

    [Fact]
    public Task SubMenu_DragAndDrop_LastItemOntoAnyEarlierItem_MovesItToTheFront() => RunAsync(
        nameof(SubMenu_DragAndDrop_LastItemOntoAnyEarlierItem_MovesItToTheFront), async () =>
    {
        await GotoAndExpandGroupAsync();

        // report-gamma (last sortable item) dropped onto report-beta (the middle one) — same
        // resulting position as dropping it onto report-alpha (first item): dragging the last
        // item upward moves it to the front of the sortable segment regardless of exactly which
        // earlier item it is dropped on.
        await DragAsync("nav-report-gamma", "nav-report-beta");

        await Assertions.Expect(Page.GetByTestId("sidebar-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged → [report-gamma, report-alpha, report-beta]");

        var order = await GetItemOrderAsync();
        Assert.Equal(
            ["nav-report-pinned-top", "nav-report-gamma", "nav-report-alpha", "nav-report-beta", "nav-report-pinned-bottom"],
            order);
    });

    [Fact]
    public Task SubMenu_DragAndDrop_RepeatedDrags_DoNotDuplicateOrCrash() => RunAsync(
        nameof(SubMenu_DragAndDrop_RepeatedDrags_DoNotDuplicateOrCrash), async () =>
    {
        // Regression test: MbxNavNode is rebuilt (with fresh TrailingActions delegates) on every
        // render, so a fix that special-cased "the first drag" without accounting for record
        // identity churn across renders could still crash or duplicate ids on a *second* drag in
        // the same session. Two consecutive drags must both succeed cleanly.
        await GotoAndExpandGroupAsync();

        await DragAsync("nav-report-gamma", "nav-report-alpha");
        await Assertions.Expect(Page.GetByTestId("sidebar-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged → [report-gamma, report-alpha, report-beta]");

        await DragAsync("nav-report-beta", "nav-report-gamma");
        await Assertions.Expect(Page.GetByTestId("sidebar-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged → [report-beta, report-gamma, report-alpha]");

        // No Blazor circuit error surfaced (the bug this guards against threw
        // InvalidOperationException: "Duplicate navigation node id" from MbxNavTree.Validate,
        // which tears down the circuit and shows this banner).
        await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();

        var order = await GetItemOrderAsync();
        Assert.Equal(
            ["nav-report-pinned-top", "nav-report-beta", "nav-report-gamma", "nav-report-alpha", "nav-report-pinned-bottom"],
            order);
        // Each id appears exactly once — the exact shape of the original bug.
        Assert.Equal(order.Length, order.Distinct().Count());
    });

    [Fact]
    public Task SubMenu_DragAndDrop_RemovedItemThenDrag_DoesNotCrash() => RunAsync(
        nameof(SubMenu_DragAndDrop_RemovedItemThenDrag_DoesNotCrash), async () =>
    {
        await GotoAndExpandGroupAsync();

        await Page.GetByTestId("nav-action-report-beta-0").ClickAsync();
        await Assertions.Expect(Page.GetByTestId("nav-report-beta")).Not.ToBeAttachedAsync();

        await DragAsync("nav-report-gamma", "nav-report-alpha");

        await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
        await Assertions.Expect(Page.GetByTestId("sidebar-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged → [report-gamma, report-alpha]");

        var order = await GetItemOrderAsync();
        Assert.Equal(
            ["nav-report-pinned-top", "nav-report-gamma", "nav-report-alpha", "nav-report-pinned-bottom"],
            order);
    });

    [Fact]
    public Task SubMenu_FixedItems_AreNotDraggable() => RunAsync(
        nameof(SubMenu_FixedItems_AreNotDraggable), async () =>
    {
        await GotoAndExpandGroupAsync();

        // Pinned items are rendered outside MudDropContainer entirely: whether or not dragging one
        // toward the sortable segment happens to raise an OnSortChanged event (MudBlazor may still
        // resolve a drop target), the fixed item itself must never actually change position.
        await DragAsync("nav-report-pinned-top", "nav-report-alpha");

        var order = await GetItemOrderAsync();
        Assert.Equal("nav-report-pinned-top", order[0]);
        Assert.Equal("nav-report-pinned-bottom", order[^1]);

        // ...and dragging a sortable item onto a pinned one must not move it past the fixed edges.
        await DragAsync("nav-report-alpha", "nav-report-pinned-top");
        order = await GetItemOrderAsync();
        Assert.Equal("nav-report-pinned-top", order[0]);
        Assert.Equal("nav-report-pinned-bottom", order[^1]);
    });
}
