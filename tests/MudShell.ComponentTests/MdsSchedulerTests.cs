using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor.Services;
using MudShell.Components.Scheduler;
using Xunit;

namespace MudShell.ComponentTests;

/// <summary>
/// Mapping validation of <see cref="MdsScheduler{TItem}"/>. Rendering the schedule itself is covered by
/// the UI tests, because RadzenScheduler does not complete its first render under bUnit.
/// </summary>
public class MdsSchedulerTests
{
    public sealed class Booking
    {
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    private static TestContext NewContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    [Fact]
    public void Should_ThrowClearError_When_MappedPropertyDoesNotExist()
    {
        // Given
        using var context = NewContext();

        // When
        var exception = Assert.Throws<InvalidOperationException>(() => context.RenderComponent<MdsScheduler<Booking>>(p => p
            .Add(c => c.StartProperty, "Begins")
            .Add(c => c.EndProperty, nameof(Booking.End))
            .Add(c => c.TextProperty, nameof(Booking.Title))));

        // Then
        Assert.Contains("MdsScheduler<Booking>", exception.Message);
        Assert.Contains("StartProperty", exception.Message);
        Assert.Contains("Begins", exception.Message);
    }

    [Fact]
    public void Should_ThrowClearError_When_MappedPropertyNameEmpty()
    {
        // Given
        using var context = NewContext();

        // When
        var exception = Assert.Throws<InvalidOperationException>(() => context.RenderComponent<MdsScheduler<Booking>>(p => p
            .Add(c => c.StartProperty, nameof(Booking.Start))
            .Add(c => c.EndProperty, nameof(Booking.End))
            .Add(c => c.TextProperty, string.Empty)));

        // Then
        Assert.Contains("TextProperty", exception.Message);
        Assert.Contains("required", exception.Message);
    }
}
