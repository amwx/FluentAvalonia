using System.ComponentModel;
using Avalonia.Threading;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="TeachingTip.Closing"/> event.
/// </summary>
public class TeachingTipClosingEventArgs : CancelEventArgs
{
    internal TeachingTipClosingEventArgs(TeachingTipCloseReason reason)
    {
        Reason = reason;
    }

    /// <summary>
    /// Gets a constant that specifies whether the cause of the Closing event was due to 
    /// user interaction (Close button click), light-dismissal, or programmatic closure.
    /// </summary>
    public TeachingTipCloseReason Reason { get; }


    /// <summary>
    /// Gets a <see cref="Deferral"/> object for managing the work done in the Closing event handler.
    /// </summary>
    public Deferral GetDeferral()
    {
        _deferralCount++;

        return new Deferral(() =>
        {
            Dispatcher.UIThread.VerifyAccess();
            DecrementDeferralCount();
        });
    }

    internal void SetDeferral(Deferral deferral)
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
    private Deferral _deferral;
}
