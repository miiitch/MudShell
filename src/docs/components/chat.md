# Chat components

MudShell's chat is built on the .NET 11 Blazor AI components (`Microsoft.AspNetCore.Components.AI`): an `IChatClient` (Microsoft.Extensions.AI) feeds a `UIAgent`, which turns streamed output into **content blocks** (rich text, tool calls, approvals, reasoning). MudShell supplies the shell: theming, layout, composer, landing and default block renderers. Models, quotas and tool execution stay yours.

> **Preview package.** `Microsoft.AspNetCore.Components.AI` is experimental and stays prerelease throughout .NET 11; its API may change. MudShell targets `net11.0` only.

| Component | Role |
|---|---|
| `MdsChat` | The conversation: transcript, input and default block renderers around a `UIAgent` |
| `MdsChatLanding` | Empty state: greeting, suggested-prompt chips (shown by `MdsChat` until the first message) |
| `MdsChatComposer` | Multiline auto-growing input with Send / Stop (used by `MdsChat`; usable on its own) |
| `MdsChatTranscript` | Scroll container that follows new content unless the reader scrolled up, with a "jump to latest" button |
| `MdsChatToolCall` | Default renderer of `FunctionInvocationContentBlock`: collapsible card with arguments and result |
| `MdsChatApproval` | Default renderer of `FunctionApprovalBlock`: Approve / Reject |
| `MdsChatReasoning` | Collapsible panel for `MdsChatReasoningBlock` (open while streaming, collapses when done) |
| `MdsChatFloating` | Corner button that opens the chat in a floating panel |

Add `@using MudShell.Components.Chat` and `@using Microsoft.AspNetCore.Components.AI`. Public components splat `data-testid` and other attributes onto their root.

## Setup

1. Load the package stylesheet before MudShell's bundle (it defines the `--sc-ai-*` tokens that MudShell maps to your MudBlazor palette):

```html
<link rel="stylesheet" href="@Assets["_content/Microsoft.AspNetCore.Components.AI/ai-chat.css"]" />
<link rel="stylesheet" href="@Assets["_content/MudShell/MudShell.bundle.scp.css"]" />
```

2. Create a `UIAgent` from any `IChatClient` and dispose it with the page.

```razor
@rendermode InteractiveServer
@implements IDisposable
@inject IChatClient ChatClient

<MdsChat Agent="_agent" Greeting="What's the latest?" Prompts="@_suggestions" MaxLength="500" />

@code {
    private UIAgent _agent = default!;
    private static readonly string[] _suggestions = ["Summarise my week", "Draft a status update"];

    protected override void OnInitialized() =>
        _agent = new UIAgent(ChatClient, options => options.AddMdsReasoning());

    public void Dispose() => _agent.Dispose();
}
```

`AddMdsReasoning()` maps `TextReasoningContent` to a collapsible reasoning panel. Without it, reasoning is dropped.

## MdsChat

| Parameter | Default | Description |
|---|---|---|
| `Agent` | required | The `UIAgent` driving the conversation |
| `Greeting` / `Subtitle` / `Icon` / `Prompts` | | Build the default landing; a prompt click sends its text |
| `WelcomeContent` | | Replaces the default landing |
| `Header` | | Content above the conversation |
| `MessageListContent` | | Your `BlockRenderer<T>` elements (see below) |
| `Placeholder` | `Ask anything…` | Input placeholder |
| `InputActions` / `InputEndActions` | | Slots: leading (`+`, mode dropdown) and before Send (mic) |
| `MaxLength` | `null` | Native `maxlength` plus an `n / max` counter |
| `MaxWidth` | `48rem` | Width of the conversation column; `100%` for full width |

The composer clears on send and turns Send into Stop while a turn streams (Stop calls `AgentContext.CancelAsync`). While an approval is pending the turn is waiting for you.

## Rendering blocks

Text and rich-text blocks use the package's renderer. MudShell registers `BlockRenderer`s for tool calls, approvals and reasoning. Add your own in `MessageListContent`; the most recently registered renderer that matches wins, so yours override the defaults:

```razor
<MdsChat Agent="_agent">
  <MessageListContent>
    <BlockRenderer TBlock="FunctionInvocationContentBlock" When="@(b => b.ToolName == "usage_chart")" Context="block">
      <MyUsageChart Block="block" />
    </BlockRenderer>
  </MessageListContent>
</MdsChat>
```

The block exposes the tool name, `Arguments`, `HasResult` and `Result`, and re-renders as they stream in. Use `Block.Result?.Result` (an object in-process, a `JsonElement` over AG-UI).

## MdsChatComposer

| Parameter | Default | Description |
|---|---|---|
| `Value` / `ValueChanged` | `""` | Two-way bind |
| `OnSend` | — | `EventCallback<string>` with the trimmed text |
| `OnStop` | — | Raised when Stop is clicked (only shown while `IsBusy`) |
| `IsBusy` | `false` | Send turns into Stop; sending is blocked |
| `Disabled` / `ReadOnly` | `false` | |
| `SendOnEnter` | `true` | `true`: Enter sends, Shift+Enter newline. `false`: Enter newline, Ctrl/Cmd+Enter sends |
| `ClearOnSend` | `true` | |
| `MaxLength` / `ShowCounter` | `null` / `true` | Native `maxlength` plus an `n / max` counter |
| `MaxRows` | `8` | Growth limit before the textarea scrolls |
| `Actions` / `EndActions` | — | Slots: leading and before Send |
| `SendLabel` / `StopLabel` / `AriaLabel` | | Accessible names |
| `MaxWidth` | `680px` | Use `100%` for full width |

Test ids: `chat-input`, `chat-send`, `chat-stop`, `chat-counter` (see `MbxTestId`). `FocusAsync()` focuses the input. Other test ids: `chat-transcript`, `chat-landing`, `chat-tool-call`, `chat-approval`, `chat-approve`, `chat-reject`, `chat-reasoning`.

## Limits of the preview

- The package's input is text only: attachments or voice go through `InputActions`/`InputEndActions` and `AgentContext.SendMessageAsync(ChatMessage)`.
- No Markdown parser ships with the package: produce `RichTextContent` in a `DelegatingChatClient`, or plain text renders as paragraphs.
- Frontend tools, shared state and AG-UI need the `AGUI.Client` package; see the Microsoft docs.

## Floating mode

`MdsChatFloating` shows a round button in the bottom corner (inline-end by default, so bottom-right in LTR) that opens the chat in a panel above the page. On phones the panel takes the full screen. Escape or the close button closes it.

```razor
<MdsChatFloating @bind-IsOpen="_open" Title="Assistant" Height="520px">
    <MdsChat Agent="_agent" MaxWidth="100%" />
</MdsChatFloating>
```

| Parameter | Default | Description |
|---|---|---|
| `IsOpen` / `IsOpenChanged` | `false` | Two-way bind |
| `Title` | `"Assistant"` | Header text and accessible name |
| `Position` | `End` | `Start` anchors to the opposite corner |
| `Width` / `Height` | `400px` / `600px` | Capped to the viewport |
| `HeaderActions` / `FabContent` | | Extra header buttons / custom button icon |
| `OpenLabel` / `CloseLabel` | | Accessible names |

Test ids: `chat-fab`, `chat-floating-panel`, `chat-floating-close`. Demo: `/chat-floating`.

## Landing

```razor
<MdsChatLanding Greeting="What's the latest?" Prompts="_suggestions" OnPromptSelected="SendAsync" />
```

## Theming

`MdsChat` maps the package's `--sc-ai-*` tokens (colours, radius, max width) onto MudBlazor palette variables and the shell's `--mbx-*` tokens, so light and dark themes work without extra CSS. Override a token on `.mds-chat-surface` to restyle. See the sample page `/chat`.
