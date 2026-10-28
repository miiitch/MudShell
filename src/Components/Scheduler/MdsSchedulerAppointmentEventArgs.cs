namespace MudShell.Components.Scheduler;

/// <summary>Raised when an appointment of <see cref="MdsScheduler{TItem}"/> is selected.</summary>
/// <typeparam name="TItem">The application item type.</typeparam>
public sealed class MdsSchedulerAppointmentEventArgs<TItem>
{
    /// <summary>The application item behind the selected appointment.</summary>
    public required TItem Item { get; init; }
}
