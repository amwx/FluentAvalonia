namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="FATeachingTip.Closed"/> event.
/// </summary>
public class FATeachingTipClosedEventArgs : EventArgs
{
    internal FATeachingTipClosedEventArgs(FATeachingTipCloseReason reason)
    {
        Reason = reason;
    }

    /// <summary>
    /// Gets a constant that specifies whether the cause of the Closed event was due to user 
    /// interaction (Close button click), light-dismissal, or programmatic closure.
    /// </summary>
    public FATeachingTipCloseReason Reason { get; }
}
