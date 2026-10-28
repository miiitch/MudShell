# MdsScheduler

Day, week and month calendar for application-owned items. `MdsScheduler<TItem>` renders the free and
open-source [Radzen Blazor](https://blazor.radzen.com) scheduler (MIT licence, no paid tooling) behind a
MudBlazor toolbar. Radzen stays an internal implementation detail: consumers only use MudShell types.

The toolbar, dialogs and loading overlay are MudBlazor. The grid colours come from the active MudBlazor
palette, so light and dark themes follow automatically. Built-in Scheduler text is always English, whatever
the application culture (`fr-FR`, for example). Application content such as appointment titles is shown as typed.

## Setup

`MdsScheduler` needs two things from the host page, because Radzen's grid is structured by its stylesheet and
measures itself through its script:

```html
<link rel="stylesheet" href="_content/Radzen.Blazor/css/default-base.css" />
<script src="_content/Radzen.Blazor/Radzen.Blazor.js"></script>
```

Load the **`-base`** stylesheet, not the full theme: the full theme sets `body` colours and would override
the MudBlazor theme. `AddMudShell()` registers the Radzen `DialogService` the month view requires. No other
Radzen service is registered.

## Example

```razor
<MdsScheduler TItem="Appointment"
              Items="_appointments"
              StartProperty="@nameof(Appointment.Start)"
              EndProperty="@nameof(Appointment.End)"
              TextProperty="@nameof(Appointment.Title)"
              @bind-SelectedDate="_date"
              @bind-View="_view"
              AppointmentClicked="OnAppointmentClicked"
              SlotClicked="OnSlotClicked" />
```

`Appointment` is an ordinary class. It does not inherit from anything MudShell-specific:

```csharp
public sealed class Appointment
{
    public Guid Id { get; init; }
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
```

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Items` | `IEnumerable<TItem>?` | `null` | Items to display. Null shows an empty schedule |
| `StartProperty` / `EndProperty` / `TextProperty` | `string` | empty | Names of the public `TItem` properties to map. Required; an unknown or empty name throws with the component and parameter name |
| `SelectedDate` / `SelectedDateChanged` | `DateTime?` / `EventCallback<DateTime?>` | today | Anchor date of the displayed period. Two-way bindable |
| `View` / `ViewChanged` | `MdsSchedulerView` / `EventCallback<MdsSchedulerView>` | `Week` | Day, Week or Month. Two-way bindable |
| `AppointmentClicked` | `EventCallback<MdsSchedulerAppointmentEventArgs<TItem>>` | | An appointment was selected. `Item` is the application item |
| `SlotClicked` | `EventCallback<MdsSchedulerSlotEventArgs>` | | An empty slot was selected. `Start` and `End` give its range |
| `AppointmentTemplate` | `RenderFragment<TItem>?` | `null` | Custom content for each appointment. Null shows the appointment text |
| `Loading` | `bool` | `false` | Shows a MudBlazor loading overlay over the schedule |
| `Disabled` | `bool` | `false` | Disables navigation and the view buttons, and makes the grid inert. Content stays readable |
| `Class` / `Style` | `string?` | `null` | Applied to the root element |

Any other HTML attribute (for example `data-testid` or an ARIA attribute) is applied to the root element.

## Events

- `AppointmentClicked` is the place to open a `MudDialog` for editing.
- `SlotClicked` is the place to open a `MudDialog` for creating an appointment.
- The Scheduler never opens a Radzen dialog and never mutates `Items`. The application owns the data.

```csharp
private async Task OnSlotClicked(MdsSchedulerSlotEventArgs args)
{
    var parameters = new DialogParameters<AppointmentDialog>
    {
        { d => d.Start, args.Start },
        { d => d.End, args.End },
    };
    var dialog = await Dialogs.ShowAsync<AppointmentDialog>("New appointment", parameters);
    // Add the appointment to _appointments when the dialog returns a result.
}
```

## Custom appointment template

```razor
<MdsScheduler TItem="Appointment" ... AppointmentTemplate="RenderAppointment" />

@code {
    private RenderFragment<Appointment> RenderAppointment = item => @<MudText Typo="Typo.body2">@item.Title</MudText>;
}
```

## Loading and disabled states

`Loading` overlays the schedule with a `MudProgressCircular` and sets `aria-busy`. `Disabled` makes the
toolbar buttons disabled, sets `aria-disabled` and makes the grid `inert`, so keyboard and pointer input both
stop, while the appointments remain visible.

## Theme behaviour

- The grid, appointments, headers and focus outlines are bound to MudBlazor palette tokens
  (`--mud-palette-*`, `--mud-typography-default-family`, `--mud-default-borderradius`).
- No light or dark colour is hard-coded, and no Radzen palette is shown.
- All Radzen rules are scoped under `.mds-scheduler`, so existing MudShell components are not affected.

## Responsive behaviour

The schedule keeps a fixed height (40 rem, or 32 rem below the `sm` breakpoint of 600 px) and scrolls inside
itself. The page never scrolls horizontally because of the schedule. The toolbar wraps on narrow screens.

## Language

Built-in Scheduler text is English everywhere: the toolbar labels (`Today`, `Day`, `Week`, `Month`),
period titles, weekday names and accessibility labels. The component passes `en-US` to Radzen explicitly
and does not change the application's `CurrentCulture`, so rendering a schedule has no global side effect.

## Limitations

- Drag, resize and recurrence are not supported in this version.
- There is no built-in empty-state message; an empty `Items` list shows an empty grid.
- The grid needs the host setup above. Without it, Radzen's stylesheet and script are missing and the
  schedule does not lay out.
- Radzen's own navigation header is hidden in favour of the MudBlazor toolbar, so Radzen's navigation
  template and header customisation are not available.
- `Items` is read as given; the component does not observe the collection for in-place changes. Assign a new
  collection after editing, as the demo does.
