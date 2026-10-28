namespace MudShell.Components.Scheduler;

/// <summary>The period shown by <see cref="MdsScheduler{TItem}"/>.</summary>
public enum MdsSchedulerView
{
    /// <summary>A single day.</summary>
    Day,

    /// <summary>A week, starting on the English first day of week (Sunday).</summary>
    Week,

    /// <summary>A calendar month.</summary>
    Month,
}
