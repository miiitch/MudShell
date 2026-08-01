using System.Reflection;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace MudShell.ComponentTests;

/// <summary>
/// Guards attribute splatting across the component surface.
/// </summary>
/// <remarks>
/// Blazor does not ignore an attribute a component has not declared: it fails at render time with
/// "does not have an [Parameter(CaptureUnmatchedValues = true)] parameter". A consumer adding a
/// <c>data-testid</c> for end-to-end tests, or an ARIA attribute for accessibility, therefore
/// crashes the page rather than degrading gracefully — and nothing catches it at compile time.
/// <para>
/// These tests discover components by reflection, so a component added later is covered without
/// touching this file.
/// </para>
/// </remarks>
public class AdditionalAttributesTests
{
    private const string ProbeAttribute = "data-testid";
    private const string ProbeValue = "splat-probe";

    public static TheoryData<Type> AllComponents()
    {
        var data = new TheoryData<Type>();
        foreach (var type in DiscoverComponents())
            data.Add(type);
        return data;
    }

    /// <summary>Every component declares the splat parameter.</summary>
    [Theory]
    [MemberData(nameof(AllComponents))]
    public void Should_DeclareSplatParameter_When_ComponentIsPublic(Type componentType)
    {
        // Given
        var closed = Close(componentType);

        // When
        var property = closed.GetProperty("AdditionalAttributes");
        var parameter = property?.GetCustomAttribute<ParameterAttribute>();

        // Then
        Assert.True(
            property is not null,
            $"{Name(closed)} does not declare an AdditionalAttributes parameter.");
        Assert.True(
            parameter?.CaptureUnmatchedValues == true,
            $"{Name(closed)}.AdditionalAttributes is not marked CaptureUnmatchedValues.");
    }

    /// <summary>Every component forwards an undeclared attribute to its rendered markup.</summary>
    [Theory]
    [MemberData(nameof(AllComponents))]
    public void Should_RenderUndeclaredAttribute_When_ConsumerPassesOne(Type componentType)
    {
        // Given
        var closed = Close(componentType);
        using var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        // When
        var rendered = context.Render(builder =>
        {
            builder.OpenComponent(0, closed);
            builder.AddAttribute(1, ProbeAttribute, ProbeValue);
            builder.CloseComponent();
        });

        // Then
        Assert.Contains($"{ProbeAttribute}=\"{ProbeValue}\"", rendered.Markup);
    }

    /// <summary>
    /// The sidebar navigation renders a different root per expansion state, so both must splat.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_RenderUndeclaredAttribute_When_SidebarNavIsCollapsedOrExpanded(bool isExpanded)
    {
        // Given
        using var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        // When
        var rendered = context.RenderComponent<Components.Navigation.MdsSidebarNav>(parameters => parameters
            .Add(c => c.IsExpanded, isExpanded)
            .AddUnmatched(ProbeAttribute, ProbeValue));

        // Then
        Assert.Contains($"{ProbeAttribute}=\"{ProbeValue}\"", rendered.Markup);
    }

    private static IEnumerable<Type> DiscoverComponents() =>
        typeof(Components.FilterTabBar.MdsFilterTabBar<>).Assembly
            .GetTypes()
            .Where(type => type.IsPublic
                           && !type.IsAbstract
                           && typeof(IComponent).IsAssignableFrom(type)
                           && type.Name.StartsWith("Mds", StringComparison.Ordinal))
            .OrderBy(type => type.Name, StringComparer.Ordinal);

    /// <summary>Closes an open generic component over <see cref="string"/> so it can be rendered.</summary>
    private static Type Close(Type componentType) =>
        componentType.IsGenericTypeDefinition
            ? componentType.MakeGenericType(
                componentType.GetGenericArguments().Select(_ => typeof(string)).ToArray())
            : componentType;

    private static string Name(Type type) => type.Name.Split('`')[0];
}
