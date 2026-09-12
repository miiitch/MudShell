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

        var subMenu = Page.Locator("nav[aria-label='Rapports']");
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
            .ToContainTextAsync("Retiré : report-beta");

        // Fixed items are unaffected by a sibling's removal.
        await Assertions.Expect(Page.GetByTestId("nav-report-pinned-top")).ToBeVisibleAsync();
        await Assertions.Expect(Page.GetByTestId("nav-report-pinned-bottom")).ToBeVisibleAsync();
    });

    [Fact]
    public Task SubMenu_DragAndDrop_ReordersSortableSegment_AndRaisesOnSortChanged() => RunAsync(
        nameof(SubMenu_DragAndDrop_ReordersSortableSegment_AndRaisesOnSortChanged), async () =>
    {
        await GotoAndExpandGroupAsync();

        var gamma = Page.GetByTestId("nav-report-gamma");
        var alpha = Page.GetByTestId("nav-report-alpha");

        await Assertions.Expect(gamma).ToBeVisibleAsync();
        await Assertions.Expect(alpha).ToBeVisibleAsync();

        // MudBlazor's drag-and-drop items are native HTML5 draggable elements (draggable="true"),
        // so the drag must go through the native dragstart/dragover/drop sequence — raw mouse
        // move/down/up (what a custom pointer-based DnD would need) does not trigger it.
        await gamma.DragToAsync(alpha);

        await Assertions.Expect(Page.GetByTestId("sidebar-nav-sort-demo-log"))
            .ToContainTextAsync("OnSortChanged");

        // Fixed items never move, regardless of drag activity in the sortable segment.
        var subMenu = Page.Locator("nav[aria-label='Rapports']");
        var itemTestIds = await subMenu.Locator("[data-testid^='nav-report']")
            .EvaluateAllAsync<string[]>("els => els.map(e => e.getAttribute('data-testid'))");

        Assert.Equal("nav-report-pinned-top", itemTestIds[0]);
        Assert.Equal("nav-report-pinned-bottom", itemTestIds[^1]);
    });
}
