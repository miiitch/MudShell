# Chat components

A provider-agnostic set for building an assistant UI. MudShell renders; you handle models, quotas and tool calling.

| Component | Role |
|---|---|
| `MdsChatLanding` | Empty state: greeting, optional composer (child content) and suggested-prompt chips |
| `MdsChatTranscript` | Scrollable message list; follows new content unless the reader scrolled up, with a "jump to latest" button |
| `MdsChatMessage` | One message: avatar, header (name + timestamp), bubble, footer. Start/End positioning, RTL aware |
| `MdsChatStream` | Streaming message body: batched token appends, thinking indicator, caret |
| `MdsChatThinking` | Standalone "thinking" indicator |
| `MdsChatComposer` | Multiline auto-growing input with Send / Stop |
| `MdsChatPartRenderer` | Renders typed message parts with components from an `MdsChatPartRegistry` |
| `MdsChatFloating` | Corner button that opens the chat in a floating panel |

Add `@using MudShell.Components.Chat`. All components splat `data-testid` and other attributes onto their root.

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
| `Actions` / `EndActions` | — | Slots: leading (`+`, mode dropdown) and before Send (mic) |
| `SendLabel` / `StopLabel` / `AriaLabel` | | Accessible names |
| `MaxWidth` | `680px` | Use `100%` for full width |

Test ids: `chat-input`, `chat-send`, `chat-stop`, `chat-counter` (see `MbxTestId`). `FocusAsync()` focuses the input.

## MdsChatMessage

`Position` (`Start`/`End`), `Variant` (`Filled`/`Outlined`/`Plain`), `AuthorName`, `Timestamp`, `TimestampFormat`, `Header`, `Footer`, `Avatar` / `AvatarIcon` / `AvatarText`. `ChildContent` is free-form: render sanitised Markdown, tables, collapsible tool-call steps or charts.

## Streaming

`MdsChatStream` keeps its own state. Call `Append(token)` from your stream loop; only that component re-renders, at most once per `FlushInterval` (75 ms). Provide `ChildContent` (a `RenderFragment<string>` receiving the accumulated text) to render Markdown.

```razor
<MdsChatTranscript>
  @foreach (var m in _messages)
  {
    <MdsChatMessage Position="..." @key="m">
      <MdsChatStream @ref="m.Stream" IsStreaming="true">
        @context   @* accumulated text; pass it through your Markdown pipeline *@
      </MdsChatStream>
    </MdsChatMessage>
  }
</MdsChatTranscript>

await foreach (var token in model.StreamAsync(prompt, ct))
    m.Stream!.Append(token);
await m.Stream!.CompleteAsync();
```

`MdsChatTranscript` observes its content's size, so it keeps following the growing message without being re-rendered itself.

## Rendering agent metadata with custom components

`MdsChatMessage` and `MdsChatStream` accept any Blazor component as content, so metadata sent by the agent (a chart, tool-call steps, a citation list…) can be rendered by a component of your own. Today the routing is done in your page:

```razor
<MdsChatMessage>
  @foreach (var part in message.Parts)
  {
      switch (part.Kind)
      {
          case "chart":     <MyChart Data="part.Payload" />        break;
          case "tool-call": <MyToolSteps Steps="part.Payload" />   break;
          default:          <MyMarkdown Text="part.Text" />        break;
      }
  }
</MdsChatMessage>
```

### Part registry

Let the agent send typed parts and have each one rendered by a component you register.

```razor
@code {
    // One registry per page or agent.
    private readonly MdsChatPartRegistry _registry = new MdsChatPartRegistry()
        .Register<ChartPart, ChartData>("chart")          // payload goes to ChartPart.Payload
        .Register<ToolSteps, ToolCall[]>("tool-call", parameterName: "Calls");
}

<MdsChatMessage Registry="_registry"
                Parts="@(new[] { new MdsChatPart("chart", chartData) })">
    Here is the summary:
</MdsChatMessage>
```

- `MdsChatPart(Kind, Payload)`: the payload is an already-deserialised, typed object.
- `Register<TComponent, TPayload>(kind, parameterName = "Payload")`: the component must expose a parameter of type `TPayload` with that name. Registering a kind twice replaces the first entry.
- Parts render after `ChildContent`, in order, through `DynamicComponent`.
- An unregistered kind, or a payload that is not a `TPayload`, renders `PartFallback` (the payload as plain text by default). It never throws, so a new kind sent by the agent cannot break the UI.
- `MdsChatPartRenderer` is the same logic as a standalone component (`Parts`, `Registry`, `Fallback`) for use outside a message.
- While streaming, keep the running text in an `MdsChatStream` inside `ChildContent` and add parts to the list as their metadata arrives.

## Floating mode

`MdsChatFloating` shows a round button in the bottom corner (inline-end by default, so bottom-right in LTR) that opens the chat in a panel above the page. On phones the panel takes the full screen. Escape or the close button closes it.

```razor
<MdsChatFloating @bind-IsOpen="_open" Title="Assistant" Height="520px">
    <MdsChatTranscript>...</MdsChatTranscript>
    <MdsChatComposer OnSend="SendAsync" MaxWidth="100%" />
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
<MdsChatLanding Greeting="What's the latest?" Prompts="_suggestions" OnPromptSelected="SendAsync">
  <MdsChatComposer @bind-Value="_draft" OnSend="SendAsync" />
</MdsChatLanding>
```

## Theming

Styling uses MudBlazor palette variables (and the shell's `--mbx-*` tokens when present), so light and dark themes work without extra CSS. See the sample page `/chat`.
