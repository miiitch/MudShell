using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;

namespace MudShell.UITests;

public class ShowcaseTests : PlaywrightTestBase
{
    private static readonly string[] Families = ["navigation", "composition", "layouts", "documents", "conversation", "data"];

    [Fact]
    public Task Catalog_ExposesEveryFamilyAndSearchesComponentNames() => RunAsync(nameof(Catalog_ExposesEveryFamilyAndSearchesComponentNames), async () =>
    {
        await Page.GotoAsync("/components", new() { WaitUntil = WaitUntilState.NetworkIdle });
        foreach (var family in Families)
            await Assertions.Expect(Page.GetByTestId($"catalog-family-{family}")).ToBeVisibleAsync();

        var search = Page.GetByRole(AriaRole.Textbox, new() { Name = "Rechercher une famille ou un composant" });
        await search.FillAsync("MdsSparkline");
        await Assertions.Expect(Page.Locator("[data-testid^='catalog-family-']")).ToHaveCountAsync(1);
        await Assertions.Expect(Page.GetByTestId("catalog-family-data")).ToBeVisibleAsync();
        await search.FillAsync("no-such-component");
        await Assertions.Expect(Page.GetByText("Aucun composant ne correspond", new() { Exact = true })).ToBeVisibleAsync();
    });

    [Fact]
    public Task FamilyRoutes_RenderOneShellAndWorkingDemonstrations() => RunAsync(nameof(FamilyRoutes_RenderOneShellAndWorkingDemonstrations), async () =>
    {
        foreach (var family in Families)
        {
            await Page.GotoAsync($"/components/{family}", new() { WaitUntil = WaitUntilState.NetworkIdle });
            await Assertions.Expect(Page.Locator(".mbx-shell")).ToHaveCountAsync(1);
            await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
            await Assertions.Expect(Page.Locator(".mbx-context-panel")).ToBeVisibleAsync();
            await Assertions.Expect(Page.GetByText("No contextual navigation", new() { Exact = true })).Not.ToBeAttachedAsync();
        }
    });

    [Fact]
    public Task Appearance_UsesPresetsAndPersistsAcrossNavigation() => RunAsync(nameof(Appearance_UsesPresetsAndPersistsAcrossNavigation), async () =>
    {
        await Page.GotoAsync("/", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.GetByTestId("open-appearance").ClickAsync();
        var preset = Page.GetByRole(AriaRole.Combobox, new() { Name = "Thème", Exact = true });
        await Assertions.Expect(preset).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator("input[type='color']")).ToHaveCountAsync(0);
        await preset.ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "Teal", Exact = true }).ClickAsync();
        await Page.GetByTestId("theme-density").ClickAsync();
        await Page.GetByTestId("theme-background").ClickAsync();
        await Page.GetByTestId("theme-dark-mode").ClickAsync();
        await Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button, new() { Name = "Close" }).ClickAsync();
        await Page.GetByTestId("explore-components").ClickAsync();
        await Assertions.Expect(Page.GetByTestId("showcase-catalog")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator(".mbx-shell")).ToHaveClassAsync(new Regex("mbx-mode-image"));
        await Page.GetByTestId("open-appearance").ClickAsync();
        await Assertions.Expect(preset).ToContainTextAsync("Teal");
        await Assertions.Expect(Page.Locator("input[data-testid='theme-density'], [data-testid='theme-density'] input")).ToBeCheckedAsync();
        await Assertions.Expect(Page.Locator("input[data-testid='theme-dark-mode'], [data-testid='theme-dark-mode'] input")).Not.ToBeCheckedAsync();
    });

    [Fact]
    public Task Composition_ErrorRetryAndEmptyActionRestoreContent() => RunAsync(nameof(Composition_ErrorRetryAndEmptyActionRestoreContent), async () =>
    {
        await Page.GotoAsync("/components/composition", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.GetByRole(AriaRole.Combobox, new() { Name = "État de la section" }).ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "Erreur simulée", Exact = true }).ClickAsync();
        await Assertions.Expect(Page.GetByTestId("part-error")).ToBeVisibleAsync();
        await Page.GetByTestId("btn-retry").ClickAsync();
        await Assertions.Expect(Page.GetByTestId("part-error")).Not.ToBeAttachedAsync();
        await Assertions.Expect(Page.GetByText("Présentation du projet", new() { Exact = true })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Combobox, new() { Name = "État de la section" }).ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "Aucun résultat", Exact = true }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Ajouter un document fictif" }).ClickAsync();
        await Assertions.Expect(Page.GetByTestId("composition-count")).ToContainTextAsync("1 éléments");
    });

    [Theory]
    [InlineData(1280)]
    [InlineData(820)]
    [InlineData(390)]
    public Task HomeAndCatalog_DoNotOverflow(int width) => RunAsync($"{nameof(HomeAndCatalog_DoNotOverflow)}_{width}", async () =>
    {
        await Page.SetViewportSizeAsync(width, 900);
        foreach (var route in new[] { "/", "/components", "/components/composition", "/components/navigation" })
        {
            await Page.GotoAsync(route, new() { WaitUntil = WaitUntilState.NetworkIdle });
            Assert.True(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= window.innerWidth"));
            var main = Page.Locator(".mbx-main-inner");
            Assert.True(await main.EvaluateAsync<bool>("el => el.scrollWidth <= el.clientWidth + 1"), $"Main content overflows at {width}px on {route}");
            await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
        }
        var directory = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
        await Page.ScreenshotAsync(new() { Path = Path.Combine(directory, $"showcase-navigation-{width}.png"), FullPage = true });
    });

    [Fact]
    public Task ExamplesAndTechnicalRoutes_AreReachable() => RunAsync(nameof(ExamplesAndTechnicalRoutes_AreReachable), async () =>
    {
        foreach (var route in new[] { "/examples", "/examples/library", "/examples/weather", "/examples/cities",
            "/examples/workspace", "/examples/workspace/detail", "/technical", "/components/navigation/mobile" })
        {
            var response = await Page.GotoAsync(route, new() { WaitUntil = WaitUntilState.NetworkIdle });
            Assert.Equal(200, response!.Status);
            await Assertions.Expect(Page.Locator(".mbx-shell")).ToBeVisibleAsync();
            await Assertions.Expect(Page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
        }
    });

    [Fact]
    public Task Layouts_ScrollsLeavesAndResizesWithoutMovingHeader() => RunAsync(nameof(Layouts_ScrollsLeavesAndResizesWithoutMovingHeader), async () =>
    {
        await Page.SetViewportSizeAsync(1440, 900);
        await Page.GotoAsync("/components/layouts", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var header = Page.Locator(".mbx-page-header");
        var headerBefore = await header.BoundingBoxAsync();
        var mainLeaf = Page.GetByTestId("layout-split").Locator(".mds-split-main .mds-fill-region");
        var sideLeaf = Page.GetByTestId("layout-split").Locator(".mds-split-side .mds-fill-region");
        await mainLeaf.EvaluateAsync("el => el.scrollTop = 400");
        Assert.True(await mainLeaf.EvaluateAsync<double>("el => el.scrollTop") > 100);
        Assert.Equal(0, await sideLeaf.EvaluateAsync<double>("el => el.scrollTop"));
        var headerAfter = await header.BoundingBoxAsync();
        Assert.Equal(headerBefore!.Y, headerAfter!.Y);
        var separator = Page.GetByRole(AriaRole.Separator, new() { Name = "Resize side pane" });
        var value = int.Parse((await separator.GetAttributeAsync("aria-valuenow"))!);
        await separator.FocusAsync();
        await separator.PressAsync("ArrowLeft");
        await Assertions.Expect(separator).ToHaveAttributeAsync("aria-valuenow", (value + 16).ToString());
        await Page.GetByRole(AriaRole.Tab, new() { Name = "Région simple" }).ClickAsync();
        await Assertions.Expect(Page.GetByText("Une région qui remplit son onglet", new() { Exact = true })).ToBeVisibleAsync();
    });

    [Fact]
    public Task MobileNavigation_DrillsDownBeforeNavigatingAndOffersSimpleBar() => RunAsync(
        nameof(MobileNavigation_DrillsDownBeforeNavigatingAndOffersSimpleBar), async () =>
    {
        await Page.SetViewportSizeAsync(390, 900);
        await Page.GotoAsync("/components/navigation/mobile", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var demo = Page.GetByTestId("mobile-drilldown-demo");
        await demo.GetByRole(AriaRole.Button, new() { Name = "Composants", Exact = true }).ClickAsync();
        var panel = demo.Locator(".mbx-mobile-drilldown-panel");
        await Assertions.Expect(panel).ToBeVisibleAsync();
        await panel.GetByText("Documents et filtres", new() { Exact = true }).ClickAsync();
        await Assertions.Expect(Page).ToHaveURLAsync(new Regex("/components/navigation/mobile$"));
        await panel.GetByText("Démonstration", new() { Exact = true }).ClickAsync();
        await Assertions.Expect(Page).ToHaveURLAsync(new Regex("/components/documents$"));

        await Page.GotoAsync("/components/navigation/mobile", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.GetByTestId("mobile-nav-mode").ClickAsync();
        await Assertions.Expect(Page.GetByTestId("mobile-bottom-demo")).ToBeVisibleAsync();
        await Page.GetByTestId("mobile-bottom-demo").GetByRole(AriaRole.Link, new() { Name = "Exemples", Exact = true }).ClickAsync();
        await Assertions.Expect(Page).ToHaveURLAsync(new Regex("/examples$"));
    });
}
