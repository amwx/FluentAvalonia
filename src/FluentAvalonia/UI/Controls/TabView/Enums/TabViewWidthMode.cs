namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines constants that specify the width of the tab
/// </summary>
public enum TabViewWidthMode
{
    /// <summary>
    /// Each tab has the same width
    /// </summary>
    Equal = 0,

    /// <summary>
    /// Each tab adjusts its width to the content within the tab
    /// </summary>
    SizeToContent = 1,

    /// <summary>
    /// Unselected tabs collapse to show only their icon. The selected tab
    /// adjusts to display the content within the tab
    /// </summary>
    Compact = 2,
}
