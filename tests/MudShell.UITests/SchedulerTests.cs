using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

/// <summary>
/// Verifies the MdsScheduler demo page: English built-in UI, application content kept as typed,
/// view and period navigation, dialogs, loading and disabled states, and theme colours.
/// </summary>
public class SchedulerTests : PlaywrightTestBase
{
    protected override BrowserNewContextOptions ContextOptions => new()
    {
        BaseURL = BaseUrl,
        IgnoreHTTPSErrors = true,
        ViewportSize = new() { Width = 1280, Height = 900 },
    };

    private ILocator Toolbar => Page.Locator(".mds-scheduler-toolbar");

    private ILocator ViewButton(string label) => Toolbar.GetByRole(AriaRole.Button, new() { Name = label, Exact = true });

    private async Task OpenPageAsync()
    {
        await Page.GotoAsync("/scheduler");
        await Assertions.Expect(Page.Locator(".mds-scheduler .rz-scheduler")).ToBeVisibleAsync();
    }

    [Fact]
    public Task Scheduler_ShowsEnglishToolbarAndWeekDays() => RunAsync(nameof(Scheduler_ShowsEnglishToolbarAndWeekDays), async () =>
    {
        await OpenPageAsync();

        foreach (var label in new[] { "Today", "Day", "Week", "Month" })
        {
            await Assertions.Expect(ViewButton(label)).ToBeVisibleAsync();
        }

        await Assertions.Expect(ViewButton("Week")).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(Page.Locator(".mds-scheduler .rz-scheduler")).ToContainTextAsync("Sun");
        await Assertions.Expect(Page.Locator(".mds-scheduler .rz-scheduler")).ToContainTextAsync("Mon");
    });

    [Fact]
    public Task Scheduler_ForwardsConsumerAttributesToRoot() => RunAsync(nameof(Scheduler_ForwardsConsumerAttributesToRoot), async () =>
    {
        await OpenPageAsync();

        await Assertions.Expect(Page.Locator(".mds-scheduler[data-testid='demo-scheduler']")).ToBeVisibleAsync();
    });

    [Fact]
    public Task Scheduler_KeepsApplicationTitlesAsTyped() => RunAsync(nameof(Scheduler_KeepsApplicationTitlesAsTyped), async () =>
    {
        await OpenPageAsync();

        await Assertions.Expect(Page.Locator(".mds-scheduler .rz-event", new() { HasText = "Réunion client" })).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator(".mds-scheduler .rz-event", new() { HasText = "Congés Michel" }).First).ToBeVisibleAsync();
    });

    [Fact]
    public Task Scheduler_SwitchesToMonthAndShowsEnglishTitle() => RunAsync(nameof(Scheduler_SwitchesToMonthAndShowsEnglishTitle), async () =>
    {
        await OpenPageAsync();

        await ViewButton("Month").ClickAsync();

        await Assertions.Expect(ViewButton("Month")).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(Toolbar.Locator(".mds-scheduler-title")).ToHaveTextAsync(
            DateTime.Today.ToString("MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("en-US")));
    });

    [Fact]
    public Task Scheduler_NavigatesToNextWeek() => RunAsync(nameof(Scheduler_NavigatesToNextWeek), async () =>
    {
        await OpenPageAsync();

        var title = Toolbar.Locator(".mds-scheduler-title");
        var before = await title.TextContentAsync();

        await Toolbar.GetByRole(AriaRole.Button, new() { Name = "Next period" }).ClickAsync();

        await Assertions.Expect(title).Not.ToHaveTextAsync(before!);
    });

    [Fact]
    public Task Scheduler_OpensEditDialogOnAppointmentClick() => RunAsync(nameof(Scheduler_OpensEditDialogOnAppointmentClick), async () =>
    {
        await OpenPageAsync();

        await Page.Locator(".mds-scheduler .rz-event", new() { HasText = "Réunion client" }).First.ClickAsync();

        await Assertions.Expect(Page.Locator(".mud-dialog-title")).ToContainTextAsync("Edit appointment");
        await Assertions.Expect(Page.GetByLabel("Title")).ToHaveValueAsync("Réunion client");
    });

    [Fact]
    public Task Scheduler_ShowsLoadingOverlay_WhenLoadingEnabled() => RunAsync(nameof(Scheduler_ShowsLoadingOverlay_WhenLoadingEnabled), async () =>
    {
        await OpenPageAsync();

        await Page.GetByText("Loading", new() { Exact = true }).ClickAsync();

        await Assertions.Expect(Page.Locator(".mds-scheduler-body")).ToHaveAttributeAsync("aria-busy", "true");
        await Assertions.Expect(Page.Locator(".mds-scheduler-loading")).ToBeVisibleAsync();
    });

    [Fact]
    public Task Scheduler_BlocksInteraction_WhenDisabled() => RunAsync(nameof(Scheduler_BlocksInteraction_WhenDisabled), async () =>
    {
        await OpenPageAsync();

        await Page.GetByText("Disabled", new() { Exact = true }).ClickAsync();

        await Assertions.Expect(Page.Locator(".mds-scheduler-body")).ToHaveAttributeAsync("aria-disabled", "true");
        await Assertions.Expect(ViewButton("Day")).ToBeDisabledAsync();
    });

    [Fact]
    public Task Scheduler_GridFollowsMudBlazorSurface() => RunAsync(nameof(Scheduler_GridFollowsMudBlazorSurface), async () =>
    {
        await OpenPageAsync();

        // The Radzen grid must paint the MudBlazor surface token of the active theme, never a Radzen palette colour.
        var matches = await Page.EvaluateAsync<bool>(@"() => {
            const root = document.querySelector('.mds-scheduler');
            const grid = document.querySelector('.mds-scheduler .rz-scheduler');
            const probe = document.createElement('div');
            probe.style.backgroundColor = getComputedStyle(root).getPropertyValue('--mud-palette-surface');
            document.body.appendChild(probe);
            const expected = getComputedStyle(probe).backgroundColor;
            probe.remove();
            return getComputedStyle(grid).backgroundColor === expected;
        }");

        Assert.True(matches);
    });
}

/// <summary>Checks that the Scheduler fits a phone-width viewport without horizontal page scroll.</summary>
public class SchedulerMobileTests : PlaywrightTestBase
{
    protected override BrowserNewContextOptions ContextOptions => new()
    {
        BaseURL = BaseUrl,
        IgnoreHTTPSErrors = true,
        ViewportSize = new() { Width = 375, Height = 812 },
    };

    [Fact]
    public Task Scheduler_HasNoHorizontalPageOverflow_OnPhone() => RunAsync(nameof(Scheduler_HasNoHorizontalPageOverflow_OnPhone), async () =>
    {
        await Page.GotoAsync("/scheduler");
        await Assertions.Expect(Page.Locator(".mds-scheduler .rz-scheduler")).ToBeVisibleAsync();

        var overflows = await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth");

        Assert.False(overflows);
    });
}
