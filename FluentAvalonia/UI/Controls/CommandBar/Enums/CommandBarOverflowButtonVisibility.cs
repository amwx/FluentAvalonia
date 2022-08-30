namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants that specify when a command bar's overflow button is shown.
/// </summary>
public enum CommandBarOverflowButtonVisibility
{
    /// <summary>
    /// The overflow button automatically hides when there are no secondary commands and the closed 
    /// state of the CommandBar is the same as the open state.
    /// </summary>
    Auto,

    /// <summary>
    /// The overflow button is always shown.
    /// </summary>
    Visible,

    /// <summary>
    /// The overflow button is never shown.
    /// </summary>
    Collapsed
}
