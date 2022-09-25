namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants that indicate the cause of the <see cref="TeachingTip"/> closure.
/// </summary>
public enum TeachingTipCloseReason
{
    /// <summary>
    /// The teaching tip was closed by the user clicking the close button.
    /// </summary>
    CloseButton = 0,

    /// <summary>
    /// The teaching tip was closed by light-dismissal.
    /// </summary>
    LightDismiss = 1,

    /// <summary>
    /// The teaching tip was programmatically closed.
    /// </summary>
    Programmatic = 2
}
