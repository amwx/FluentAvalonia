using System;

namespace FluentAvalonia.UI.Controls;

public class TeachingTipClosedEventArgs : EventArgs
{
    internal TeachingTipClosedEventArgs(TeachingTipCloseReason reason)
    {
        Reason = reason;
    }

    public TeachingTipCloseReason Reason { get; }
}
