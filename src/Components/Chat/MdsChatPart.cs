namespace MudShell.Components.Chat;

/// <summary>A piece of a message: a <see cref="Kind"/> sent by the agent and an already-deserialised <see cref="Payload"/>.</summary>
/// <param name="Kind">Key looked up in an <see cref="MdsChatPartRegistry"/> (e.g. <c>"chart"</c>, <c>"tool-call"</c>).</param>
/// <param name="Payload">The typed object handed to the registered component.</param>
public sealed record MdsChatPart(string Kind, object? Payload = null);
