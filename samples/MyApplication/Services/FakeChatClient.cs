using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace MyApplication.Services;

/// <summary>
/// Stand-in for a model so the chat demos need no AI service. It streams reasoning then text, and fakes server-side
/// tool calls: "usage" produces a tool call whose result is rendered as a chart, "delete" asks for approval first.
/// </summary>
public sealed class FakeChatClient : IChatClient
{
    public const string UsageTool = "usage_chart";
    public const string DeleteTool = "delete_report";

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("The demo agent only streams.");

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var history = messages.ToList();

        // Second leg of an approval flow: the user answered the request.
        var approval = history.LastOrDefault()?.Contents.OfType<ToolApprovalResponseContent>().FirstOrDefault();
        if (approval is not null)
        {
            if (approval.Approved)
            {
                var call = (approval.ToolCall as FunctionCallContent) ?? new FunctionCallContent("delete-1", DeleteTool);
                yield return Update(new FunctionResultContent(call.CallId, "deleted"));
                await foreach (var u in Words("Done: the report was deleted.", cancellationToken)) { yield return u; }
            }
            else
            {
                await foreach (var u in Words("Understood, nothing was deleted.", cancellationToken)) { yield return u; }
            }
            yield break;
        }

        var prompt = history.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "";

        foreach (var chunk in new[] { "The user wrote \"", prompt, "\". ", "Deciding how to answer…" })
        {
            await Task.Delay(120, cancellationToken);
            yield return Update(new TextReasoningContent(chunk));
        }

        if (prompt.Contains("usage", StringComparison.OrdinalIgnoreCase))
        {
            var call = new FunctionCallContent("usage-1", UsageTool, new Dictionary<string, object?> { ["period"] = "this week" });
            yield return Update(call);
            await Task.Delay(700, cancellationToken);
            yield return Update(new FunctionResultContent(call.CallId, new[] { new { Label = "Mon", Value = 40 }, new { Label = "Tue", Value = 65 }, new { Label = "Wed", Value = 85 } }));
            await foreach (var u in Words("Here is your token usage for this week.", cancellationToken)) { yield return u; }
        }
        else if (prompt.Contains("delete", StringComparison.OrdinalIgnoreCase))
        {
            var call = new FunctionCallContent("delete-1", DeleteTool, new Dictionary<string, object?> { ["report"] = "Q3 summary" });
            yield return Update(new ToolApprovalRequestContent("approval-1", call));
        }
        else
        {
            var text = $"You asked: \"{prompt}\". This reply streams token by token. Only the block being streamed re-renders, and the view keeps following the newest text until you scroll up. ";
            await foreach (var u in Words(string.Concat(Enumerable.Repeat(text, 12)), cancellationToken)) { yield return u; }
        }
    }

    private static async IAsyncEnumerable<ChatResponseUpdate> Words(string text, [EnumeratorCancellation] CancellationToken ct)
    {
        foreach (var word in text.Split(' '))
        {
            await Task.Delay(15, ct);
            yield return Update(new TextContent(word + " "));
        }
    }

    private static ChatResponseUpdate Update(AIContent content) =>
        new(ChatRole.Assistant, [content]) { MessageId = "assistant-msg" };

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose() { }
}
