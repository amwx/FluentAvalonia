namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants that specify what action causes a SelectionChanged event to occur.
/// </summary>
public enum FAComboBoxSelectionChangedTrigger
{
    /// <summary>
    /// A change event occurs when the user commits a selection in the ComboBox
    /// </summary>
    Committed,

    /// <summary>
    /// A change event occurs each time the user navigates to a new selection in the ComboBox
    /// </summary>
    Always
}
