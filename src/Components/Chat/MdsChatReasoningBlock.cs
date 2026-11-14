using System.Text.Json;
using Microsoft.AspNetCore.Components.AI;
using Microsoft.Extensions.AI;

namespace MudShell.Components.Chat;

/// <summary>
/// A model-provided reasoning summary that grows while it streams. Produced by <see cref="MdsChatReasoningHandler"/>
/// and rendered by <see cref="MdsChatReasoning"/> as a collapsible panel.
/// </summary>
/// <remarks>
/// Derives from <see cref="ActivityContentBlock"/> because that is the only block family the preview package lets
/// applications extend (it assigns the block identity).
/// </remarks>
public sealed class MdsChatReasoningBlock : ActivityContentBlock
{
    /// <summary><see cref="ActivityContentBlock.ActivityType"/> of reasoning blocks.</summary>
    public const string ReasoningActivityType = "reasoning";

    private string _text = string.Empty;

    public MdsChatReasoningBlock() => ActivityType = ReasoningActivityType;

    /// <summary>Reasoning text received so far.</summary>
    public string Text => _text;

    internal void Append(string text)
    {
        _text += text;
        Content = JsonSerializer.SerializeToElement(_text);
        NotifyChanged();
    }
}

/// <summary>Turns <see cref="TextReasoningContent"/> into a <see cref="MdsChatReasoningBlock"/> that updates in place.</summary>
public sealed class MdsChatReasoningHandler : ActivityHandler<MdsChatReasoningBlock>
{
    /// <inheritdoc />
    protected override bool TryCreateBlock(BlockMappingContext context, MdsChatReasoningBlock state) =>
        Consume(context, state);

    /// <inheritdoc />
    protected override bool TryUpdateBlock(BlockMappingContext context, MdsChatReasoningBlock state, out bool isCompleted)
    {
        var handled = Consume(context, state);
        // Any other content means the reasoning phase is over.
        isCompleted = !handled;
        return handled;
    }

    private static bool Consume(BlockMappingContext context, MdsChatReasoningBlock block)
    {
        string? text = null;
        foreach (var content in context.UnhandledContents)
        {
            if (content is TextReasoningContent reasoning)
            {
                text += reasoning.Text;
                context.MarkHandled(reasoning);
            }
        }

        if (text is null) { return false; }
        block.Append(text);
        return true;
    }
}

/// <summary>Registration helpers for MudShell's agent extensions.</summary>
public static class MdsChatAgentOptionsExtensions
{
    /// <summary>Renders <see cref="TextReasoningContent"/> as a collapsible <see cref="MdsChatReasoning"/> panel.</summary>
    public static UIAgentOptions AddMdsReasoning(this UIAgentOptions options)
    {
        options.AddBlockHandler(new MdsChatReasoningHandler());
        return options;
    }
}
