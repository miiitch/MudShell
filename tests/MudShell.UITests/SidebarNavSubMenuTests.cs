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

        var order = await GetItemOrderAsync();
        Assert.NotEmpty(order);
        Assert.Equal("nav-report-pinned-top", order[0]);
        Assert.Equal("nav-report-pinned-bottom", order[^1]);
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

    /// <summary>
    /// Drags <paramref name="sourceId"/> onto <paramref name="targetId"/>. Dropping on the target's
    /// top half inserts the dragged item just before it; dropping on its bottom half inserts it
    /// just after — MdsSidebarNav tracks the live index via MudDropContainer.TransactionIndexChanged
    /// (MudItemDropInfo.IndexInZone itself is unreliable, see MdsSidebarSubMenu.HandleItemDropped).
    /// </summary>
    /// <remarks>
    /// MudBlazor's drop items are native HTML5 draggable elements, so the drag must go through the
    /// native dragstart/dragover/drop sequence — raw mouse move/down/up (what a custom pointer-based
    /// DnD would need) does not trigger it; Locator.DragToAsync does.
    ///
    /// Playwright's synthetic native drag simulation is occasionally swallowed by MudBlazor's JS
    /// interop (the drop never registers, so no ItemDropped/OnSortChanged fires at all) — not a
    /// product bug, just simulation flakiness. Retry the gesture a few times rather than the whole
    /// test, waiting for the round trip to Blazor Server to actually land before deciding whether
    /// this attempt took effect (checking too early would otherwise look like a no-op and cause an
    /// extra, unwanted real retry).
    /// </remarks>
    private async Task DragAsync(string sourceId, string targetId, bool dropOnBottomHalf = false)
    {
        var targetPosition = dropOnBottomHalf ? new TargetPosition { X = 5, Y = 30 } : new TargetPosition { X = 5, Y = 3 };

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            var orderBefore = await GetItemOrderAsync();
            await Page.GetByTestId(sourceId).DragToAsync(
                Page.GetByTestId(targetId),
                new LocatorDragToOptions { TargetPosition = targetPosition });
            await Page.WaitForTimeoutAsync(200);

            var orderAfter = await GetItemOrderAsync();
            if (!orderBefore.SequenceEqual(orderAfter))
                return;
        }

        // Every attempt left the order unchanged. That is the expected, genuine outcome for a
        // pinned item (nothing to reorder), so callers that expect an actual reorder must assert
        // on the resulting order/log themselves — this helper does not throw on a no-op.
    }

    [Fact]
    public Task SubMenu_DragAndDrop_DropOnTopHalf_InsertsBeforeTarget() => RunAsync(
        nameof(SubMenu_DragAndDrop_DropOnTopHalf_InsertsBeforeTarget), async () =>
    {
        await GotoAndExpandGroupAsync();

        // Starting order: alpha, beta, gamma. Dropping gamma on alpha's top half inserts it right
        // before alpha, at the very front of the sortable segment.
        await DragAsync("nav-report-gamma", "nav-report-alpha", dropOnBottomHalf: false);

        await Assertions.Expect(Page.GetByTestId("sidebar-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged → [report-gamma, report-alpha, report-beta]");

        var order = await GetItemOrderAsync();
        Assert.Equal(
            ["nav-report-pinned-top", "nav-report-gamma", "nav-report-alpha", "nav-report-beta", "nav-report-pinned-bottom"],
            order);
    });

    [Fact]
    public Task SubMenu_DragAndDrop_DropOnBottomHalfOfLastItem_MovesFirstItemToTheEnd() => RunAsync(
        nameof(SubMenu_DragAndDrop_DropOnBottomHalfOfLastItem_MovesFirstItemToTheEnd), async () =>
    {
        await GotoAndExpandGroupAsync();

        // Starting order: alpha, beta, gamma. Dropping alpha (currently first) on gamma's (last)
        // bottom half moves it all the way to the end of the sortable segment — this is the
        // "insert after the very last item" case that used to be unreachable (MudItemDropInfo's
        // own IndexInZone was always -1, so every drop clamped back to the front).
        await DragAsync("nav-report-alpha", "nav-report-gamma", dropOnBottomHalf: true);

        await Assertions.Expect(Page.GetByTestId("sidebar-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged → [report-beta, report-gamma, report-alpha]");

        var order = await GetItemOrderAsync();
        Assert.Equal(
            ["nav-report-pinned-top", "nav-report-beta", "nav-report-gamma", "nav-report-alpha", "nav-report-pinned-bottom"],
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

        // Drag 1: alpha, beta, gamma -> gamma dropped before alpha -> gamma, alpha, beta.
        await DragAsync("nav-report-gamma", "nav-report-alpha");
        await Assertions.Expect(Page.GetByTestId("sidebar-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged → [report-gamma, report-alpha, report-beta]");

        // Drag 2: gamma, alpha, beta -> alpha dropped after beta (now last) -> gamma, beta, alpha.
        await DragAsync("nav-report-alpha", "nav-report-beta", dropOnBottomHalf: true);
        await Assertions.Expect(Page.GetByTestId("sidebar-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged → [report-gamma, report-beta, report-alpha]");

        // No Blazor circuit error surfaced (the bug this guards against threw
        // InvalidOperationException: "Duplicate navigation node id" from MbxNavTree.Validate,
        // which tears down the circuit and shows this banner).
        await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();

        var order = await GetItemOrderAsync();
        Assert.Equal(
            ["nav-report-pinned-top", "nav-report-gamma", "nav-report-beta", "nav-report-alpha", "nav-report-pinned-bottom"],
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
