using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.Data;
using MudShell.Components.Layout;
using Xunit;

namespace MudShell.ComponentTests;

public class MdsDataBlocksTests
{
    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    // ── MdsStatTile ──────────────────────────────────────────────────────────

    [Fact]
    public void Should_RenderLabelAndValueAsDiv_When_NoOnClick()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsStatTile>(p => p
            .Add(c => c.Label, "Failing").Add(c => c.Value, "12"));

        Assert.Equal("DIV", rendered.Find(".mds-stat-tile").TagName);
        Assert.Equal("Failing", rendered.Find(".mds-stat-label").TextContent);
        Assert.Equal("12", rendered.Find(".mds-stat-value").TextContent);
    }

    [Fact]
    public void Should_RenderButtonAndInvokeCallback_When_OnClickSet()
    {
        using var context = NewContext();
        var clicks = 0;

        var rendered = context.RenderComponent<MdsStatTile>(p => p
            .Add(c => c.Label, "L").Add(c => c.Value, "1")
            .Add(c => c.OnClick, EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, () => clicks++)));
        rendered.Find("button.mds-stat-tile").Click();

        Assert.Equal(1, clicks);
        Assert.Contains("mds-stat-tile-clickable", rendered.Find(".mds-stat-tile").ClassList);
    }

    [Theory]
    [InlineData(MbxTone.Warning, "mds-tone-warning")]
    [InlineData(MbxTone.Danger, "mds-tone-danger")]
    [InlineData(MbxTone.Success, "mds-tone-success")]
    public void Should_ApplyToneClass_When_ToneNotDefault(MbxTone tone, string expected)
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsStatTile>(p => p.Add(c => c.Tone, tone).Add(c => c.Value, "1"));

        Assert.Contains(expected, rendered.Find(".mds-stat-tile").ClassList);
    }

    [Fact]
    public void Should_NotApplyToneClass_When_ToneDefault()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsStatTile>(p => p.Add(c => c.Value, "1"));

        Assert.DoesNotContain(rendered.Find(".mds-stat-tile").ClassList, c => c.StartsWith("mds-tone-"));
    }

    [Fact]
    public void Should_ShowSkeletonInsteadOfValue_When_Loading()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsStatTile>(p => p.Add(c => c.Loading, true).Add(c => c.Value, "99"));

        Assert.NotNull(rendered.Find(".mds-stat-skeleton"));
        Assert.Empty(rendered.FindAll(".mds-stat-value"));
    }

    [Fact]
    public void Should_RenderDeltaWithTone_When_DeltaSet()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsStatTile>(p => p
            .Add(c => c.Value, "1").Add(c => c.Delta, "+3").Add(c => c.DeltaTone, MbxDeltaTone.Positive));

        var delta = rendered.Find(".mds-stat-delta");
        Assert.Equal("+3", delta.TextContent);
        Assert.Contains("mds-delta-positive", delta.ClassList);
    }

    [Fact]
    public void Should_PreferValueContent_When_BothValueAndContentGiven()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsStatTile>(p => p
            .Add(c => c.Value, "plain")
            .Add(c => c.ValueContent, (RenderFragment)(b => b.AddMarkupContent(0, "<b>rich</b>"))));

        Assert.Equal("rich", rendered.Find(".mds-stat-value b").TextContent);
        Assert.DoesNotContain("plain", rendered.Find(".mds-stat-value").TextContent);
    }

    [Fact]
    public void Should_UseCellClass_When_VariantIsCell()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsStatTile>(p => p
            .Add(c => c.Variant, MbxStatTileVariant.Cell).Add(c => c.Value, "1"));

        Assert.Contains("mds-stat-tile-cell", rendered.Find(".mds-stat-tile").ClassList);
    }

    // ── MdsStatStrip ─────────────────────────────────────────────────────────

    [Fact]
    public void Should_RenderTilesInOneRowContainer_When_StripHasChildren()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsStatStrip>(p => p
            .AddChildContent<MdsStatTile>(t => t.Add(c => c.Value, "1"))
            .AddChildContent<MdsStatTile>(t => t.Add(c => c.Value, "2")));

        Assert.Equal(2, rendered.FindAll(".mds-stat-strip .mds-stat-tile").Count);
    }

    // ── MdsDistributionBar ───────────────────────────────────────────────────

    [Fact]
    public void Should_FoldTailIntoOther_When_SegmentsExceedMax()
    {
        using var context = NewContext();
        var segments = Enumerable.Range(1, 6).Select(i => new MbxDistributionSegment($"S{i}", i * 10)).ToList();

        var rendered = context.RenderComponent<MdsDistributionBar>(p => p
            .Add(c => c.Segments, segments).Add(c => c.MaxSegments, 3));

        var labels = rendered.FindAll(".mds-dist-label").Select(e => e.TextContent).ToArray();
        Assert.Equal(["S6", "S5", "S4", "Other"], labels);
        Assert.Equal("60", rendered.FindAll(".mds-dist-value").Last().TextContent); // 30 + 20 + 10
    }

    [Fact]
    public void Should_NotShowOther_When_SegmentsFitMax()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsDistributionBar>(p => p
            .Add(c => c.Segments, new[] { new MbxDistributionSegment("A", 1), new MbxDistributionSegment("B", 2) }));

        Assert.DoesNotContain(rendered.FindAll(".mds-dist-label"), e => e.TextContent == "Other");
    }

    [Fact]
    public void Should_UseToneClassInsteadOfRampShade_When_SegmentHasTone()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsDistributionBar>(p => p
            .Add(c => c.Segments, new[]
            {
                new MbxDistributionSegment("Fail", 5, Tone: MbxTone.Danger),
                new MbxDistributionSegment("Pass", 50),
            }));

        var segs = rendered.FindAll(".mds-dist-seg");
        Assert.Contains("mds-tone-bg-danger", segs[1].ClassList);
        Assert.DoesNotContain("background", segs[1].GetAttribute("style"));
        Assert.Contains("color-mix", segs[0].GetAttribute("style"));
    }

    [Fact]
    public void Should_RenderNothing_When_NoPositiveSegments()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsDistributionBar>(p => p
            .Add(c => c.Segments, new[] { new MbxDistributionSegment("A", 0) }));

        Assert.Empty(rendered.FindAll(".mds-dist-bar"));
    }

    // ── MdsSparkline ─────────────────────────────────────────────────────────

    [Fact]
    public void Should_RenderPolylineWithOnePointPerValue_When_ValuesGiven()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsSparkline>(p => p
            .Add(c => c.Values, new double[] { 1, 5, 3, 9 }));

        var points = rendered.Find("polyline").GetAttribute("points")!.Split(' ');
        Assert.Equal(4, points.Length);
        Assert.Equal("80", rendered.Find("svg").GetAttribute("width"));
        Assert.Equal("20", rendered.Find("svg").GetAttribute("height"));
    }

    [Fact]
    public void Should_RenderNoLine_When_FewerThanTwoValues()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsSparkline>(p => p.Add(c => c.Values, new double[] { 1 }));

        Assert.Empty(rendered.FindAll("polyline"));
    }

    [Fact]
    public void Should_ApplyToneClassAndNotDivideByZero_When_ValuesFlat()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsSparkline>(p => p
            .Add(c => c.Values, new double[] { 4, 4, 4 }).Add(c => c.Tone, MbxTone.Danger));

        Assert.Contains("mds-tone-danger", rendered.Find("svg").ClassList);
        Assert.DoesNotContain("NaN", rendered.Find("polyline").GetAttribute("points"));
    }

    // ── MdsRunStrip ──────────────────────────────────────────────────────────

    [Fact]
    public void Should_ShowOnlyLastMaxRuns_When_MoreRunsThanMax()
    {
        using var context = NewContext();
        var runs = Enumerable.Range(0, 8)
            .Select(i => new MbxRun(i < 5 ? MbxRunOutcome.Success : MbxRunOutcome.Failed, $"run {i}")).ToList();

        var rendered = context.RenderComponent<MdsRunStrip>(p => p.Add(c => c.Runs, runs).Add(c => c.Max, 4));

        var dots = rendered.FindAll(".mds-run-dot");
        Assert.Equal(["run 4", "run 5", "run 6", "run 7"], dots.Select(d => d.GetAttribute("title")).ToArray());
    }

    [Theory]
    [InlineData(MbxRunOutcome.Success, "mds-run-success")]
    [InlineData(MbxRunOutcome.Failed, "mds-run-failed")]
    [InlineData(MbxRunOutcome.Running, "mds-run-running")]
    [InlineData(MbxRunOutcome.Cancelled, "mds-run-cancelled")]
    [InlineData(MbxRunOutcome.Queued, "mds-run-queued")]
    public void Should_MapOutcomeToClassAndDefaultTooltip_When_NoTooltipGiven(MbxRunOutcome outcome, string cls)
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsRunStrip>(p => p.Add(c => c.Runs, new[] { new MbxRun(outcome) }));

        var dot = rendered.Find(".mds-run-dot");
        Assert.Contains(cls, dot.ClassList);
        Assert.Equal(outcome.ToString(), dot.GetAttribute("title"));
    }

    // ── MdsTabHeader / MdsTabbedFill ─────────────────────────────────────────

    [Fact]
    public void Should_ShowCountWithTone_When_CountAndToneSet()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsTabHeader>(p => p
            .Add(c => c.Text, "Findings").Add(c => c.Count, 7).Add(c => c.Tone, MbxTone.Danger));

        Assert.Equal("7", rendered.Find(".mds-tab-header-count").TextContent);
        Assert.Contains("mds-tone-danger", rendered.Find(".mds-tab-header").ClassList);
    }

    [Fact]
    public void Should_HideCount_When_CountNull()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsTabHeader>(p => p.Add(c => c.Text, "Overview"));

        Assert.Empty(rendered.FindAll(".mds-tab-header-count"));
    }

    [Fact]
    public void Should_RenderTabsWithFillClasses_When_PanelsProvided()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsTabbedFill>(p => p
            .Add(c => c.ChildContent, (RenderFragment)(b =>
            {
                b.OpenComponent<MudBlazor.MudTabPanel>(0);
                b.AddAttribute(1, "Text", "One");
                b.AddAttribute(2, "ChildContent", (RenderFragment)(c => c.AddContent(0, "panel one")));
                b.CloseComponent();
            })));

        Assert.NotNull(rendered.Find(".mds-tabbed-fill-host .mud-tabs.mds-tabbed-fill"));
        Assert.Contains("mds-tabbed-fill-panels", rendered.Find(".mud-tabs-panels").ClassList);
        Assert.Contains("panel one", rendered.Markup);
    }
}
