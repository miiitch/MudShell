using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

/// <summary>
/// Covers the sub-menu pin/trailing-action/drag-and-drop-sort demo on the "/demo" showcase page,
/// exercised through MdsContextNavPanel specifically (see SidebarNavSubMenuTests for the same
/// behavior through MdsSidebarNav — both share MdsSidebarSubMenu underneath).
///
/// The demo has two sibling groups under the same context-panel section — "Context Sort Demo"
/// (the primary group, at the root of the section) and "Context Sort Demo Secondary" (a second,
/// independent group next to it) — so both are covered: sorting at the root of the section, and
/// sorting in another group of that same section. Because MdsContextNavPanel's OnSortChanged
/// fires once per panel (not once per group), a drag in either must never bleed into the other's
/// order or log; that cross-group isolation is asserted explicitly below.
/// </summary>
public class ContextNavPanelSubMenuTests : PlaywrightTestBase
{
    private async Task ExpandGroupAsync(string groupTestId, string firstItemTestId)
    {
        var toggle = Page.GetByTestId(groupTestId);
        await Assertions.Expect(toggle).ToBeVisibleAsync();
        await toggle.ClickAsync();

        await Assertions.Expect(Page.GetByTestId(firstItemTestId)).ToBeVisibleAsync();

        // Blazor Server prerenders "/demo" over plain HTTP before the SignalR circuit takes over;
        // a click issued the instant the interactive DOM appears can still land on the outgoing
        // prerendered markup. Give the circuit a moment to fully take over before interacting further.
        await Page.WaitForTimeoutAsync(200);
    }

    private Task GotoDemoAsync() =>
        Page.GotoAsync("/demo", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

    private Task<string[]> GetItemOrderAsync(string idPrefix) =>
        Page.Locator($"[data-testid^='{idPrefix}']")
            .EvaluateAllAsync<string[]>("els => els.map(e => e.getAttribute('data-testid'))");

    /// <summary>
    /// Drags <paramref name="sourceId"/> onto <paramref name="targetId"/>. Dropping on the target's
    /// top half inserts the dragged item just before it; dropping on its bottom half inserts it
    /// just after — see SidebarNavSubMenuTests.DragAsync for why the live drag-over index (rather
    /// than MudItemDropInfo.IndexInZone) is what actually determines drop position.
    /// </summary>
    private async Task DragAsync(string idPrefix, string sourceId, string targetId, bool dropOnBottomHalf = false)
    {
        // MdsContextNavPanel's rows (~31px, vs. the sidebar demo's taller rows) leave less margin
        // below the row's own bottom edge, so a bottom-half offset copied verbatim from
        // SidebarNavSubMenuTests would land right on the boundary with the next row and intercept
        // pointer events instead of registering on the target itself.
        var targetPosition = dropOnBottomHalf ? new TargetPosition { X = 5, Y = 22 } : new TargetPosition { X = 5, Y = 3 };

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            var orderBefore = await GetItemOrderAsync(idPrefix);
            await Page.GetByTestId(sourceId).DragToAsync(
                Page.GetByTestId(targetId),
                new LocatorDragToOptions { TargetPosition = targetPosition });
            await Page.WaitForTimeoutAsync(200);

            var orderAfter = await GetItemOrderAsync(idPrefix);
            if (!orderBefore.SequenceEqual(orderAfter))
                return;
        }

        // Every attempt left the order unchanged. That is the expected, genuine outcome for a
        // pinned item (nothing to reorder), so callers that expect an actual reorder must assert
        // on the resulting order/log themselves — this helper does not throw on a no-op.
    }

    // ── Scenario 1: sorting at the root of the section ("Context Sort Demo") ───────────────────

    private const string PrimaryGroupTestId = "nav-group-context-sort-demo";
    private const string PrimaryIdPrefix = "nav-context-item";

    private Task ExpandPrimaryGroupAsync() =>
        ExpandGroupAsync(PrimaryGroupTestId, "nav-context-item-pinned-top");

    [Fact]
    public Task PrimaryGroup_FixedTopAndBottomItems_AreRenderedAtTheirPositions() => RunAsync(
        nameof(PrimaryGroup_FixedTopAndBottomItems_AreRenderedAtTheirPositions), async () =>
    {
        await GotoDemoAsync();
        await ExpandPrimaryGroupAsync();

        var order = await GetItemOrderAsync(PrimaryIdPrefix);
        Assert.NotEmpty(order);
        Assert.Equal("nav-context-item-pinned-top", order[0]);
        Assert.Equal("nav-context-item-pinned-bottom", order[^1]);
    });

    [Fact]
    public Task PrimaryGroup_FixedItems_HaveNoTrailingAction() => RunAsync(
        nameof(PrimaryGroup_FixedItems_HaveNoTrailingAction), async () =>
    {
        await GotoDemoAsync();
        await ExpandPrimaryGroupAsync();

        await Assertions.Expect(Page.GetByTestId("nav-action-context-item-pinned-top-0")).Not.ToBeAttachedAsync();
        await Assertions.Expect(Page.GetByTestId("nav-action-context-item-pinned-bottom-0")).Not.ToBeAttachedAsync();
    });

    [Fact]
    public Task PrimaryGroup_ClickingTrailingAction_RemovesItemAndLogsEvent() => RunAsync(
        nameof(PrimaryGroup_ClickingTrailingAction_RemovesItemAndLogsEvent), async () =>
    {
        await GotoDemoAsync();
        await ExpandPrimaryGroupAsync();

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

    [Fact]
    public Task PrimaryGroup_DragAndDrop_DropOnTopHalf_InsertsBeforeTarget() => RunAsync(
        nameof(PrimaryGroup_DragAndDrop_DropOnTopHalf_InsertsBeforeTarget), async () =>
    {
        await GotoDemoAsync();
        await ExpandPrimaryGroupAsync();

        // Starting order: alpha, beta, gamma. Dropping gamma on alpha's top half inserts it right
        // before alpha, at the very front of the sortable segment.
        await DragAsync(PrimaryIdPrefix, "nav-context-item-gamma", "nav-context-item-alpha", dropOnBottomHalf: false);

        await Assertions.Expect(Page.GetByTestId("context-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged[context-sort-demo] → [context-item-gamma, context-item-alpha, context-item-beta]");

        var order = await GetItemOrderAsync(PrimaryIdPrefix);
        Assert.Equal(
            [
                "nav-context-item-pinned-top", "nav-context-item-gamma", "nav-context-item-alpha",
                "nav-context-item-beta", "nav-context-item-pinned-bottom"
            ],
            order);
    });

    [Fact]
    public Task PrimaryGroup_DragAndDrop_RepeatedDrags_DoNotDuplicateOrCrash() => RunAsync(
        nameof(PrimaryGroup_DragAndDrop_RepeatedDrags_DoNotDuplicateOrCrash), async () =>
    {
        // Regression test: MbxNavNode is rebuilt (with fresh TrailingActions delegates) on every
        // render, so a fix that special-cased "the first drag" without accounting for record
        // identity churn across renders could still crash or duplicate ids on a *second* drag in
        // the same session. Two consecutive drags must both succeed cleanly.
        await GotoDemoAsync();
        await ExpandPrimaryGroupAsync();

        await DragAsync(PrimaryIdPrefix, "nav-context-item-gamma", "nav-context-item-alpha");
        await Assertions.Expect(Page.GetByTestId("context-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged[context-sort-demo] → [context-item-gamma, context-item-alpha, context-item-beta]");

        await DragAsync(PrimaryIdPrefix, "nav-context-item-alpha", "nav-context-item-beta", dropOnBottomHalf: true);
        await Assertions.Expect(Page.GetByTestId("context-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged[context-sort-demo] → [context-item-gamma, context-item-beta, context-item-alpha]");

        // No Blazor circuit error surfaced (the bug this guards against threw
        // InvalidOperationException: "Duplicate navigation node id" from MbxNavTree.Validate,
        // which tears down the circuit and shows this banner).
        await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();

        var order = await GetItemOrderAsync(PrimaryIdPrefix);
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
    public Task PrimaryGroup_FixedItems_AreNotDraggable() => RunAsync(
        nameof(PrimaryGroup_FixedItems_AreNotDraggable), async () =>
    {
        await GotoDemoAsync();
        await ExpandPrimaryGroupAsync();

        // Pinned items are rendered outside MudDropContainer entirely: whether or not dragging one
        // toward the sortable segment happens to raise an OnSortChanged event (MudBlazor may still
        // resolve a drop target), the fixed item itself must never actually change position.
        await DragAsync(PrimaryIdPrefix, "nav-context-item-pinned-top", "nav-context-item-alpha");

        var order = await GetItemOrderAsync(PrimaryIdPrefix);
        Assert.Equal("nav-context-item-pinned-top", order[0]);
        Assert.Equal("nav-context-item-pinned-bottom", order[^1]);
    });

    // ── Scenario 2: sorting in another group of the same section ("Context Sort Demo Secondary") ─

    private const string SecondaryGroupTestId = "nav-group-context-sort-demo-secondary";
    private const string SecondaryIdPrefix = "nav-context-secondary-item";

    private Task ExpandSecondaryGroupAsync() =>
        ExpandGroupAsync(SecondaryGroupTestId, "nav-context-secondary-item-pinned-top");

    [Fact]
    public Task SecondaryGroup_FixedTopAndBottomItems_AreRenderedAtTheirPositions() => RunAsync(
        nameof(SecondaryGroup_FixedTopAndBottomItems_AreRenderedAtTheirPositions), async () =>
    {
        await GotoDemoAsync();
        await ExpandSecondaryGroupAsync();

        var order = await GetItemOrderAsync(SecondaryIdPrefix);
        Assert.NotEmpty(order);
        Assert.Equal("nav-context-secondary-item-pinned-top", order[0]);
        Assert.Equal("nav-context-secondary-item-pinned-bottom", order[^1]);
    });

    [Fact]
    public Task SecondaryGroup_DragAndDrop_DropOnTopHalf_InsertsBeforeTarget() => RunAsync(
        nameof(SecondaryGroup_DragAndDrop_DropOnTopHalf_InsertsBeforeTarget), async () =>
    {
        await GotoDemoAsync();
        await ExpandSecondaryGroupAsync();

        // Starting order: alpha, beta, gamma. Dropping gamma on alpha's top half inserts it right
        // before alpha, at the very front of the sortable segment.
        await DragAsync(
            SecondaryIdPrefix, "nav-context-secondary-item-gamma", "nav-context-secondary-item-alpha",
            dropOnBottomHalf: false);

        await Assertions.Expect(Page.GetByTestId("context-nav-sort-demo-log"))
            .ToContainTextAsync(
                "OnSortChanged[context-sort-demo-secondary] → " +
                "[context-secondary-item-gamma, context-secondary-item-alpha, context-secondary-item-beta]");

        var order = await GetItemOrderAsync(SecondaryIdPrefix);
        Assert.Equal(
            [
                "nav-context-secondary-item-pinned-top", "nav-context-secondary-item-gamma",
                "nav-context-secondary-item-alpha", "nav-context-secondary-item-beta",
                "nav-context-secondary-item-pinned-bottom"
            ],
            order);
    });

    [Fact]
    public Task SecondaryGroup_FixedItems_AreNotDraggable() => RunAsync(
        nameof(SecondaryGroup_FixedItems_AreNotDraggable), async () =>
    {
        await GotoDemoAsync();
        await ExpandSecondaryGroupAsync();

        await DragAsync(
            SecondaryIdPrefix, "nav-context-secondary-item-pinned-top", "nav-context-secondary-item-alpha");

        var order = await GetItemOrderAsync(SecondaryIdPrefix);
        Assert.Equal("nav-context-secondary-item-pinned-top", order[0]);
        Assert.Equal("nav-context-secondary-item-pinned-bottom", order[^1]);
    });

    // ── Cross-group isolation: the two groups share one OnSortChanged handler on the panel ─────

    [Fact]
    public Task Groups_DragInOneGroup_DoesNotAffectTheOthersOrderOrLog() => RunAsync(
        nameof(Groups_DragInOneGroup_DoesNotAffectTheOthersOrderOrLog), async () =>
    {
        await GotoDemoAsync();

        // Opening the secondary group collapses the primary one (MdsContextNavPanel's
        // SingleExpandedLevel2 accordion) but its items stay in the DOM, just hidden — so both
        // orders can still be read back after each drag without re-expanding.
        await ExpandPrimaryGroupAsync();
        await DragAsync(PrimaryIdPrefix, "nav-context-item-gamma", "nav-context-item-alpha");
        await Assertions.Expect(Page.GetByTestId("context-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged[context-sort-demo] → [context-item-gamma, context-item-alpha, context-item-beta]");

        await ExpandSecondaryGroupAsync();
        await DragAsync(
            SecondaryIdPrefix, "nav-context-secondary-item-gamma", "nav-context-secondary-item-alpha");
        await Assertions.Expect(Page.GetByTestId("context-nav-sort-demo-log"))
            .ToContainTextAsync(
                "OnSortChanged[context-sort-demo-secondary] → " +
                "[context-secondary-item-gamma, context-secondary-item-alpha, context-secondary-item-beta]");

        // The primary group's own reorder from before the secondary group's drag is still intact —
        // a GroupId mix-up would otherwise have overwritten it with the secondary group's ids.
        var primaryOrder = await GetItemOrderAsync(PrimaryIdPrefix);
        Assert.Equal(
            [
                "nav-context-item-pinned-top", "nav-context-item-gamma", "nav-context-item-alpha",
                "nav-context-item-beta", "nav-context-item-pinned-bottom"
            ],
            primaryOrder);

        var secondaryOrder = await GetItemOrderAsync(SecondaryIdPrefix);
        Assert.Equal(
            [
                "nav-context-secondary-item-pinned-top", "nav-context-secondary-item-gamma",
                "nav-context-secondary-item-alpha", "nav-context-secondary-item-beta",
                "nav-context-secondary-item-pinned-bottom"
            ],
            secondaryOrder);

        await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
    });
}
