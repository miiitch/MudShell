using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

/// <summary>
/// Covers the sub-menu pin/trailing-action/drag-and-drop-sort demo on the "/demo" showcase page,
/// exercised through MdsContextNavPanel specifically (see SidebarNavSubMenuTests for the same
/// behavior through MdsSidebarNav — both share MdsSidebarSubMenu underneath).
/// </summary>
public class ContextNavPanelSubMenuTests : PlaywrightTestBase
{
    private const string GroupTestId = "nav-group-context-sort-demo";

    private async Task GotoAndExpandGroupAsync()
    {
        await Page.GotoAsync("/demo", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var toggle = Page.GetByTestId(GroupTestId);
        await Assertions.Expect(toggle).ToBeVisibleAsync();
        await toggle.ClickAsync();

        await Assertions.Expect(Page.GetByTestId("nav-context-item-pinned-top")).ToBeVisibleAsync();

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
        Assert.Equal("nav-context-item-pinned-top", order[0]);
        Assert.Equal("nav-context-item-pinned-bottom", order[^1]);
    });

    [Fact]
    public Task SubMenu_FixedItems_HaveNoTrailingAction() => RunAsync(
        nameof(SubMenu_FixedItems_HaveNoTrailingAction), async () =>
    {
        await GotoAndExpandGroupAsync();

        await Assertions.Expect(Page.GetByTestId("nav-action-context-item-pinned-top-0")).Not.ToBeAttachedAsync();
        await Assertions.Expect(Page.GetByTestId("nav-action-context-item-pinned-bottom-0")).Not.ToBeAttachedAsync();
    });

    [Fact]
    public Task SubMenu_ClickingTrailingAction_RemovesItemAndLogsEvent() => RunAsync(
        nameof(SubMenu_ClickingTrailingAction_RemovesItemAndLogsEvent), async () =>
    {
        await GotoAndExpandGroupAsync();

        await Assertions.Expect(Page.GetByTestId("nav-context-item-beta")).ToBeVisibleAsync();

        // The trailing action is only visually revealed on hover (opacity transition), but it is
        // already in the DOM and clickable regardless of hover state via Playwright.
        await Page.GetByTestId("nav-action-context-item-beta-0").ClickAsync();

        await Assertions.Expect(Page.GetByTestId("nav-context-item-beta")).Not.ToBeAttachedAsync();
        await Assertions.Expect(Page.GetByTestId("context-nav-sort-demo-log"))
            .ToContainTextAsync("Removed: context-item-beta");

        // Fixed items are unaffected by a sibling's removal.
        await Assertions.Expect(Page.GetByTestId("nav-context-item-pinned-top")).ToBeVisibleAsync();
        await Assertions.Expect(Page.GetByTestId("nav-context-item-pinned-bottom")).ToBeVisibleAsync();
    });

    private ILocator SubMenu => Page.Locator("nav[aria-label='Context Sort Demo']");

    private Task<string[]> GetItemOrderAsync() =>
        SubMenu.Locator("[data-testid^='nav-context-item']")
            .EvaluateAllAsync<string[]>("els => els.map(e => e.getAttribute('data-testid'))");

    /// <summary>
    /// Drags <paramref name="sourceId"/> onto <paramref name="targetId"/>. Dropping on the target's
    /// top half inserts the dragged item just before it; dropping on its bottom half inserts it
    /// just after — see SidebarNavSubMenuTests.DragAsync for why the live drag-over index (rather
    /// than MudItemDropInfo.IndexInZone) is what actually determines drop position.
    /// </summary>
    private async Task DragAsync(string sourceId, string targetId, bool dropOnBottomHalf = false)
    {
        // MdsContextNavPanel's rows (~31px, vs. the sidebar demo's taller rows) leave less margin
        // below the row's own bottom edge, so a bottom-half offset copied verbatim from
        // SidebarNavSubMenuTests would land right on the boundary with the next row and intercept
        // pointer events instead of registering on the target itself.
        var targetPosition = dropOnBottomHalf ? new TargetPosition { X = 5, Y = 22 } : new TargetPosition { X = 5, Y = 3 };

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
        await DragAsync("nav-context-item-gamma", "nav-context-item-alpha", dropOnBottomHalf: false);

        await Assertions.Expect(Page.GetByTestId("context-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged → [context-item-gamma, context-item-alpha, context-item-beta]");

        var order = await GetItemOrderAsync();
        Assert.Equal(
            [
                "nav-context-item-pinned-top", "nav-context-item-gamma", "nav-context-item-alpha",
                "nav-context-item-beta", "nav-context-item-pinned-bottom"
            ],
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

        await DragAsync("nav-context-item-gamma", "nav-context-item-alpha");
        await Assertions.Expect(Page.GetByTestId("context-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged → [context-item-gamma, context-item-alpha, context-item-beta]");

        await DragAsync("nav-context-item-alpha", "nav-context-item-beta", dropOnBottomHalf: true);
        await Assertions.Expect(Page.GetByTestId("context-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged → [context-item-gamma, context-item-beta, context-item-alpha]");

        // No Blazor circuit error surfaced (the bug this guards against threw
        // InvalidOperationException: "Duplicate navigation node id" from MbxNavTree.Validate,
        // which tears down the circuit and shows this banner).
        await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();

        var order = await GetItemOrderAsync();
        Assert.Equal(
            [
                "nav-context-item-pinned-top", "nav-context-item-gamma", "nav-context-item-beta",
                "nav-context-item-alpha", "nav-context-item-pinned-bottom"
            ],
            order);
        // Each id appears exactly once — the exact shape of the original bug.
        Assert.Equal(order.Length, order.Distinct().Count());
    });

    [Fact]
    public Task SubMenu_FixedItems_AreNotDraggable() => RunAsync(
        nameof(SubMenu_FixedItems_AreNotDraggable), async () =>
    {
        await GotoAndExpandGroupAsync();

        // Pinned items are rendered outside MudDropContainer entirely: whether or not dragging one
        // toward the sortable segment happens to raise an OnSortChanged event (MudBlazor may still
        // resolve a drop target), the fixed item itself must never actually change position.
        await DragAsync("nav-context-item-pinned-top", "nav-context-item-alpha");

        var order = await GetItemOrderAsync();
        Assert.Equal("nav-context-item-pinned-top", order[0]);
        Assert.Equal("nav-context-item-pinned-bottom", order[^1]);
    });
}
