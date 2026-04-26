namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants that indicate the cause of the InfoBar closure.
/// </summary>
public enum InfoBarCloseReason
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

/// <summary>
/// Defines constants that indicate the criticality of the InfoBar that is shown.
/// </summary>
public enum InfoBarSeverity
{
    /// <summary>
    /// Communicates that the InfoBar is displaying general information that requires 
    /// the user's attention. For assistive technologies, they will follow the 
    /// behavior set in the Processing_All constant.
    /// </summary>
    Informational = 0,

    /// <summary>
    /// Communicates that the InfoBar is displaying information regarding a long-running 
    /// and/or background task that has completed successfully. For assistive technologies,
    /// they will follow the behavior set in the Processing_All constant.
    /// </summary>
    Success = 1,

    /// <summary>
    /// Communicates that the InfoBar is displaying information regarding a condition that 
    /// might cause a problem in the future. For assistive technologies, they will follow 
    /// the behavior set in the NotificationProcessing_ImportantAll constan
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Communicates that the InfoBar is displaying information regarding an error or problem 
    /// that has occurred. For assistive technologies, they will follow the behavior set in the 
    /// NotificationProcessing_ImportantAll constant.
    /// </summary>
    Error = 3,
}
