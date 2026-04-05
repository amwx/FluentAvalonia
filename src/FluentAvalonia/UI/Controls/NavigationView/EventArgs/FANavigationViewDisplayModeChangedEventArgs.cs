namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the NavigationView.DisplayModeChanged event.
/// </summary>
public class FANavigationViewDisplayModeChangedEventArgs : EventArgs
{
    internal FANavigationViewDisplayModeChangedEventArgs(FANavigationViewDisplayMode mode)
    {
        DisplayMode = mode;
    }

    /// <summary>
    /// Gets the new display mode.
    /// </summary>
    public FANavigationViewDisplayMode DisplayMode { get; }
}
