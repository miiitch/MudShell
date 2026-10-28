namespace MudShell.Components.Scheduler;

/// <summary>Raised when an empty time slot of <see cref="MdsScheduler{TItem}"/> is selected.</summary>
public sealed class MdsSchedulerSlotEventArgs
{
    /// <summary>Start of the selected slot.</summary>
    public DateTime Start { get; init; }

    /// <summary>End of the selected slot.</summary>
    public DateTime End { get; init; }
}
