namespace MudShell.Components.Chat;

/// <summary>Which side of the transcript a message sits on. Resolved against the document direction, so it flips in RTL.</summary>
public enum MdsChatPosition
{
    /// <summary>Inline start (left in LTR) — typically the assistant.</summary>
    Start,

    /// <summary>Inline end (right in LTR) — typically the user.</summary>
    End
}

/// <summary>Visual treatment of a message bubble.</summary>
public enum MdsChatBubbleVariant
{
    /// <summary>Tinted filled bubble.</summary>
    Filled,

    /// <summary>Transparent bubble with a border.</summary>
    Outlined,

    /// <summary>No bubble chrome: content flows on the page (assistant-style).</summary>
    Plain
}
