using System.ComponentModel;

namespace FluentAvalonia.UI.Controls;

public class TeachingTipClosingEventArgs : CancelEventArgs
{
    public TeachingTipClosingEventArgs(TeachingTipCloseReason reason)
    {
        Reason = reason;
    }

    public TeachingTipCloseReason Reason { get; }

    // public Deferral GetDeferral
}
