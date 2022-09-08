namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants that describe the behavior of the close button contained with
/// each TabViewItem
/// </summary>
public enum TabViewCloseButtonOverlayMode
{
    /// <summary>
    /// Behavior is defined by the framework. Default: this always maps to Always
    /// </summary>
    Auto = 0,

    /// <summary>
    /// The selected tab always shows the close button if it is closable. Unselected
    /// tabs show the close button when the tab is closable and the user has their 
    /// pointer over the tab
    /// </summary>
    OnPointerOver = 1,

    /// <summary>
    /// The selected tab always show the close button. Unselected tabs always show
    /// the close button if they are closable
    /// </summary>
    Always = 2,
}
