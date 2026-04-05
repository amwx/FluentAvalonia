namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants that indicate the cause of the InfoBar closure.
/// </summary>
public enum FAInfoBarCloseReason
{
    /// <summary>
    /// The InfoBar was closed by the user clicking the close button.
    /// </summary>
    CloseButton = 0,

    /// <summary>
    /// The InfoBar was programmatically closed.
    /// </summary>
    Programmatic = 1
}
