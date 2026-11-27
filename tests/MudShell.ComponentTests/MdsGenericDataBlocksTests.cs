using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudShell.Components.Data;
using Xunit;

namespace MudShell.ComponentTests;

public class MdsGenericDataBlocksTests
{
    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    [Fact]
    public void Should_RenderRichAndLinkedValuesAndClampColumns_When_ItemsProvided()
    {
        using var context = NewContext();
        var items = new[]
        {
            new MbxKeyValue("Name", "alpha", MbxTone.Danger, Mono: true),
            new MbxKeyValue("Empty", ""),
            new MbxKeyValue("Link", "details", Href: "/details"),
            new MbxKeyValue("Custom", "plain", Href: "/ignored", Content: builder => builder.AddContent(0, "rich")),
        };

        var rendered = context.RenderComponent<MdsKeyValueGrid>(parameters => parameters
            .Add(c => c.Items, items)
            .Add(c => c.Columns, 9)
            .Add(c => c.Class, "extra-class")
            .AddUnmatched("data-testid", "key-values"));

        var grid = rendered.Find("dl");
        var values = rendered.FindAll("dd");
        Assert.Contains("mds-key-value-grid-cols-2", grid.ClassList);
        Assert.Contains("extra-class", grid.ClassList);
        Assert.Equal("key-values", grid.GetAttribute("data-testid"));
        Assert.Contains("mds-key-value-mono", values[0].ClassList);
        Assert.Contains("mds-tone-danger", values[0].ClassList);
        Assert.Equal("—", values[1].TextContent.Trim());
        Assert.Equal("/details", rendered.Find("a").GetAttribute("href"));
        Assert.Equal("rich", values[3].TextContent.Trim());
        Assert.DoesNotContain(rendered.FindAll("dd a"), a => a.GetAttribute("href") == "/ignored");
    }

    [Fact]
    public void Should_OmitNonPositiveItemsAndPreserveGenericToneAndLink_When_RenderingAttentionBlock()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsAttentionBlock>(parameters => parameters
            .Add(c => c.Items, new[]
            {
                new MbxAttentionItem("failed checks", 2, MbxTone.Danger, "/checks"),
                new MbxAttentionItem("not shown", 0),
                new MbxAttentionItem("healthy", 1, MbxTone.Success),
            }));

        var items = rendered.FindAll(".mds-attention-item");
        Assert.Equal(2, items.Count);
        Assert.Contains("mds-attention-danger", items[0].ClassList);
        Assert.Equal("/checks", items[0].QuerySelector("a")!.GetAttribute("href"));
        Assert.Contains("2 failed checks", items[0].TextContent);
        Assert.Contains("mds-attention-success", items[1].ClassList);
        Assert.Contains("1 healthy", items[1].TextContent);
        Assert.DoesNotContain("not shown", rendered.Markup);
        Assert.Equal("status", rendered.Find(".mds-attention-block").GetAttribute("role"));
    }

    [Fact]
    public void Should_RenderNothing_When_AttentionItemsAreEmptyOrNonPositive()
    {
        using var context = NewContext();

        var noItems = context.RenderComponent<MdsAttentionBlock>();
        var emptyItems = context.RenderComponent<MdsAttentionBlock>(parameters => parameters
            .Add(c => c.Items, Array.Empty<MbxAttentionItem>()));
        var nonPositiveItems = context.RenderComponent<MdsAttentionBlock>(parameters => parameters
            .Add(c => c.Items, new[]
            {
                new MbxAttentionItem("zero", 0),
                new MbxAttentionItem("negative", -1),
            }));

        Assert.Empty(noItems.FindAll(".mds-attention-block"));
        Assert.Empty(emptyItems.FindAll(".mds-attention-block"));
        Assert.Empty(nonPositiveItems.FindAll(".mds-attention-block"));
    }

    [Fact]
    public void Should_RenderFullAndShortenedValueWithoutLink_When_HrefIsMissing()
    {
        using var context = NewContext();
        const string value = "repo/organization/component/12345678901234";

        var rendered = context.RenderComponent<MdsCopyableValue>(parameters => parameters
            .Add(c => c.Value, value));

        Assert.Empty(rendered.FindAll("a.mds-copyable-value-text"));
        Assert.Equal(value, rendered.Find(".mds-copyable-value-full").TextContent);
        Assert.Equal("repo/organization/component", rendered.Find(".mds-copyable-value-head").TextContent);
        Assert.Equal("/12345678901234", rendered.Find(".mds-copyable-value-tail").TextContent);
    }

    [Fact]
    public void Should_CopyFullValueToClipboard_When_CopyButtonIsClicked()
    {
        using var context = NewContext();

        var rendered = context.RenderComponent<MdsCopyableValue>(parameters => parameters
            .Add(c => c.Value, "secret-value")
            .Add(c => c.Href, "/value"));

        Assert.Equal("/value", rendered.Find("a.mds-copyable-value-text").GetAttribute("href"));
        rendered.Find("button").Click();

        var invocation = Assert.Single(
            context.JSInterop.Invocations,
            item => item.Identifier == "navigator.clipboard.writeText");
        Assert.Equal("navigator.clipboard.writeText", invocation.Identifier);
        Assert.Equal("secret-value", invocation.Arguments[0]);
    }
}
