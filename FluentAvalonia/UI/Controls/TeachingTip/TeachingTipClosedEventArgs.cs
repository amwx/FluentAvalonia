using System;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="TeachingTip.Closed"/> event.
/// </summary>
public class TeachingTipClosedEventArgs : EventArgs
{
    internal TeachingTipClosedEventArgs(TeachingTipCloseReason reason)
    {
        Reason = reason;
    }

    /// <summary>
    /// Gets a constant that specifies whether the cause of the Closed event was due to user 
    /// interaction (Close button click), light-dismissal, or programmatic closure.
    /// </summary>
    public TeachingTipCloseReason Reason { get; }
}
