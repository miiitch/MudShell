# MdsConnectionStatus

Static connection and error UI for Blazor interactive server applications.
It remains usable while the circuit is disconnected, so retry and resume actions
are handled by the component's JavaScript module rather than Blazor event handlers.

Place one instance outside the interactive route tree, after `Routes` in the
application document:

```razor
<Routes @rendermode="InteractiveServer" />
<MdsConnectionStatus />
<script src="_framework/blazor.web.js"></script>
```

Import `MudShell.Components.Connection` and load MudShell's scoped CSS bundle.
The component provides the standard `components-reconnect-modal` and
`blazor-error-ui` elements, including retry, circuit resume and reload actions.
Do not add another instance of either element in the application layout.
`AdditionalAttributes` are forwarded to the reconnect dialog.
