using Avalonia.Controls;
using System;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the NavigationView.PaneClosing event.
/// </summary>
public class NavigationViewPaneClosingEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets a value that indicates whether the event should be canceled.
    /// </summary>
    public bool Cancel
    {
        get => _cancel;
        set
        {
            _cancel = value;
            if (SplitViewClosingArgs != null)
            {
                SplitViewClosingArgs.Cancel = value;
            }
        }
    }

    /// <summary>
    /// Gets the events pane closing event args from the SplitView
    /// </summary>
    public SplitViewPaneClosingEventArgs SplitViewClosingArgs { get; internal set; }

    private bool _cancel;
}
