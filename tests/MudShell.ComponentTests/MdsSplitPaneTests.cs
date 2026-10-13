using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.Layout;
using Xunit;

namespace MudShell.ComponentTests;

public class MdsSplitPaneTests
{
    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private static RenderFragment Text(string text) => b => b.AddContent(0, text);

    [Fact]
    public void Should_RenderMainAndSideRegions_When_SideOpen()
    {
        // Given
        using var context = NewContext();

        // When
        var rendered = context.RenderComponent<MdsSplitPane>(p => p
            .Add(c => c.Main, Text("main"))
            .Add(c => c.Side, Text("side")));

        // Then
        Assert.Equal("main", rendered.Find(".mds-split-main .mds-fill-region").TextContent);
        Assert.Equal("side", rendered.Find(".mds-split-side .mds-fill-region").TextContent);
        Assert.Equal("separator", rendered.Find(".mds-split-handle").GetAttribute("role"));
        Assert.Contains("width:360px", rendered.Find(".mds-split-side").GetAttribute("style"));
    }

    [Fact]
    public void Should_RenderRailInsteadOfSide_When_SideClosed()
    {
        // Given
        using var context = NewContext();

        // When
        var rendered = context.RenderComponent<MdsSplitPane>(p => p
            .Add(c => c.Main, Text("main"))
            .Add(c => c.Side, Text("side"))
            .Add(c => c.SideOpen, false));

        // Then
        Assert.Empty(rendered.FindAll(".mds-split-side"));
        Assert.NotNull(rendered.Find(".mds-split-rail"));
    }

    [Fact]
    public void Should_ReportSideOpenChange_When_ToggleClicked()
    {
        // Given
        using var context = NewContext();
        bool? reported = null;
        var rendered = context.RenderComponent<MdsSplitPane>(p => p
            .Add(c => c.Main, Text("main"))
            .Add(c => c.Side, Text("side"))
            .Add(c => c.SideOpenChanged, EventCallback.Factory.Create<bool>(this, v => reported = v)));

        // When
        rendered.Find(".mds-split-handle .mds-split-toggle").Click();

        // Then
        Assert.False(reported);
        Assert.NotNull(rendered.Find(".mds-split-rail"));
    }

    [Fact]
    public void Should_ClampWidthToBounds_When_SideWidthOutOfRange()
    {
        // Given
        using var context = NewContext();

        // When
        var rendered = context.RenderComponent<MdsSplitPane>(p => p
            .Add(c => c.Main, Text("main"))
            .Add(c => c.Side, Text("side"))
            .Add(c => c.SideWidth, 2000)
            .Add(c => c.MaxSideWidth, 500));

        // Then
        Assert.Contains("width:500px", rendered.Find(".mds-split-side").GetAttribute("style"));
    }

    [Fact]
    public void Should_ResizeWithKeyboardAndPersist_When_StorageKeySet()
    {
        // Given
        using var context = NewContext();
        var module = context.JSInterop.SetupModule("./_content/MudShell/js/mudshell-layout.js");
        module.Mode = JSRuntimeMode.Loose;
        var rendered = context.RenderComponent<MdsSplitPane>(p => p
            .Add(c => c.Main, Text("main"))
            .Add(c => c.Side, Text("side"))
            .Add(c => c.StorageKey, "k"));

        // When
        rendered.Find(".mds-split-handle").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowLeft" });

        // Then
        Assert.Contains("width:376px", rendered.Find(".mds-split-side").GetAttribute("style"));
        var save = module.Invocations.Single(i => i.Identifier == "save");
        Assert.Equal("k", save.Arguments[0]);
        Assert.Equal("376|1", save.Arguments[1]);
    }

    [Fact]
    public void Should_RestoreStoredState_When_StorageHasValue()
    {
        // Given
        using var context = NewContext();
        var module = context.JSInterop.SetupModule("./_content/MudShell/js/mudshell-layout.js");
        module.Mode = JSRuntimeMode.Loose;
        module.Setup<string?>("load", "k").SetResult("420|0");
        bool? open = null;

        // When
        var rendered = context.RenderComponent<MdsSplitPane>(p => p
            .Add(c => c.Main, Text("main"))
            .Add(c => c.Side, Text("side"))
            .Add(c => c.StorageKey, "k")
            .Add(c => c.SideOpenChanged, EventCallback.Factory.Create<bool>(this, v => open = v)));

        // Then
        Assert.False(open);
        Assert.NotNull(rendered.Find(".mds-split-rail"));
    }
    [Fact]
    public async Task Should_NotUseModuleAfterDispose_When_DisposedWhileRestoring()
    {
        // Given: restoring the stored width is still in flight when the component is disposed
        using var context = NewContext();
        var module = context.JSInterop.SetupModule("./_content/MudShell/js/mudshell-layout.js");
        var load = module.Setup<string?>("load", "k");
        var rendered = context.RenderComponent<MdsSplitPane>(p => p
            .Add(c => c.Main, Text("main"))
            .Add(c => c.Side, Text("side"))
            .Add(c => c.StorageKey, "k"));

        // When
        await rendered.InvokeAsync(() => context.DisposeComponents());
        load.SetResult("420|1");

        // Then: the pending render pass stops instead of wiring the resize handle on a disposed module
        Assert.DoesNotContain(module.Invocations, i => i.Identifier == "initResize");
    }
}
