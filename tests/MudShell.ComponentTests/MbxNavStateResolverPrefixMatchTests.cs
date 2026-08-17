using MudShell.Components.Navigation.State;
using Xunit;

namespace MudShell.ComponentTests;

/// <summary>
/// <see cref="MbxNavStateResolver.IsPrefixMatch"/> backs <c>MdsSidebar</c>'s active-item highlighting.
/// A root href of "/" must not swallow every route in the app the way Blazor's built-in
/// <c>NavLinkMatch.Prefix</c> does, or the "Home" item stays highlighted no matter which page is open.
/// </summary>
public class MbxNavStateResolverPrefixMatchTests
{
    [Theory]
    [InlineData("/", "/", true)]
    [InlineData("/", "/admin/users", false)]
    [InlineData("/", "/cmdb/asset-types", false)]
    public void Should_OnlyMatchRootExactly_When_HrefIsSlash(string href, string currentUri, bool expected)
    {
        Assert.Equal(expected, MbxNavStateResolver.IsPrefixMatch(href, currentUri));
    }

    [Theory]
    [InlineData("/admin/settings", "/admin/settings", true)]
    [InlineData("/admin/settings", "/admin/users", false)]
    [InlineData("/cmdb/asset-types", "/cmdb/asset-types/new", true)]
    [InlineData("/cmdb/asset-types", "/cmdb/asset-types/ABC/edit", true)]
    [InlineData("/cmdb/asset-types", "/cmdb/assets", false)]
    public void Should_MatchOnSegmentBoundary_When_HrefIsNested(string href, string currentUri, bool expected)
    {
        Assert.Equal(expected, MbxNavStateResolver.IsPrefixMatch(href, currentUri));
    }

    [Fact]
    public void Should_ReturnFalse_When_HrefIsNull()
    {
        Assert.False(MbxNavStateResolver.IsPrefixMatch(null, "/admin/users"));
    }
}
