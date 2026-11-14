namespace MudShell.Components.Chat;

/// <summary>Which side of the transcript a message sits on. Resolved against the document direction, so it flips in RTL.</summary>
public enum MdsChatPosition
{
    /// <summary>Inline start (left in LTR) — typically the assistant.</summary>
    Start,

    /// <summary>Inline end (right in LTR) — typically the user.</summary>
    End
}
