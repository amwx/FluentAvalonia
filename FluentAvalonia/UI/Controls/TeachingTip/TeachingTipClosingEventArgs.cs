using System.ComponentModel;
using Avalonia.Threading;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

public class TeachingTipClosingEventArgs : CancelEventArgs
{
    public TeachingTipClosingEventArgs(TeachingTipCloseReason reason)
    {
        Reason = reason;
    }

    public TeachingTipCloseReason Reason { get; }

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
