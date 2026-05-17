using System.ComponentModel;
using Avalonia.Threading;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="FATeachingTip.Closing"/> event.
/// </summary>
public class FATeachingTipClosingEventArgs : CancelEventArgs
{
    internal FATeachingTipClosingEventArgs(FATeachingTipCloseReason reason)
    {
        Reason = reason;
    }

    /// <summary>
    /// Gets a constant that specifies whether the cause of the Closing event was due to 
    /// user interaction (Close button click), light-dismissal, or programmatic closure.
    /// </summary>
    public FATeachingTipCloseReason Reason { get; }


    /// <summary>
    /// Gets a <see cref="FADeferral"/> object for managing the work done in the Closing event handler.
    /// </summary>
    public FADeferral GetDeferral()
    {
        _deferralCount++;

        return new FADeferral(() =>
        {
            Dispatcher.UIThread.VerifyAccess();
            DecrementDeferralCount();
        });
    }

    internal void SetDeferral(FADeferral deferral)
    {
        _deferral = deferral;
    }

    internal void DecrementDeferralCount()
    {
        _deferralCount--;
        if (_deferralCount == 0)
        {
            _deferral.Complete();
        }
    }

    internal void IncrementDeferralCount() => _deferralCount++;

    private int _deferralCount;
    private FADeferral _deferral;
}
