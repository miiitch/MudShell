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

### Not yet available: part registry (planned)

MudShell has no built-in mapping from agent metadata to components. The planned design, not implemented yet:

- `MdsChatMessage` accepts a list of parts (`Kind` + `Payload`) instead of a single content block.
- `MdsChatPartRegistry` maps a kind to a component type, e.g. `Register<ChartPart>("chart")`, registered once at startup or passed per instance.
- `MdsChatPartRenderer` resolves the component for each part with `DynamicComponent` and passes the payload as a parameter.
- An unregistered kind renders a `Fallback` (plain text by default), so a new kind sent by the agent never breaks the UI.
- Parts can be appended while streaming: the running text part stays in `MdsChatStream`, other parts appear as soon as their metadata arrives.

Open questions: typed object vs raw JSON payload, and global vs per-instance registry.

## Landing

```razor
<MdsChatLanding Greeting="What's the latest?" Prompts="_suggestions" OnPromptSelected="SendAsync">
  <MdsChatComposer @bind-Value="_draft" OnSend="SendAsync" />
</MdsChatLanding>
```

## Theming

Styling uses MudBlazor palette variables (and the shell's `--mbx-*` tokens when present), so light and dark themes work without extra CSS. See the sample page `/chat`.
