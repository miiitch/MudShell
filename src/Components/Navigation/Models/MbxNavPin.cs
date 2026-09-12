namespace MudShell.Components.Navigation.Models;

/// <summary>Fixed position of an <see cref="MbxNavNode"/> among its parent's children.</summary>
public enum MbxNavPin
{
    /// <summary>Sortable node (middle segment).</summary>
    None,

    /// <summary>Always shown at the top of the sub-menu, in the order received; not draggable.</summary>
    Top,

    /// <summary>Always shown at the bottom of the sub-menu, in the order received; not draggable.</summary>
    Bottom,
}
