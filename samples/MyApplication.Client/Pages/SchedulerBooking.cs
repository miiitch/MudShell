namespace MyApplication.Client.Pages;

/// <summary>Application-owned appointment shown by the Scheduler demo. Business text stays as typed.</summary>
public sealed class SchedulerBooking
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public DateTime Start { get; set; }

    public DateTime End { get; set; }
}
